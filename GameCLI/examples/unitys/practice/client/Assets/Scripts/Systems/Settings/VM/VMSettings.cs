using Oathx.Widget;

/// <summary>Dynamic values and interactions for the original five settings tabs.</summary>
public partial class VMSettings : VM
{
    /// <summary>Settings screenClick binding.</summary>
    [Binding]
    ButtonClick screenClick;

    /// <summary>Settings screenVisible binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool screenVisible;

    /// <summary>Settings screenUnselected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool screenUnselected;

    /// <summary>Settings soundClick binding.</summary>
    [Binding]
    ButtonClick soundClick;

    /// <summary>Settings soundVisible binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool soundVisible;

    /// <summary>Settings soundUnselected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool soundUnselected;

    /// <summary>Settings functionClick binding.</summary>
    [Binding]
    ButtonClick functionClick;

    /// <summary>Settings functionVisible binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool functionVisible;

    /// <summary>Settings functionUnselected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool functionUnselected;

    /// <summary>Settings privacyClick binding.</summary>
    [Binding]
    ButtonClick privacyClick;

    /// <summary>Settings privacyVisible binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool privacyVisible;

    /// <summary>Settings privacyUnselected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool privacyUnselected;

    /// <summary>Settings downloadClick binding.</summary>
    [Binding]
    ButtonClick downloadClick;

    /// <summary>Settings downloadVisible binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool downloadVisible;

    /// <summary>Settings downloadUnselected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool downloadUnselected;

    /// <summary>Settings audienceClick binding.</summary>
    [Binding]
    ButtonClick audienceClick;

    /// <summary>Settings audienceOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool audienceOn;

    /// <summary>Settings audienceOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool audienceOff;

    /// <summary>Settings playerOutlineClick binding.</summary>
    [Binding]
    ButtonClick playerOutlineClick;

    /// <summary>Settings playerOutlineOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool playerOutlineOn;

    /// <summary>Settings playerOutlineOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool playerOutlineOff;

    /// <summary>Settings tapEffectClick binding.</summary>
    [Binding]
    ButtonClick tapEffectClick;

    /// <summary>Settings tapEffectOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool tapEffectOn;

    /// <summary>Settings tapEffectOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool tapEffectOff;

    /// <summary>Settings soundEnabledClick binding.</summary>
    [Binding]
    ButtonClick soundEnabledClick;

    /// <summary>Settings soundEnabledOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool soundEnabledOn;

    /// <summary>Settings soundEnabledOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool soundEnabledOff;

    /// <summary>Settings musicEnabledClick binding.</summary>
    [Binding]
    ButtonClick musicEnabledClick;

    /// <summary>Settings musicEnabledOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool musicEnabledOn;

    /// <summary>Settings musicEnabledOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool musicEnabledOff;

    /// <summary>Settings voiceEnabledClick binding.</summary>
    [Binding]
    ButtonClick voiceEnabledClick;

    /// <summary>Settings voiceEnabledOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool voiceEnabledOn;

    /// <summary>Settings voiceEnabledOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool voiceEnabledOff;

    /// <summary>Settings showFpsClick binding.</summary>
    [Binding]
    ButtonClick showFpsClick;

    /// <summary>Settings showFpsOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool showFpsOn;

    /// <summary>Settings showFpsOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool showFpsOff;

    /// <summary>Settings vibrationClick binding.</summary>
    [Binding]
    ButtonClick vibrationClick;

    /// <summary>Settings vibrationOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool vibrationOn;

    /// <summary>Settings vibrationOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool vibrationOff;

    /// <summary>Settings matchEffectClick binding.</summary>
    [Binding]
    ButtonClick matchEffectClick;

    /// <summary>Settings matchEffectOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool matchEffectOn;

    /// <summary>Settings matchEffectOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool matchEffectOff;

    /// <summary>Settings matchAnimationClick binding.</summary>
    [Binding]
    ButtonClick matchAnimationClick;

    /// <summary>Settings matchAnimationOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool matchAnimationOn;

    /// <summary>Settings matchAnimationOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool matchAnimationOff;

    /// <summary>Settings friendInviteClick binding.</summary>
    [Binding]
    ButtonClick friendInviteClick;

    /// <summary>Settings friendInviteOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool friendInviteOn;

    /// <summary>Settings friendInviteOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool friendInviteOff;

    /// <summary>Settings associationInviteClick binding.</summary>
    [Binding]
    ButtonClick associationInviteClick;

    /// <summary>Settings associationInviteOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool associationInviteOn;

    /// <summary>Settings associationInviteOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool associationInviteOff;

    /// <summary>Settings personalInfoClick binding.</summary>
    [Binding]
    ButtonClick personalInfoClick;

    /// <summary>Settings personalInfoOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool personalInfoOn;

    /// <summary>Settings personalInfoOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool personalInfoOff;

    /// <summary>Settings horseRaceLampClick binding.</summary>
    [Binding]
    ButtonClick horseRaceLampClick;

    /// <summary>Settings horseRaceLampOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool horseRaceLampOn;

