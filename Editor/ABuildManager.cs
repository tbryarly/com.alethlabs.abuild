using System;
using UnityEngine;

namespace AlethEditor.Build
{
    [System.Flags]
    public enum BuildGroups
    {
        Windows = 1,
        Mac = 2,
        Linux = 4
    }
    
    public enum BuildArchs 
    { 
        x86 = 1, 
        x86_64 = 2 
    }

    public static class ABuildManager
    {
        public static event EventHandler OnBeforeBuild;
        public static event EventHandler OnAfterBuild;
        public static event EventHandler OnBeforeRunBuild;
        public static event EventHandler OnAfterRunBuild;

        public static void BuildAll(BuildGroups groups, BuildArchs arch, bool isDebug)
        {
            OnBeforeBuild?.Invoke(null, null);

            if (groups.HasFlag(BuildGroups.Windows))
                ABuildInstructions.WindowsBuild(arch.HasFlag(BuildArchs.x86_64), isDebug);

            if (groups.HasFlag(BuildGroups.Linux))
                ABuildInstructions.LinuxBuild(arch.HasFlag(BuildArchs.x86_64), isDebug);

            if (groups.HasFlag(BuildGroups.Mac))
                ABuildInstructions.MacBuild(arch.HasFlag(BuildArchs.x86_64), isDebug);

            OnAfterBuild?.Invoke(null, null);
        }

        public static void RunBuild(bool deepProfile = true)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                RunBuild(BuildGroups.Windows, deepProfile: deepProfile);
            else if (Application.platform == RuntimePlatform.LinuxEditor)
                RunBuild(BuildGroups.Linux, deepProfile: deepProfile);
        }

        public static void RunBuild(BuildGroups platform, bool deepProfile = true)
        {
            OnBeforeRunBuild?.Invoke(null, null);

            var p = new System.Diagnostics.Process();
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ABuildInstructions.GetBuildPath(platform))); 
            p.StartInfo.FileName = path;

            if (deepProfile)
                p.StartInfo.Arguments = "-deepprofiling";

            DateTime writeTime = System.IO.File.GetLastWriteTime(path);
            TimeSpan delta = DateTime.Now - writeTime;
            if (delta.Days > 1)
            {
                if (!UnityEditor.EditorUtility.DisplayDialog("Old Build Detected",
                                                             $"Last build was {delta.Days} days ago.\nProceed?",
                                                             "Yes", 
                                                             "No"))
                {
                    return;
                }
            }

            p.Start();

            OnAfterRunBuild?.Invoke(null, null);
        }
    }
}