using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Text;

namespace Oathx.Entity.Editor
{
    public class XEntityEditorWindow : EditorWindow
    {
        [MenuItem("Entity/Window")]
        static void OpenEditor()
        {
            XEntityEditorWindow window = GetWindow<XEntityEditorWindow>();
            window.titleContent = new GUIContent("Entity");
            window.Show();
        }

        private TreeViewState entityTreeViewState;
        private XEntityTreeView entityTreeView;

        private XEntityAttrTreeView entityAttrTreeView;
        private TreeViewState entityAttrTreeViewState;
        private MultiColumnHeaderState assetMCHState;

        private float splitterHeight = 300f;
        private bool isDraggingSplitter = false;
        private float minTreeViewHeight = 100f;
        private float splitterThickness = 5f;
        private float entityDetailHeight = 100f;

        /// <summary>
        /// entity editor window
        /// </summary>
        public XEntityEditorWindow()
        {

        }

        /// <summary>
        /// entity changed event register
        /// </summary>
        private void OnEnable()
        {
            XEntityEditorUtility.EntityChanged += OnEntityChanged;
        }

        /// <summary>
        /// disable event unregister
        /// </summary>
        private void OnDisable()
        {
            XEntityEditorUtility.EntityChanged -= OnEntityChanged;
        }

        /// <summary>
        /// entity changed callback
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnEntityChanged(XEntity entity)
        {
            if (entityTreeView != null)
                entityTreeView.Reload();
        }

        /// <summary>
        /// on gui
        /// </summary>
        private void OnGUI()
        {
            IfNeedInitEntityTreeView();
            IfNeedInitAttrTreeView();

            Rect totalRect = GetSubWindowArea();
            Rect entityTreeRect = new Rect(
                5,
                5,
                totalRect.width - 10,
                splitterHeight - 5
            );

            Rect splitterRect = new Rect(
                5,
                splitterHeight,
                totalRect.width - 10,
                splitterThickness
            );

            float maxHeight = totalRect.height - minTreeViewHeight - splitterThickness - 10;
            splitterHeight = Mathf.Clamp(splitterHeight, minTreeViewHeight, maxHeight);

            Rect attrTreeRect = new Rect(
                5,
                splitterHeight + splitterThickness,
                totalRect.width - 10,
                totalRect.height - splitterHeight - splitterThickness - entityDetailHeight
            );

            if (entityTreeView != null)
                entityTreeView.OnGUI(entityTreeRect);
            
            EditorGUI.DrawRect(splitterRect, Color.gray);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);

            HandleSplitterDrag(splitterRect);

            if (entityAttrTreeView != null)
                entityAttrTreeView.OnGUI(attrTreeRect);

            Rect detailsTreeRect = new Rect(
                  5,
                  totalRect.height - entityDetailHeight + 5,
                  totalRect.width - 10,
                  totalRect.height - (totalRect.height - entityDetailHeight + 15)
            );
            DrawDetailsArea(detailsTreeRect);

            if (isDraggingSplitter)
            {
                Repaint();
            }
        }

        /// <summary>
        /// draw details area
        /// </summary>
        /// <param name="rect"></param>
        private void DrawDetailsArea(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.helpBox);

            if (entityAttrTreeView != null)
            {
                var entity = entityAttrTreeView.GetSelectEntity();
                if (entity)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"iid {entity.iid}");
                    //sb.AppendLine($"timestamp {entity.timestamp}");
                    //sb.AppendLine($"valid {entity.valid}");
                    //sb.AppendLine($"path {entity.GetSerializeFilePath()}");

                    EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);
                }
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// process splitter drag
        /// </summary>
        private void HandleSplitterDrag(Rect splitterRect)
        {
            Event currentEvent = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(currentEvent.mousePosition))
                    {
                        isDraggingSplitter = true;
                        GUIUtility.hotControl = controlID;
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (isDraggingSplitter)
                    {
                        isDraggingSplitter = false;
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDraggingSplitter)
                    {
                        splitterHeight += currentEvent.delta.y;
                        splitterHeight = Mathf.Clamp(
                            splitterHeight,
                            minTreeViewHeight,
                            position.height - minTreeViewHeight - splitterThickness - 10
                        );
                        currentEvent.Use();
                    }
                    break;
            }
        }


        /// <summary>
        /// get sub window area
        /// </summary>
        /// <returns></returns>
        protected Rect GetSubWindowArea()
        {
            return new Rect(0, 0, position.width, position.height);
        }

        /// <summary>
        /// if need init entity tree view
        /// </summary>
        private void IfNeedInitEntityTreeView()
        {
            if (entityTreeView == null)
            {
                if (entityTreeViewState == null)
                    entityTreeViewState = new TreeViewState();

                entityTreeView = new XEntityTreeView(entityTreeViewState, this);
                entityTreeView.Reload();
                entityTreeView.ExpandAll();
            }
        }

        /// <summary>
        /// if need init attribute tree view
        /// </summary>
        private void IfNeedInitAttrTreeView()
        {
            if (entityAttrTreeView == null)
            {
                if (entityAttrTreeViewState == null)
                    entityAttrTreeViewState = new TreeViewState();

                var headerState = XEntityAttrTreeView.CreateDefaultMultiColumnHeaderState();
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(assetMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(assetMCHState, headerState);

                assetMCHState = headerState;

                entityAttrTreeView = new XEntityAttrTreeView(entityAttrTreeViewState, assetMCHState);
                entityAttrTreeView.Reload();
            }
        }

        /// <summary>
        /// update entity attribute
        /// </summary>
        /// <param name="selecteds"></param>
        public void UpdateEntityAttr(IEnumerable<XEntity> selecteds)
        {
            if (entityAttrTreeView != null)
                entityAttrTreeView.SetSelectEntitys(selecteds.First());
        }
    }
}
