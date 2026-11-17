using Oathx.Widget;

/// <summary>Presentation bindings for the basic lobby.</summary>
public partial class VMHome : VM
{
    /// <summary>Authored activity widgets in stable hierarchy order.</summary>
    [Binding(typeof(SyncGroup))]
    VList<VMActivityEntry> acitiveList;

    /// <summary>Request the Profile entry.</summary>
    [Binding]
    ButtonClick profileClick;

    /// <summary>Display PlayerName.</summary>
    [Binding(typeof(SyncText))]
    string playerName;

    /// <summary>Request the Avatar entry.</summary>
    [Binding]
    ButtonClick avatarClick;

    /// <summary>Display PlayerLevel.</summary>
    [Binding(typeof(SyncText))]
    string playerLevel;

    /// <summary>Request the Mail entry.</summary>
    [Binding]
    ButtonClick mailClick;

    /// <summary>Request the Settings entry.</summary>
    [Binding]
    ButtonClick settingsClick;

    /// <summary>Request the Notice entry.</summary>
    [Binding]
    ButtonClick noticeClick;

    /// <summary>Request the Friends entry.</summary>
    [Binding]
    ButtonClick friendsClick;

    /// <summary>Request the Affinity entry.</summary>
    [Binding]
    ButtonClick affinityClick;

    /// <summary>Request the CurrencyOne entry.</summary>
    [Binding]
    ButtonClick currencyOneClick;

    /// <summary>Display CurrencyOneAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyOneAmount;

    /// <summary>Request the AddCurrencyOne entry.</summary>
    [Binding]
    ButtonClick addCurrencyOneClick;

    /// <summary>Request the CurrencyTwo entry.</summary>
    [Binding]
    ButtonClick currencyTwoClick;

    /// <summary>Display CurrencyTwoAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyTwoAmount;

    /// <summary>Request the AddCurrencyTwo entry.</summary>
    [Binding]
    ButtonClick addCurrencyTwoClick;

    /// <summary>Request the CurrencyThree entry.</summary>
    [Binding]
    ButtonClick currencyThreeClick;

    /// <summary>Display CurrencyThreeAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyThreeAmount;

    /// <summary>Request the AddCurrencyThree entry.</summary>
    [Binding]
    ButtonClick addCurrencyThreeClick;

    /// <summary>Request the Shop entry.</summary>
    [Binding]
    ButtonClick shopClick;

    /// <summary>Request the Benefits entry.</summary>
    [Binding]
    ButtonClick benefitsClick;

    /// <summary>Request the Rankings entry.</summary>
    [Binding]
    ButtonClick rankingsClick;

    /// <summary>Request the Chat entry.</summary>
    [Binding]
    ButtonClick chatClick;

    /// <summary>Request the ChatChannel entry.</summary>
    [Binding]
    ButtonClick chatChannelClick;

    /// <summary>Display ChatMessage.</summary>
    [Binding(typeof(SyncText))]
    string chatMessage;

    /// <summary>Request the Backpack entry.</summary>
    [Binding]
    ButtonClick backpackClick;

    /// <summary>Request the Collection entry.</summary>
    [Binding]
    ButtonClick collectionClick;

    /// <summary>Request the Honor entry.</summary>
    [Binding]
    ButtonClick honorClick;

    /// <summary>Request the Story entry.</summary>
    [Binding]
    ButtonClick storyClick;

    /// <summary>Request the Matches entry.</summary>
    [Binding]
    ButtonClick matchesClick;

    /// <summary>Request the Training entry.</summary>
    [Binding]
    ButtonClick trainingClick;

    /// <summary>Request the Recruit entry.</summary>
    [Binding]
    ButtonClick recruitClick;

    /// <summary>Request the Players entry.</summary>
    [Binding]
    ButtonClick playersClick;

    /// <summary>Request the Team entry.</summary>
    [Binding]
    ButtonClick teamClick;

    /// <summary>Request the Club entry.</summary>
    [Binding]
    ButtonClick clubClick;

    /// <summary>Request the Activities entry.</summary>
    [Binding]
    ButtonClick activitiesClick;

}
