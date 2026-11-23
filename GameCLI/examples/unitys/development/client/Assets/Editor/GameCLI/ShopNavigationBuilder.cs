using Oathx.Widget;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Bind the reviewed SV routing table to the native mall prefab.</summary>
public static class ShopNavigationBuilder
{
    [Serializable]
    private class Catalog { public Section[] sections; }

    [Serializable]
    private class Section { public string icon; public string title; public Page[] pages; }

    [Serializable]
    private class Page { public int id; public string title; public string window; public string asset; }

    /// <summary>Apply native card dimensions and preserve a nested prefab reference in the decoration list.</summary>
    public static void FinishCards()
    {
        FixCard("GoodField", new Vector2(262f,375.32074f));
        FixCard("LongStripField", new Vector2(1306.34f,320f));
        FixCard("GiftBagField", new Vector2(263f,286f));
        FixCard("RechargeGifField", new Vector2(326.12024f,375.32074f));
        FixCard("UIShopWallPaperItem", new Vector2(193f,219f));

        const string path = "Assets/Data/Widgets/Shop/Pages/WallpaperMallView.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name != "UIShopWallPaperItem" || PrefabUtility.IsAnyPrefabInstanceRoot(rect.gameObject))
                    continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Common/Micros/UIShopWallPaperItem.prefab");
                var nested = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rect.parent);
                var target = (RectTransform)nested.transform;
                target.SetSiblingIndex(rect.GetSiblingIndex());
                target.anchorMin = rect.anchorMin;
                target.anchorMax = rect.anchorMax;
                target.pivot = rect.pivot;
                target.sizeDelta = rect.sizeDelta;
                target.anchoredPosition = rect.anchoredPosition;
                target.localScale = rect.localScale;
                nested.SetActive(rect.gameObject.activeSelf);
                UnityEngine.Object.DestroyImmediate(rect.gameObject);
                break;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Activate the real shop for navigation checks independent of account unlocks.</summary>
    public static void Open()
    {
        var plugin = Oathx.Core.XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var shop = plugin.Load(typeof(ShopSystem));
        if (!shop.IsActive())
            shop.Active();
    }

    /// <summary>Check the requested section and its active content root.</summary>
    public static string Inspect(int index)
    {
        var shop = Oathx.Core.XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<ShopSystem>();
        if (!shop.OpenPage(index, 0))
            throw new InvalidOperationException("Section failed to open: " + index);

        UISystem.GetSingleton().TryGetWidget<UIShop>(out var view);
        var page = view.GetSection(index).pages[0];
        if (page.prefab)
        {
            var host = view.transform.Find("Animator/ProductPages");
            var child = host.GetChild(host.childCount - 1);
            if (!child.gameObject.activeInHierarchy || !child.name.StartsWith(page.prefab.name))
                throw new InvalidOperationException("Page inactive: " + page.window);
        }

        return page.window;
    }

    /// <summary>Assign serialized references and validate every configured page before saving.</summary>
    public static int Bind(string source)
    {
        const string path = "Assets/Data/Widgets/Shop/Shop.prefab";
        var catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText(source));
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var widget = new SerializedObject(root.GetComponent<UIShop>());
            widget.FindProperty("tabStrip").objectReferenceValue = root.transform.Find("Animator/UILayer/TabsNode/SecTabList").gameObject;
            var cell = root.GetComponentInChildren<UIShopGroup>(true);
            var group = new SerializedObject(cell);
            group.FindProperty("sectionIcon").objectReferenceValue = cell.transform.Find("ShopTab/Icon").GetComponent<UnityEngine.UI.Image>();
            group.ApplyModifiedPropertiesWithoutUndo();
            var animator = root.transform.Find("Animator");
            var host = animator.Find("ProductPages") as RectTransform;
            if (!host)
            {
                host = new GameObject("ProductPages", typeof(RectTransform)).GetComponent<RectTransform>();
                host.SetParent(animator, false);
                host.anchorMin = Vector2.zero;
                host.anchorMax = Vector2.one;
                host.offsetMin = Vector2.zero;
                host.offsetMax = Vector2.zero;
                host.SetSiblingIndex(animator.Find("UILayer").GetSiblingIndex());
            }
            if (!host)
                throw new InvalidDataException("Missing native mall page host.");

            widget.FindProperty("pageHost").objectReferenceValue = host;
            var sections = widget.FindProperty("sections");
            sections.arraySize = catalog.sections.Length;
            for (int i = 0; i < sections.arraySize; i++)
            {
                var data = catalog.sections[i];
                var section = sections.GetArrayElementAtIndex(i);
                section.FindPropertyRelative("title").stringValue = data.title;
                var iconPath = "Assets/Data/Widgets/Shop/Images/" + data.icon;
                if (!System.IO.File.Exists(iconPath))
                    iconPath = "Assets/Data/Widgets/Common/Icons/Imported/Shop/" + data.icon;
                var importer = (TextureImporter)AssetImporter.GetAtPath(iconPath);
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
                section.FindPropertyRelative("icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                var pages = section.FindPropertyRelative("pages");
                pages.arraySize = data.pages.Length;
                for (int j = 0; j < pages.arraySize; j++)
                {
                    var route = data.pages[j];
                    var page = pages.GetArrayElementAtIndex(j);
                    page.FindPropertyRelative("id").intValue = route.id;
                    page.FindPropertyRelative("title").stringValue = route.title;
                    page.FindPropertyRelative("window").stringValue = route.window;
                    var prefab = string.IsNullOrEmpty(route.asset) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(route.asset);
                    if (!string.IsNullOrEmpty(route.asset) && !prefab)
                        throw new FileNotFoundException(route.asset);

                    page.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                }
            }

            widget.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);

            return sections.arraySize;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
    /// <summary>Restore the original natural size when a card is dragged into another page.</summary>
    private static void FixCard(string name, Vector2 size)
    {
        string path = "Assets/Data/Widgets/Common/Micros/" + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f,0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}



