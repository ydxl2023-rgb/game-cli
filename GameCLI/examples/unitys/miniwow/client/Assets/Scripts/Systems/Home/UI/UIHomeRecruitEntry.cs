using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Projects original recruitment bubbles over scene entities.</summary>
[Config("Assets/Data/Widgets/Home/UIHomeRecruitEntry.prefab", WidgetLayer.background, cacheWidget: false)]
public sealed class UIHomeRecruitEntry : UIWidget<VMHomeRecruitEntry>
{
    /// <summary>Brother, horn and the two original introductory hero bubbles.</summary>
    [SerializeField]
    private RectTransform[] bubbles;

    /// <summary>Configured interaction IDs corresponding to the authored bubbles.</summary>
    [SerializeField]
    private int[] steps;

    /// <summary>Presentation supplied by the scene owner.</summary>
    private HomeRecruitScene scene;

    /// <summary>Receives scene projection without hierarchy searches.</summary>
    /// <param name="context">Current recruitment scene.</param>
    public void BindScene(HomeRecruitScene context)
    {
        scene = context;
    }

    /// <summary>Binds explicit interaction callbacks.</summary>
    /// <returns>Whether bindings succeeded.</returns>
    protected override bool Bind()
    {
        vm.BrotherClick = OnBrother;
        vm.HornClick = OnHorn;
        vm.FirstClick = OnFirst;
        vm.SecondClick = OnSecond;
        foreach (var bubble in bubbles)
            bubble.gameObject.SetActive(false);

        return true;
    }

    /// <summary>Projects available bubbles after camera movement.</summary>
    private void LateUpdate()
    {
        if (!scene)
            return;

        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var canvas = GetComponentInParent<Canvas>();
        for (int index = 0; index < steps.Length; index++)
        {
            var step = Config.Excel.recruitStepConfig[steps[index]];
            bool visible = scene.CanInteract() && tasks.IsClaimed(step.RequiredTaskID)
                && !home.village.HasRecruitmentReceipt(step.ID);
            if ((RecruitStepKind)step.Kind == RecruitStepKind.Receive)
            {
                var definition = Config.Excel.recruitHeroConfig[step.HeroID];
                var actor = home.FindActor(definition.ActorID);
                visible &= actor.phase == ActorPhase.Waiting;
            }

            Vector3 point = scene.GetScreenPoint(index);
            bubbles[index].gameObject.SetActive(visible && point.z > 0);
            if (!visible)
                continue;

            var parent = (RectTransform)bubbles[index].parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, point,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out var position);
            bubbles[index].anchoredPosition = position;
        }
    }

    /// <summary>Opens the brother's original conversation.</summary>
    /// <param name="button">Clicked bubble.</param>
    /// <returns>Whether the explicit click was accepted.</returns>
    private bool OnBrother(Button button)
    {
        OpenStep(0);

        return true;
    }

    /// <summary>Opens the paid horn interaction.</summary>
    /// <param name="button">Clicked bubble.</param>
    /// <returns>Whether the explicit click was accepted.</returns>
    private bool OnHorn(Button button)
    {
        OpenStep(1);

        return true;
    }

    /// <summary>Opens the first free hero conversation.</summary>
    /// <param name="button">Clicked bubble.</param>
    /// <returns>Whether the explicit click was accepted.</returns>
    private bool OnFirst(Button button)
    {
        OpenStep(2);

        return true;
    }

    /// <summary>Opens the second free hero conversation.</summary>
    /// <param name="button">Clicked bubble.</param>
    /// <returns>Whether the explicit click was accepted.</returns>
    private bool OnSecond(Button button)
    {
        OpenStep(3);

        return true;
    }

    /// <summary>Dispatches the configured destination without changing business state.</summary>
    /// <param name="index">Authored interaction slot.</param>
    private void OpenStep(int index)
    {
        var step = Config.Excel.recruitStepConfig[steps[index]];
        XPluginUtility.FireEvent(new RecruitOpenRequested { buildingId = step.BuildingID });
    }
}