    /// <summary>Settings horseRaceLampOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool horseRaceLampOff;

    /// <summary>Settings inGamePushClick binding.</summary>
    [Binding]
    ButtonClick inGamePushClick;

    /// <summary>Settings inGamePushOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool inGamePushOn;

    /// <summary>Settings inGamePushOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool inGamePushOff;

    /// <summary>Settings nightPushClick binding.</summary>
    [Binding]
    ButtonClick nightPushClick;

    /// <summary>Settings nightPushOn binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool nightPushOn;

    /// <summary>Settings nightPushOff binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool nightPushOff;

    /// <summary>Settings chatWorldClick binding.</summary>
    [Binding]
    ButtonClick chatWorldClick;

    /// <summary>Settings chatWorldSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool chatWorldSelected;

    /// <summary>Settings chatAssociationClick binding.</summary>
    [Binding]
    ButtonClick chatAssociationClick;

    /// <summary>Settings chatAssociationSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool chatAssociationSelected;

    /// <summary>Settings onlyWifiClick binding.</summary>
    [Binding]
    ButtonClick onlyWifiClick;

    /// <summary>Settings onlyWifiSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool onlyWifiSelected;

    /// <summary>Settings wifiAndNetClick binding.</summary>
    [Binding]
    ButtonClick wifiAndNetClick;

    /// <summary>Settings wifiAndNetSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool wifiAndNetSelected;

    /// <summary>Settings resolutionLowClick binding.</summary>
    [Binding]
    ButtonClick resolutionLowClick;

    /// <summary>Settings resolutionLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool resolutionLowSelected;

    /// <summary>Settings resolutionMidClick binding.</summary>
    [Binding]
    ButtonClick resolutionMidClick;

    /// <summary>Settings resolutionMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool resolutionMidSelected;

    /// <summary>Settings resolutionHighClick binding.</summary>
    [Binding]
    ButtonClick resolutionHighClick;

    /// <summary>Settings resolutionHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool resolutionHighSelected;

    /// <summary>Settings fpsLowClick binding.</summary>
    [Binding]
    ButtonClick fpsLowClick;

    /// <summary>Settings fpsLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool fpsLowSelected;

    /// <summary>Settings fpsMidClick binding.</summary>
    [Binding]
    ButtonClick fpsMidClick;

    /// <summary>Settings fpsMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool fpsMidSelected;

    /// <summary>Settings fpsHighClick binding.</summary>
    [Binding]
    ButtonClick fpsHighClick;

    /// <summary>Settings fpsHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool fpsHighSelected;

    /// <summary>Settings refLowClick binding.</summary>
    [Binding]
    ButtonClick refLowClick;

    /// <summary>Settings refLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool refLowSelected;

    /// <summary>Settings refMidClick binding.</summary>
    [Binding]
    ButtonClick refMidClick;

    /// <summary>Settings refMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool refMidSelected;

    /// <summary>Settings refHighClick binding.</summary>
    [Binding]
    ButtonClick refHighClick;

    /// <summary>Settings refHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool refHighSelected;

    /// <summary>Settings sceneLowClick binding.</summary>
    [Binding]
    ButtonClick sceneLowClick;

    /// <summary>Settings sceneLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool sceneLowSelected;

    /// <summary>Settings sceneMidClick binding.</summary>
    [Binding]
    ButtonClick sceneMidClick;

    /// <summary>Settings sceneMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool sceneMidSelected;

    /// <summary>Settings sceneHighClick binding.</summary>
    [Binding]
    ButtonClick sceneHighClick;

    /// <summary>Settings sceneHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool sceneHighSelected;

    /// <summary>Settings tex2dLowClick binding.</summary>
    [Binding]
    ButtonClick tex2dLowClick;

    /// <summary>Settings tex2dLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool tex2dLowSelected;

    /// <summary>Settings tex2dMidClick binding.</summary>
    [Binding]
    ButtonClick tex2dMidClick;

    /// <summary>Settings tex2dMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool tex2dMidSelected;

    /// <summary>Settings tex2dHighClick binding.</summary>
    [Binding]
    ButtonClick tex2dHighClick;

    /// <summary>Settings tex2dHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool tex2dHighSelected;

    /// <summary>Settings volLowClick binding.</summary>
    [Binding]
    ButtonClick volLowClick;

    /// <summary>Settings volLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool volLowSelected;

    /// <summary>Settings volMidClick binding.</summary>
    [Binding]
    ButtonClick volMidClick;

    /// <summary>Settings volMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool volMidSelected;

    /// <summary>Settings volHighClick binding.</summary>
    [Binding]
    ButtonClick volHighClick;

    /// <summary>Settings volHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool volHighSelected;

    /// <summary>Settings msaaLowClick binding.</summary>
    [Binding]
    ButtonClick msaaLowClick;

    /// <summary>Settings msaaLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool msaaLowSelected;

    /// <summary>Settings msaaMidClick binding.</summary>
    [Binding]
    ButtonClick msaaMidClick;

    /// <summary>Settings msaaMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool msaaMidSelected;

    /// <summary>Settings msaaHighClick binding.</summary>
    [Binding]
    ButtonClick msaaHighClick;

