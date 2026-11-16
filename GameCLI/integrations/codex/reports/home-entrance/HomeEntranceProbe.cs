using System.Collections;
using System.Collections.Generic;
using System.IO;
using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Temporary observer of automatic Home entrance playback.</summary>
public static class HomeEntranceProbe
{
    /// <summary>Runtime samples from two separate openings.</summary>
    private static readonly List<object> samples = new List<object>();

    /// <summary>Run two page openings without manually starting an animation.</summary>
    /// <returns>True after scheduling the observation.</returns>
    public static bool Run()
    {
        samples.Clear();
        XCoroutine.Run(Observe());

        return true;
    }

    /// <summary>Observe the actual default state on each fresh page instance.</summary>
    /// <returns>The asynchronous observation sequence.</returns>
    private static IEnumerator Observe()
    {
        System.Type[] widgets = new System.Type[] { typeof(UIHome) };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        for (int pass = 0; pass < 2; pass++)
        {
            UIHome page = null;
            UISystem.GetSingleton().OpenWidget<UIHome>((widget) => { page = widget; });
            float deadline = Time.realtimeSinceStartup + 10;
            while (!page && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (page)
            {
                var animator = page.GetComponentInChildren<Animator>();
                var group = page.GetComponentInChildren<CanvasGroup>();
                var panel = (RectTransform)group.transform;
                float opened = Time.realtimeSinceStartup;
                foreach (float time in new float[] { 0, 0.15f, 0.4f, 1.4f })
                {
                    while (Time.realtimeSinceStartup - opened < time)
                        yield return null;

                    samples.Add(new { pass, time, elapsed = Time.realtimeSinceStartup - opened,
                        state = animator.GetCurrentAnimatorStateInfo(0).IsName("Open"),
                        normalized = animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                        alpha = group.alpha, y = panel.anchoredPosition.y,
                        controller = animator.runtimeAnimatorController.name });
                }
            }

            UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
            yield return null;
        }

        File.WriteAllText(Path.Combine(Application.dataPath, "../HomeEntranceEvidence.json"), Newtonsoft.Json.JsonConvert.SerializeObject(samples));
    }
}
