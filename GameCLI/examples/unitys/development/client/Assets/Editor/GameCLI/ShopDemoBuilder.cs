using System;
using System.IO;
using System.Reflection;
using Oathx.Widget;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>Bind the SV demo tables and the original list-cell controls at production time.</summary>
public static class ShopDemoBuilder
{
    private const string shop = "Assets/Data/Widgets/Shop/";

    public static int Bind(string name)
    {
        string path = shop+"Pages/"+name+".prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var page=Get<UIShopProductList>(root);
            var data=new SerializedObject(page);
            var catalog = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Jsons/Excels/ShopPageItemConfig.bytes");
            var rows = Newtonsoft.Json.JsonConvert.DeserializeObject<Config.ShopPageItemConfig>(catalog.text).data.Values;
            var art = new List<Sprite>();
            var files = Directory.GetFiles("Assets/Data/Widgets/Common/Icons", "*.png")
                .Concat(Directory.GetFiles(shop + "Demo/Images", "*.png"))
                .ToDictionary(Path.GetFileName, StringComparer.OrdinalIgnoreCase);
            foreach(var key in rows.SelectMany(e=>new[]{e.Icon,e.Preview,e.Currency}).Where(k=>!string.IsNullOrEmpty(k)).Distinct())
            {
                string file=files[key].Replace(Path.DirectorySeparatorChar,'/');
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);
                if(importer.textureType!=TextureImporterType.Sprite || importer.spriteImportMode!=SpriteImportMode.Single)
                {
                    importer.textureType=TextureImporterType.Sprite;
                    importer.spriteImportMode=SpriteImportMode.Single;
                    importer.spritePixelsPerUnit=100;
                    importer.SaveAndReimport();
                }
                art.Add(AssetDatabase.LoadAssetAtPath<Sprite>(file));
            }
            var sprites=data.FindProperty("sprites");
            sprites.arraySize=art.Count;
            for(int i=0;i<art.Count;i++) sprites.GetArrayElementAtIndex(i).objectReferenceValue=art[i];

