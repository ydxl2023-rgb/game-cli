using UnityEditor;
using UnityEngine;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class BundleMessage
    {
        static BundleMessage instance = new BundleMessage();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static BundleMessage GetSingleton()
        {
            return instance;
        }

        /// <summary>
        /// styles for rows
        /// </summary>
        private GUIStyle[] styles = new GUIStyle[2];
        private Vector2 dimensions = new Vector2(0, 0);

        /// <summary>
        /// The asset info to display messages for
        /// </summary>
        private BundleAssetInfo assetInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        public BundleMessage()
        {
            styles[0] = "OL EntryBackOdd";
            styles[1] = "OL EntryBackEven";

            styles[0].wordWrap = true;
            styles[1].wordWrap = true;
            styles[0].padding = new RectOffset(32, 0, 1, 4);
            styles[1].padding = new RectOffset(32, 0, 1, 4);
        }

        /// <summary>
        /// Set the message info to display
        /// </summary>
        /// <param name="info"></param>
        public void SetMessage(BundleAssetInfo info)
        {
            assetInfo = info;
        }

        /// <summary>
        /// Draw the message tree
        /// </summary>
        /// <param name="rect"></param>
        public bool OnGUI(Rect rect)
        {
            if (!assetInfo)
                return false;

            DrawOutline(rect, 1f);

            // calc dimensions
            if (dimensions.y == 0 || dimensions.x != rect.width - 16)
            {
                dimensions.x = rect.width - 16;
                dimensions.y = 0;

                foreach (var message in assetInfo.messages)
                {
                    dimensions.y += styles[0].CalcHeight(new GUIContent(message.message), dimensions.x);
                }
            }

            int counter = 0;
            float runningHeight = 0.0f;

            foreach (var message in assetInfo.messages)
            {
                int index = counter % 2;
                var content = new GUIContent(message.message);

                float height = styles[index].CalcHeight(content, dimensions.x);
                GUI.Box(new Rect(0, runningHeight, dimensions.x, height),
                    content, styles[index]);
                GUI.DrawTexture(new Rect(0, runningHeight, 32f, 32f), message.icon);

                counter++;
                runningHeight += height;
            }

            return true;
        }

        /// <summary>
        /// Draw outline around rect
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="size"></param>
        private void DrawOutline(Rect rect, float size)
        {
            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color = GUI.color * color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);

            GUI.color = orgColor;
        }
    }
}
