using System;
using System.IO;
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

    public enum BuildMarketplaces
    {
        None = 0,
        Steam = 1,
        Epic = 2,
        Itch = 4,        
    }

    public static class ABuildManager
    {
        public static event EventHandler OnBeforeBuild;
        public static event EventHandler OnAfterBuild;
        public static event EventHandler OnBeforeRunBuild;
        public static event EventHandler OnAfterRunBuild;

        public static void BuildAll(
            BuildGroups groups, 
            BuildArchs arch, 
            bool isDebug, 
            bool deepProfile,
            bool detailedBuildReport)
        {
            OnBeforeBuild?.Invoke(null, null);

            if (groups.HasFlag(BuildGroups.Windows))
                ABuildInstructions.WindowsBuild(arch.HasFlag(BuildArchs.x86_64), isDebug, deepProfile, detailedBuildReport);

            if (groups.HasFlag(BuildGroups.Linux))
                ABuildInstructions.LinuxBuild(arch.HasFlag(BuildArchs.x86_64), isDebug, deepProfile, detailedBuildReport);

            if (groups.HasFlag(BuildGroups.Mac))
                ABuildInstructions.MacBuild(arch.HasFlag(BuildArchs.x86_64), isDebug, deepProfile, detailedBuildReport);

            OnAfterBuild?.Invoke(null, null);
        }

        public static void RunBuild()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                RunBuild(BuildGroups.Windows);
            else if (Application.platform == RuntimePlatform.LinuxEditor)
                RunBuild(BuildGroups.Linux);
        }

        public static void RunBuild(BuildGroups platform)
        {
            OnBeforeRunBuild?.Invoke(null, null);

            var p = new System.Diagnostics.Process();
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, ABuildInstructions.GetBuildPath(platform))); 
            p.StartInfo.FileName = path;

            TimeSpan delta = GetLastModifyDelta(path);
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

        public static string GetLastModifiedString(string path)
        {
            DateTime writeTime = GetLastModifyTime(path);
            TimeSpan delta = DateTime.Now - writeTime;
            if (delta.Days > 1) return $"{delta.Days} days";
            if (delta.Hours > 1) return $"{delta.Hours} hours";
            return $"{delta.Minutes} minutes";
        }

        public static TimeSpan GetLastModifyDelta(string path)
        {
            DateTime writeTime = GetLastModifyTime(path);
            return DateTime.Now - writeTime;
        }

        private static DateTime GetLastModifyTime(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                //Debug.LogError($"{path} does not exist.");
                return default;
            }

            DateTime lastMod = default;
            foreach (string s in Directory.GetFileSystemEntries(dir))
            {
                DateTime thisMod = default;
                if (Directory.Exists(s))
                {
                    thisMod = Directory.GetLastWriteTime(s);
                }
                else if (File.Exists(s))
                {
                    thisMod = File.GetLastWriteTime(s);
                }
                
                if (lastMod == default || thisMod > lastMod) lastMod = thisMod;                
            }
            return lastMod;
        }
    }
}