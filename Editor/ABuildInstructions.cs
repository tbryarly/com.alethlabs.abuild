using System;
using System.Collections.Generic;
using System.IO;
using AlethEditor.Prefs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace AlethEditor.Build
{
    /// <summary>
    /// Custom build profiles: 
    ///     Build Win64
    ///     Build Win64 (debug)
    /// </summary>
    public class ABuildInstructions : MonoBehaviour
    {
        private static IncludeSceneLocations sceneLocations 
        { 
            get 
            {
                return ABuildPrefs.ShowDemoBuild ?
                    ABuildPrefs.DemoBuildSceneLocations :
                    ABuildPrefs.BuildSceneLocations;
            } 
        }

        public static string GetBuildPath(BuildGroups buildGroup)
        {
            // REVIEW: if multiple, fail!
            
            string retString = ABuildPrefs.BuildPath; 

            retString += $"/{buildGroup}/";

            retString += ((int)ABuildPrefs.BuildArch == 2) ? "x86_64/" : "x86/";

            retString += ABuildPrefs.ShowDemoBuild ? "Demo/" : "";

            retString += ABuildPrefs.IsDebugBuild ? "Debug/" : "Production/";

            retString += Application.productName;

            if (buildGroup == BuildGroups.Windows)
                retString += ".exe";
            else if (buildGroup == BuildGroups.Linux)
                retString += ((int)ABuildPrefs.BuildArch == 2) ? ".x86_64" : ".x86";

            return retString;
        }

        /// <summary>
        /// Default build configuration for 64 bit system.
        /// Will open folder containing build when complete.
        /// </summary>
        public static void WindowsBuild(bool x64 = true,
                                        bool debug = false,
                                        bool deepProfile = false)
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            string path = Path.GetFullPath(GetBuildPath(BuildGroups.Windows));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenes(sceneLocations),
                locationPathName = path,
                target = x64 ? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneWindows,
                options = debug ?
                                  (BuildOptions.Development |
                                   BuildOptions.AllowDebugging |
                                   BuildOptions.ConnectWithProfiler)
                                : BuildOptions.None,
            };

            if (deepProfile)
            {
                buildPlayerOptions.options |= BuildOptions.EnableDeepProfilingSupport;
            }

            Debug.Log($"Building {Application.productName} for Windows to {path}.\nx64: {x64}\nDebug: {debug}");
            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Successfully built {Application.productName} for Win64. Opening folder now.");
                System.Diagnostics.Process.Start(path.Replace(@"\" + $"{Application.productName}.exe", ""));
            }
            else
            {
                Debug.LogError("Build failed:\n" + result.summary);
                EditorUtility.DisplayDialog("Build Failed",
                                            "Build was not sucessful. See console for details.",
                                            "Ok");
            }
        }

        /// <summary>
        /// Default build configuration for Linux system with 
        /// debug enabled. 
        /// </summary>
        public static void LinuxBuild(bool x64 = true,
                                      bool debug = false,
                                      bool deepProfile = false,
                                      bool ForceDebugLogging = false,
                                      bool runBuild = false,
                                      bool nukeSettings = true)
        {
            if (!x64)
            {
                Debug.LogWarning("Unity does not support 32 bit linux builds. Canceling build...");
                return;
            }

            // if (nukeSettings)
            //     OptionsController.NukeSettings();

            // if (ForceDebugLogging)
            //     PlayerPrefsManager.SetForceDebugLogging(true);
            // else
            //     PlayerPrefsManager.SetForceDebugLogging(false);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            string path = Path.GetFullPath(GetBuildPath(BuildGroups.Linux));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenes(sceneLocations),
                locationPathName = path,
                target = BuildTarget.StandaloneLinux64,
                options = debug ?
                                  (BuildOptions.Development |
                                   BuildOptions.AllowDebugging |
                                   BuildOptions.ConnectWithProfiler)
                                : BuildOptions.None,
            };

            if (deepProfile)
            {
                buildPlayerOptions.options |= BuildOptions.EnableDeepProfilingSupport;
            }

            Debug.Log($"Building {Application.productName} for Linux to {path}.\nx64: {x64}\nDebug: {debug}\nForce Debug Logging: {ForceDebugLogging}");
            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string folderPath = path.Replace(@"\" + $"{Application.productName}." + (x64 ? "x86_64" : "x86"), "");
                Debug.Log($"Successfully built {Application.productName} for Linux. Opening folder now ({folderPath}).");
                System.Diagnostics.Process.Start(folderPath);                
            }
            else
            {
                Debug.LogError("Build failed:\n" + result.summary);
                EditorUtility.DisplayDialog("Build Failed",
                                            "Build was not sucessful. See console for details.",
                                            "Ok");
            }
        }

        /// <summary>
        /// Default build configuration for Mac system with 
        /// debug enabled. 
        /// </summary>
        public static void MacBuild(bool x64 = true,
                                    bool debug = false,
                                    bool deepProfile = false,
                                    bool ForceDebugLogging = false,
                                    bool runBuild = false,
                                    bool nukeSettings = true)
        {
            if (!x64)
            {
                Debug.LogWarning("Unity does not support 32 bit Mac builds. Canceling build...");
                return;
            }

            // if (nukeSettings)
            //     OptionsController.NukeSettings();

            // if (ForceDebugLogging)
            //     PlayerPrefsManager.SetForceDebugLogging(true);
            // else
            //     PlayerPrefsManager.SetForceDebugLogging(false);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            string path = Path.GetFullPath(GetBuildPath(BuildGroups.Mac));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenes(sceneLocations),
                locationPathName = path,
                target = BuildTarget.StandaloneOSX,
                options = debug ?
                                  (BuildOptions.Development |
                                   BuildOptions.AllowDebugging |
                                   BuildOptions.ConnectWithProfiler)
                                : BuildOptions.None,
            };

            if (deepProfile)
            {
                buildPlayerOptions.options |= BuildOptions.EnableDeepProfilingSupport;
            }

            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log($"Building {Application.productName} for Mac to {path}.\nx64: {x64}\nDebug: {debug}\nForce Debug Logging: {ForceDebugLogging}");
            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Successfully built {Application.productName} for Mac (debug).");
                System.Diagnostics.Process.Start(path);

                if (runBuild)
                    Debug.LogWarning("Run on build not implemented for Mac.");
            }
            else
            {
                Debug.LogError("Build failed:\n" + result.summary);
                EditorUtility.DisplayDialog("Build Failed",
                                            "Build was not sucessful. See console for details.",
                                            "Ok");
            }
        }

        public static string[] GetScenes(IncludeSceneLocations locations)
        {           
            if (locations.HasFlag(IncludeSceneLocations.AllScenes))
                return FindAllScenes();

            //Debug.Log(locations);
            if (locations.HasFlag(IncludeSceneLocations.LocalFolders) ||
                locations.HasFlag(IncludeSceneLocations.LocalFoldersRecursive))
            {
                if (locations.HasFlag(IncludeSceneLocations.IsDemo))
                {
                    return FinalLocalScenes(
                          locations.HasFlag(IncludeSceneLocations.LocalFoldersRecursive),
                          ABuildPrefs.DemoBuildSceneFolders);
                }
                else
                {
                    return FinalLocalScenes(
                        locations.HasFlag(
                            IncludeSceneLocations.LocalFoldersRecursive),
                            ABuildPrefs.BuildSceneFolders);
                }
            }                
            
            if (locations.HasFlag(IncludeSceneLocations.UseBuildSettings))
                return GetBuildSettingScenes();

            if (locations.HasFlag(IncludeSceneLocations.SelectScenes))
                return GetSelectSceneSettings(locations.HasFlag(IncludeSceneLocations.IsDemo));

            return null;
        }

        private static string[] FindAllScenes()
        {
            List<string> retList = new List<string>();

            string[] guids = AssetDatabase.FindAssets("t:scene");
            foreach (string guid in guids)
                retList.Add(AssetDatabase.GUIDToAssetPath(guid));

            return retList.ToArray();
        }

        private static string[] FinalLocalScenes(bool isRecursive, string[] source)
        {
            List<string> retList = new List<string>();

            foreach (string path in source)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue; 

                foreach (string s in Directory.GetFiles(path, "*.unity", isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    retList.Add(s.Replace(Application.dataPath, "Assets"));
                }
            }

            return retList.ToArray();
        }

        private static string[] GetBuildSettingScenes()
        {
            List<string> retList = new List<string>();
            
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                retList.Add($"Assets/{scene.path}");
            }

            return retList.ToArray();
        }

        private static string[] GetSelectSceneSettings(bool isDemo)
        {
            List<string> retList = new List<string>();

            string[] sources = isDemo ? ABuildPrefs.SelectedDemoScenesForBuild : ABuildPrefs.SelectedScenesForBuild;
            foreach (string source in sources)
            {
                string path = source.Replace(Application.dataPath + @"\", "");
                path = path.Replace(Application.dataPath, "");

                if (string.IsNullOrWhiteSpace(path))
                    continue;
                retList.Add(path);
            }

            return retList.ToArray();
        }

        public static void SetBuildScenes(List<string> sceneList, bool isDemo)
        {
            if (isDemo)
                ABuildPrefs.SelectedDemoScenesForBuild = sceneList.ToArray();
            else
                ABuildPrefs.SelectedScenesForBuild = sceneList.ToArray();

        }
    }
}