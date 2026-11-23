using Oathx.Widget;
using UnityEngine;

/// <summary>Displays the original settings layout and owns presentation-only tab selection.</summary>
[Config("Assets/Data/Widgets/Settings/Settings.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UISettings : UIWidget<VMSettings>
{
    /// <summary>Original sound-page animation, played when that tab is selected.</summary>
    [SerializeField]
    private Animator soundAnimation;

    /// <summary>Currently displayed page; this is not a persisted player preference.</summary>
    public SettingsTab selectedTab
    { get; private set; }

    /// <summary>Publish settings requests from the bound controls.</summary>
    protected override bool Bind()
    {
        vm.ScreenClick = (sender) => Send(new ReqSettingsSelectTab { tab = SettingsTab.Screen });
        vm.SoundClick = (sender) => Send(new ReqSettingsSelectTab { tab = SettingsTab.Sound });
        vm.FunctionClick = (sender) => Send(new ReqSettingsSelectTab { tab = SettingsTab.Function });
        vm.PrivacyClick = (sender) => Send(new ReqSettingsSelectTab { tab = SettingsTab.Privacy });
        vm.DownloadClick = (sender) => Send(new ReqSettingsSelectTab { tab = SettingsTab.Download });
        vm.AudienceClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.Audience });
        vm.PlayerOutlineClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.PlayerOutline });
        vm.TapEffectClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.TapEffect });
        vm.SoundEnabledClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.SoundEnabled });
        vm.MusicEnabledClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.MusicEnabled });
        vm.VoiceEnabledClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.VoiceEnabled });
        vm.ShowFpsClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.ShowFps });
        vm.VibrationClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.Vibration });
        vm.MatchEffectClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.MatchEffect });
        vm.MatchAnimationClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.MatchAnimation });
        vm.FriendInviteClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.FriendInvite });
        vm.AssociationInviteClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.AssociationInvite });
        vm.PersonalInfoClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.PersonalInfo });
        vm.HorseRaceLampClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.HorseRaceLamp });
        vm.InGamePushClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.InGamePush });
        vm.NightPushClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.NightPush });
        vm.ChatWorldClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.ChatWorld });
        vm.ChatAssociationClick = (sender) => Send(new ReqSettingsTogglePreference { option = SettingsOption.ChatAssociation });
        vm.OnlyWifiClick = (sender) => Send(new ReqSettingsSetNetwork { wifiOnly = true });
        vm.WifiAndNetClick = (sender) => Send(new ReqSettingsSetNetwork { wifiOnly = false });
        vm.ResolutionLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Resolution, quality = SettingsQuality.Low });
        vm.ResolutionMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Resolution, quality = SettingsQuality.Medium });
        vm.ResolutionHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Resolution, quality = SettingsQuality.High });
        vm.FpsLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.FrameRate, quality = SettingsQuality.Low });
        vm.FpsMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.FrameRate, quality = SettingsQuality.Medium });
        vm.FpsHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.FrameRate, quality = SettingsQuality.High });
        vm.RefLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Reflection, quality = SettingsQuality.Low });
        vm.RefMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Reflection, quality = SettingsQuality.Medium });
        vm.RefHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Reflection, quality = SettingsQuality.High });
        vm.SceneLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Scene, quality = SettingsQuality.Low });
        vm.SceneMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Scene, quality = SettingsQuality.Medium });
        vm.SceneHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Scene, quality = SettingsQuality.High });
        vm.Tex2dLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Texture, quality = SettingsQuality.Low });
        vm.Tex2dMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Texture, quality = SettingsQuality.Medium });
        vm.Tex2dHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Texture, quality = SettingsQuality.High });
        vm.VolLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Volume, quality = SettingsQuality.Low });
        vm.VolMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Volume, quality = SettingsQuality.Medium });
        vm.VolHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Volume, quality = SettingsQuality.High });
        vm.MsaaLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.AntiAliasing, quality = SettingsQuality.Low });
        vm.MsaaMidClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.AntiAliasing, quality = SettingsQuality.Medium });
        vm.MsaaHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.AntiAliasing, quality = SettingsQuality.High });
        vm.ModelLowClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Model, quality = SettingsQuality.Low });
        vm.ModelHighClick = (sender) => Send(new ReqSettingsSetQuality { option = SettingsOption.Model, quality = SettingsQuality.High });
        vm.PresetLowClick = (sender) => Send(new ReqSettingsSelectPreset { quality = SettingsQuality.Low });
        vm.PresetMidClick = (sender) => Send(new ReqSettingsSelectPreset { quality = SettingsQuality.Medium });
        vm.PresetHighClick = (sender) => Send(new ReqSettingsSelectPreset { quality = SettingsQuality.High });
        vm.CustomClick = Send<ReqSettingsShowCustom>;
        vm.MusicChanged = (sender, value) => Send(new ReqSettingsSetVolume { option = SettingsOption.MusicVolume, volume = value });
        vm.SoundChanged = (sender, value) => Send(new ReqSettingsSetVolume { option = SettingsOption.SoundVolume, volume = value });
        vm.VoiceChanged = (sender, value) => Send(new ReqSettingsSetVolume { option = SettingsOption.VoiceVolume, volume = value });
        vm.CloseClick = Send<ReqSettingsClose>;
        vm.RuleButtonClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.RuleButton });
        vm.ReLoginClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ReLogin });
        vm.DeleteDataClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.DeleteData });
        vm.SwitchAccountClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.SwitchAccount });
        vm.GiftExchangeClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.GiftExchange });
        vm.ConfirmDetailBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ConfirmDetailBtn });
        vm.TakeOverCodeBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.TakeOverCodeBtn });
        vm.BtnBindClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.BtnBind });
        vm.ConfirmAIHelpBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ConfirmAIHelpBtn });
        vm.GrowthBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.GrowthBtn });
        vm.UserBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.UserBtn });
        vm.PrivacyBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.PrivacyBtn });
        vm.ThreeBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ThreeBtn });
        vm.PersonInfoBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.PersonInfoBtn });
        vm.ChildrenBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ChildrenBtn });
        vm.AntiAddictionBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.AntiAddictionBtn });
        vm.ICPBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.ICPBtn });
        vm.TakeOverCloseBtnClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.TakeOverCloseBtn });
        vm.TakeOverCopyButtonClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.TakeOverCopyButton });
        vm.TakeOverConfirmButtonClick = (sender) => Send(new ReqSettingsOpenService { action = SettingsAction.TakeOverConfirmButton });

        return true;
    }

    /// <summary>Restore the default page for each opening.</summary>
    /// <returns>True when the widget became active.</returns>
    public override bool Active()
    {
        if (!base.Active() || !vm)
            return false;

        selectedTab = SettingsTab.Screen;

        return true;
    }

    /// <summary>Select a valid presentation tab and replay its original animation.</summary>
    /// <param name="tab">Page to display.</param>
    /// <returns>True when the tab was selected.</returns>
    public bool SelectTab(SettingsTab tab)
    {
        if (!System.Enum.IsDefined(typeof(SettingsTab), tab))
            return false;

        selectedTab = tab;

        if (tab == SettingsTab.Sound && soundAnimation)
            soundAnimation.Play("Sound", 0, 0f);

        return true;
    }

    /// <summary>Refresh the current tab from local preferences.</summary>
    public bool Invalidate(SettingsEntity entity)
    {
        if (!entity || !vm)
            return false;

        vm.ScreenVisible = selectedTab == SettingsTab.Screen;
        vm.ScreenUnselected = selectedTab != SettingsTab.Screen;
        vm.SoundVisible = selectedTab == SettingsTab.Sound;
        vm.SoundUnselected = selectedTab != SettingsTab.Sound;
        vm.FunctionVisible = selectedTab == SettingsTab.Function;
        vm.FunctionUnselected = selectedTab != SettingsTab.Function;
        vm.PrivacyVisible = selectedTab == SettingsTab.Privacy;
        vm.PrivacyUnselected = selectedTab != SettingsTab.Privacy;
        vm.DownloadVisible = selectedTab == SettingsTab.Download;
        vm.DownloadUnselected = selectedTab != SettingsTab.Download;
        vm.AudienceOn = entity.IsEnabled(SettingsOption.Audience);
        vm.AudienceOff = !entity.IsEnabled(SettingsOption.Audience);
        vm.PlayerOutlineOn = entity.IsEnabled(SettingsOption.PlayerOutline);
        vm.PlayerOutlineOff = !entity.IsEnabled(SettingsOption.PlayerOutline);
        vm.TapEffectOn = entity.IsEnabled(SettingsOption.TapEffect);
        vm.TapEffectOff = !entity.IsEnabled(SettingsOption.TapEffect);
        vm.SoundEnabledOn = entity.IsEnabled(SettingsOption.SoundEnabled);
        vm.SoundEnabledOff = !entity.IsEnabled(SettingsOption.SoundEnabled);
        vm.MusicEnabledOn = entity.IsEnabled(SettingsOption.MusicEnabled);
        vm.MusicEnabledOff = !entity.IsEnabled(SettingsOption.MusicEnabled);
        vm.VoiceEnabledOn = entity.IsEnabled(SettingsOption.VoiceEnabled);
        vm.VoiceEnabledOff = !entity.IsEnabled(SettingsOption.VoiceEnabled);
        vm.ShowFpsOn = entity.IsEnabled(SettingsOption.ShowFps);
        vm.ShowFpsOff = !entity.IsEnabled(SettingsOption.ShowFps);
        vm.VibrationOn = entity.IsEnabled(SettingsOption.Vibration);
        vm.VibrationOff = !entity.IsEnabled(SettingsOption.Vibration);
        vm.MatchEffectOn = entity.IsEnabled(SettingsOption.MatchEffect);
        vm.MatchEffectOff = !entity.IsEnabled(SettingsOption.MatchEffect);
        vm.MatchAnimationOn = entity.IsEnabled(SettingsOption.MatchAnimation);
        vm.MatchAnimationOff = !entity.IsEnabled(SettingsOption.MatchAnimation);
        vm.FriendInviteOn = entity.IsEnabled(SettingsOption.FriendInvite);
        vm.FriendInviteOff = !entity.IsEnabled(SettingsOption.FriendInvite);
        vm.AssociationInviteOn = entity.IsEnabled(SettingsOption.AssociationInvite);
        vm.AssociationInviteOff = !entity.IsEnabled(SettingsOption.AssociationInvite);
        vm.PersonalInfoOn = entity.IsEnabled(SettingsOption.PersonalInfo);
        vm.PersonalInfoOff = !entity.IsEnabled(SettingsOption.PersonalInfo);
        vm.HorseRaceLampOn = entity.IsEnabled(SettingsOption.HorseRaceLamp);
        vm.HorseRaceLampOff = !entity.IsEnabled(SettingsOption.HorseRaceLamp);
        vm.InGamePushOn = entity.IsEnabled(SettingsOption.InGamePush);
        vm.InGamePushOff = !entity.IsEnabled(SettingsOption.InGamePush);
        vm.NightPushOn = entity.IsEnabled(SettingsOption.NightPush);
        vm.NightPushOff = !entity.IsEnabled(SettingsOption.NightPush);
        vm.ChatWorldSelected = entity.IsEnabled(SettingsOption.ChatWorld);
        vm.ChatAssociationSelected = entity.IsEnabled(SettingsOption.ChatAssociation);
        vm.OnlyWifiSelected = entity.IsEnabled(SettingsOption.WifiOnly);
        vm.WifiAndNetSelected = !entity.IsEnabled(SettingsOption.WifiOnly);
        vm.ResolutionLowSelected = entity.ReadQuality(SettingsOption.Resolution) == SettingsQuality.Low;
        vm.ResolutionMidSelected = entity.ReadQuality(SettingsOption.Resolution) == SettingsQuality.Medium;
        vm.ResolutionHighSelected = entity.ReadQuality(SettingsOption.Resolution) == SettingsQuality.High;
        vm.FpsLowSelected = entity.ReadQuality(SettingsOption.FrameRate) == SettingsQuality.Low;
        vm.FpsMidSelected = entity.ReadQuality(SettingsOption.FrameRate) == SettingsQuality.Medium;
        vm.FpsHighSelected = entity.ReadQuality(SettingsOption.FrameRate) == SettingsQuality.High;
        vm.RefLowSelected = entity.ReadQuality(SettingsOption.Reflection) == SettingsQuality.Low;
        vm.RefMidSelected = entity.ReadQuality(SettingsOption.Reflection) == SettingsQuality.Medium;
        vm.RefHighSelected = entity.ReadQuality(SettingsOption.Reflection) == SettingsQuality.High;
        vm.SceneLowSelected = entity.ReadQuality(SettingsOption.Scene) == SettingsQuality.Low;
        vm.SceneMidSelected = entity.ReadQuality(SettingsOption.Scene) == SettingsQuality.Medium;
        vm.SceneHighSelected = entity.ReadQuality(SettingsOption.Scene) == SettingsQuality.High;
        vm.Tex2dLowSelected = entity.ReadQuality(SettingsOption.Texture) == SettingsQuality.Low;
        vm.Tex2dMidSelected = entity.ReadQuality(SettingsOption.Texture) == SettingsQuality.Medium;
        vm.Tex2dHighSelected = entity.ReadQuality(SettingsOption.Texture) == SettingsQuality.High;
        vm.VolLowSelected = entity.ReadQuality(SettingsOption.Volume) == SettingsQuality.Low;
        vm.VolMidSelected = entity.ReadQuality(SettingsOption.Volume) == SettingsQuality.Medium;
        vm.VolHighSelected = entity.ReadQuality(SettingsOption.Volume) == SettingsQuality.High;
        vm.MsaaLowSelected = entity.ReadQuality(SettingsOption.AntiAliasing) == SettingsQuality.Low;
        vm.MsaaMidSelected = entity.ReadQuality(SettingsOption.AntiAliasing) == SettingsQuality.Medium;
        vm.MsaaHighSelected = entity.ReadQuality(SettingsOption.AntiAliasing) == SettingsQuality.High;
        vm.ModelLowSelected = entity.ReadQuality(SettingsOption.Model) == SettingsQuality.Low;
        vm.ModelHighSelected = entity.ReadQuality(SettingsOption.Model) == SettingsQuality.High;
        vm.PresetLowSelected = entity.qualityPreset == SettingsQuality.Low;
        vm.PresetMidSelected = entity.qualityPreset == SettingsQuality.Medium;
        vm.PresetHighSelected = entity.qualityPreset == SettingsQuality.High;
        vm.CustomSelected = entity.qualityPreset == SettingsQuality.Custom;
        vm.MusicVolume = entity.ReadVolume(SettingsOption.MusicVolume);
        vm.MusicPercent = UnityEngine.Mathf.RoundToInt(entity.ReadVolume(SettingsOption.MusicVolume) * 100f).ToString() + "%";
        vm.SoundVolume = entity.ReadVolume(SettingsOption.SoundVolume);
        vm.SoundPercent = UnityEngine.Mathf.RoundToInt(entity.ReadVolume(SettingsOption.SoundVolume) * 100f).ToString() + "%";
        vm.VoiceVolume = entity.ReadVolume(SettingsOption.VoiceVolume);
        vm.VoicePercent = UnityEngine.Mathf.RoundToInt(entity.ReadVolume(SettingsOption.VoiceVolume) * 100f).ToString() + "%";

        return true;
    }

    /// <summary>Display the server label provided by the product.</summary>
    public bool ShowServer(string name)
    {
        if (!vm)
            return false;

        vm.ServerName = name ?? string.Empty;

        return true;
    }

}

