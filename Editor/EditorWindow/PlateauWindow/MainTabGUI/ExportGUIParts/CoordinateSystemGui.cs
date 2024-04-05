using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Geometries;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// エクスポートにおいて、座標軸の向きを選択するGUIです。
    /// </summary>
    public class CoordinateSystemGui
    {
        private static readonly List<CoordinateSystem> MeshAxisChoices = ((CoordinateSystem[])Enum.GetValues(typeof(CoordinateSystem))).ToList();
        public CoordinateSystem SelectedCoordinateSystem { get; private set; } = CoordinateSystem.ENU;
        private static readonly string[] MeshAxisDisplay = MeshAxisChoices.Select(axis => axis.ToNaturalLanguage()).ToArray();

        public void Draw()
        {
            this.SelectedCoordinateSystem = MeshAxisChoices[EditorGUILayout.Popup("座標軸", MeshAxisChoices.IndexOf(this.SelectedCoordinateSystem), MeshAxisDisplay)];
        }
    }
}