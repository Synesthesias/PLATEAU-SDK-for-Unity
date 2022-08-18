using System;
using System.Threading.Tasks;
using PLATEAU.CityGrid;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityGrid
{
    [CustomEditor(typeof(CityGridLoader))]
    public class CityGridLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var gridLoader = (CityGridLoader)target;
            base.OnInspectorGUI();
            if (PlateauEditorStyle.MainButton("ロード"))
            {
                var task = gridLoader.Load();
                task.ContinueWith(t =>
                {
                    if (t.Exception is { } age)
                    {
                        var inner = age.InnerException;
                        if (inner == null) return;
                        Debug.LogError($"{inner.Message}\n{inner.StackTrace}");
                    }
                });
            }
        }     
    }
}