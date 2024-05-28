using PLATEAU.Editor.Window.Common;
using PLATEAU.Util.Async;
using System.Threading.Tasks;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
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
            if (PlateauEditorStyle.MainButton("マテリアル分けを実行"))
            {
                Exec().ContinueWithErrorCatch();
            }
        }

        /// <summary> 実行 </summary>
        private async Task Exec()
        {
            var executor = adjustGui.GenerateExecutor();
            var result = await executor.Exec();
            adjustGui.ReceiveMAResult(result);
        }

        public override void Dispose()
        {
            
        }
    }
}