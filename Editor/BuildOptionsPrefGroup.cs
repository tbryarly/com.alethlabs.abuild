using System;
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

        #region APrefGroup
        public override string HeaderName { get { return "Options"; } }

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

            ABuildPrefs.IsDebugBuild = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.IsDebugBuild, "Debug Build");

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
            if (GUILayout.Button("Run Last Build"))
            {
                ABuildManager.RunBuild();
                GUIUtility.ExitGUI();
            }

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
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.Space();
            //EditorGUILayout.BeginVertical();

            ABuildPrefs.ABuildDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(ABuildPrefs.ABuildDebugLevels, "Build Debug Level");

            ABuildPrefs.RunDeepProfile = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.RunDeepProfile, "Deep Profile");
            //EditorGUILayout.LabelField("Set Debug State", EditorStyles.boldLabel);
            //ABuildPrefs.DebugUpdateContext = (ProjectScopes)EditorPrefAttribute.DrawPref(ABuildPrefs.DebugUpdateContext, "Scope to Update");
            //if (GUILayout.Button("Apply debug state"))
            //{
            //    Debug.Log("Apply to scope TBI!");
            //}

            //EditorGUILayout.EndVertical();
            //EditorGUILayout.Space();
            //EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneLocations()
        {
            EditorGUILayout.LabelField("Scene Locations");
            IncludeSceneLocations locationCheck = (IncludeSceneLocations)EditorPrefAttribute.DrawPref(ABuildPrefs.BuildSceneLocations, "Get Scenes From");
            if (ABuildPrefs.BuildSceneLocations != locationCheck)
            {
                ABuildPrefs.BuildSceneLocations = locationCheck;
                ResetSceneCount();
            }

            if (ABuildPrefs.BuildSceneFolders == null)
                ABuildPrefs.BuildSceneFolders = new string[] { };
            if (ABuildPrefs.BuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFolders) ||
                ABuildPrefs.BuildSceneLocations.HasFlag(IncludeSceneLocations.LocalFoldersRecursive))
                SceneFolderList.DoLayoutList();

            string[] scenes = ABuildInstructions.GetScenes();
            sceneListFold = EditorGUILayout.BeginFoldoutHeaderGroup(sceneListFold, $"Found {SceneCount} scenes to build.");
            if (sceneListFold && scenes != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                foreach (string scene in scenes)
                    EditorGUILayout.LabelField(scene);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            // EditorGUILayout.LabelField($"Found {SceneCount} scenes to build.");
        }

        private bool sceneListFold = false;
        private int _sceneCount = -1;
        private int SceneCount { get { if (_sceneCount == -1) _sceneCount = ABuildInstructions.GetScenes()?.Length ?? 0; return _sceneCount; } }
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
    }
}