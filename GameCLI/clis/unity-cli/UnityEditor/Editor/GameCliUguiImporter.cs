#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

// Source companion to the CLI UguiPlugin. Install under Assets/Editor/GameCLI.
[InitializeOnLoad]
public static class GameCliUguiImporter
{
    [Serializable] public class Request { public string designPath, assetsDirectory, prefabPath, expiresUtc; public bool dryRun, overwrite; }
    public class Design { public float width, height; }
    [Serializable] public class Result { public bool ok, dryRun; public string message, prefabPath; public int nodeCount, animationCount; public string templateId; public int templateVersion; public string[] warnings; }
    static double next;
    static GameCliUguiImporter() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + .25;
        var queue = Path.GetFullPath("Library/GameCLI/UGUI");
        if (!Directory.Exists(queue)) return;
        foreach (var file in Directory.GetFiles(queue, "request.json", SearchOption.AllDirectories))
        {
            var claimed = file + ".processing";
            try { File.Move(file, claimed); } catch (IOException) { continue; }
            var result = new Result();
            try
            {
                var request = JsonUtility.FromJson<Request>(File.ReadAllText(claimed));
                if (DateTime.Parse(request.expiresUtc).ToUniversalTime() < DateTime.UtcNow) throw new Exception("Request expired; run ugui again.");
                result = Run(request);
            }
            catch (Exception e) { result.message = e.Message; }
            var output = Path.Combine(Path.GetDirectoryName(file), "result.json");
            File.WriteAllText(output + ".tmp", JsonUtility.ToJson(result, true));
            File.Move(output + ".tmp", output);
            File.Delete(claimed);
        }
    }
    static Vector2 V2(JToken t) { return new Vector2((float)t["x"], (float)t["y"]); }
    static float Number(JToken n, string key, float fallback = 0) { return (float?)n[key] ?? fallback; }
    static void AssetPath(string path)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/") || path.Contains('\\') || path.Split('/').Any(p => p == ".." || p == "." || p == "")) throw new Exception("Invalid Assets path: " + path);
    }
    static Result Run(Request request)
    {
        string path = request.prefabPath; AssetPath(path);
        if (!path.EndsWith(".prefab")) throw new Exception("Expected .prefab output.");
        if (File.Exists(path) && !request.overwrite) throw new Exception("Prefab exists. Use --overwrite explicitly.");
        var record = JObject.Parse(File.ReadAllText(request.designPath));
        var payload = (JObject)(record["payload"] ?? record);
        var designJson = (JObject)payload["design"];
        if (designJson == null || (int?)designJson["schemaVersion"] != 1) throw new Exception("Expected database payload.design schemaVersion 1.");
        var design = new Design { width = Number(designJson, "width"), height = Number(designJson, "height") };
        if (design.width < 240 || design.height < 240 || design.width > 4096 || design.height > 4096) throw new Exception("Invalid design resolution.");
        var nodes = ((JArray)designJson["nodes"]).Cast<JObject>().ToArray();
        if (nodes.Length == 0 || nodes.Length > 300) throw new Exception("Expected 1..300 nodes.");
        var byId = nodes.ToDictionary(n => (string)n["id"]);
        var roots = nodes.Where(n => string.IsNullOrEmpty((string)n["parent"])).ToArray();
        if (roots.Length != 1 || (string)roots[0]["id"] != (string)payload["root"]) throw new Exception("payload.root must identify the only root node.");
        var warnings = new HashSet<string>();
        foreach (var n in nodes)
        {
            string type = (string)n["type"];
            if (!new[] {"Container", "Image", "Text", "Button", "Progress"}.Contains(type)) throw new Exception("Unsupported database node type: " + type);
            if (string.IsNullOrWhiteSpace((string)n["id"]) || string.IsNullOrWhiteSpace((string)n["name"]) || ((string)n["name"]).Contains('/')) throw new Exception("Invalid node identity.");
            if (Number(n,"width") <= 0 || Number(n,"height") <= 0) throw new Exception("Invalid node size.");
            var current = n; var seen = new HashSet<string>();
            while (!string.IsNullOrEmpty((string)current["parent"]))
                if (!seen.Add((string)current["id"]) || !byId.TryGetValue((string)current["parent"], out current)) throw new Exception("Invalid hierarchy.");
            if (n["action"] != null) warnings.Add("action is preview behavior metadata; runtime navigation is not generated.");
            if (n["previewHidden"] != null) warnings.Add("previewHidden is editor preview state; active controls Unity activation.");
            if ((string)n["imageTone"] == "light") throw new Exception("imageTone=light requires a material implementation; refusing to change the appearance silently.");
            if (n["imageFit"] != null && !new[] {"cover","contain","100% 100%"}.Contains((string)n["imageFit"])) throw new Exception("Unsupported imageFit.");
            if (n["action"]?["target"] != null && !byId.ContainsKey((string)n["action"]["target"])) throw new Exception("Missing action target.");
        }
        var textures = new Dictionary<string, Texture2D>();
        var temporary = new List<UnityEngine.Object>();
        var objects = new Dictionary<string, GameObject>();
        var imageBindings = new List<KeyValuePair<Graphic, JObject>>();
        GameObject root = null;
        string assetFolder = Path.GetDirectoryName(path).Replace('\\', '/') + "/" + Path.GetFileNameWithoutExtension(path) + "Assets";
        try
        {
            foreach (var id in nodes.Where(n => n["image"] != null).Select(n => (string)n["image"]).Distinct())
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-f0-9]{64}$")) throw new Exception("Invalid image ID.");
                var bytes = File.ReadAllBytes(Path.Combine(request.assetsDirectory, id + ".png"));
                using (var hash = System.Security.Cryptography.SHA256.Create())
                    if (BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant() != id) throw new Exception("Image hash mismatch.");
                var texture = new Texture2D(2,2); temporary.Add(texture);
                if (!texture.LoadImage(bytes)) throw new Exception("Invalid PNG: " + id);
                texture.wrapMode = TextureWrapMode.Repeat; textures.Add(id, texture);
            }
            foreach (var n in nodes)
            {
                var go = new GameObject((string)n["name"], typeof(RectTransform));
                objects.Add((string)n["id"], go); if (n == roots[0]) root = go;
            }
            foreach (var n in nodes)
            {
                var go = objects[(string)n["id"]]; var rect = (RectTransform)go.transform;
                string parent = (string)n["parent"];
                if (!string.IsNullOrEmpty(parent)) rect.SetParent(objects[parent].transform, false);
                var r = n["rectTransform"];
                if (r != null)
                {
                    rect.anchorMin = V2(r["anchorMin"]); rect.anchorMax = V2(r["anchorMax"]); rect.pivot = V2(r["pivot"]);
                    rect.anchoredPosition = V2(r["anchoredPosition"]); rect.sizeDelta = V2(r["sizeDelta"]);
                }
                else
                {
                    // Legacy persisted bounds are parent-local, top-left origin, y down.
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
                    rect.anchoredPosition = new Vector2(Number(n,"x"), -Number(n,"y")); rect.sizeDelta = new Vector2(Number(n,"width"), Number(n,"height"));
                }
                rect.localScale = n["localScale"] == null ? Vector3.one : new Vector3(Number(n["localScale"],"x",1),Number(n["localScale"],"y",1),1);
                rect.localEulerAngles = new Vector3(0,0,Number(n,"rotation"));
                go.layer = LayerMask.NameToLayer("UI"); go.SetActive((bool?)n["active"] ?? true);
                Color color;
                if (!ColorUtility.TryParseHtmlString((string)n["color"], out color)) throw new Exception("Invalid node color.");
                color.a *= Number(n,"opacity",1);
                string type = (string)n["type"]; Graphic graphic = null;
                if (type == "Text")
                {
                    var text = go.AddComponent<Text>(); text.text = (string)n["text"] ?? ""; text.fontSize = Mathf.RoundToInt(Number(n,"fontSize",20));
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    string alignment = ((string)n["verticalAlign"] == "top" ? "Upper" : (string)n["verticalAlign"] == "bottom" ? "Lower" : "Middle") + ((string)n["textAlign"] == "right" ? "Right" : (string)n["textAlign"] == "center" ? "Center" : "Left");
                    text.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), alignment); graphic = text;
                }
                else if (n["image"] != null)
                {
                    var texture = textures[(string)n["image"]]; var uv = n["uvRect"];
                    bool raw = uv != null || (string)n["imageFit"] == "cover";
                    if (raw)
                    {
                        var image = go.AddComponent<RawImage>(); image.texture = texture;
                        if (uv != null) image.uvRect = new Rect(Number(uv,"x"),Number(uv,"y"),Number(uv,"width",1),Number(uv,"height",1));
                        else
                        {
                            float boxRatio = Number(n,"width") / Number(n,"height"), ratio = (float)texture.width / texture.height;
                            image.uvRect = boxRatio < ratio ? new Rect((1-boxRatio/ratio)/2,0,boxRatio/ratio,1) : new Rect(0,(1-ratio/boxRatio)/2,1,ratio/boxRatio);
                        }
                        graphic = image;
                    }
                    else
                    {
                        var image = go.AddComponent<Image>(); var b = n["imageBorder"];
                        var border = b == null ? Vector4.zero : new Vector4(Number(b,"x"),Number(b,"y"),Number(b,"z"),Number(b,"w"));
                        image.sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border); temporary.Add(image.sprite);
                        image.type = border != Vector4.zero && (string)n["imageType"] != "simple" ? Image.Type.Sliced : Image.Type.Simple;
                        image.preserveAspect = image.type == Image.Type.Simple && ((string)n["imageFit"] ?? "contain") == "contain"; graphic = image;
                    }
                    imageBindings.Add(new KeyValuePair<Graphic,JObject>(graphic,n));
                }
                else if ((string)n["role"] == "frame")
                {
                    float border = Number(n,"borderWidth",6);
                    for (int i=0;i<4;i++)
                    {
                        var edge = new GameObject("Border"+i,typeof(RectTransform),typeof(Image)); edge.transform.SetParent(rect,false);
                        var e=(RectTransform)edge.transform; e.anchorMin=Vector2.zero; e.anchorMax=Vector2.one; e.offsetMin=e.offsetMax=Vector2.zero;
                        if(i<2){e.anchorMin=new Vector2(0,i);e.anchorMax=new Vector2(1,i);e.sizeDelta=new Vector2(0,border);e.anchoredPosition=new Vector2(0,i==0?border/2:-border/2);}
                        else {e.anchorMin=new Vector2(i-2,0);e.anchorMax=new Vector2(i-2,1);e.sizeDelta=new Vector2(border,0);e.anchoredPosition=new Vector2(i==2?border/2:-border/2,0);}
                        edge.GetComponent<Image>().color=color; edge.GetComponent<Image>().raycastTarget=false;
                    }
                }
                else if (type != "Container" || ((string)n["surface"] != "transparent" && (string)n["template"] != "ConfirmDialog" && (string)n["template"] != "InventorySystem")) graphic = go.AddComponent<Image>();
                if(graphic!=null){graphic.color=color;graphic.raycastTarget=type=="Button";}
                if(type=="Button") {var button=go.AddComponent<Button>(); button.targetGraphic=graphic;button.interactable=(bool?)n["interactable"]??true;}
                if((bool?)n["animationHost"]==true) go.AddComponent<Animator>();
            }
            foreach(var n in nodes)
            {
                var go=objects[(string)n["id"]];
                if((string)n["template"]=="ScrollPanel")
                {
                    var content=nodes.SingleOrDefault(c=>(string)c["parent"]==(string)n["id"] && (string)c["role"]=="content");
                    if(content==null)throw new Exception("ScrollPanel requires a content child.");
                    go.AddComponent<RectMask2D>();var scroll=go.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.viewport=(RectTransform)go.transform;scroll.content=(RectTransform)objects[(string)content["id"]].transform;
                    var graphic=go.GetComponent<Graphic>();if(graphic!=null)graphic.raycastTarget=true;
                }
                if((string)n["type"]=="Progress")
                {
                    var fill=nodes.SingleOrDefault(c=>(string)c["parent"]==(string)n["id"] && (string)c["name"]=="Fill");
                    if(fill==null)throw new Exception("Progress requires a Fill child.");
                    var slider=go.AddComponent<Slider>();slider.interactable=false;slider.fillRect=(RectTransform)objects[(string)fill["id"]].transform;slider.value=Number(n,"progress");
                }
            }
            // System documents fill a canvas; reusable prefab templates retain their persisted root size.
            bool system = (string)record["category_id"]=="system" || (string)record["category"]=="system" || (Number(roots[0],"width")==design.width && Number(roots[0],"height")==design.height);
            if(system){var rect=(RectTransform)root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,.5f);rect.offsetMin=rect.offsetMax=Vector2.zero;}
            var result = new Result {ok=true,dryRun=request.dryRun,prefabPath=path,nodeCount=nodes.Length,templateId=(string)record["id"],templateVersion=(int?)record["version"]??0,warnings=warnings.ToArray(),message="Database template validation passed: "+nodes.Length+" nodes"};
            result.animationCount = GameCliUguiAnimations.Apply(designJson["animations"],objects,assetFolder,true);
            var namesFile=Path.Combine(request.assetsDirectory,"asset-names.json");
            var sourceNames=File.Exists(namesFile)?JObject.Parse(File.ReadAllText(namesFile)):new JObject();
            var usedNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assetPaths=new Dictionary<Graphic,string>();
            foreach(var binding in imageBindings)
            {
                var n=binding.Value;var id=(string)n["image"];
                string original=((string)sourceNames[id]??"").Replace('\\','/');
                string stem=Path.GetFileNameWithoutExtension(original);
                if(string.IsNullOrWhiteSpace(stem))stem=(string)n["name"]+"Texture";
                stem=System.Text.RegularExpressions.Regex.Replace(stem,@"[^A-Za-z0-9_-]","_").Trim('_');
                if(string.IsNullOrEmpty(stem))stem="Texture";
                if(stem.Length>100)stem=stem.Substring(0,100);
                if(System.Text.RegularExpressions.Regex.IsMatch(stem,@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])$",System.Text.RegularExpressions.RegexOptions.IgnoreCase))stem="Texture_"+stem;
                stem=stem.ToLowerInvariant();
                string name=stem;int suffix=2;while(!usedNames.Add(name))name=stem+"_"+suffix++;
                string destination=assetFolder+"/"+name+".png";
                // Never replace an unrelated existing resource that happens to have the same name.
                if(File.Exists(destination)&&!File.ReadAllBytes(destination).SequenceEqual(File.ReadAllBytes(Path.Combine(request.assetsDirectory,id+".png"))))
                    throw new Exception("Resource name conflicts with existing different image: "+destination);
                assetPaths.Add(binding.Key,destination);
            }
            if(request.dryRun)return result;
            Directory.CreateDirectory(assetFolder);
            foreach(var binding in imageBindings)
            {
                var n=binding.Value;string id=(string)n["image"];
                // Per-node sprite import settings avoid shared-image border conflicts.
                string asset=assetPaths[binding.Key];
                // Windows needs a two-step move for case-only renames; retain the .meta GUID.
                string existing=Directory.GetFiles(assetFolder,"*.png").Select(p=>p.Replace('\\','/'))
                    .FirstOrDefault(p=>string.Equals(p,asset,StringComparison.OrdinalIgnoreCase));
                if(existing!=null&&!string.Equals(existing,asset,StringComparison.Ordinal)) {
                    string temporaryPath=assetFolder+"/rename_"+Guid.NewGuid().ToString("N")+".png";
                    string error=AssetDatabase.MoveAsset(existing,temporaryPath);
                    if(!string.IsNullOrEmpty(error))throw new Exception("Resource case rename failed: "+error);
                    error=AssetDatabase.MoveAsset(temporaryPath,asset);
                    if(!string.IsNullOrEmpty(error)) {
                        string rollback=AssetDatabase.MoveAsset(temporaryPath,existing);
                        throw new Exception("Resource case rename failed: "+error+"; rollback: "+rollback);
                    }
                }
                string legacy=assetFolder+"/"+id+"_"+Array.IndexOf(nodes,n)+".png";
                if(!File.Exists(asset)&&File.Exists(legacy)) {
                    string error=AssetDatabase.MoveAsset(legacy,asset);
                    if(!string.IsNullOrEmpty(error))throw new Exception("Resource rename failed: "+error);
                }
                File.Copy(Path.Combine(request.assetsDirectory,id+".png"),asset,true);AssetDatabase.ImportAsset(asset,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(asset);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                var image=binding.Key as Image;if(image!=null)importer.spriteBorder=image.sprite.border;
                importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Repeat;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;importer.SaveAndReimport();
                if(image!=null)image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(asset);else ((RawImage)binding.Key).texture=AssetDatabase.LoadAssetAtPath<Texture2D>(asset);
            }
            GameCliUguiAnimations.Apply(designJson["animations"],objects,assetFolder,false);
            bool saved;PrefabUtility.SaveAsPrefabAsset(root,path,out saved);if(!saved)throw new Exception("Prefab save failed.");AssetDatabase.SaveAssets();
            VerifySavedHierarchy(root,AssetDatabase.LoadAssetAtPath<GameObject>(path));
            RenderPreview(root,design,Path.Combine(request.assetsDirectory,"preview.png"));
            result.message="Created "+path+" from database payload ("+nodes.Length+" nodes)";return result;
        }
        finally {foreach(var go in objects.Values)if(go!=null)UnityEngine.Object.DestroyImmediate(go);foreach(var item in temporary)if(item!=null)UnityEngine.Object.DestroyImmediate(item);}
    }
    // Check the serialized result too: updates must remove old children and retain edited text/layout.
    static void VerifySavedHierarchy(GameObject expected, GameObject actual)
    {
        if(actual==null)throw new Exception("Saved prefab could not be loaded.");
        if(expected.name!=actual.name || expected.activeSelf!=actual.activeSelf || expected.transform.childCount!=actual.transform.childCount)
            throw new Exception("Saved prefab hierarchy mismatch: "+expected.name);
        var source=(RectTransform)expected.transform;var target=(RectTransform)actual.transform;
        if(source.anchorMin!=target.anchorMin || source.anchorMax!=target.anchorMax || source.pivot!=target.pivot || source.anchoredPosition!=target.anchoredPosition || source.sizeDelta!=target.sizeDelta || source.localScale!=target.localScale)
            throw new Exception("Saved prefab layout mismatch: "+expected.name);
        var text=expected.GetComponent<Text>();var savedText=actual.GetComponent<Text>();
        if((text==null)!=(savedText==null) || (text!=null && (text.text!=savedText.text || text.fontSize!=savedText.fontSize || text.alignment!=savedText.alignment)))
            throw new Exception("Saved prefab text mismatch: "+expected.name);
        for(int i=0;i<source.childCount;i++)VerifySavedHierarchy(source.GetChild(i).gameObject,target.GetChild(i).gameObject);
    }
    static void RenderPreview(GameObject root, Design design, string output)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var canvasObject = new GameObject("PreviewCanvas", typeof(RectTransform), typeof(Canvas));
        var cameraObject = new GameObject("PreviewCamera", typeof(Camera));
        var target = new RenderTexture((int)design.width, (int)design.height, 24);
        var previous = RenderTexture.active;
        Texture2D image = null;
        try
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasObject, scene);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetParent(canvasObject.transform, false);
            // Keep the prefab root layout; system roots already stretch, reusable controls retain size.
            var camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene; camera.targetTexture = target; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            Canvas.ForceUpdateCanvases();
            camera.Render(); RenderTexture.active = target;
            image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
            File.WriteAllBytes(output, image.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            cameraObject.GetComponent<Camera>().targetTexture = null;
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif
