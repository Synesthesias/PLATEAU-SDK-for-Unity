using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// マテリアル分けGuiの共通クラスです。
    /// </summary>
    internal abstract class MaterialCriterionGuiBase
    {
        private MaterialAdjusterBase adjuster;

        public MaterialCriterionGuiBase(MaterialAdjusterBase adjuster)
        {
            this.adjuster = adjuster;
        }
        
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。成功したかどうかをboolで返します。 </summary>
        public bool Search(GameObject[] selectedObjs)
        {
            adjuster.InitBySearch(selectedObjs); // ここで検索します。
            if (adjuster.MaterialAdjustConf.Length <= 0)
            {
                Dialogue.Display("地物型が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                return false;
            }

            return true;
        }

        /// <summary> マテリアル変換の設定をセットします。 </summary>
        public void SetConfig(MeshGranularity granularity, bool doDestroySrcObjs)
        {
            adjuster.Granularity = granularity;
            adjuster.DoDestroySrcObjects = doDestroySrcObjs;
        }

        public MeshGranularity GetGranularity() => adjuster.Granularity;

        /// <summary> 「検索」ボタンを押す前のGUIです。 </summary>
        public abstract void DrawBeforeTargetSelect();

        /// <summary> 対象を選択して「検索」ボタンを押したあとのGUIです。  </summary>
        public void DrawAfterTargetSelect()
        {
            MaterialByTypeConfGui.Draw(adjuster.MaterialAdjustConf);

            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                adjuster.Exec().ContinueWithErrorCatch(); // ここで実行します。
            }
        }
        
    }
}