            bool daily=name=="Daily",wall=name=="WallpaperMallView",fund=name=="GrowthFindsView";
            var hierarchy=root.transform;
            Transform cell,viewport;
            if(daily)
            {
                var host=hierarchy.Find("Animator/GoodsScroll1");
                host.gameObject.SetActive(true);
                Hide(hierarchy,"ScrollView");
                viewport=host.Find("Viewport");
                cell=host.Find("ScrollCell");
            }
            else if(fund)
            {
                viewport=hierarchy.Find("Animator/GameBg/ScrollRect/Viewport");
                cell=Find(viewport,"ScrollCell");
            }
            else
            {
                var host=Find(hierarchy,wall?"WallPaperScrollView":"GiftScrollView2");
                viewport=host.Find("Viewport");
                cell=host.Find("ScrollCell");
            }
            if(!viewport || !cell) throw new InvalidDataException("Missing native list: "+name);
            for(var t=viewport;t!=root.transform;t=t.parent) t.gameObject.SetActive(true);
            if(!viewport.GetComponent<RectMask2D>())viewport.gameObject.AddComponent<RectMask2D>();
            var cellView=Get<UIShopProductCell>(cell.gameObject);
            var item = new Dictionary<string, UnityEngine.Object>();
            var pageTargets = new Dictionary<string, UnityEngine.Object>();
            Ref(data,"template",cellView);
            Ref(data,"viewport",viewport);
            data.FindProperty("columns").intValue=daily?6:1;
            var size=((RectTransform)cell).rect.size;
            data.FindProperty("cellSize").vector2Value=fund?new Vector2(200,552):wall?new Vector2(200,200):size;
            if(fund)
            {
                TextRef(item,"title",cell,"TextNotReachLevel");
                TextRef(item,"quantity",cell,"FRewardNumber");
                TextRef(item,"price",cell,"CRewardNumber");
                ImgRef(item,"icon",cell,"FItemIcon");
                ImgRef(item,"premium",cell,"CItemIcon");
                ObjRef(item,"purchased",cell,"FCheck");
                Hide(cell,"FRewardCanGet","CRewardCanGet","FMask","CMask","CurrentLevelNode","ReachNode","CCheck");
                Find(cell,"NotReachNode").gameObject.SetActive(true);
                Find(cell,"CImgLock").gameObject.SetActive(true);
                TextRef(pageTargets,"level",hierarchy,"PlayerLevel");
                TextRef(pageTargets,"passPrice",hierarchy,"PayNum");
            }
            else
            {
                var icon=Find(cell,"GoodsIcon") ?? Find(cell,"ItemIcon");
                if(wall)
                {
                    Hide(cell,"ItemInfo","Design1","TextOwned","Discount","TextName");
                    var display=Find(cell,"ConfiguredThumbnail") ?? new GameObject("ConfiguredThumbnail",typeof(RectTransform),typeof(Image)).transform;
                    display.SetParent(cell,false);
                    display.SetAsFirstSibling();
                    var rect=(RectTransform)display;
                    rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(6,26);rect.offsetMax=new Vector2(-6,-6);
                    icon=display;
                }
                if(!icon) throw new InvalidDataException("Missing item art: "+name);
                var slot=Find(cell,"ItemInfo");
                if(slot)
                {
                    foreach(var graphic in slot.GetComponentsInChildren<Graphic>(true))
                        graphic.enabled=graphic.transform==icon;
                }
                Enable(icon,cell);
                Ref(item,"icon",icon.GetComponent<Image>());
                if(wall)
                {
                    Hide(cell,"Design1","Own","Discount","HavePurchased");
                    TextRef(item,"price",cell,"TextPrice");
                    var button=Get<Button>(cell.gameObject);
                    var hit=Get<Image>(cell.gameObject);
                    hit.color=Color.clear;hit.raycastTarget=true;
                    Ref(item,"select",button);
                    var bg=hierarchy.Find("DemoWallpaper") ?? new GameObject("DemoWallpaper",typeof(RectTransform),typeof(Image)).transform;
                    bg.SetParent(hierarchy,false);bg.SetAsFirstSibling();
                    var rect=(RectTransform)bg;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                    var image=bg.GetComponent<Image>();image.raycastTarget=false;Ref(pageTargets,"preview",image);
                    TextRef(pageTargets,"previewTitle",hierarchy,"TextWallPaperName");
                    TextRef(pageTargets,"previewPrice",Find(hierarchy,"BtnBuy"),"TextPrice");
                    Ref(pageTargets,"previous",Get<Button>(Find(hierarchy,"LArrow").gameObject));
                    Ref(pageTargets,"next",Get<Button>(Find(hierarchy,"RArrow").gameObject));
                    Hide(hierarchy,"MaskChange");
                }
                else
                {
                    TextRef(item,"title",cell,daily?"GoodsName":"Name");
                    TextRef(item,"price",cell,"Price");
                    TextRef(item,"quantity",cell,"GoodsAmount");
                    TextRef(item,"limit",cell,daily?"LimitText":"LimitNum");
                    if(daily)ImgRef(item,"currency",cell,"Currency");else ImgRef(item,"currency",cell,"GoldIcon");
                    ObjRef(item,"purchased",cell,"SoldOut");
                    ObjRef(item,"locked",cell,"Lock");
                    TextRef(item,"condition",cell,"TextUnlockCondition");
                    TextRef(item,"discount",cell,"DisText");
                    ObjRef(item,"discountTag",cell,"LabelNode");
                    Hide(cell,"RedTips","Discount","LimitTime","LockMask","LockMaskSupport","OnSale");
                }
            }
            if(!fund)
            {
                if(wall)
                {
                    var artRoot=(Image)item["icon"];
                    artRoot.transform.SetAsLastSibling();
                    Hide(cell,"ImgBG","OwnedNode","UnSelectNode","TimeNode","BtnClick");
                    var thumbButton=cell.GetComponent<Button>();
                    thumbButton.targetGraphic=artRoot;
                    artRoot.raycastTarget=true;
                    var priceText=(Text)item["price"];
                    Pin(priceText.rectTransform,cell,new Vector2(.5f,.08f),new Vector2(170,40),Vector2.zero);
                    priceText.fontSize=27;priceText.alignment=TextAnchor.MiddleCenter;
                    var big=(Text)pageTargets["previewPrice"];
                    Pin(big.rectTransform,Find(hierarchy,"BtnBuy"),new Vector2(.5f,.5f),new Vector2(250,90),Vector2.zero);
                    big.alignment=TextAnchor.MiddleCenter;
                    var heading=(Text)pageTargets["previewTitle"];
                    Pin(heading.rectTransform,hierarchy,new Vector2(.73f,.34f),new Vector2(1050,100),Vector2.zero);
                    heading.fontSize=55;heading.alignment=TextAnchor.MiddleRight;
                }
                else
                {
                    var field=Find(cell,daily?"GoodField":"RechargeField");
                    var priceText=(Text)item["price"];
                    var money=(Image)item["currency"];
                    Pin(priceText.rectTransform,field,new Vector2(.5f,.075f),new Vector2(daily?230:360,85),new Vector2(10,0));
                    priceText.horizontalOverflow=HorizontalWrapMode.Overflow;
                    priceText.verticalOverflow=VerticalWrapMode.Overflow;
                    priceText.alignment=TextAnchor.MiddleCenter;
                    priceText.fontSize=daily?34:46;
                    if(money)Pin(money.rectTransform,field,new Vector2(.5f,.075f),new Vector2(38,38),new Vector2(daily?-70:-100,0));
                    Hide(cell,"DiscountPrice","CurrencyFreeShow","LockMaskLanText","LimitTimeHour");
                    if(!daily)
                    {
                        var artwork=(Image)item["icon"];
                        if(artwork)Pin(artwork.rectTransform,field,new Vector2(.5f,.57f),new Vector2(240,240),Vector2.zero);
                        BindGrowthOverlays(item, cell, field);
                    }
                }
            }

