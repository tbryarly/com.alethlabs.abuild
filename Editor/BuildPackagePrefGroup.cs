﻿using System;
using System.Collections;
using System.Collections.Generic;
using Aleth.Prefs;
using AlethEditor.Prefs;
using AlethEditor.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class BuildPackagePrefGroup : APrefGroup
    {
        private const string DEMO_BUILD_KEY = "DEMO_BUILD";

        #region APrefGroup
        public override string HeaderName { get { return "Package Options"; } }

        [EditorPref("BuildPackageColumnNum", 0f, Mathf.Infinity)]
        private static int BuildPackageColumnNum = 0;
        public override int Column
        {
            get { return BuildPackageColumnNum; }
            set { BuildPackageColumnNum = value; }
        }

        [EditorPref("BuildPackageColumnPriority", 0f, Mathf.Infinity)]
        private static int BuildPackageColumnPriority = 0;
        public override int ColumnPriority
        {
            get { return BuildPackageColumnPriority; }
            set { BuildPackageColumnPriority = value; }
        }

        [EditorPref("BuildPackageColumnWidth", APrefGroup.minColWidth, APrefGroup.maxColWidth)]
        public static float BuildPackageColumnWidth = 250f;
        public override float ColumnWidth
        {
            get { return BuildPackageColumnWidth; }
            set { BuildPackageColumnWidth = value; }
        }

        public override Rect DrawGroup()
        {
            Rect rect = GUILayoutUtility.GetLastRect();

            ABuildPrefs.ABuildPackageDebugLevels = (DebugLevels)EditorPrefAttribute.DrawPref(
                ABuildPrefs.ABuildPackageDebugLevels, 
                "Package Debug Level");

            ABuildPrefs.ShowDemoBuild = EditorPrefAttribute.DrawBoolWithDefineSymbol(
                ABuildPrefs.ShowDemoBuild,
                "Show Demo Build",
                DEMO_BUILD_KEY);

            rect.height = (GUILayoutUtility.GetLastRect().y - rect.y) + GUILayoutUtility.GetLastRect().height;
            return rect;
        }


        #endregion
    }
}