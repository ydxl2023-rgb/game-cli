using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oathx.Widget;
using UnityEngine;

public partial class VMGridItemSlot : VM
{
    [Binding]
    string back;

    [Binding]
    string icon;

    [Binding]
    string text;

    [Binding]
    bool locked;

    [Binding]
    float amount;

    [Binding]
    bool select;

    [Binding]
    bool arrow;

    [Binding]
    Color edge;
}

