using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Restore source UGUI presentation after the layout import.</summary>
public static class ShopVisualRepair
{
    /// <summary>Apply source anchors, typography, graphic visibility and clipping to the imported views.</summary>
    public static int Apply(string folder, string asset)
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Resources/msyhbd.ttc");
        var regular = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Resources/msyh.ttc");
        int count = 0;
        foreach (string path in Directory.GetFiles("Assets/Data/Widgets/Shop/Pages", "*.prefab").Concat(
            new[]{"GoodField","GiftBagField","LongStripField","RechargeGifField","UIShopWallPaperItem"}.Select(n => "Assets/Data/Widgets/Common/Micros/"+n+".prefab")))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (asset != name) continue;
            var design = JObject.Parse(File.ReadAllText(Path.Combine(folder,"full-designs",name,"template.json")))["design"]["nodes"];
            var source = JObject.Parse(File.ReadAllText(Path.Combine(folder,"full-export",name,"unity-prefab.json")))["nodes"];
            var nodes = design.ToDictionary(n => (string)n["id"]);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var original in source)
                {
                    string id = name + ((string)original["path"]).Replace('/','_');
                    if (!nodes.TryGetValue(id, out var node))
                        continue;

                    var names = new Stack<string>();
                    for (var n = node; n["parent"].Type != JTokenType.Null; n = nodes[(string)n["parent"]])
                        names.Push((string)n["name"]);
                    var target = names.Count == 0 ? root.transform : root.transform.Find(string.Join("/",names));
                    if (!target)
                        continue;

                    var r = (RectTransform)target;
                    var src = original["rectTransform"];
                    if (r != root.transform)
                    {
                        r.anchorMin = XY(src["anchorMin"]);
                        r.anchorMax = XY(src["anchorMax"]);
                        r.pivot = XY(src["pivot"]);
                        r.sizeDelta = XY(src["sizeDelta"]);
                        r.anchoredPosition = XY(src["anchoredPosition"]);
                        var scale = src["localScale"];
                        r.localScale = new Vector3((float)scale["x"],(float)scale["y"],(float)scale["z"]);
                        r.localRotation = Quaternion.Euler(0,0,(float)src["localEulerAngles"]["z"]);
                    }
                    target.gameObject.SetActive((bool)original["activeSelf"]);
                    var graphic = target.GetComponent<Graphic>();
                    bool sourceGraphic = false;
                    foreach (var component in original["components"])
                    {
                        string json = (string)component["json"];
                        if (string.IsNullOrEmpty(json)) continue;
                        var data = JObject.Parse(json).Properties().First().Value;
                        string type = ((string)component["type"]).Split(',')[0];
                        if (type.EndsWith(".RectMask2D") && !target.GetComponent<RectMask2D>())
                            target.gameObject.AddComponent<RectMask2D>();
                        if (graphic && data["m_Color"] != null && (data["m_Sprite"] != null || data["m_Texture"] != null || data["m_FontData"] != null))
                        {
                            sourceGraphic = true;
                            graphic.color = data["m_Color"].ToObject<Color>();
                            graphic.enabled = (bool?)data["m_Enabled"] ?? true;
                            graphic.raycastTarget = (bool?)data["m_RaycastTarget"] ?? false;
                        }
                        if (data["m_FontData"] != null && !(graphic is Text))
                        {
                            if (graphic)
                                UnityEngine.Object.DestroyImmediate(graphic);
                            graphic = target.gameObject.AddComponent<Text>();
                            graphic.color = data["m_Color"].ToObject<Color>();
                            graphic.raycastTarget = (bool?)data["m_RaycastTarget"] ?? false;
                        }
                        if (graphic is Text text && data["m_FontData"] is JObject fd)
                        {
                            var refs = component["references"];
                            var reference = refs.FirstOrDefault(v=>(string)v["property"]=="m_FontData.m_Font");
                            text.font = ((string)reference?["path"] ?? "").EndsWith("msyh.ttc",StringComparison.OrdinalIgnoreCase) ? regular : font;
                            string label = (string)node["text"] ?? "";
                            text.text = label.StartsWith("[FID:") || label.StartsWith("[LID:") ? "" : label;
                            text.fontSize = (int)fd["m_FontSize"];
                            text.alignment = (TextAnchor)(int)fd["m_Alignment"];
                            text.horizontalOverflow = (HorizontalWrapMode)(int)fd["m_HorizontalOverflow"];
                            text.verticalOverflow = (VerticalWrapMode)(int)fd["m_VerticalOverflow"];
                            text.resizeTextForBestFit = (bool)fd["m_BestFit"];
                            text.fontStyle = (FontStyle)(int)fd["m_FontStyle"];
                        }
                        if (graphic is Text && type.EndsWith("OutlineEx") && data["OutlineWidth"] != null)
                        {
                            var effect = target.GetComponent<Outline>() ?? target.gameObject.AddComponent<Outline>();
                            float width = (float)data["OutlineWidth"];
                            effect.effectColor = data["OutlineColor"].ToObject<Color>();
                            effect.effectDistance = new Vector2(width,-width);
                        }
                    }
                    if (graphic && !sourceGraphic) graphic.color = Color.clear;
                    bool viewport = original["components"].Any(c => ((string)c["type"]).Split(',')[0] == "UnityEngine.UI.RectMask2D");
                    bool hiddenMask = original["components"].Any(c => ((string)c["type"]).Split(',')[0] == "UnityEngine.UI.Mask" && JToken.Parse((string)c["json"]).First.First.Value<bool>("m_ShowMaskGraphic") == false);
                    if (graphic && (hiddenMask || (viewport && node["image"] == null)))
                        graphic.color = Color.clear;
                    if (hiddenMask && !target.GetComponent<RectMask2D>())
                        target.gameObject.AddComponent<RectMask2D>();
                    count++;
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        return count;
    }

    /// <summary>Compose the three configured offers from the original recommendation cell.</summary>
    public static int BuildOffers(string catalog)
    {
        const string path = "Assets/Data/Widgets/Shop/Pages/ShopEverydaySaleView.prefab";
        const string cardPath = "Assets/Data/Widgets/Common/Micros/ShopRecommendedItem.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var scroll = root.transform.Find("Animator/ScrollView");
            var template = scroll.Find("ScrollCell");
            var prefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject,cardPath);
            var content = (RectTransform)scroll.Find("Viewport/Content");
            for (int i=content.childCount-1;i>=0;i--)
                if (content.GetChild(i).name.StartsWith("Offer_"))
                    UnityEngine.Object.DestroyImmediate(content.GetChild(i).gameObject);
            template.gameObject.SetActive(false);
            var offers = JArray.Parse(File.ReadAllText(catalog));
            for (int i=0;i<offers.Count;i++)
            {
                var offer = offers[i];
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab,content);
                instance.name = "Offer_" + i;
                instance.SetActive(true);
                var rect = (RectTransform)instance.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
                rect.anchoredPosition = new Vector2((i-(offers.Count-1)*.5f)*510,0);
                var field = rect.Find("RechargeField");
                field.Find("Price").GetComponent<Text>().text = (string)offer["price"];
                field.Find("ZuanShi").gameObject.SetActive(true);
                field.Find("ZuanShi/Name").GetComponent<Text>().text = (string)offer["name"];
                field.Find("LimitNum").GetComponent<Text>().text = (string)offer["limit"] + "/" + (string)offer["limit"];
                var item = field.Find("ItemInfo");
                SetIcon(item, (string)offer["icon"]);
                var list = (RectTransform)field.Find("ScrollView/Viewport/ListRewards");
                list.anchorMin = list.anchorMax = list.pivot = new Vector2(.5f,.5f);
                list.anchoredPosition = Vector2.zero;
                var reward = list.Find("RewardItem");
                reward.gameObject.SetActive(false);
                var rewards = (JArray)offer["rewards"];
                for (int j=0;j<rewards.Count;j++)
                {
                    var child = UnityEngine.Object.Instantiate(reward,list,false);
                    child.gameObject.SetActive(true);
                    var slot = (RectTransform)child;
                    slot.anchorMin = slot.anchorMax = slot.pivot = new Vector2(.5f,.5f);
                    slot.anchoredPosition = new Vector2((j-(rewards.Count-1)*.5f)*145,0);
                    SetIcon(child,(string)rewards[j]["icon"]);
                    var count = child.Find("ItemName").GetComponent<Text>();
                    count.gameObject.SetActive(true);
                    count.text = (string)rewards[j]["count"];
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root,path);

            return offers.Count;
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>Show only the icon path through the source slot presentation tree.</summary>
    private static void SetIcon(Transform slot, string filename)
    {
        string path = "Assets/Data/Widgets/Common/Icons/" + filename.ToLowerInvariant();
        if (!File.Exists(path))
            path = "Assets/Data/Widgets/Common/Icons/Imported/Shop/" + filename;
        if (!File.Exists(path))
            path = "Assets/Data/Widgets/Shop/Images/" + filename;
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
        var image = slot.GetComponentsInChildren<Image>(true).First(n=>n.name=="GoodsIcon" || n.name=="ItemIcon");
        foreach (var graphic in slot.GetComponentsInChildren<Graphic>(true))
            graphic.enabled = graphic == image || graphic.name == "ItemName";
        for (var t=image.transform;t!=slot.parent;t=t.parent)
            t.gameObject.SetActive(true);
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        image.color = Color.white;
        image.preserveAspect = true;
    }

    /// <summary>Read Unity vector components from the original export.</summary>
    private static Vector2 XY(JToken vector)
    {
        return new Vector2((float)vector["x"],(float)vector["y"]);
    }
}




