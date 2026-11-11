using Oathx.Core;
using Oathx.Widget;

/// <summary>
/// Manages local announcement content and the notice page lifecycle.
/// </summary>
public class NoticeSystem : RuntimeSystem
{
    /// <summary>
    /// Local sample title used until live announcement content is connected.
    /// </summary>
    private const string defaultTitle = "欢迎体验 · 公告功能测试";

    /// <summary>
    /// Local sample covering rich text, web links and scrolling without server data.
    /// </summary>
    private const string defaultContent =
        "<b>欢迎来到游戏！</b>\n\n" +
        "这是一份本地测试公告，用于检查文字排版、超链接、滚动和展开收起，不代表正式运营通知。\n\n" +
        "<size=40><b>一、文字显示</b></size>\n" +
        "普通文字应清晰显示，长句应根据公告区域宽度自动换行，不超出面板边界。\n" +
        "<b>这是加粗文字</b>，<i>这是斜体文字</i>。\n" +
        "<color=#D46A26>这是需要重点关注的提示文字。</color>\n\n" +
        "<size=40><b>二、超链接测试</b></size>\n" +
        "点击蓝色文字，会使用系统浏览器打开对应网页：\n" +
        "<a href=\"https://example.com\">打开示例网站（测试链接）</a>\n" +
        "<a href=\"https://example.org\">打开另一个示例网站，检查多个链接是否分别响应</a>\n\n" +
        "以下为长链接，用于检查自动换行后的点击区域：\n" +
        "<a href=\"https://example.com/\">这是一段用于测试跨行点击的超链接文字，请分别点击第一行和换行后的文字，确认它们都能打开同一个示例网站，并检查链接前后的普通文字不会触发跳转。</a>\n\n" +
        "<size=40><b>三、滚动与交互</b></size>\n" +
        "1. 在正文区域上下拖动，检查长公告是否能够顺畅滚动。\n" +
        "2. 从蓝色链接文字处开始拖动，应该只滚动内容，不打开网页。\n" +
        "3. 点击公告标题或箭头，检查正文的收起和展开。\n" +
        "4. 勾选“一周内不再提示”，关闭后重新打开，检查勾选状态是否保留。\n" +
        "5. 点击右上角关闭按钮，应返回登录页面。\n\n" +
        "<size=40><b>四、底部检查</b></size>\n" +
        "看到这里，说明已经滚动到公告底部。请检查最后一行没有被裁掉。\n" +
        "<a href=\"https://example.net\">底部测试链接</a>\n\n" +
        "<color=#888888>—— 本地公告测试文本结束 ——</color>";

    /// <summary>
    /// Open the notice page and initialize its local content and close action.
    /// </summary>
    /// <returns>True after requesting page opening while active.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UINotice>((widget) => {
            if (widget && widget.Initialize())
            {
                widget.vm.CloseClick = (sender) => CloseNotice();
                widget.SetContent(defaultTitle, defaultContent);
            }
        });

        return true;
    }

    /// <summary>
    /// Ask MasterSystem to unload the notice after an explicit close request.
    /// </summary>
    /// <returns>True when the unload request was queued.</returns>
    private bool CloseNotice()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemDetive() {
            type = typeof(NoticeSystem)
        });
    }

    /// <summary>
    /// Release subscriptions and close the notice page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UINotice)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }
}
