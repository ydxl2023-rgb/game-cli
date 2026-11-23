namespace Oathx.Widget
{
    /// <summary>Receives UI lifecycle notifications without a dependency on the game event framework.</summary>
    public interface IUIEventSender
    {
        /// <summary>Notify after widget activation and before the caller's completion callback.</summary>
        /// <param name="widget">The activated widget.</param>
        void OnWidgetOpened(IWidget widget);

        /// <summary>Notify before the widget starts its closing animation or immediate cleanup.</summary>
        /// <param name="widget">The widget that is starting to close.</param>
        void OnWidgetClosing(IWidget widget);
    }
}
