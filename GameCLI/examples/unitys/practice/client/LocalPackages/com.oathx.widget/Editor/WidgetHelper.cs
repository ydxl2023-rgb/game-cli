using UnityEngine;
using UnityEditor;

namespace Oathx.Widget.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public static class WidgetHelper
    {
        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text)
        {
            return DrawHeader(text, text, false);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, string key)
        {
            return DrawHeader(text, key, false);
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, bool forceOn)
        {
            return DrawHeader(text, text, forceOn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static public bool DrawFoldout(string text)
        {
            bool state = EditorPrefs.GetBool(text, true);

            GUI.changed = false;
            state = EditorGUILayout.Foldout(state, text);
            if (GUI.changed)
                EditorPrefs.SetBool(text, state);

            return state;
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, string key, bool forceOn)
        {
            bool state = EditorPrefs.GetBool(key, true);

            GUILayout.Space(3f);
            if (!forceOn && !state)
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);

            GUI.changed = false;
#if UNITY_3_5
            if (state) text = "\u25B2 " + text;
            else text = "\u25BC " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#else
            text = "<b><size=11>" + text + "</size></b>";
            if (state)
                text = "\u25B2 " + text;
            else
                text = "\u25BC " + text;

            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f)))
                state = !state;
#endif
            if (GUI.changed)
                EditorPrefs.SetBool(key, state);

            GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            if (!forceOn && !state)
                GUILayout.Space(3f);

            return state;
        }

        static public void DrawTitle(string title)
        {
            string text = "<b><size=11>" + title + "</size></b>";
            GUILayout.Label(text, "dragtab", GUILayout.MinWidth(20f));
        }

        /// <summary>
        /// Begin drawing the content area.
        /// </summary>

        static public void BeginContents()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal("box", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// End drawing the content area.
        /// </summary>

        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }
    }
}