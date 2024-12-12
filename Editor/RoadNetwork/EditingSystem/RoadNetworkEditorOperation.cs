using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 内部システムが利用するインターフェイス
    /// </summary>
    internal interface IRoadNetworkEditingSystemInterface
    {
        //public RoadNetworkUIDoc UIDocEditor { get; }
        public IRoadNetworkEditOperation NetworkOperator { get; }
        public RoadNetworkSceneGUISystem SceneGUISystem { get; }
    }


    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        EditTrafficRegulation, // 交通規制編集
        EditRoadStructure, // 道路構造編集
    }

    /// <summary>
    /// 道路ネットーワークの編集機能を提供するインターフェイス
    /// 整備予定　このクラスを使ったり使わなかったりする。
    /// 実装コストからそのまま実装を直書きしている箇所がある、データが更新された際の処理を統一したいので（通知を飛ばす対象など
    /// </summary>
    public interface IRoadNetworkEditOperation
    {
        /// <summary>
        /// ポイントの追加、削除、移動
        /// </summary>
        /// <param name="way"></param>
        /// <param name="idx"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        bool AddPoint(RnWay parent, int idx, RnPoint point);

        bool RemovePoint(RnWay parent, RnPoint point);
    }

    /// <summary>
    /// RoadNetwork編集機能のデータ操作部分の機能を提供するクラス
    /// EditingSystemのサブクラスに移動するかも
    /// </summary>
    public class RoadNetworkEditorOperation : IRoadNetworkEditOperation
    {
        public bool AddPoint(RnWay way, int idx, RnPoint point)
        {
            //var v = new RoadNetworkPoint(new Vector3());
            way.LineString.Points.Insert(idx, point);
            return true;
        }

        public bool RemovePoint(RnWay way, RnPoint point)
        {
            var isSuc = way.LineString.Points.Remove(point);
            if (isSuc)
            {
                return true;
            }
            else
            {
                Debug.Log("Can't remove point.");
                return false;
            }
        }
    }
}