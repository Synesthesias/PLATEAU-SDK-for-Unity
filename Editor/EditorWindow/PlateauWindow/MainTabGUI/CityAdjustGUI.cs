using System.Threading.Tasks;
using ICSharpCode.NRefactory.Ast;
using PLATEAU.CityAdjust;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAdjustGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel adjustTarget;
        private CityObjectTypeSelectGUI typeSelectGUI = new CityObjectTypeSelectGUI();
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
            PlateauEditorStyle.Separator(0);

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.SubTitle("フィルタリング");
                EditorGUILayout.LabelField("条件に応じてゲームオブジェクトのON/OFFを切り替えます。");
                
                this.typeSelectGUI.Draw();
                
                if (PlateauEditorStyle.MainButton("フィルタリング実行"))
                {
                    
                    if (this.adjustTarget == null)
                    {
                        EditorUtility.DisplayDialog("PLATEAU", "対象を指定してください。", "OK");
                        return;
                    }

                    CityFilterByCityObjectType.FilterAsync(this.adjustTarget, this.typeSelectGUI.SelectionDict)
                        .ContinueWithErrorCatch()
                        .ContinueWith((Task _) => EditorUtility.DisplayDialog("PLATEAU", "フィルタリングが完了しました。", "OK"));
                }
            }
        }
    }
}
