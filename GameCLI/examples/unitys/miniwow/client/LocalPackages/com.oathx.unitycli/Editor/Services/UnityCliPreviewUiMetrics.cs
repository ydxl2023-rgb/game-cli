using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Records native layout and hit-test evidence from isolated preview instances.</summary>
    internal static class UnityCliPreviewUiMetrics
    {
        internal static void Write(GameObject root, GameObject host, Camera camera, string output)
        {
            List<GraphicRaycaster> raycasters = new List<GraphicRaycaster>();
            foreach (Canvas canvas in host.GetComponentsInChildren<Canvas>())
            {
                canvas.worldCamera = camera;
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                raycasters.Add(raycaster);
            }
            JArray nodes = new JArray();
            Vector3[] corners = new Vector3[4];
            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                rect.GetWorldCorners(corners);
                JArray projected = new JArray();
                foreach (Vector3 corner in corners)
                {
                    Vector3 point = camera.WorldToScreenPoint(corner);
                    projected.Add(new JArray(point.x, point.y));
                }
                Graphic graphic = rect.GetComponent<Graphic>();
                JObject evidence = new JObject
                {
                    ["path"] = PathOf(rect, root.transform),
                    ["active"] = rect.gameObject.activeInHierarchy,
                    ["width"] = rect.rect.width,
                    ["height"] = rect.rect.height,
                    ["localPosition"] = new JArray(rect.localPosition.x, rect.localPosition.y, rect.localPosition.z),
                    ["localScale"] = new JArray(rect.localScale.x, rect.localScale.y, rect.localScale.z),
                    ["activeSelf"] = rect.gameObject.activeSelf,
                    ["localRotation"] = new JArray(rect.localRotation.x, rect.localRotation.y, rect.localRotation.z, rect.localRotation.w),
                    ["anchoredPosition"] = new JArray(rect.anchoredPosition.x, rect.anchoredPosition.y),
                    ["screenCorners"] = projected,
                    ["graphic"] = graphic == null ? null : graphic.GetType().FullName
                };
                if (graphic != null)
                {
                    evidence["alpha"] = graphic.color.a;
                    evidence["graphicEnabled"] = graphic.enabled;
                    evidence["graphicColor"] = new JArray(graphic.color.r, graphic.color.g, graphic.color.b, graphic.color.a);
                    evidence["raycastTarget"] = graphic.raycastTarget;
                    if (graphic.isActiveAndEnabled && graphic.raycastTarget && rect.rect.width > 0 && rect.rect.height > 0)
                    {
                        Vector2 center = camera.WorldToScreenPoint(rect.TransformPoint(rect.rect.center));
                        Vector2 outside = camera.WorldToScreenPoint(rect.TransformPoint(new Vector3(rect.rect.xMax + 1f, rect.rect.center.y, 0f)));
                        evidence["centerHit"] = Hit(raycasters, graphic.gameObject, center);
                        evidence["outsideHit"] = Hit(raycasters, graphic.gameObject, outside);
                        evidence["probeScope"] = "Native GraphicRaycaster containment and filters only; no pointer event or business callback is dispatched.";
                    }
                }
                JArray groupAlphas = new JArray();
                foreach (CanvasGroup group in rect.GetComponents<CanvasGroup>())
                {
                    groupAlphas.Add(group.alpha);
                }
                evidence["canvasGroupAlphas"] = groupAlphas;
                nodes.Add(evidence);
            }
            File.WriteAllText(output + ".ui-metrics.json", new JObject
            {
                ["schemaVersion"] = "unity-preview-ui-metrics/1",
                ["isolatedPreviewOnly"] = true,
                ["businessCallbacksInvoked"] = false,
                ["nodes"] = nodes
            }.ToString());
        }

        private static bool Hit(List<GraphicRaycaster> raycasters, GameObject target, Vector2 position)
        {
            // PointerEventData is consumed by raycasters without an EventSystem update.
            // No scene-global EventSystem or input module is created or modified.
            PointerEventData pointer = new PointerEventData(null)
            {
                position = position
            };
            List<RaycastResult> results = new List<RaycastResult>();
            foreach (GraphicRaycaster raycaster in raycasters)
            {
                raycaster.Raycast(pointer, results);
            }
            return results.Exists(result => result.gameObject == target);
        }

        private static string PathOf(Transform transform, Transform root)
        {
            string path = transform.name;
            while (transform != root && transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}
