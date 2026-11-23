﻿﻿﻿﻿﻿﻿﻿using Oathx.Widget;

public partial class VMEmail : VM
{
    /// <summary>
    /// close button clicked
    /// </summary>
    [Binding]
    ButtonClick close;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    ButtonClick read;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    bool showRead;

    /// <summary>
    /// click replay button
    /// </summary>
    [Binding]
    ButtonClick replay;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    bool showReplay;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    VList<VMEmailSlot> emails;

    /// <summary>
    /// 
    /// </summary>
    [Binding(SkipEqualsCheck = true)]
    string clip;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    VList<VMItemSlot> rewards;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    bool showReward;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    ButtonClick receiveAll;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    FancyClick fancyClick;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    string content;

    /// <summary>
    /// email title 
    /// </summary>
    [Binding]
    string title;
}
