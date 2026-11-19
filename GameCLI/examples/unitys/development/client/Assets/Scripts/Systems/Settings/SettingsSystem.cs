using System;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Coordinates local settings and product integration events without service dependencies.</summary>
public class SettingsSystem : RuntimeSystem
{
    /// <summary>Open settings and bind the original controls after loading.</summary>
    /// <returns>True when the asynchronous opening was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        MetadataSyncFactory.GetSingleton().Register(nameof(SyncSliderChange), new SyncSliderChange());

        UISystem.GetSingleton().OpenWidget<UISettings>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                vm.ScreenClick = (sender) => SelectTab(SettingsTab.Screen);
                vm.SoundClick = (sender) => SelectTab(SettingsTab.Sound);
                vm.FunctionClick = (sender) => SelectTab(SettingsTab.Function);
                vm.PrivacyClick = (sender) => SelectTab(SettingsTab.Privacy);
                vm.DownloadClick = (sender) => SelectTab(SettingsTab.Download);
                vm.AudienceClick = (sender) => TogglePreference(SettingsOption.Audience);
                vm.PlayerOutlineClick = (sender) => TogglePreference(SettingsOption.PlayerOutline);
                vm.TapEffectClick = (sender) => TogglePreference(SettingsOption.TapEffect);
                vm.SoundEnabledClick = (sender) => TogglePreference(SettingsOption.SoundEnabled);
                vm.MusicEnabledClick = (sender) => TogglePreference(SettingsOption.MusicEnabled);
                vm.VoiceEnabledClick = (sender) => TogglePreference(SettingsOption.VoiceEnabled);
                vm.ShowFpsClick = (sender) => TogglePreference(SettingsOption.ShowFps);
                vm.VibrationClick = (sender) => TogglePreference(SettingsOption.Vibration);
                vm.MatchEffectClick = (sender) => TogglePreference(SettingsOption.MatchEffect);
                vm.MatchAnimationClick = (sender) => TogglePreference(SettingsOption.MatchAnimation);
                vm.FriendInviteClick = (sender) => TogglePreference(SettingsOption.FriendInvite);
                vm.AssociationInviteClick = (sender) => TogglePreference(SettingsOption.AssociationInvite);
                vm.PersonalInfoClick = (sender) => TogglePreference(SettingsOption.PersonalInfo);
                vm.HorseRaceLampClick = (sender) => TogglePreference(SettingsOption.HorseRaceLamp);
                vm.InGamePushClick = (sender) => TogglePreference(SettingsOption.InGamePush);
                vm.NightPushClick = (sender) => TogglePreference(SettingsOption.NightPush);
                vm.ChatWorldClick = (sender) => TogglePreference(SettingsOption.ChatWorld);
                vm.ChatAssociationClick = (sender) => TogglePreference(SettingsOption.ChatAssociation);
                vm.OnlyWifiClick = (sender) => ChangeNetwork(true);
                vm.WifiAndNetClick = (sender) => ChangeNetwork(false);
                vm.ResolutionLowClick = (sender) => ChooseQuality(SettingsOption.Resolution, SettingsQuality.Low);
                vm.ResolutionMidClick = (sender) => ChooseQuality(SettingsOption.Resolution, SettingsQuality.Medium);
                vm.ResolutionHighClick = (sender) => ChooseQuality(SettingsOption.Resolution, SettingsQuality.High);
                vm.FpsLowClick = (sender) => ChooseQuality(SettingsOption.FrameRate, SettingsQuality.Low);
                vm.FpsMidClick = (sender) => ChooseQuality(SettingsOption.FrameRate, SettingsQuality.Medium);
                vm.FpsHighClick = (sender) => ChooseQuality(SettingsOption.FrameRate, SettingsQuality.High);
                vm.RefLowClick = (sender) => ChooseQuality(SettingsOption.Reflection, SettingsQuality.Low);
                vm.RefMidClick = (sender) => ChooseQuality(SettingsOption.Reflection, SettingsQuality.Medium);
                vm.RefHighClick = (sender) => ChooseQuality(SettingsOption.Reflection, SettingsQuality.High);
                vm.SceneLowClick = (sender) => ChooseQuality(SettingsOption.Scene, SettingsQuality.Low);
                vm.SceneMidClick = (sender) => ChooseQuality(SettingsOption.Scene, SettingsQuality.Medium);
                vm.SceneHighClick = (sender) => ChooseQuality(SettingsOption.Scene, SettingsQuality.High);
                vm.Tex2dLowClick = (sender) => ChooseQuality(SettingsOption.Texture, SettingsQuality.Low);
                vm.Tex2dMidClick = (sender) => ChooseQuality(SettingsOption.Texture, SettingsQuality.Medium);
                vm.Tex2dHighClick = (sender) => ChooseQuality(SettingsOption.Texture, SettingsQuality.High);
                vm.VolLowClick = (sender) => ChooseQuality(SettingsOption.Volume, SettingsQuality.Low);
                vm.VolMidClick = (sender) => ChooseQuality(SettingsOption.Volume, SettingsQuality.Medium);
                vm.VolHighClick = (sender) => ChooseQuality(SettingsOption.Volume, SettingsQuality.High);
                vm.MsaaLowClick = (sender) => ChooseQuality(SettingsOption.AntiAliasing, SettingsQuality.Low);
                vm.MsaaMidClick = (sender) => ChooseQuality(SettingsOption.AntiAliasing, SettingsQuality.Medium);
                vm.MsaaHighClick = (sender) => ChooseQuality(SettingsOption.AntiAliasing, SettingsQuality.High);
                vm.ModelLowClick = (sender) => ChooseQuality(SettingsOption.Model, SettingsQuality.Low);
                vm.ModelHighClick = (sender) => ChooseQuality(SettingsOption.Model, SettingsQuality.High);
                vm.PresetLowClick = (sender) => SelectPreset(SettingsQuality.Low);
                vm.PresetMidClick = (sender) => SelectPreset(SettingsQuality.Medium);
                vm.PresetHighClick = (sender) => SelectPreset(SettingsQuality.High);
                vm.CustomClick = (sender) => ShowCustom();
                vm.MusicChanged = (sender, value) => ChangeVolume(SettingsOption.MusicVolume, value);
                vm.SoundChanged = (sender, value) => ChangeVolume(SettingsOption.SoundVolume, value);
                vm.VoiceChanged = (sender, value) => ChangeVolume(SettingsOption.VoiceVolume, value);
                vm.CloseClick = (sender) => Close();
                vm.RuleButtonClick = (sender) => RequestService(SettingsAction.RuleButton);
                vm.ReLoginClick = (sender) => RequestService(SettingsAction.ReLogin);
                vm.DeleteDataClick = (sender) => RequestService(SettingsAction.DeleteData);
                vm.SwitchAccountClick = (sender) => RequestService(SettingsAction.SwitchAccount);
                vm.GiftExchangeClick = (sender) => RequestService(SettingsAction.GiftExchange);
                vm.ConfirmDetailBtnClick = (sender) => RequestService(SettingsAction.ConfirmDetailBtn);
                vm.TakeOverCodeBtnClick = (sender) => RequestService(SettingsAction.TakeOverCodeBtn);
                vm.BtnBindClick = (sender) => RequestService(SettingsAction.BtnBind);
                vm.ConfirmAIHelpBtnClick = (sender) => RequestService(SettingsAction.ConfirmAIHelpBtn);
                vm.GrowthBtnClick = (sender) => RequestService(SettingsAction.GrowthBtn);
                vm.UserBtnClick = (sender) => RequestService(SettingsAction.UserBtn);
                vm.PrivacyBtnClick = (sender) => RequestService(SettingsAction.PrivacyBtn);
                vm.ThreeBtnClick = (sender) => RequestService(SettingsAction.ThreeBtn);
                vm.PersonInfoBtnClick = (sender) => RequestService(SettingsAction.PersonInfoBtn);
                vm.ChildrenBtnClick = (sender) => RequestService(SettingsAction.ChildrenBtn);
                vm.AntiAddictionBtnClick = (sender) => RequestService(SettingsAction.AntiAddictionBtn);
                vm.ICPBtnClick = (sender) => RequestService(SettingsAction.ICPBtn);
                vm.TakeOverCloseBtnClick = (sender) => RequestService(SettingsAction.TakeOverCloseBtn);
                vm.TakeOverCopyButtonClick = (sender) => RequestService(SettingsAction.TakeOverCopyButton);
                vm.TakeOverConfirmButtonClick = (sender) => RequestService(SettingsAction.TakeOverConfirmButton);

