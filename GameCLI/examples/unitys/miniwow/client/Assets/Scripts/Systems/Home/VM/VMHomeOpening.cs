using Oathx.Widget;

/// <summary>Bindings for the opening movie, dialogue and first rescue.</summary>
public partial class VMHomeOpening : VM
{
    /// <summary>Binding for continueClick presentation.</summary>
    [Binding]
    ButtonClick continueClick;

    /// <summary>Binding for rescueClick presentation.</summary>
    [Binding]
    ButtonClick rescueClick;

    /// <summary>Binding for claimClick presentation.</summary>
    [Binding]
    ButtonClick claimClick;

    /// <summary>Binding for skipClick presentation.</summary>
    [Binding]
    ButtonClick skipClick;

    /// <summary>Binding for confirmClick presentation.</summary>
    [Binding]
    ButtonClick confirmClick;

    /// <summary>Binding for cancelClick presentation.</summary>
    [Binding]
    ButtonClick cancelClick;

    /// <summary>Binding for retryClick presentation.</summary>
    [Binding]
    ButtonClick retryClick;

    /// <summary>Binding for movieVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool movieVisible;

    /// <summary>Binding for dialogueVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool dialogueVisible;

    /// <summary>Binding for objectiveVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool objectiveVisible;

    /// <summary>Binding for rescueVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool rescueVisible;

    /// <summary>Binding for claimVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimVisible;

    /// <summary>Binding for confirmVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool confirmVisible;

    /// <summary>Binding for errorVisible presentation.</summary>
    [Binding(typeof(SyncVisible))]
    bool errorVisible;

    /// <summary>Binding for speaker presentation.</summary>
    [Binding(typeof(SyncText))]
    string speaker;

    /// <summary>Binding for dialogue presentation.</summary>
    [Binding(typeof(SyncText))]
    string dialogue;

    /// <summary>Binding for objective presentation.</summary>
    [Binding(typeof(SyncText))]
    string objective;

    /// <summary>Binding for waterCount presentation.</summary>
    [Binding(typeof(SyncText))]
    string waterCount;

    /// <summary>Binding for continueLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string continueLabel;

    /// <summary>Binding for skipLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string skipLabel;

    /// <summary>Binding for confirmLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string confirmLabel;

    /// <summary>Binding for cancelLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string cancelLabel;

    /// <summary>Binding for confirmText presentation.</summary>
    [Binding(typeof(SyncText))]
    string confirmText;

    /// <summary>Binding for rescueLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string rescueLabel;

    /// <summary>Binding for claimLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string claimLabel;

    /// <summary>Binding for retryLabel presentation.</summary>
    [Binding(typeof(SyncText))]
    string retryLabel;

    /// <summary>Binding for errorText presentation.</summary>
    [Binding(typeof(SyncText))]
    string errorText;

}
