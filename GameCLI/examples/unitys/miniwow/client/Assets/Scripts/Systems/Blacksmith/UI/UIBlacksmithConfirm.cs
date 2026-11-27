using System.Linq;
using Oathx.Widget;
using UnityEngine.UI;

/// <summary>Shows explicit material or equipment transaction confirmation.</summary>
[Config("Assets/Data/Widgets/Blacksmith/UIBlacksmithConfirm.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIBlacksmithConfirm : UIWidget<VMBlacksmithConfirm>
{
    /// <summary>Transaction represented by the current confirmation.</summary>
    private BlacksmithAction action;

    /// <summary>Projects the current pending selection.</summary>
    /// <param name="forge">Selection owner.</param>
    /// <param name="village">Committed inventory.</param>
    public void Invalidate(BlacksmithEntity forge, HomeVillageEntity village)
    {
        action = forge.selectedEquipment != 0 ? BlacksmithAction.Sell : BlacksmithAction.Forge;
        var saved = village.Snapshot();
        int recipeId = action == BlacksmithAction.Forge ? forge.selectedRecipe
            : saved.blacksmith.shelf.Single(item => item.id == forge.selectedEquipment).recipeId;
        var recipe = Config.Excel.blacksmithRecipeConfig[recipeId];
        vm.Title = CommonText.Get(recipe.NameTextID);
        vm.Icon = recipe.IconPath;
        vm.QuantityVisible = action == BlacksmithAction.Forge;
        vm.Quantity = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_QUANTITY), forge.quantity);
        vm.ConfirmText = action == BlacksmithAction.Forge ? CommonText.Get(Config.Excel.TEXT_BLACKSMITH_CONFIRM)
            : string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_SELL_CONFIRM), recipe.Price);
        vm.Cost = action == BlacksmithAction.Forge ? string.Join("\n",
            Config.Excel.blacksmithCostConfig.Values.Where(row => row.RecipeID == recipeId).Select(row =>
                string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_COST),
                    CommonText.Get(Config.Excel.blacksmithMaterialConfig[row.ItemID].NameTextID),
                    village.Count(row.ItemID), (long)row.Amount * forge.quantity)))
            : string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_HERO_GOLD), saved.heroGold);
    }

    /// <summary>Displays a localized rejection without dismissing the confirmation.</summary>
    /// <param name="textId">Feedback identity.</param>
    public void ShowFeedback(int textId)
    {
        vm.Feedback = CommonText.Get(textId);
    }

    /// <summary>Binds transaction intentions.</summary>
    /// <returns>Whether bindings succeeded.</returns>
    protected override bool Bind()
    {
        vm.MinusClick = OnMinusClick;
        vm.PlusClick = OnPlusClick;
        vm.ConfirmClick = OnConfirmClick;
        vm.CancelClick = Send<ReqBlacksmithCancel>;

        return true;
    }

    /// <summary>Requests one fewer item.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether sent.</returns>
    private bool OnMinusClick(Button button)
    {
        return Send(new ReqBlacksmithQuantity { delta = -1 });
    }

    /// <summary>Requests one more item.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether sent.</returns>
    private bool OnPlusClick(Button button)
    {
        return Send(new ReqBlacksmithQuantity { delta = 1 });
    }

    /// <summary>Requests the explicitly displayed transaction.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether sent.</returns>
    private bool OnConfirmClick(Button button)
    {
        return Send(new ReqBlacksmithAction { action = action });
    }
}
