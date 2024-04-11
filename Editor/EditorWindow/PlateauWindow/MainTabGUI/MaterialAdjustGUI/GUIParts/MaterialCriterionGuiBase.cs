using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// マテリアル分けGuiの共通クラスです。
    /// </summary>
    internal abstract class MaterialCriterionGuiBase
    {
        protected MaterialAdjusterBase adjuster;
        
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。成功したかどうかをboolで返します。 </summary>
        public abstract bool Search(GameObject[] selectedObjs);

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
        public abstract void DrawAfterTargetSelect();
        
    }
}