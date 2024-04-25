using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Util.Async;
using System;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分けを実行するボタンです。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    internal class MAExecButton : Element
    {
        private readonly CityMaterialAdjustGUI adjustGui;
        public MAExecButton(CityMaterialAdjustGUI adjustGui)
        {
            this.adjustGui = adjustGui;
        }
        
        public override void DrawContent()
        {
            if (PlateauEditorStyle.MainButton("実行"))
            {
                var granularity = adjustGui.Guis.Get<MAGranularityGui>().Granularity;
                
                // GUIから設定値を取得します。
                var executorConf = adjustGui.SelectedCriterion switch
                {
                
                    MaterialCriterion.ByType => new MAExecutorConf(
                        adjustGui.CurrentSearcher.MaterialAdjustConf,
                        adjustGui.Guis.Get<ObjectSelectGui>().SelectedTransforms,
                        granularity,
                        adjustGui.Guis.Get<DestroyOrPreserveSrcGui>().DoDestroySrcObjs,
                        adjustGui.Guis.Get<NameSelectGui>().Name,
                        adjustGui.Guis.Get<ToggleLeftElement>("skipNotChangingMaterial").Value,
                        new MAConditionSimple()),
                    
                    MaterialCriterion.ByAttribute => new MAExecutorConfByAttr(
                        adjustGui.CurrentSearcher.MaterialAdjustConf,
                        adjustGui.Guis.Get<ObjectSelectGui>().SelectedTransforms,
                        granularity,
                        adjustGui.Guis.Get<DestroyOrPreserveSrcGui>().DoDestroySrcObjs,
                        adjustGui.Guis.Get<NameSelectGui>().Name,
                        adjustGui.Guis.Get<ToggleLeftElement>("skipNotChangingMaterial").Value,
                        new MAConditionSimple(),
                        adjustGui.Guis.Get<AttributeKeyGui>().AttrKey
                        ),
                    
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                // マテリアル分けインスタンスを作ります。
                var executor = adjustGui.SelectedCriterion switch
                {
                    MaterialCriterion.ByType => MAExecutorFactory.CreateTypeExecutor(executorConf),
                    MaterialCriterion.ByAttribute => MAExecutorFactory.CreateAttrExecutor((MAExecutorConfByAttr)executorConf),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                // 実行
                executor.Exec().ContinueWithErrorCatch();
            }
        }

        public override void Dispose()
        {
            
        }
    }
}