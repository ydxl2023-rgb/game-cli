using System;
using System.IO;
using System.Collections.Generic;
using Oathx.Widget;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Repair the two monthly pass layouts using the existing reward slot prefab.</summary>
public static class ShopMonthCardBuilder
{
    public static int Bind()
    {
        const string directory = "Assets/Data/Widgets/Shop/Demo/MonthCard";
        const string path = "Assets/Data/Widgets/Shop/Pages/MonthCardView.prefab";
        foreach (string file in Directory.GetFiles(directory,"*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace(Path.DirectorySeparatorChar,'/'));
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        var catalog = AssetDatabase.LoadAssetAtPath<TextAsset>(directory+"/MonthCard.json");
        var data = JsonUtility.FromJson<UIShopMonthCard.Catalog>(catalog.text);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Common/Micros/ItemSlot.prefab");
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var script = root.GetComponent<UIShopMonthCard>();
            if (!script) script = root.AddComponent<UIShopMonthCard>();
            var binding = new SerializedObject(script);
            var slots = new List<UIItemSlot>();
            binding.FindProperty("catalog").objectReferenceValue = catalog;
            var prices = binding.FindProperty("prices");
            prices.arraySize = data.cards.Length;
            var panel = (RectTransform)root.transform.Find("Animator");
            panel.localScale = Vector3.one * .90f;
            panel.anchoredPosition = new Vector2(18,-62);
            for (int i=0; i<data.cards.Length; i++)
            {
                bool regular = i==0;
                string prefix = regular ? "MC" : "SC";
                var card = data.cards[i];
                var bg = panel.Find(regular ? "MonthCard/MonthCardBG/MonthCardMainBg" : "SeasonCard/SeasonCardBG/SCMainBg");
                var button = bg.Find(prefix+"BuyButton");
                var price = button.GetComponentsInChildren<Text>(true);
                Text label = Array.Find(price,t=>t.name.EndsWith("LanguageTextNew"));
                label.transform.SetParent(button,false);
                label.transform.SetAsLastSibling();
                var rect = label.rectTransform;
                rect.anchorMin=Vector2.zero;
                rect.anchorMax=Vector2.one;
                rect.pivot=new Vector2(.5f,.5f);
                rect.offsetMin=rect.offsetMax=Vector2.zero;
                rect.localScale=Vector3.one;
                label.alignment=TextAnchor.MiddleCenter;
                label.text=card.price;
                prices.GetArrayElementAtIndex(i).objectReferenceValue=label;
                var groups=regular ? new[]{"BuyAward/BuyAwardItemGroup","DailyAward/DailyAwardGroup","MCAllAward/MCALLAwardGroup"} :
                    new[]{"SCBuyAward/SCBuyAwardItemGroup","SCDailyAward1/SCDailyAward","SCAllAward/SCAllAwardItemGroup"};
                var rewards=new[]{card.immediate,card.total,card.daily};
                for(int j=0;j<groups.Length;j++)
                {
                    var holder=(RectTransform)bg.Find(groups[j]);
                    foreach(Transform child in holder) child.gameObject.SetActive(false);
                    holder.anchorMin=holder.anchorMax=new Vector2(.5f,1f);
                    holder.pivot=new Vector2(.5f,.5f);
                    holder.localScale=Vector3.one;
                    holder.anchoredPosition=new Vector2(0,j==2?-167:-103);
                    holder.sizeDelta=new Vector2(j==2?550:310,130);
                    for(int k=0;k<rewards[j].Length;k++)
                    {
                        var instance=holder.Find("RewardSlot"+k);
                        if(!instance)
                        {
                            instance=((GameObject)PrefabUtility.InstantiatePrefab(prefab,holder)).transform;
                            instance.name="RewardSlot"+k;
                        }
                        instance.gameObject.SetActive(true);
                        var slot=(RectTransform)instance;
                        slot.anchorMin=slot.anchorMax=slot.pivot=new Vector2(.5f,.5f);
                        slot.localScale=Vector3.one*.80f;
                        slot.sizeDelta=new Vector2(152,152);
                        slot.anchoredPosition=new Vector2((k-(rewards[j].Length-1)*.5f)*145,0);
                        slots.Add(slot.GetComponent<UIItemSlot>());
                    }
                }
            }
            var list=binding.FindProperty("slots");
            list.arraySize=slots.Count;
            for(int i=0;i<slots.Count;i++)list.GetArrayElementAtIndex(i).objectReferenceValue=slots[i];
            binding.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root,path);

            return slots.Count;
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
