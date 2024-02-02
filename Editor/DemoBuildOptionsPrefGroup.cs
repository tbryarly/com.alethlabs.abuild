using Aleth.Prefs;
using AlethEditor.Build;
using AlethEditor.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class DemoBuildOptionsPrefGroup : APrefGroup 
    {
        private readonly Color _headingColor = new Color32(255, 165, 0, 255);
        protected override Color HeadingColor => _headingColor;

        private ReorderableList _sceneFolderList;
        protected ReorderableList SceneFolderList
        {
            get
            {
                if (_sceneFolderList == null)
                {
                    _sceneFolderList = new ReorderableList(ABuildPrefs.DemoBuildSceneFolders,
                                                          typeof(string),
                                                          true,
                                                          false,
                                                          true,
                                                          true);
                    _sceneFolderList.elementHeight = AlethEditorUtils.SLine;
                    _sceneFolderList.drawHeaderCallback = DrawSceneHeader;
                    _sceneFolderList.drawElementCallback = DrawSceneLocationElement;
                    _sceneFolderList.onAddCallback = AddLocationElement;
                    _sceneFolderList.onRemoveCallback = RemoveLocationElement;
                    _sceneFolderList.onReorderCallbackWithDetails = ReorderLocationList;
                }
                return _sceneFolderList;
            }
        }

        private ReorderableList _scenePathList;
        protected ReorderableList ScenePathList
        {
            get
            {
                if (_scenePathList == null)
                {
                    _scenePathList = new ReorderableList(ABuildPrefs.SelectedDemoScenesForBuild,
                                                          typeof(string),
                                                          true,
                                                          false,
                                                          true,
                                                          true);
                    _scenePathList.elementHeight = AlethEditorUtils.SLine;
                    _scenePathList.drawHeaderCallback = DrawScenePathHeader;
                    _scenePathList.drawElementCallback = DrawScenePathElement;
                    _scenePathList.onAddCallback = AddScenePathElement;
                    _scenePathList.onRemoveCallback = RemoveScenePathElement;
                    _scenePathList.onReorderCallbackWithDetails = ReorderScenePathList;
                }
                return _scenePathList;
            }
        }

        #region APrefGroup
        public override string HeaderName { get { return "Demo Build Options"; } }

        [EditorPref("DemoBuildOptionsColumnNum", 0f, Mathf.Infinity)]
        private static int DemoBuildOptionsColumnNum = 0;
        public override int Column
        {
            get { return DemoBuildOptionsColumnNum; }
            set { DemoBuildOptionsColumnNum = value; }
        }

        [EditorPref("DemoBuildOptionsColumnPriority", 0f, Mathf.Infinity)]
        private static int DemoBuildOptionsColumnPriority = 0;
        public override int ColumnPriority
        {
            get { return DemoBuildOptionsColumnPriority; }
            set { DemoBuildOptionsColumnPriority = value; }
        }

        [EditorPref("DemoBuildOptionsColumnWidth", APrefGroup.minColWidth, APrefGroup.maxColWidth)]
        public static float DemoBuildOptionsColumnWidth = 250f;
        public override float ColumnWidth
        {
            get { return DemoBuildOptionsColumnWidth; }
            set { DemoBuildOptionsColumnWidth = value; }
        }

        public override bool IgnoreContextMenu(Vector2 parentScroll) { return true; }

        public override Rect DrawGroup()
        {
            Rect rect = GUILayoutUtility.GetLastRect();

            ABuildPrefs.DemoBuildGroup = (BuildGroups)EditorPrefAttribute.DrawPref(ABuildPrefs.DemoBuildGroup, "Targets");
            ABuildPrefs.DemoBuildArch = (BuildArchs)EditorPrefAttribute.DrawPref(ABuildPrefs.DemoBuildArch, "Architecture");
            DrawOutputPath();

            // Debug build is shared with regular options
            // to ensure they do not get out of sync with each other
            ABuildPrefs.IsDebugBuild = (bool)EditorPrefAttribute.DrawBoolWithDefineSymbol(
                ABuildPrefs.IsDebugBuild,
                "Debug Build",
                BuildOptionsPrefGroup.DEBUG_BUILD_KEY);

            if (ABuildPrefs.IsDebugBuild)
            {
                DrawDebugOptions();
            }

            ABuildPrefs.DemoDetailedBuildReport = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.DemoDetailedBuildReport, "Detailed Build Report");

            EditorGUILayout.Space();
            DrawSceneLocations();
            EditorGUILayout.Space();

            DrawPaths();

            if (GUILayout.Button("Build Selected"))
            {
                ABuildManager.BuildAll(ABuildPrefs.DemoBuildGroup,
                                       ABuildPrefs.DemoBuildArch,
                                       ABuildPrefs.IsDebugBuild,
                                       ABuildPrefs.DemoRunDeepProfile,
                                       ABuildPrefs.DemoDetailedBuildReport);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space();

            int buildAge = GetPathAge();
            bool oldGUI = GUI.enabled;
            bool validBuild = buildAge < 36500; // 100 years
            if (validBuild == false)
                GUI.enabled = false;

            if (GUILayout.Button(validBuild == false ? "No valid build" : $"Run Last Build ({GetPathAgeString()} old)"))
            {
                ABuildManager.RunBuild();
                GUIUtility.ExitGUI();
            }

            GUI.enabled = oldGUI;

            rect.height = (GUILayoutUtility.GetLastRect().y - rect.y) + GUILayoutUtility.GetLastRect().height;
            return rect;
        }
        #endregion

        #region Methods
        private void DrawOutputPath()
        {
            Rect pathRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                pathRect.Contains(Event.current.mousePosition))
            {
                string path = EditorUtility.OpenFolderPanel("Build Output Folder", ABuildPrefs.DemoBuildPath, "");
                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.DemoBuildPath != path)
                {
                    ABuildPrefs.DemoBuildPath = path;
                }
                Event.current.Use();
                GUIUtility.ExitGUI();
            }
            EditorGUI.TextField(pathRect, "Build Output Path", ABuildPrefs.DemoBuildPath);
        }

        private void DrawDebugOptions()
        {
            ABuildPrefs.DemoABuildDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(ABuildPrefs.DemoABuildDebugLevels, "Build Debug Level");

            ABuildPrefs.DemoRunDeepProfile = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.DemoRunDeepProfile, "Deep Profile");            
        }

        private void DrawSceneLocations()
        {
            EditorGUILayout.LabelField("Scene Locations");
            IncludeSceneLocations locationCheck = (IncludeSceneLocations)EditorPrefAttribute.DrawPref(
                ABuildPrefs._demoBuildSceneLocations, 
                "Get Scenes From");

            if (ABuildPrefs._demoBuildSceneLocations != locationCheck)
            {
                ABuildPrefs._demoBuildSceneLocations = locationCheck;
                ResetSceneCount();
            }

            bool selectScenes = ABuildPrefs.DemoBuildSceneLocations.HasFlag(IncludeSceneLocations.SelectScenes);

            if (ABuildPrefs.DemoBuildSceneFolders == null)
                ABuildPrefs.DemoBuildSceneFolders = new string[] { };
            if (ABuildPrefs.DemoBuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFolders) ||
                ABuildPrefs.DemoBuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFoldersRecursive))
                SceneFolderList.DoLayoutList();
            else if (selectScenes)
                ScenePathList.DoLayoutList();

            if (!selectScenes)
            {
                string[] scenes = ABuildInstructions.GetScenes(ABuildPrefs.DemoBuildSceneLocations);
                sceneListFold = EditorGUILayout.BeginFoldoutHeaderGroup(sceneListFold, $"Found {SceneCount} scenes to build.");
                if (sceneListFold && scenes != null)
                {
                    foreach (string scene in scenes)
                        EditorGUILayout.LabelField($"        {scene}");
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private bool sceneListFold = false;
        private int _sceneCount = -1;
        private int SceneCount
        {
            get
            {
                if (_sceneCount == -1)
                    _sceneCount = ABuildInstructions.GetScenes(ABuildPrefs.DemoBuildSceneLocations)?.Length ?? 0;
                return _sceneCount;
            }
        }

        private void ResetSceneCount() { _sceneCount = -1; }

        private void DrawPaths()
        {
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Windows) ||
                ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Linux) ||
                ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Mac))
                EditorGUILayout.LabelField("Paths");

            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Windows))
                EditorGUILayout.TextField("Windows Path", ABuildInstructions.GetBuildPath(BuildGroups.Windows));
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Linux))
                EditorGUILayout.TextField("Linux Path", ABuildInstructions.GetBuildPath(BuildGroups.Linux));
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Mac))
                EditorGUILayout.TextField("Mac Path", ABuildInstructions.GetBuildPath(BuildGroups.Mac));
        }

        private int GetPathAge()
        {
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Windows))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Windows)).Days;
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Linux))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Linux)).Days;
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Mac))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Mac)).Days;
            return int.MaxValue;
        }

        private string GetPathAgeString()
        {
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Windows))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Windows));
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Linux))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Linux));
            if (ABuildPrefs.DemoBuildGroup.HasFlag(BuildGroups.Mac))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Mac));
            return "";
        }
        #endregion

        #region ReordList SceneFolder Callbacks
        private void DrawSceneHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Scene Search Folders");
        }

        private void DrawSceneLocationElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown &&
                e.button == 0 &&
                rect.Contains(e.mousePosition))
            {
                string path = EditorUtility.OpenFolderPanel("Scene Search Location", ABuildPrefs.DemoBuildSceneFolders[i], "");
                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.DemoBuildSceneFolders[i] != path)
                {
                    ABuildPrefs.DemoBuildSceneFolders[i] = path;
                    ResetSceneCount();
                }
                GUIUtility.ExitGUI();
                e.Use();
            }

            EditorGUI.TextField(rect, ABuildPrefs.DemoBuildSceneFolders[i]);
        }

        private void AddLocationElement(ReorderableList list)
        {
            ABuildPrefs.DemoBuildSceneFolders = ABuildPrefs.DemoBuildSceneFolders.Append("").ToArray();
            _sceneFolderList = null; // Rebuild list
            ResetSceneCount();
        }

        private void RemoveLocationElement(ReorderableList list)
        {
            List<string> sList = ABuildPrefs.DemoBuildSceneFolders.ToList();
            sList.RemoveAt(list.index);
            ABuildPrefs.DemoBuildSceneFolders = sList.ToArray();
            _sceneFolderList = null; // Rebuild list
            ResetSceneCount();
        }

        private void ReorderLocationList(ReorderableList list, int oldIndex, int newIndex)
        {
            ABuildPrefs.DemoBuildSceneFolders = (string[])list.list;
        }
        #endregion

        #region ReordList ScenePath Callbacks
        private void DrawScenePathHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Scene Paths");
        }

        private void DrawScenePathElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            CheckDrag(rect, i);

            Event e = Event.current;
            if (e.type == EventType.MouseDown &&
                e.button == 0 &&
                rect.Contains(e.mousePosition))
            {
                string path = EditorUtility.OpenFilePanel(
                    "Scene Path Location", 
                    ABuildPrefs.SelectedDemoScenesForBuild[i], 
                    "unity");

                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.SelectedDemoScenesForBuild[i] != path)
                {
                    ABuildPrefs.SelectedDemoScenesForBuild[i] = path;
                    ResetSceneCount();
                }
                GUIUtility.ExitGUI();
                e.Use();
            }

            EditorGUI.TextField(rect, ABuildPrefs.SelectedDemoScenesForBuild[i]);
        }

        private void AddScenePathElement(ReorderableList list)
        {
            ABuildPrefs.SelectedDemoScenesForBuild = ABuildPrefs.SelectedDemoScenesForBuild.Append("").ToArray();
            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }

        private void RemoveScenePathElement(ReorderableList list)
        {
            List<string> sList = ABuildPrefs.SelectedDemoScenesForBuild.ToList();
            sList.RemoveAt(list.index);
            ABuildPrefs.SelectedDemoScenesForBuild = sList.ToArray();
            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }

        private void ReorderScenePathList(ReorderableList list, int oldIndex, int newIndex)
        {
            ABuildPrefs.SelectedDemoScenesForBuild = (string[])list.list;
        }

        private void CheckDrag(Rect rect, int index)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:

                        if (DragIsScene)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            Event.current.Use();
                        }
                        break;
                    case EventType.DragPerform:
                        if (DragIsScene)
                        {
                            DragAndDrop.AcceptDrag();

                            AddSceneAt(DragAndDrop.objectReferences, index);
                            Event.current.Use();
                        }
                        break;
                }
            }
        }

        bool DragIsScene
        {
            get
            {
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (!(obj is SceneAsset))
                        return false;
                }
                return true;
            }
        }

        private void AddSceneAt(Object[] objs, int index)
        {
            List<string> retList = ABuildPrefs.SelectedDemoScenesForBuild.ToList();

            int count = 0;
            foreach (Object obj in objs)
            {
                if (obj is SceneAsset scene)
                {
                    string path = AssetDatabase.GetAssetPath(scene);
                    retList.Insert(index + count, path);
                    count++;
                }
            }

            ABuildPrefs.SelectedDemoScenesForBuild = retList.ToArray();

            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }
        #endregion
    }
}