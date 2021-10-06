using System;
using System.Collections;
using System.Collections.Generic;
using Aleth.Prefs;
using AlethEditor.Build;
using AlethEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class ABuildTab : IPrefTab
    {
        #region ControlValues
        private Vector2 scrollPos;
        #endregion

        #region Style
        private static readonly Color HeadingColor = new Color(0.074f, 0.478f, 0.705f);
        private GUIStyle _colStyle;
        protected GUIStyle ColStyle
        {
            get
            {
                if (_colStyle == null)
                {
                    _colStyle = new GUIStyle()
                    {                        
                        fixedWidth = ABuildPrefs.BuildColWidth,
                    };
                }
                return _colStyle;
            }
        }

        private GUIStyle _headerStyle;
        protected GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.UpperCenter,
                    };

                    if (EditorGUIUtility.isProSkin)
                        _headerStyle.normal.textColor = Color.white;
                    else
                        _headerStyle.normal.textColor = Color.black;
                }
                return _headerStyle;
            }
        }
        #endregion 

        #region Properties
        
        private GenericMenu _cMenu;
        protected GenericMenu CMenu
        { 
            get 
            {
                if (_cMenu == null)
                {
                    _cMenu = new GenericMenu();
                    _cMenu.AddItem(new GUIContent("Clear Cache"), false, ClearCache);
                }
                return _cMenu;
            }
        }

        private void ClearCache()
        {
            _colStyle = null;
        }
        #endregion

        #region IPrefTab
        public string DisplayName { get { return "Aleth Build"; } }
        public int Priority { get { return 1000; } }

        public void OnEnable()
        {
            Debug.Log($"{Application.dataPath}");
            if (string.IsNullOrWhiteSpace(ABuildPrefs.BuildPath))
                ABuildPrefs.BuildPath = Application.dataPath;
        }

        public void OnDisable()
        {
            
        }

        public void DrawTab(float padding)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padding * 2);
            EditorGUILayout.BeginVertical(ColStyle);
            GUILayout.Space(padding);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawPackageOptions();
            EditorGUILayout.Space();
            DrawOptions();

            // EditorGUILayout.Space();            
            // DrawBuild();

            EditorGUILayout.EndScrollView();

            GUILayout.Space(padding);
            EditorGUILayout.EndVertical();
            GUILayout.Space(padding * 2);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public void DrawContext()
        {
            CMenu.ShowAsContext();
        }
        #endregion

        #region Draw Methods
        private void DrawHeader(string headerText)
        {
            Rect boxRect = EditorGUILayout.GetControlRect();
            AlethEditorUtils.DrawBox(boxRect, color: HeadingColor, title: headerText, style: HeaderStyle);

            float delta = AlethEditorUtils.DrawHeaderDrag(boxRect, this);
            if (delta != 0f)
            {
                ABuildPrefs.BuildColWidth += delta;
                ColStyle.fixedWidth = ABuildPrefs.BuildColWidth;                
            }
        }        

        private void DrawPackageOptions()
        {
            DrawHeader("Package Options");
            ABuildPrefs.ABuildPackageDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(ABuildPrefs.ABuildPackageDebugLevels, "Package Debug Level");            
        }

        private void DrawOptions()
        {            
            DrawHeader("Build Options");

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
