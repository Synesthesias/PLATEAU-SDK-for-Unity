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
                var executor = adjustGui.GenerateExecutor();
                
                // 実行
                executor.Exec().ContinueWithErrorCatch();
            }
        }

        public override void Dispose()
        {
            
        }
    }
}