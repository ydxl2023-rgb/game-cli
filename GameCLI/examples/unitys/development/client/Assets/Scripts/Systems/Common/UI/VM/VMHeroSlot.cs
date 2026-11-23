using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Oathx.Widget;

public partial class VMHeroSlot : VM
{
    [Binding]
    string head;

    [Binding]
    bool locked;

    [Binding]
    Color edge;
}

