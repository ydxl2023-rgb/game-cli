namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// 
    /// </summary>
    public enum HotfixMessageType
    {
        GetDownloadSizeSucceed,
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IHotfixWindow
    {
        void MessageBox(HotfixMessageType messageType, string param, System.Action<bool, bool> click);
    }
}