            data.ApplyModifiedPropertiesWithoutUndo();
            BindWidgets(data, pageTargets, cellView, item);
            PrefabUtility.SaveAsPrefabAsset(root,path);
            return art.Count;
        }
        finally {PrefabUtility.UnloadPrefabContents(root);}
    }

    /// <summary>Store the authored skin targets in Widget binding order.</summary>
    private static void BindWidgets(SerializedObject data, Dictionary<string, UnityEngine.Object> pageTargets,
        UIShopProductCell cell, Dictionary<string, UnityEngine.Object> item)
    {
        var viewport = (RectTransform)data.FindProperty("viewport").objectReferenceValue;
        var scroll = Get<ScrollRect>(viewport.gameObject);
        var content = viewport.Find("ConfiguredItems") as RectTransform;
        if (!content)
        {
            content = new GameObject("ConfiguredItems", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
        }

        content.anchorMin = content.anchorMax = content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;
        bool grid = data.FindProperty("columns").intValue > 1;
        scroll.enabled = false;
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = !grid;
        scroll.vertical = grid;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        Ref(data, "scroll", scroll);
        Ref(data, "layout", Get<GridLayoutGroup>(content.gameObject));
        data.FindProperty("wallpaper").boolValue = pageTargets.ContainsKey("preview");
        data.ApplyModifiedPropertiesWithoutUndo();

        if (grid && item.TryGetValue("quantity", out var count) && count is Text quantity)
        {
            var rect = quantity.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, .5f);
            rect.anchoredPosition = new Vector2(-18f, -60f);
            quantity.alignment = TextAnchor.MiddleRight;
        }

        pageTargets["items"] = scroll;
        WriteMetadata<VMShopProductList>((Component)data.targetObject, pageTargets);
        WriteMetadata<VMShopProductCell>(cell, item);
    }

    /// <summary>Keep empty slots for fields omitted by a skin so VM indices stay stable.</summary>
    private static void WriteMetadata<T>(Component owner, Dictionary<string, UnityEngine.Object> targets) where T : VM
    {
        var metadata = Get<Metadata>(owner.gameObject);
        metadata.className = typeof(T).Name;
        metadata.classGuid = AssetDatabase.FindAssets(metadata.className + " t:MonoScript")
            .Single(guid => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) == metadata.className);
        metadata.metadatas.Clear();

        foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).OrderBy(field => field.MetadataToken))
        {
            var binding = field.GetCustomAttribute<BindingAttribute>();
            if (binding == null)
                continue;

            string name = field.Name;
            string target = name.EndsWith("Visible", StringComparison.Ordinal) ? name.Substring(0, name.Length - 7) : name;
            targets.TryGetValue(target, out var reference);
            var component = reference is GameObject node ? node.transform : reference as Component;
            var value = new MetadataField {
                name = name,
                type = binding.name,
                targets = new List<GameObject>(),
                components = new List<Component>()
            };
            if (component)
            {
                value.targets.Add(component.gameObject);
                value.components.Add(binding.name == nameof(SyncVisible) ? component.transform : component);
                if (binding.name == nameof(SyncSprite) && component is Image image)
                {
                    image.enabled = true;
                    image.preserveAspect = true;
                }
            }

            metadata.metadatas.Add(value);
        }
    }

    /// <summary>Keep both state overlays above the artwork and inside the animated card.</summary>
    private static void BindGrowthOverlays(Dictionary<string, UnityEngine.Object> item, Transform cell, Transform field)
    {
        var sold = field.Find("SoldOut") ?? cell.Find("SoldOut");
        var condition = Find(cell, "TextUnlockCondition");
        var locked = condition ? condition.parent : null;
        if (!sold || !locked || !locked.Find("SoldOutBg") || !locked.Find("ImgLock"))
            throw new InvalidDataException("Growth offer is missing its SoldOut or Lock overlay.");

        foreach (var overlay in new[] { sold, locked })
        {
            overlay.SetParent(field, false);
            overlay.SetAsLastSibling();
        }

        Ref(item, "purchased", sold.gameObject);
        Ref(item, "locked", locked.gameObject);
        Ref(item, "condition", condition.GetComponent<Text>());
    }

    /// <summary>Read active configuration rows and native layout in Play Mode.</summary>
    public static string Inspect()
    {
        var lists=UnityEngine.Object.FindObjectsOfType<UIShopProductList>();
        return string.Join(";",lists.Select(p=>p.name+" "+string.Join(",",p.GetComponentsInChildren<UIShopProductCell>().Select(v=>v.name+":"+((RectTransform)v.transform).rect.size+":"+v.transform.localScale).Take(2))+" total="+p.GetComponentsInChildren<UIShopProductCell>().Length));
    }

    /// <summary>Capture a rendered Game view frame after navigating to a demo page.</summary>
    public static int Capture(int section)
    {
        var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        EditorWindow.GetWindow(type).Focus();
        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/shop-demo-"+section+".png"));
        return Screen.width;
    }

    /// <summary>Select the next local wallpaper and read the configured label for verification.</summary>
    public static string NextWallpaper()
    {
        var list=UnityEngine.Object.FindObjectOfType<UIShopProductList>();
        var metadata = list.GetComponent<Metadata>();
        var next = (Button)metadata.metadatas.Single(field => field.name == "next").components.Single();
        next.onClick.Invoke();

        return list.vm.PreviewTitle;
    }

    private static void Pin(RectTransform rect,Transform parent,Vector2 anchor,Vector2 size,Vector2 offset)
    {
        rect.SetParent(parent,false);
        rect.SetAsLastSibling();
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.anchorMin=rect.anchorMax=anchor;
        rect.localScale=Vector3.one;
        rect.sizeDelta=size;
        rect.anchoredPosition=offset;
    }

    private static T Get<T>(GameObject obj) where T:Component
    {
        var value=obj.GetComponent<T>();
        return value?value:obj.AddComponent<T>();
    }

    private static Transform Find(Transform root,string name)
    {
        return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
    }

    private static void Enable(Transform t, Transform root)
    {
        for(;t && t!=root;t=t.parent)t.gameObject.SetActive(true);
    }

    private static void Ref(Dictionary<string, UnityEngine.Object> targets, string key, UnityEngine.Object value)
    {
        targets[key] = value;
    }

    private static void Ref(SerializedObject obj,string key,UnityEngine.Object value)
    {
        obj.FindProperty(key).objectReferenceValue=value;
    }

    private static void TextRef(Dictionary<string, UnityEngine.Object> obj,string key,Transform root,string name)
    {
        var t=Find(root,name);
        if(t && t.GetComponent<Text>()){Enable(t,root);Ref(obj,key,t.GetComponent<Text>());}
    }

    private static void ImgRef(Dictionary<string, UnityEngine.Object> obj,string key,Transform root,string name)
    {
        var t=Find(root,name);
        if(t){Enable(t,root);Ref(obj,key,t.GetComponent<Image>());}
    }

    private static void ObjRef(Dictionary<string, UnityEngine.Object> obj,string key,Transform root,string name)
    {
        var t=Find(root,name);
        if(t) Ref(obj,key,t.gameObject);
    }

    private static void Hide(Transform root,params string[] names)
    {
        foreach(var name in names){var t=Find(root,name);if(t)t.gameObject.SetActive(false);}
    }
}
