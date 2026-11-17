// Extend the generated Home source with explicit local presentation state.
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
const folder=process.env.HOME_OUTPUT??'.data/imports/sv-home/fidelity-candidate';
const base=folder+'/source/Assets/Scripts/Systems/Home/';
const contract=JSON.parse(await readFile(folder+'/presentation.json','utf8'));
const states=contract.states.filter(s=>s.sourcePath!=='0/6/5/0/4');
const names=new Set();
for(const state of states){
  let name=state.name.replace(/[^a-zA-Z0-9]/g,'');
  name=name[0].toUpperCase()+name.slice(1);
  if(names.has(name))name+=state.sourcePath.replaceAll('/','');
  names.add(name);state.key=name;
  state.value=createHash('sha256').update(state.sourcePath).digest().readUInt32BE(0)&0x7fffffff;
}
let entity=await readFile(base+'HomeEntity.cs','utf8');
entity=entity.replace('using Oathx.Entity;','using Oathx.Entity;\nusing System.Collections.Generic;');
entity=entity.trimEnd().slice(0,-1)+`
    /// <summary>Local reference drop state; the product may replace it.</summary>
    public HomeDropState dropState
    { get; set; } = HomeDropState.Triple;

    /// <summary>Independent indicator states; missing keys are hidden.</summary>
    public Dictionary<HomeIndicator, bool> indicators
    { get; private set; } = new Dictionary<HomeIndicator, bool> {
${states.map(s=>`        { HomeIndicator.${s.key}, ${s.defaultActive} },`).join('\n')}
    };
}

/// <summary>Mutually exclusive reward multiplier presentations.</summary>
public enum HomeDropState
{
    /// <summary>No promotion is displayed.</summary>
    None,

    /// <summary>Display the double drop artwork.</summary>
    Double,

    /// <summary>Display the triple drop artwork.</summary>
    Triple
}

/// <summary>Stable identities for independently controlled lobby elements.</summary>
public enum HomeIndicator
{
${states.map(s=>`    /// <summary>Controls the ${s.name} presentation.</summary>\n    ${s.key} = ${s.value},`).join('\n\n')}
}
`;
await writeFile(base+'HomeEntity.cs',entity);
await writeFile(base+'HomeVisibility.cs',`using System;
using UnityEngine;

/// <summary>Serialized connection between entity state and one lobby element.</summary>
[Serializable]
public class HomeVisibility
{
    /// <summary>Stable entity state key.</summary>
    [SerializeField]
    private HomeIndicator indicator;

    /// <summary>Element controlled by the key.</summary>
    [SerializeField]
    private GameObject target;

    /// <summary>Apply one explicit entity visibility state.</summary>
    /// <param name="entity">The current lobby presentation data.</param>
    /// <returns>True when the serialized target was updated.</returns>
    public bool Apply(HomeEntity entity)
    {
        if (!entity || !target)
            return false;

        target.SetActive(entity.indicators.TryGetValue(indicator, out bool visible) && visible);

        return true;
    }
}
`);
await writeFile(base+'UI/UIHome.cs',`using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the lobby and its independent promotion states.</summary>
[Config("Assets/Data/Widgets/Home/Home.prefab", WidgetLayer.center, cacheWidget: false)]
public class UIHome : UIWidget<VMHome>
{
    /// <summary>Explicit serialized state targets; no runtime hierarchy search.</summary>
    [SerializeField]
    private HomeVisibility[] indicators;

    /// <summary>The single image shared by both drop promotions.</summary>
    [SerializeField]
    private Image dropImage;

    /// <summary>Original double drop artwork.</summary>
    [SerializeField]
    private Sprite doubleDrop;

    /// <summary>Original triple drop artwork.</summary>
    [SerializeField]
    private Sprite tripleDrop;

    /// <summary>Refresh all independent indicators from entity data.</summary>
    /// <param name="entity">The current product or local example state.</param>
    /// <returns>True when all required references were updated.</returns>
    public bool RefreshPresentation(HomeEntity entity)
    {
        if (!entity || indicators == null || !dropImage || !doubleDrop || !tripleDrop)
            return false;

        bool complete = true;
        foreach (var indicator in indicators)
            complete &= indicator != null && indicator.Apply(entity);

        dropImage.sprite = entity.dropState == HomeDropState.Triple ? tripleDrop : doubleDrop;
        dropImage.SetNativeSize();
        dropImage.gameObject.SetActive(entity.dropState == HomeDropState.Double || entity.dropState == HomeDropState.Triple);

        return complete;
    }
}
`);
let system=await readFile(base+'HomeSystem.cs','utf8');
system=system.replace('        XPluginUtility.FireEvent(new HomePresentationRequested()', '        if (!widget.RefreshPresentation(entity))\n            return false;\n\n        XPluginUtility.FireEvent(new HomePresentationRequested()');
await writeFile(base+'HomeSystem.cs',system);
contract.states=states;
await writeFile(folder+'/presentation.json',JSON.stringify(contract,null,2));
console.log(JSON.stringify({indicators:states.length}));
