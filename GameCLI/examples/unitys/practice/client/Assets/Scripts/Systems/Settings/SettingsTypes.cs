/// <summary>Supported local preference identifiers.</summary>
public enum SettingsOption
{
    /// <summary>Audience preference.</summary>
    Audience = 0,

    /// <summary>PlayerOutline preference.</summary>
    PlayerOutline = 1,

    /// <summary>TapEffect preference.</summary>
    TapEffect = 2,

    /// <summary>SoundEnabled preference.</summary>
    SoundEnabled = 3,

    /// <summary>MusicEnabled preference.</summary>
    MusicEnabled = 4,

    /// <summary>VoiceEnabled preference.</summary>
    VoiceEnabled = 5,

    /// <summary>ShowFps preference.</summary>
    ShowFps = 6,

    /// <summary>Vibration preference.</summary>
    Vibration = 7,

    /// <summary>MatchEffect preference.</summary>
    MatchEffect = 8,

    /// <summary>MatchAnimation preference.</summary>
    MatchAnimation = 9,

    /// <summary>FriendInvite preference.</summary>
    FriendInvite = 16,

    /// <summary>AssociationInvite preference.</summary>
    AssociationInvite = 17,

    /// <summary>PersonalInfo preference.</summary>
    PersonalInfo = 18,

    /// <summary>HorseRaceLamp preference.</summary>
    HorseRaceLamp = 19,

    /// <summary>InGamePush preference.</summary>
    InGamePush = 20,

    /// <summary>NightPush preference.</summary>
    NightPush = 21,

    /// <summary>ChatWorld preference.</summary>
    ChatWorld = 22,

    /// <summary>ChatAssociation preference.</summary>
    ChatAssociation = 23,

    /// <summary>WifiOnly preference.</summary>
    WifiOnly = 24,

    /// <summary>Resolution preference.</summary>
    Resolution = 25,

    /// <summary>FrameRate preference.</summary>
    FrameRate = 26,

    /// <summary>Reflection preference.</summary>
    Reflection = 27,

    /// <summary>Scene preference.</summary>
    Scene = 28,

    /// <summary>Texture preference.</summary>
    Texture = 29,

    /// <summary>Volume preference.</summary>
    Volume = 30,

    /// <summary>AntiAliasing preference.</summary>
    AntiAliasing = 31,

    /// <summary>Model preference.</summary>
    Model = 32,

    /// <summary>MusicVolume preference.</summary>
    MusicVolume = 33,

    /// <summary>SoundVolume preference.</summary>
    SoundVolume = 34,

    /// <summary>VoiceVolume preference.</summary>
    VoiceVolume = 35,
}

/// <summary>Original page categories.</summary>
public enum SettingsTab
{
    /// <summary>Screen page.</summary>
    Screen,

    /// <summary>Sound page.</summary>
    Sound,

    /// <summary>Function page.</summary>
    Function,

    /// <summary>Privacy page.</summary>
    Privacy,

    /// <summary>Download page.</summary>
    Download,
}

/// <summary>Quality tiers; Custom describes mixed values.</summary>
public enum SettingsQuality
{
    /// <summary>Low quality.</summary>
    Low,

    /// <summary>Medium quality.</summary>
    Medium,

    /// <summary>High quality.</summary>
    High,

    /// <summary>Mixed quality options.</summary>
    Custom
}

/// <summary>Product service requests; the template never executes these services.</summary>
public enum SettingsAction
{
    /// <summary>Request the original RuleButton entry.</summary>
    RuleButton = 1,

    /// <summary>Request the original ReLogin entry.</summary>
    ReLogin = 2,

    /// <summary>Request the original DeleteData entry.</summary>
    DeleteData = 3,

    /// <summary>Request the original SwitchAccount entry.</summary>
    SwitchAccount = 4,

    /// <summary>Request the original GiftExchange entry.</summary>
    GiftExchange = 5,

    /// <summary>Request the original ConfirmDetailBtn entry.</summary>
    ConfirmDetailBtn = 6,

    /// <summary>Request the original TakeOverCodeBtn entry.</summary>
    TakeOverCodeBtn = 7,

    /// <summary>Request the original BtnBind entry.</summary>
    BtnBind = 8,

    /// <summary>Request the original ConfirmAIHelpBtn entry.</summary>
    ConfirmAIHelpBtn = 9,

    /// <summary>Request the original GrowthBtn entry.</summary>
    GrowthBtn = 10,

    /// <summary>Request the original UserBtn entry.</summary>
    UserBtn = 11,

    /// <summary>Request the original PrivacyBtn entry.</summary>
    PrivacyBtn = 12,

    /// <summary>Request the original ThreeBtn entry.</summary>
    ThreeBtn = 13,

    /// <summary>Request the original PersonInfoBtn entry.</summary>
    PersonInfoBtn = 14,

    /// <summary>Request the original ChildrenBtn entry.</summary>
    ChildrenBtn = 15,

    /// <summary>Request the original AntiAddictionBtn entry.</summary>
    AntiAddictionBtn = 16,

    /// <summary>Request the original ICPBtn entry.</summary>
    ICPBtn = 17,

    /// <summary>Request the original TakeOverCloseBtn entry.</summary>
    TakeOverCloseBtn = 18,

    /// <summary>Request the original TakeOverCopyButton entry.</summary>
    TakeOverCopyButton = 19,

    /// <summary>Request the original TakeOverConfirmButton entry.</summary>
    TakeOverConfirmButton = 20,
}