    /// <summary>Settings msaaHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool msaaHighSelected;

    /// <summary>Settings modelLowClick binding.</summary>
    [Binding]
    ButtonClick modelLowClick;

    /// <summary>Settings modelLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool modelLowSelected;

    /// <summary>Settings modelHighClick binding.</summary>
    [Binding]
    ButtonClick modelHighClick;

    /// <summary>Settings modelHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool modelHighSelected;

    /// <summary>Settings presetLowClick binding.</summary>
    [Binding]
    ButtonClick presetLowClick;

    /// <summary>Settings presetLowSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool presetLowSelected;

    /// <summary>Settings presetMidClick binding.</summary>
    [Binding]
    ButtonClick presetMidClick;

    /// <summary>Settings presetMidSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool presetMidSelected;

    /// <summary>Settings presetHighClick binding.</summary>
    [Binding]
    ButtonClick presetHighClick;

    /// <summary>Settings presetHighSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool presetHighSelected;

    /// <summary>Settings customClick binding.</summary>
    [Binding]
    ButtonClick customClick;

    /// <summary>Settings customSelected binding.</summary>
    [Binding(typeof(SyncVisible))]
    bool customSelected;

    /// <summary>Settings musicVolume binding.</summary>
    [Binding(typeof(SyncSlider))]
    float musicVolume;

    /// <summary>Settings musicChanged binding.</summary>
    [Binding(typeof(SyncSliderChange))]
    SliderChange musicChanged;

    /// <summary>Settings musicPercent binding.</summary>
    [Binding(typeof(SyncText))]
    string musicPercent;

    /// <summary>Settings soundVolume binding.</summary>
    [Binding(typeof(SyncSlider))]
    float soundVolume;

    /// <summary>Settings soundChanged binding.</summary>
    [Binding(typeof(SyncSliderChange))]
    SliderChange soundChanged;

    /// <summary>Settings soundPercent binding.</summary>
    [Binding(typeof(SyncText))]
    string soundPercent;

    /// <summary>Settings voiceVolume binding.</summary>
    [Binding(typeof(SyncSlider))]
    float voiceVolume;

    /// <summary>Settings voiceChanged binding.</summary>
    [Binding(typeof(SyncSliderChange))]
    SliderChange voiceChanged;

    /// <summary>Settings voicePercent binding.</summary>
    [Binding(typeof(SyncText))]
    string voicePercent;

    /// <summary>Settings closeClick binding.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Settings ruleButtonClick binding.</summary>
    [Binding]
    ButtonClick ruleButtonClick;

    /// <summary>Settings reLoginClick binding.</summary>
    [Binding]
    ButtonClick reLoginClick;

    /// <summary>Settings deleteDataClick binding.</summary>
    [Binding]
    ButtonClick deleteDataClick;

    /// <summary>Settings switchAccountClick binding.</summary>
    [Binding]
    ButtonClick switchAccountClick;

    /// <summary>Settings giftExchangeClick binding.</summary>
    [Binding]
    ButtonClick giftExchangeClick;

    /// <summary>Settings confirmDetailBtnClick binding.</summary>
    [Binding]
    ButtonClick confirmDetailBtnClick;

    /// <summary>Settings takeOverCodeBtnClick binding.</summary>
    [Binding]
    ButtonClick takeOverCodeBtnClick;

    /// <summary>Settings btnBindClick binding.</summary>
    [Binding]
    ButtonClick btnBindClick;

    /// <summary>Settings confirmAIHelpBtnClick binding.</summary>
    [Binding]
    ButtonClick confirmAIHelpBtnClick;

    /// <summary>Settings growthBtnClick binding.</summary>
    [Binding]
    ButtonClick growthBtnClick;

    /// <summary>Settings userBtnClick binding.</summary>
    [Binding]
    ButtonClick userBtnClick;

    /// <summary>Settings privacyBtnClick binding.</summary>
    [Binding]
    ButtonClick privacyBtnClick;

    /// <summary>Settings threeBtnClick binding.</summary>
    [Binding]
    ButtonClick threeBtnClick;

    /// <summary>Settings personInfoBtnClick binding.</summary>
    [Binding]
    ButtonClick personInfoBtnClick;

    /// <summary>Settings childrenBtnClick binding.</summary>
    [Binding]
    ButtonClick childrenBtnClick;

    /// <summary>Settings antiAddictionBtnClick binding.</summary>
    [Binding]
    ButtonClick antiAddictionBtnClick;

    /// <summary>Settings iCPBtnClick binding.</summary>
    [Binding]
    ButtonClick iCPBtnClick;

    /// <summary>Settings takeOverCloseBtnClick binding.</summary>
    [Binding]
    ButtonClick takeOverCloseBtnClick;

    /// <summary>Settings takeOverCopyButtonClick binding.</summary>
    [Binding]
    ButtonClick takeOverCopyButtonClick;

    /// <summary>Settings takeOverConfirmButtonClick binding.</summary>
    [Binding]
    ButtonClick takeOverConfirmButtonClick;

    /// <summary>Settings serverName binding.</summary>
    [Binding(typeof(SyncText))]
    string serverName;
}
