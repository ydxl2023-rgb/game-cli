using Oathx.Entity;

/// <summary>Stores local trial identity without network dependencies.</summary>
[Initialize]
public partial class LoginEntity : XEntity
{
    /// <summary>Local trial player identifier.</summary>
    public int pid
    { get; private set; }

    /// <summary>Initialize the local test identity.</summary>
    public LoginEntity()
    {
        pid = 1;
    }
}
