using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// new Bundle Configure Editor
    /// </summary>
    public class BundleConfigureEditor : IBundleEditor
    {
        /// <summary>
        /// parent window
        /// </summary>
        private BundleEditor window;
        
        // window split into 4 sections:
        private float verticalSplitPos = 100f;
        private float leftSplitPos = 200f;
        private float rightSplitPos = 200f;

        /// <summary>
        /// current dragging state
        /// </summary>
        private bool isDraggingVertical = false;
        private bool isDraggingLeftHorizontal = false;
        private bool isDraggingRightHorizontal = false;

        // TreeView instances
        private BundleTreeView bundleTreeView;
        private AssetTreeView assetTreeView;
        private MultiColumnHeaderState assetMCHState;
        private DetailsTreeView detailsTreeView;

        // search field
        private SearchField searchField;

        // drag scroll positions
        private Vector2 bundleScrollPos;
        private Vector2 assetScrollPos;
        private Vector2 detailsScrollPos;
        private Vector2 messageScrollPos;

        /// <summary>
        /// Minimum Split Sizes
        /// </summary>
        private const float MIN_VERTICAL_SPLIT = 150f;
        private const float MIN_HORIZONTAL_SPLIT = 100f;
        private const float SPLITTER_WIDTH = 5f;

        /// <summary>
        /// New Bundle Configure Editor
        /// </summary>
        /// <param name="editor"></param>
        public BundleConfigureEditor(BundleEditor editor)
        {
            window = editor;
        }

        /// <summary>
        /// enable window
        /// </summary>
        /// <param name="rect"></param>
        public void OnEnable(Rect rect)
        {
            InitializeTreeViews();

            // load window layout
            LoadLayout();
        }

        /// <summary>
        /// disable window
        /// </summary>
        public void OnDisable()
        {
            SaveLayout();
        }

        /// <summary>
        /// override gui
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        {
            // calc available area
            float toolbarHeight = 20f;
            float totalWidth = rect.width;
            float totalHeight = rect.height - toolbarHeight;

            // value: verticalSplitPos
            Rect verticalSplitRect = new Rect(verticalSplitPos, toolbarHeight, SPLITTER_WIDTH, totalHeight);

            // left horizontal split and right horizontal split
            Rect leftHorizontalSplitRect = new Rect(0, toolbarHeight + leftSplitPos, verticalSplitPos, SPLITTER_WIDTH);
            Rect rightHorizontalSplitRect = new Rect(verticalSplitPos + SPLITTER_WIDTH, toolbarHeight + rightSplitPos,
                                                    totalWidth - verticalSplitPos - SPLITTER_WIDTH, SPLITTER_WIDTH);

           
            Rect bundleRect = new Rect(0, toolbarHeight, verticalSplitPos, leftSplitPos);
            Rect detailsRect = new Rect(0, toolbarHeight + leftSplitPos + SPLITTER_WIDTH,
                                       verticalSplitPos,
                                       totalHeight - leftSplitPos - SPLITTER_WIDTH);
            Rect assetRect = new Rect(verticalSplitPos + SPLITTER_WIDTH, toolbarHeight,
                                     totalWidth - verticalSplitPos - SPLITTER_WIDTH,
                                     rightSplitPos);
            Rect messageRect = new Rect(verticalSplitPos + SPLITTER_WIDTH,
                                       toolbarHeight + rightSplitPos + SPLITTER_WIDTH,
                                       totalWidth - verticalSplitPos - SPLITTER_WIDTH,
                                       totalHeight - rightSplitPos - SPLITTER_WIDTH);

            // draw splitters
            DrawSplitters(verticalSplitRect, leftHorizontalSplitRect, rightHorizontalSplitRect);

            // draw four sections
            DrawBundleSection(bundleRect);
            DrawAssetSection(assetRect);
            DrawDetailsSection(detailsRect);
            DrawMessageSection(messageRect);

            // process splitter drag
            HandleSplitterDrag(verticalSplitRect, leftHorizontalSplitRect, rect,
                leftHorizontalSplitRect, rightHorizontalSplitRect);
        }

        /// <summary>
        /// on update
        /// </summary>
        public void OnUpdate()
        {
        }

        /// <summary>
        /// on reload
        /// </summary>
        public void OnReload()
        {
        }

        /// <summary>
        /// initialize tree views
        /// </summary>
        private void InitializeTreeViews()
        {
            var bundleTreeViewState = new TreeViewState();
            bundleTreeView = new BundleTreeView(bundleTreeViewState);
            bundleTreeView.OnBundleSelected += OnBundleSelected;

            var assetTreeViewState = new TreeViewState();
            var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(assetMCHState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(assetMCHState, headerState);
            assetMCHState = headerState;

            assetTreeView = new AssetTreeView(assetTreeViewState, assetMCHState);
            assetTreeView.OnAssetSelected += OnAssetSelected;

            var detailsTreeViewState = new TreeViewState();
            detailsTreeView = new DetailsTreeView(detailsTreeViewState);

            searchField = new SearchField();
            RefreshBundleList();
        }

        void OnGUISearchBar(Rect rect)
        {
            if (searchField != null)
            {
                assetTreeView.searchString = searchField.OnGUI(rect, assetTreeView.searchString);
            }
        }

        /// <summary>
        /// refresh bundle list
        /// </summary>
        private void RefreshBundleList()
        {
            if (bundleTreeView != null)
                bundleTreeView.Refresh();
        }

        /// <summary>
        /// Bundle Selected Handler
        /// </summary>
        /// <param name="bundleItem"></param>
        private void OnBundleSelected(BundleTreeItem bundleItem)
        {
            if (detailsTreeView != null)
                detailsTreeView.Refresh(bundleItem.bundle);

            if (assetTreeView != null)
            {
                assetTreeView.Refresh(new List<BundleNode>() { bundleItem.bundle });
            }
        }

        /// <summary>
        /// Asset Selected Handler
        /// </summary>
        /// <param name="assetItem"></param>
        private void OnAssetSelected(AssetTreeItem assetItem)
        {

        }

        /// <summary>
        /// Draw Bundle Section
        /// </summary>
        /// <param name="rect"></param>
        private void DrawBundleSection(Rect rect)
        {
            GUILayout.BeginArea(rect, "Bundles", EditorStyles.helpBox);
            bundleScrollPos = EditorGUILayout.BeginScrollView(bundleScrollPos);

            Rect treeViewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (bundleTreeView != null)
            {
                bundleTreeView.OnGUI(treeViewRect);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw Asset Section
        /// </summary>
        /// <param name="rect"></param>
        private void DrawAssetSection(Rect rect)
        {
            Rect serachRect = new Rect(rect.x, rect.y + 4, rect.width, 20f);
            OnGUISearchBar(serachRect);

            Rect assetRect = new Rect(rect.x, rect.y + 24f, rect.width, rect.height - 24f);
            GUILayout.BeginArea(assetRect, "Assets", EditorStyles.helpBox);

            assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos);
            Rect treeViewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (assetTreeView != null)
            {
                assetTreeView.OnGUI(treeViewRect);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw Details Section
        /// </summary>
        /// <param name="rect"></param>
        private void DrawDetailsSection(Rect rect)
        {
            GUILayout.BeginArea(rect, "Details", EditorStyles.helpBox);
            detailsScrollPos = EditorGUILayout.BeginScrollView(detailsScrollPos);

            Rect treeViewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (detailsTreeView != null)
            {
                detailsTreeView.OnGUI(treeViewRect);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw Message Section
        /// </summary>
        /// <param name="rect"></param>
        private void DrawMessageSection(Rect rect)
        {
            GUILayout.BeginArea(rect, "", EditorStyles.helpBox);
            messageScrollPos = EditorGUILayout.BeginScrollView(messageScrollPos);

            BundleMessage.GetSingleton().OnGUI(rect);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw Splitters
        /// </summary>
        /// <param name="verticalSplitRect"></param>
        /// <param name="leftHorizontalSplitRect"></param>
        /// <param name="rightHorizontalSplitRect"></param>
        private void DrawSplitters(Rect verticalSplitRect, Rect leftHorizontalSplitRect, Rect rightHorizontalSplitRect)
        {
            EditorGUI.DrawRect(verticalSplitRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUI.DrawRect(leftHorizontalSplitRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUI.DrawRect(rightHorizontalSplitRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Handle Splitter Drag
        /// </summary>
        /// <param name="verticalSplitRect"></param>
        /// <param name="horizontalSplitRect"></param>
        /// <param name="rect"></param>
        /// <param name="leftHorizontalSplitRect"></param>
        /// <param name="rightHorizontalSplitRect"></param>
        private void HandleSplitterDrag(Rect verticalSplitRect, Rect horizontalSplitRect, Rect rect,
            Rect leftHorizontalSplitRect, Rect rightHorizontalSplitRect)
        {
            Event e = Event.current;

            EditorGUIUtility.AddCursorRect(verticalSplitRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(leftHorizontalSplitRect, MouseCursor.ResizeVertical);
            EditorGUIUtility.AddCursorRect(rightHorizontalSplitRect, MouseCursor.ResizeVertical);

            if (e.type == EventType.MouseDown && verticalSplitRect.Contains(e.mousePosition))
            {
                isDraggingVertical = true;
                e.Use();
            }

            if (e.type == EventType.MouseDown && leftHorizontalSplitRect.Contains(e.mousePosition))
            {
                isDraggingLeftHorizontal = true;
                e.Use();
            }

            if (e.type == EventType.MouseDown && rightHorizontalSplitRect.Contains(e.mousePosition))
            {
                isDraggingRightHorizontal = true;
                e.Use();
            }

            if (e.type == EventType.MouseDrag)
            {
                float toolbarHeight = 20f;
                float totalHeight = rect.height - toolbarHeight;
                float totalWidth = rect.width;

                if (isDraggingVertical)
                {
                    verticalSplitPos = Mathf.Clamp(e.mousePosition.x, MIN_VERTICAL_SPLIT, totalWidth - MIN_VERTICAL_SPLIT);
                    if (window)
                        window.Repaint();
                }
                else if (isDraggingLeftHorizontal)
                {
                    float localY = e.mousePosition.y - toolbarHeight;
                    leftSplitPos = Mathf.Clamp(localY, MIN_HORIZONTAL_SPLIT, totalHeight - MIN_HORIZONTAL_SPLIT - SPLITTER_WIDTH);
                    if (window)
                        window.Repaint();
                }
                else if (isDraggingRightHorizontal)
                {
                    float localY = e.mousePosition.y - toolbarHeight;
                    rightSplitPos = Mathf.Clamp(localY, MIN_HORIZONTAL_SPLIT, totalHeight - MIN_HORIZONTAL_SPLIT - SPLITTER_WIDTH);
                    if (window)
                        window.Repaint();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                isDraggingVertical = false;
                isDraggingLeftHorizontal = false;
                isDraggingRightHorizontal = false;
                SaveLayout();
            }
        }

        /// <summary>
        /// Load Layout
        /// </summary>
        private void LoadLayout()
        {
            verticalSplitPos = EditorPrefs.GetFloat("AssetBundleEditor_VerticalSplit", 300f);
            leftSplitPos = EditorPrefs.GetFloat("AssetBundleEditor_LeftSplit", 200f);
            rightSplitPos = EditorPrefs.GetFloat("AssetBundleEditor_RightSplit", 200f);
        }

        /// <summary>
        /// Save Layout 
        /// </summary>
        private void SaveLayout()
        {
            EditorPrefs.SetFloat("AssetBundleEditor_VerticalSplit", verticalSplitPos);
            EditorPrefs.SetFloat("AssetBundleEditor_LeftSplit", leftSplitPos);
            EditorPrefs.SetFloat("AssetBundleEditor_RightSplit", rightSplitPos);
        }
    }
}
