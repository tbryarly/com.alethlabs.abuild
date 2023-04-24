using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aleth.Prefs;
using AlethEditor.Build;
using AlethEditor.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class BuildOptionsPrefGroup : APrefGroup
    {
        public const string DEBUG_BUILD_KEY = "DEBUG_BUILD";

        private ReorderableList _sceneFolderList;
        protected ReorderableList SceneFolderList
        { 
            get 
            { 
                if (_sceneFolderList == null) 
                {
                    _sceneFolderList = new ReorderableList(ABuildPrefs.BuildSceneFolders, 
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
                    _scenePathList = new ReorderableList(ABuildPrefs.SelectedScenesForBuild,
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
        public override string HeaderName { get { return "Build Options"; } }

        [EditorPref("BuildOptionsColumnNum", 0f, Mathf.Infinity)]
        private static int BuildOptionsColumnNum = 0;
        public override int Column
        {
            get { return BuildOptionsColumnNum; }
            set { BuildOptionsColumnNum = value; }
        }

        [EditorPref("BuildOptionsColumnPriority", 0f, Mathf.Infinity)]
        private static int BuildOptionsColumnPriority = 0;
        public override int ColumnPriority
        {
            get { return BuildOptionsColumnPriority; }
            set { BuildOptionsColumnPriority = value; }
        }

        [EditorPref("BuildOptionsColumnWidth", APrefGroup.minColWidth, APrefGroup.maxColWidth)]
        public static float BuildOptionsColumnWidth = 250f;
        public override float ColumnWidth
        {
            get { return BuildOptionsColumnWidth; }
            set { BuildOptionsColumnWidth = value; }
        }

        public override bool IgnoreContextMenu(Vector2 parentScroll) { return true; }

        public override Rect DrawGroup()
        {
            Rect rect = GUILayoutUtility.GetLastRect();
        
            ABuildPrefs.BuildGroup = (BuildGroups)EditorPrefAttribute.DrawPref(ABuildPrefs.BuildGroup, "Targets");
            ABuildPrefs.BuildArch = (BuildArchs)EditorPrefAttribute.DrawPref(ABuildPrefs.BuildArch, "Architecture");
            DrawOutputPath();

            //ABuildPrefs.IsDebugBuild = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.IsDebugBuild, "Debug Build");
            ABuildPrefs.IsDebugBuild = (bool)EditorPrefAttribute.DrawBoolWithDefineSymbol(
                ABuildPrefs.IsDebugBuild, 
                "Debug Build", 
                DEBUG_BUILD_KEY);

            if (ABuildPrefs.IsDebugBuild)
            {
                DrawDebugOptions();
            }

            EditorGUILayout.Space();
            DrawSceneLocations();
            EditorGUILayout.Space();

            DrawPaths();
            
            if (GUILayout.Button("Build Selected"))
            {
                ABuildManager.BuildAll(ABuildPrefs.BuildGroup, 
                                       ABuildPrefs.BuildArch, 
                                       ABuildPrefs.IsDebugBuild, 
                                       ABuildPrefs.RunDeepProfile);
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
                string path = EditorUtility.OpenFolderPanel("Build Output Folder", ABuildPrefs.BuildPath, "");
                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.BuildPath != path)
                {
                    ABuildPrefs.BuildPath = path;
                }
                Event.current.Use();
                GUIUtility.ExitGUI();
            }
            EditorGUI.TextField(pathRect, "Build Output Path", ABuildPrefs.BuildPath);
        }

        private void DrawDebugOptions()
        {
            ABuildPrefs.ABuildDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(ABuildPrefs.ABuildDebugLevels, "Build Debug Level");

            ABuildPrefs.RunDeepProfile = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.RunDeepProfile, "Deep Profile");
        }

        private void DrawSceneLocations()
        {
            EditorGUILayout.LabelField("Scene Locations");
            IncludeSceneLocations locationCheck = (IncludeSceneLocations)EditorPrefAttribute.DrawPref(
                ABuildPrefs._buildSceneLocations, 
                "Get Scenes From");

            if (ABuildPrefs._buildSceneLocations != locationCheck)
            {
                ABuildPrefs._buildSceneLocations = locationCheck;
                ResetSceneCount();
            }

            bool selectScenes = ABuildPrefs.BuildSceneLocations.HasFlag(IncludeSceneLocations.SelectScenes);

            if (ABuildPrefs.BuildSceneFolders == null)
                ABuildPrefs.BuildSceneFolders = new string[] { };
            if (ABuildPrefs.BuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFolders) ||
                ABuildPrefs.BuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFoldersRecursive))
                SceneFolderList.DoLayoutList();
            else if (selectScenes)
                ScenePathList.DoLayoutList();

            if (!selectScenes)
            {
                string[] scenes = ABuildInstructions.GetScenes(ABuildPrefs.BuildSceneLocations);
                sceneListFold = EditorGUILayout.BeginFoldoutHeaderGroup(
                    sceneListFold,
                    $"Found {SceneCount} scenes to build.");

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
                    _sceneCount = ABuildInstructions.GetScenes(ABuildPrefs.BuildSceneLocations)?.Length ?? 0; 
                return _sceneCount; 
            } 
        }

        private void ResetSceneCount() { _sceneCount = -1; }

        private void DrawPaths()
        {
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Windows) ||
                ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Linux) ||
                ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Mac))
                EditorGUILayout.LabelField("Paths");
                
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Windows))
                EditorGUILayout.TextField("Windows Path", ABuildInstructions.GetBuildPath(BuildGroups.Windows));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Linux))
                EditorGUILayout.TextField("Linux Path", ABuildInstructions.GetBuildPath(BuildGroups.Linux));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Mac))
                EditorGUILayout.TextField("Mac Path", ABuildInstructions.GetBuildPath(BuildGroups.Mac));            
        }

        private int GetPathAge()
        {
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Windows))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Windows)).Days;
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Linux))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Linux)).Days;
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Mac))
                return ABuildManager.GetLastModifyDelta(ABuildInstructions.GetBuildPath(BuildGroups.Mac)).Days;
            return int.MaxValue;
        }

        private string GetPathAgeString()
        {
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Windows))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Windows));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Linux))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Linux));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Mac))
                return ABuildManager.GetLastModifiedString(ABuildInstructions.GetBuildPath(BuildGroups.Mac));
            return "";
        }
        #endregion

        #region ReordList Callbacks
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
                string path = EditorUtility.OpenFolderPanel("Scene Search Location", ABuildPrefs.BuildSceneFolders[i], "");
                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.BuildSceneFolders[i] != path)
                {
                    ABuildPrefs.BuildSceneFolders[i] = path;
                    ResetSceneCount();
                }
                GUIUtility.ExitGUI();
                e.Use();
            }

            EditorGUI.TextField(rect, ABuildPrefs.BuildSceneFolders[i]);
        }

        private void AddLocationElement(ReorderableList list)
        {
            ABuildPrefs.BuildSceneFolders = ABuildPrefs.BuildSceneFolders.Append("").ToArray();
            _sceneFolderList = null; // Rebuild list
            ResetSceneCount();
        }

        private void RemoveLocationElement(ReorderableList list)
        {
            List<string> sList = ABuildPrefs.BuildSceneFolders.ToList();
            sList.RemoveAt(list.index);
            ABuildPrefs.BuildSceneFolders = sList.ToArray();
            _sceneFolderList = null; // Rebuild list
            ResetSceneCount();
        }

        private void ReorderLocationList(ReorderableList list, int oldIndex, int newIndex)
        {
            ABuildPrefs.BuildSceneFolders = (string[])list.list;
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
                    ABuildPrefs.SelectedScenesForBuild[i],
                    "unity");

                if (!string.IsNullOrWhiteSpace(path) &&
                    ABuildPrefs.SelectedScenesForBuild[i] != path)
                {
                    ABuildPrefs.SelectedScenesForBuild[i] = path;
                    ResetSceneCount();
                }
                GUIUtility.ExitGUI();
                e.Use();
            }

            EditorGUI.TextField(rect, ABuildPrefs.SelectedScenesForBuild[i]);
        }

        private void AddScenePathElement(ReorderableList list)
        {
            ABuildPrefs.SelectedScenesForBuild = ABuildPrefs.SelectedScenesForBuild.Append("").ToArray();
            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }

        private void RemoveScenePathElement(ReorderableList list)
        {
            List<string> sList = ABuildPrefs.SelectedScenesForBuild.ToList();
            sList.RemoveAt(list.index);
            ABuildPrefs.SelectedScenesForBuild = sList.ToArray();
            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }

        private void ReorderScenePathList(ReorderableList list, int oldIndex, int newIndex)
        {
            ABuildPrefs.SelectedScenesForBuild = (string[])list.list;
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
            List<string> retList = ABuildPrefs.SelectedScenesForBuild.ToList();

            int count = 0;
            foreach (Object obj in objs)
            {
                if (obj is SceneAsset scene)
                {
                    string path = Path.Join(
                        Application.dataPath,
                        AssetDatabase.GetAssetPath(scene));

                    retList.Insert(index + count, path);
                    count++;
                }
            }

            ABuildPrefs.SelectedScenesForBuild = retList.ToArray();

            _scenePathList = null; // Rebuild list
            ResetSceneCount();
        }
        #endregion
    }
}