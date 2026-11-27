using Oathx.Widget;

/// <summary>Opens original first-stage detail.</summary>
public sealed class ReqBattleEntryStage : UIRequestEvent { }

/// <summary>Selects the unlocked first stage.</summary>
public sealed class ReqBattleEntryGather : UIRequestEvent { }

/// <summary>Closes exploration entry.</summary>
public sealed class ReqBattleEntryClose : UIRequestEvent { }

/// <summary>Displays original entry assets and emits explicit selection intentions.</summary>
[Config("Assets/Data/Widgets/BattleEntry/UIBattleEntry.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIBattleEntry : UIWidget<VMBattleEntry>
{
    /// <summary>Projects configured first-stage labels.</summary>
    /// <param name="definition">Original stage configuration.</param>
    public void Invalidate(Config.BattleEntry definition)
    {
        vm.Title = CommonText.Get(definition.TitleTextID);
        vm.DetailTitle = vm.Title;
        vm.StageName = CommonText.Get(definition.NameTextID);
        vm.Description = CommonText.Get(definition.DescriptionTextID);
        vm.Power = string.Format(CommonText.Get(Config.Excel.TEXT_BATTLE_ENTRY_POWER), definition.RecommendPower);
        vm.Feedback = CommonText.Get(Config.Excel.TEXT_BATTLE_ENTRY_READY);
    }

    /// <summary>Switches between the map and its stage details.</summary>
    /// <param name="visible">Whether the first-stage list is open.</param>
    public void ShowStage(bool visible)
    {
        vm.MapVisible = true;
        vm.StageVisible = visible;
    }

    /// <summary>Displays the actual available selection outcome.</summary>
    /// <param name="textId">Localized outcome.</param>
    public void ShowFeedback(int textId)
    {
        vm.Feedback = CommonText.Get(textId);
    }

    /// <summary>Binds named page intentions.</summary>
    /// <returns>Whether binding succeeded.</returns>
    protected override bool Bind()
    {
        vm.StageClick = Send<ReqBattleEntryStage>;
        vm.SelectClick = Send<ReqBattleEntryGather>;
        vm.GatherClick = Send<ReqBattleEntryGather>;
        vm.CloseClick = Send<ReqBattleEntryClose>;

        return true;
    }
}
