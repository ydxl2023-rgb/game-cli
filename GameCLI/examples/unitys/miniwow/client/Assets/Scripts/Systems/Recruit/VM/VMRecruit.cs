using Oathx.Widget;

/// <summary>Bindings for the original conversation and recruitment movie.</summary>
public partial class VMRecruit : VM
{
    /// <summary>Explicit acknowledgement of the current line.</summary>
    [Binding]
    ButtonClick continueClick;

    /// <summary>Explicit conversation cancellation.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Retries a failed movie without another inventory transaction.</summary>
    [Binding]
    ButtonClick retryClick;

    /// <summary>Original speaker name.</summary>
    [Binding(typeof(SyncText))]
    string speaker;

    /// <summary>Original dialogue content.</summary>
    [Binding(typeof(SyncText))]
    string dialogue;

    /// <summary>Original character portrait.</summary>
    [Binding(typeof(SyncSprite))]
    string portrait;

    /// <summary>Conversation group visibility.</summary>
    [Binding(typeof(SyncVisible))]
    bool dialogueVisible;

    /// <summary>Movie group visibility.</summary>
    [Binding(typeof(SyncVisible))]
    bool movieVisible;

    /// <summary>Movie retry control visibility.</summary>
    [Binding(typeof(SyncVisible))]
    bool retryVisible;
}
