using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// マテリアル分けGuiの共通クラスです。
    /// </summary>
    public interface IMaterialGui
    {
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。 </summary>
        public void Search(GameObject[] selectedObjs);

        /// <summary> マテリアル変換の設定をセットします。 </summary>
        public void SetConfig(MeshGranularity granularity, bool doDestroySrcObjs);

        public MeshGranularity GetGranularity();

        /// <summary> 対象を選択して「検索」ボタンを押したあとのGUIです。  </summary>
        public void DrawAfterTargetSelect();
        
    }
}