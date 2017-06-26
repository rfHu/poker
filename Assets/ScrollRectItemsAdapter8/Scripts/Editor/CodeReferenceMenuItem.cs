using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Editor
{
    class CodeReferenceMenuItem
    {
        [MenuItem("frame8/Optimized ScrollView Adapter/Code reference")]
        public static void OpenDoc()
        { Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/doc"); }
    }
}
