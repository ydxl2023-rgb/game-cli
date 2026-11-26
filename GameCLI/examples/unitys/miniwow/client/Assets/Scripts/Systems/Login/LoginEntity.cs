using Oathx.Entity;

/// <summary>Local entry request states, independent of account authentication.</summary>
public enum LoginPhase
{
    /// <summary>The player may start the local game.</summary>
    Ready,
    /// <summary>An accepted entry request is in progress.</summary>
    Entering,
    /// <summary>The previous entry failed and may be retried.</summary>
    Failed
}

/// <summary>Owns local entry request validation and retry state.</summary>
[Initialize]
public sealed class LoginEntity : XEntity
{
    /// <summary>Current local entry state.</summary>
    public LoginPhase phase
    { get; private set; }

    /// <summary>Accepts one request and rejects repeated clicks.</summary>
    /// <returns>Whether a new entry request was accepted.</returns>
    public bool TryBegin()
    {
        if (phase == LoginPhase.Entering)
            return false;

        phase = LoginPhase.Entering;

        return true;
    }

    /// <summary>Resets the entry page after opening or a recoverable failure.</summary>
    /// <param name="failed">Whether the page should offer retry feedback.</param>
    public void Reset(bool failed)
    {
        phase = failed ? LoginPhase.Failed : LoginPhase.Ready;
    }
}
