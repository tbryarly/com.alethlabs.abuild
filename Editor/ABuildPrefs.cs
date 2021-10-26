using System.Collections;
using System.Collections.Generic;
using AlethEditor.Build;
using Aleth.Prefs;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public enum IncludeSceneLocations 
    {
        None  = 0,

        /// <summary>
        /// Specify folders to look for scenes.
        /// </summary>
        LocalFolders = 1,

        /// <summary>
        /// Specify folders to look for scenes.
        /// Will include child folders as well.
        /// </summary>
        LocalFoldersRecursive = 2,

        /// <summary>
        /// Use only scenes in build settings.
        /// </summary>
        UseBuildSettings = 4,

        /// <summary>
        /// Include all scenes in folder.
        /// </summary>
        AllScenes = 8
    }

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

        [EditorPref]
        public static IncludeSceneLocations BuildSceneLocations = IncludeSceneLocations.UseBuildSettings;

        [EditorPref]
        public static string[] BuildSceneFolders = new string[] { };
    }
}
