using System.Collections;
using System.Collections.Generic;
using AlethEditor.Build;
using Aleth.Prefs;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public static class ABuildPrefs
    {
        [EditorPref]
        public static DebugLevels ABuildPackageDebugLevels = DebugLevels.Silent;

        [EditorPref]
        public static DebugLevels ABuildDebugLevels = DebugLevels.Silent;

        [EditorPref("BuildColWidth")]
        public static float BuildColWidth = 200f;

        [EditorPref]
        public static BuildGroups BuildGroup = BuildGroups.Windows;

        [EditorPref]
        public static BuildArchs BuildArch = BuildArchs.x86_64;

        [EditorPref]
        public static string BuildPath = "";

        [EditorPref]
        public static bool IsDebugBuild = false;

        [EditorPref]
        public static bool RunDeepProfile = false;

        [EditorPref]
        public static ProjectScopes DebugUpdateContext;
    }
}
