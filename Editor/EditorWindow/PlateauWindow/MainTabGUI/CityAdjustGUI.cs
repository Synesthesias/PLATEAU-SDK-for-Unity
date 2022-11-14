using PLATEAU.CityAdjust;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAdjustGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel adjustTarget;
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("配置済みモデルデータの調整を行います。");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.adjustTarget =
                    (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                        "調整対象", this.adjustTarget,
                        typeof(PLATEAUInstancedCityModel), true);
                if (this.adjustTarget != null)
                {
                    if(PlateauEditorStyle.MainButton("重複した地物のうち、\nLODが最大のもののみ有効化"))
                    {
                        CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);            
                    }
                }
            }
        }
    }
}
