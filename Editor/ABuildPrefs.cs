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
        AllScenes = 8,

        /// <summary>
        /// Explicitly select scenes to include.
        /// </summary>
        SelectScenes = 16,

        /// <summary>
        /// Use Demo values.
        /// </summary>
        IsDemo = 128,
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
        public static BuildMarketplaces BuildMarketplace = BuildMarketplaces.None;

        [EditorPref]
        public static string BuildPath = "";

        [EditorPref]
        public static bool IsDebugBuild = false;

        [EditorPref]
        public static bool RunDeepProfile = false;

        [EditorPref]
        public static ProjectScopes DebugUpdateContext;

        [EditorPref]
        public static IncludeSceneLocations _buildSceneLocations = IncludeSceneLocations.UseBuildSettings;
        public static IncludeSceneLocations BuildSceneLocations
        {
            get
            {
                return _buildSceneLocations & ~IncludeSceneLocations.IsDemo;
            }
            set
            {
                _buildSceneLocations = value;
            }
        }

        [EditorPref]
        public static string[] SelectedScenesForBuild = new string[] { };

        [EditorPref]
        public static string[] BuildSceneFolders = new string[] { };

        [EditorPref]
        public static bool ShowDemoBuild = false;

        [EditorPref]
        public static DebugLevels DemoABuildPackageDebugLevels = DebugLevels.Silent;

        [EditorPref]
        public static DebugLevels DemoABuildDebugLevels = DebugLevels.Silent;

        [EditorPref("BuildColWidth")]
        public static float DemoBuildColWidth = 200f;

        [EditorPref]
        public static BuildGroups DemoBuildGroup = BuildGroups.Windows;

        [EditorPref]
        public static BuildArchs DemoBuildArch = BuildArchs.x86_64;

        [EditorPref]
        public static string DemoBuildPath = "";

        [EditorPref]
        public static bool DemoRunDeepProfile = false;

        [EditorPref]
        public static ProjectScopes DemoDebugUpdateContext;

        [EditorPref]
        public static IncludeSceneLocations _demoBuildSceneLocations = IncludeSceneLocations.UseBuildSettings;
        public static IncludeSceneLocations DemoBuildSceneLocations 
        { 
            get 
            { 
                return _demoBuildSceneLocations | IncludeSceneLocations.IsDemo; 
            } 
            set
            {
                 _demoBuildSceneLocations = value;
            }
        }

        [EditorPref]
        public static string[] DemoBuildSceneFolders = new string[] { };

        [EditorPref]
        public static string[] SelectedDemoScenesForBuild = new string[] { };
    }
}
