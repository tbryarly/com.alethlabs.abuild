using System.Collections;
using System.Collections.Generic;
using Aleth.Prefs;
using AlethEditor.Build;
using AlethEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class BuildOptionsPrefGroup : APrefGroup
    {
        #region ABuildGroup
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

        public override void DrawGroup()
        {
            base.DrawGroup();

            DrawOutputPath();

            ABuildPrefs.BuildGroup = (BuildGroups)EditorPrefAttribute.DrawPref(ABuildPrefs.BuildGroup, "Targets");
            ABuildPrefs.BuildArch = (BuildArchs)EditorPrefAttribute.DrawPref(ABuildPrefs.BuildArch, "Architecture");

            ABuildPrefs.IsDebugBuild = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.IsDebugBuild, "Debug Build");

            if (ABuildPrefs.IsDebugBuild)
            {
                DrawDebugOptions();
            }

            EditorGUILayout.Space();

            DrawPaths();

            if (GUILayout.Button("Build Selected"))
            {
                ABuildManager.BuildAll(ABuildPrefs.BuildGroup, ABuildPrefs.BuildArch, ABuildPrefs.IsDebugBuild);
            }

            ABuildPrefs.RunDeepProfile = (bool)EditorPrefAttribute.DrawPref(ABuildPrefs.RunDeepProfile, "Deep Profile On Run");
            if (GUILayout.Button("Run Last Build"))
            {
                ABuildManager.RunBuild(ABuildPrefs.RunDeepProfile);
            }
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
                string path = EditorUtility.OpenFolderPanel("Audio Workspace", ABuildPrefs.BuildPath, "");
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();

            ABuildPrefs.ABuildDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(ABuildPrefs.ABuildDebugLevels, "Build Debug Level");

            EditorGUILayout.LabelField("Set Debug State", EditorStyles.boldLabel);
            ABuildPrefs.DebugUpdateContext = (ProjectScopes)EditorPrefAttribute.DrawPref(ABuildPrefs.DebugUpdateContext, "Scope to Update");
            if (GUILayout.Button("Apply debug state"))
            {
                Debug.Log("Apply to scope TBI!");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPaths()
        {
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Windows))
                EditorGUILayout.TextField("Windows Path", ABuildInstructions.GetBuildPath(BuildGroups.Windows));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Linux))
                EditorGUILayout.TextField("Linux Path", ABuildInstructions.GetBuildPath(BuildGroups.Linux));
            if (ABuildPrefs.BuildGroup.HasFlag(BuildGroups.Mac))
                EditorGUILayout.TextField("Mac Path", ABuildInstructions.GetBuildPath(BuildGroups.Mac));
        }
        #endregion
    }
}