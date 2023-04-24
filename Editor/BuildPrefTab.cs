using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aleth.Prefs;
using AlethEditor.Build;
using AlethEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace AlethEditor.Prefs
{
    public class BuildPrefTab : APrefTab
    {
        private readonly List<Type> _optionsTypes = new List<Type>() {typeof(BuildPackagePrefGroup) };
        private readonly List<Type> _buildTypes = new List<Type>() { typeof(BuildOptionsPrefGroup) };
        private readonly List<Type> _demoTypes = new List<Type>() { typeof(DemoBuildOptionsPrefGroup) };

        protected override List<Type> GroupTypes 
        { 
            get 
            
            {
                //return _optionsTypes.Union(_buildTypes).Union(_demoTypes).ToList();
                if (ABuildPrefs.ShowDemoBuild)
                    return _optionsTypes.Union(_demoTypes).ToList();
                return _optionsTypes.Union(_buildTypes).ToList();
            } 
        }

        public override string DisplayName { get { return "Aleth Build"; } }
        public override int Priority { get { return 200; } }
    }
}
