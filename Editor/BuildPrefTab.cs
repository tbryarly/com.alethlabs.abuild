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
    public class BuildPrefTab : APrefTab
    {
        private readonly List<Type> _groupTypes = new List<Type>() { typeof(BuildPackagePrefGroup), typeof(BuildOptionsPrefGroup) };
        protected override List<Type> GroupTypes { get { return _groupTypes; } }

        public override string DisplayName { get { return "Aleth Build"; } }
        public override int Priority { get { return 200; } }
    }
}
