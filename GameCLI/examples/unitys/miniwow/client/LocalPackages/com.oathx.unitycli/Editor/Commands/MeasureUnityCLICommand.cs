using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Measure active UI instances under a uniquely identified runtime container.</summary>
    internal sealed class MeasureUnityCLICommand : IUnityCLICommand
    {
        public string Path => "/measure";
        public string Method => "GET";

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                return await context.RunOnMainThreadAsync(() => Measure(request));
            }
            catch (Exception exception)
            {
                return PingResult.Fail("measure_failed", exception.Message);
            }
        }

        [Serializable]
        private sealed class Result
        {
            public bool ok = true;
            public Data data = new Data();
        }

        [Serializable]
        private sealed class Data
        {
            public int width;
            public int height;
            public int visibleCount;
            public List<Item> items = new List<Item>();
        }

        [Serializable]
        private sealed class Item
        {
            public int instanceId;
            public string name;
            public Rect bounds, visible;
            public Vector3 scale;
        }

        private static Result Measure(UnityCliBridgeRequest request)
        {
            if (!Application.isPlaying || string.IsNullOrWhiteSpace(request.objectPath))
            {
                throw new InvalidOperationException("Play Mode and a container path are required.");
            }

            Canvas.ForceUpdateCanvases();
            var path = request.objectPath.Trim('/').Split('/');
            var roots = Resources.FindObjectsOfTypeAll<RectTransform>().Where(r =>
                r.gameObject.scene.IsValid() && r.gameObject.activeInHierarchy &&
                MatchesPath(r, path)).ToArray();
            if (roots.Length != 1)
            {
                throw new InvalidOperationException("Expected one active container at " + path + ", found " + roots.Length);
            }

            var result = new Result();
            result.data.width = Screen.width;
            result.data.height = Screen.height;
            foreach (RectTransform child in roots[0].OfType<RectTransform>())
            {
                if (!child.gameObject.activeInHierarchy || (!string.IsNullOrEmpty(request.search) && child.name != request.search))
                {
                    continue;
                }

                var target = string.IsNullOrEmpty(request.childName) ? child : child.Find(request.childName) as RectTransform;
                if (!target || !target.gameObject.activeInHierarchy)
                {
                    throw new InvalidOperationException("Missing active target on " + child.name + ": " + request.childName);
                }

                var canvas = target.GetComponentInParent<Canvas>().rootCanvas;
                var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && !camera)
                {
                    throw new InvalidOperationException("The canvas requires an explicit rendering camera.");
                }

                var bounds = Project(target, target.rect, camera);
                var visible = Intersect(bounds, new Rect(0, 0, Screen.width, Screen.height));
                foreach (var mask in target.GetComponentsInParent<RectMask2D>())
                {
                    if (mask.isActiveAndEnabled && mask.transform != target)
                    {
                        var rect = mask.rectTransform.rect;
                        var padding = mask.padding;
                        rect = Rect.MinMaxRect(rect.xMin + padding.x, rect.yMin + padding.y, rect.xMax - padding.z, rect.yMax - padding.w);
                        visible = Intersect(visible, Project(mask.rectTransform, rect, camera));
                    }
                }

                foreach (var mask in target.GetComponentsInParent<Mask>())
                {
                    if (mask.isActiveAndEnabled && mask.transform != target)
                    {
                        var rect = (RectTransform)mask.transform;
                        visible = Intersect(visible, Project(rect, rect.rect, camera));
                    }
                }

                if (visible.width > 0 && visible.height > 0)
                {
                    result.data.visibleCount++;
                }

                result.data.items.Add(new Item
                {
                    instanceId = child.GetInstanceID(), name = target.name,
                    bounds = bounds, visible = visible, scale = target.lossyScale
                });
            }

            return result;
        }

        private static bool MatchesPath(Transform node, string[] parts)
        {
            for (int index = parts.Length - 1; index >= 0; index--)
            {
                if (!node || node.name != parts[index])
                {
                    return false;
                }
                node = node.parent;
            }

            return true;
        }

        private static Rect Project(RectTransform node, Rect rect, Camera camera)
        {
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int index = 0; index < 4; index++)
            {
                var local = new Vector3(index < 2 ? rect.xMin : rect.xMax, index % 2 == 0 ? rect.yMin : rect.yMax);
                var screen = RectTransformUtility.WorldToScreenPoint(camera, node.TransformPoint(local));
                min = Vector2.Min(min, screen);
                max = Vector2.Max(max, screen);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float x = Mathf.Max(a.xMin, b.xMin);
            float y = Mathf.Max(a.yMin, b.yMin);
            return new Rect(x, y, Mathf.Max(0f, Mathf.Min(a.xMax, b.xMax) - x), Mathf.Max(0f, Mathf.Min(a.yMax, b.yMax) - y));
        }
    }
}
