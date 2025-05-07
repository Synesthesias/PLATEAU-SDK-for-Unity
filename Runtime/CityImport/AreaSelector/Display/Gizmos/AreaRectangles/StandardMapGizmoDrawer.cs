using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos.AreaRectangles
{
    /// <summary>
    /// 国土基本図郭の範囲を表示するギズモの描画クラスです。
    /// </summary>
    internal class StandardMapGizmoDrawer : GridCodeGizmoDrawer
    {
        private static readonly Color BoxColorStandardMap = new(0f, 0.7f, 0f);
        protected override Color BoxColorNormalLevel2 => BoxColorStandardMap;
        protected override Color BoxColorNormalLevel3 => BoxColorStandardMap;
        protected override Color HandleColor => new(1f, 250f / 255f, 203f / 255f);
        
        private readonly List<string> selectedMeshCodes = new ();
        
        protected override void ApplyStyle()
        {
            if (GridCode.IsNormalGmlLevel) // 通常のメッシュコード
            {
                LineWidth = LineWidthLevel3;
                BoxColor = BoxColorStandardMap;
                divideNumColumn = 1;
                divideNumRow = 1;
                return;
            }
            if (GridCode.IsSmallerThanNormalGml) // 小さいメッシュコード
            {
                LineWidth = LineWidthLevel4;
                BoxColor = BoxColorStandardMap;
                divideNumColumn = 1;
                divideNumRow = 1;
                return;
            }

            // 大きいメッシュコード
            LineWidth = LineWidthLevel2;
            BoxColor = BoxColorStandardMap;
            divideNumColumn = 1;
            divideNumRow = 1;
        }
        
        public void SetMeshCodeSelection(string meshCode, bool isSelected)
        {
            if (isSelected)
            {
                if (selectedMeshCodes.Contains(meshCode))
                {
                    return;
                }
                selectedMeshCodes.Add(meshCode);
                SetSelectArea(0, true);
            }
            else
            {
                if (!selectedMeshCodes.Contains(meshCode))
                {
                    return;
                }
                selectedMeshCodes.Remove(meshCode);
                if (selectedMeshCodes.Count == 0)
                {
                    SetSelectArea(0, false);
                }
            }
        }

        public override void ResetSelectedArea()
        {
            base.ResetSelectedArea();
            selectedMeshCodes.Clear();
        }
    }
}