/// <summary>Select the indicated settings tab.</summary>
public sealed class ReqSettingsSelectTab : UIRequestEvent
{
    /// <summary>Requested tab.</summary>
    public SettingsTab tab
    { get; set; }
}

/// <summary>Toggle the indicated local preference.</summary>
public sealed class ReqSettingsTogglePreference : UIRequestEvent
{
    /// <summary>Requested option.</summary>
    public SettingsOption option
    { get; set; }
}

/// <summary>Choose the allowed download network.</summary>
public sealed class ReqSettingsSetNetwork : UIRequestEvent
{
    /// <summary>Whether downloads should use Wi-Fi only.</summary>
    public bool wifiOnly
    { get; set; }
}

/// <summary>Set a graphics option to the requested quality.</summary>
public sealed class ReqSettingsSetQuality : UIRequestEvent
{
    /// <summary>Requested option.</summary>
    public SettingsOption option
    { get; set; }

    /// <summary>Requested quality.</summary>
    public SettingsQuality quality
    { get; set; }
}

/// <summary>Apply a graphics preset.</summary>
public sealed class ReqSettingsSelectPreset : UIRequestEvent
{
    /// <summary>Requested quality.</summary>
    public SettingsQuality quality
    { get; set; }
}

/// <summary>Display the custom graphics settings.</summary>
public sealed class ReqSettingsShowCustom : UIRequestEvent
{
}

/// <summary>Set the volume of one audio channel.</summary>
public sealed class ReqSettingsSetVolume : UIRequestEvent
{
    /// <summary>Requested option.</summary>
    public SettingsOption option
    { get; set; }

    /// <summary>Requested volume.</summary>
    public float volume
    { get; set; }
}

/// <summary>Close the settings page.</summary>
public sealed class ReqSettingsClose : UIRequestEvent
{
}

/// <summary>Request a product service from settings.</summary>
public sealed class ReqSettingsOpenService : UIRequestEvent
{
    /// <summary>Requested action.</summary>
    public SettingsAction action
    { get; set; }
}