                Refresh();
                NotifyPreferences();
            }
        });

        return true;
    }

    /// <summary>Release the page using the system-level array close interface.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UISettings)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Refresh dynamic values while fixed labels remain on the prefab.</summary>
    /// <returns>True when an active page was refreshed.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UISettings>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<SettingsEntity>();
        if (!entity)
            return false;

        vm.ScreenVisible = widget.selectedTab == SettingsTab.Screen;
        vm.ScreenUnselected = widget.selectedTab != SettingsTab.Screen;
        vm.SoundVisible = widget.selectedTab == SettingsTab.Sound;
        vm.SoundUnselected = widget.selectedTab != SettingsTab.Sound;
        vm.FunctionVisible = widget.selectedTab == SettingsTab.Function;
        vm.FunctionUnselected = widget.selectedTab != SettingsTab.Function;
        vm.PrivacyVisible = widget.selectedTab == SettingsTab.Privacy;
        vm.PrivacyUnselected = widget.selectedTab != SettingsTab.Privacy;
        vm.DownloadVisible = widget.selectedTab == SettingsTab.Download;
        vm.DownloadUnselected = widget.selectedTab != SettingsTab.Download;
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

    /// <summary>Provide the product's current server label without depending on Login.</summary>
    /// <param name="name">Product-supplied server label.</param>
    /// <returns>True when the visible label was updated.</returns>
    public bool ShowServer(string name)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UISettings>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        vm.ServerName = name ?? string.Empty;

        return true;
    }

    /// <summary>Select the presentation page.</summary>
    /// <param name="tab">Page to display.</param>
    /// <returns>True when a valid page was displayed.</returns>
    public bool SelectTab(SettingsTab tab)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UISettings>(out var widget) || !widget.SelectTab(tab))
            return false;

        return Refresh();
    }

    /// <summary>Save a local switch and notify product consumers.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True when the preference changed.</returns>
    public bool TogglePreference(SettingsOption option)
    {
        return Change((entity) => entity.TogglePreference(option));
    }

    /// <summary>Select the allowed download network without initiating resource transfer.</summary>
    /// <param name="wifiOnly">Whether downloads should use Wi-Fi only.</param>
    /// <returns>True when the policy changed.</returns>
    public bool ChangeNetwork(bool wifiOnly)
    {
        return Change((entity) => entity.SelectNetwork(wifiOnly));
    }

    /// <summary>Save one graphics tier.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="quality">Requested graphics tier.</param>
    /// <returns>True when the quality preference changed.</returns>
    public bool ChooseQuality(SettingsOption option, SettingsQuality quality)
    {
        return Change((entity) => entity.SelectQuality(option, quality));
    }

    /// <summary>Save a complete graphics preference preset.</summary>
    /// <param name="quality">Requested graphics tier.</param>
    /// <returns>True when the preset changed.</returns>
    public bool SelectPreset(SettingsQuality quality)
    {
        return Change((entity) => entity.SelectPreset(quality));
    }

    /// <summary>Save a channel volume while retaining its mute switch.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="volume">Requested normalized channel volume.</param>
    /// <returns>True when the channel level changed.</returns>
    public bool ChangeVolume(SettingsOption option, float volume)
    {
        return Change((entity) => entity.ChangeVolume(option, volume));
    }

    /// <summary>Keep custom selection derived from individual graphics options.</summary>
    /// <returns>True when the graphics page was shown.</returns>
    public bool ShowCustom()
    {
        return SelectTab(SettingsTab.Screen);
    }

    /// <summary>Forward an original service entry without inventing account or SDK behavior.</summary>
    /// <param name="action">Product service entry to request.</param>
    /// <returns>True when a supported request was dispatched.</returns>
    public bool RequestService(SettingsAction action)
    {
        if (!IsActive() || !Enum.IsDefined(typeof(SettingsAction), action))
            return false;

        return XPluginUtility.PostEvent(new SettingsActionRequested { action = action });
    }

    /// <summary>Ask the product coordinator to deactivate this system.</summary>
    /// <returns>True when closure was requested.</returns>
    public bool Close()
    {
        return IsActive() && XPluginUtility.PostEvent(new SystemDetive { type = typeof(SettingsSystem) });
    }

    /// <summary>Broadcast a snapshot so adapters can apply settings without referencing this page.</summary>
    /// <returns>True when validated preferences were dispatched.</returns>
    public bool NotifyPreferences()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<SettingsEntity>();
        if (!entity)
            return false;

        return XPluginUtility.PostEvent(new SettingsChanged { values = entity.values });
    }

    /// <summary>Coordinate an entity operation, presentation refresh and change notification.</summary>
    /// <param name="operation">Entity operation that reports whether data changed.</param>
    /// <returns>True when the entity accepted a change.</returns>
    private bool Change(Func<SettingsEntity, bool> operation)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<SettingsEntity>();
        if (!entity)
            return false;

        if (!operation(entity))
            return false;

        Refresh();
        NotifyPreferences();

        return true;
    }
}
