using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// マテリアル分けの対象検索と設定、実行を行うGUIです。
    /// 検索の基準は、コンストラクタへの注入によって指定します。具体的には:
    /// 地物型で検索する場合はコンストラクタに<see cref="MaterialAdjusterByType"/>を渡し、
    /// 属性情報で検索する場合はコンストラクタに<see cref="MaterialAdjusterByAttr"/>を渡します。
    /// </summary>
    internal class MaterialSearchAndExecGui
    {
        private MaterialAdjusterBase adjuster;
        public bool IsSearched { get; set; }

        public MaterialSearchAndExecGui(MaterialAdjusterBase adjuster)
        {
            this.adjuster = adjuster;
        }
        
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。成功したかどうかをboolで返します。 </summary>
        public bool Search(MaterialAdjusterBase.SearchArg searchArg)
        {
            adjuster.InitBySearch(searchArg); // ここで検索します。
            if (adjuster.MaterialAdjustConf.Length <= 0)
            {
                Dialogue.Display("対象が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                return false;
            }
            IsSearched = true;
            return true;
        }

        /// <summary> マテリアル変換の設定をセットします。 </summary>
        public void SetConfig(MeshGranularity granularity, bool doDestroySrcObjs)
        {
            adjuster.Granularity = granularity;
            adjuster.DoDestroySrcObjects = doDestroySrcObjs;
        }

        public MeshGranularity GetGranularity() => adjuster.Granularity;
        

        /// <summary> 対象を選択して「検索」ボタンを押したあとのGUIです。  </summary>
        public void DrawAfterTargetSelect()
        {
            MaterialConfGui.Draw(adjuster.MaterialAdjustConf);

            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                adjuster.Exec().ContinueWithErrorCatch(); // ここで実行します。
            }
        }
        
    }
}