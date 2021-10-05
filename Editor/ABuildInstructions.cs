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
        public static string GetBuildPath(BuildGroups buildGroup)
        {
            string retString = ABuildPrefs.BuildPath; 

            retString += $"{buildGroup}/";

            retString += ((int)ABuildPrefs.BuildArch == 2) ? "x86_64/" : "x86/";

            retString += ABuildPrefs.IsDebugBuild ? "Debug/" : "Production/";

            retString += "ELON";

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
                                        bool debug = false)
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, GetBuildPath(BuildGroups.Windows)));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                // scenes = LevelManager.TopFullPathLevelList.ToArray(),   // assumes all folders are for debug purposes and game scenes are all in top folder
                locationPathName = path,
                target = x64 ? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneWindows,
                options = debug ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler) : BuildOptions.None,
            };

            Debug.Log($"Building ELON for Windows to {path}.\nx64: {x64}\nDebug: {debug}");
            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("Successfully built ELON for Win64. Opening folder now.");
                System.Diagnostics.Process.Start(path.Replace(@"\ELON.exe", ""));
            }
            else
                Debug.LogError("Build failed:\n" + result.summary);
        }

        /// <summary>
        /// Default build configuration for Linux system with 
        /// debug enabled. 
        /// </summary>
        public static void LinuxBuild(bool x64 = true,
                                      bool debug = false,
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
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, GetBuildPath(BuildGroups.Linux)));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                // scenes = LevelManager.FullPathLevelList.ToArray(),
                locationPathName = path,
                target = BuildTarget.StandaloneLinux64,
                options = debug ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler) : BuildOptions.None
            };

            Debug.Log($"Building ELON for Linux to {path}.\nx64: {x64}\nDebug: {debug}\nForce Debug Logging: {ForceDebugLogging}");
            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string folderPath = path.Replace(@"/ELON." + (x64 ? "x86_64" : "x86"), "");
                Debug.Log($"Successfully built ELON for Linux. Opening folder now ({folderPath}).");
                System.Diagnostics.Process.Start(folderPath);                
            }
            else
                Debug.LogError("Build failed:\n" + result.summary);
        }

        /// <summary>
        /// Default build configuration for Mac system with 
        /// debug enabled. 
        /// </summary>
        public static void MacBuild(bool x64 = true,
                                    bool debug = false,
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
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, GetBuildPath(BuildGroups.Mac)));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                // scenes = LevelManager.FullPathLevelList.ToArray(),
                locationPathName = path,
                target = BuildTarget.StandaloneOSX,
                options = debug ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler) : BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log($"Building ELON for Mac to {path}.\nx64: {x64}\nDebug: {debug}\nForce Debug Logging: {ForceDebugLogging}");
            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("Successfully built ELON for Mac (debug).");
                System.Diagnostics.Process.Start(path);

                if (runBuild)
                    Debug.LogWarning("Run on build not implemented for Mac.");
            }
            else
                Debug.LogError("Build failed:\n" + result.summary);
        }
    }
}