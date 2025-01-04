using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路ネットワークの信号情報編集です。
    /// 作りかけであり、今は利用されていません。
    /// </summary>
    public class RoadNetworkEditingTrafficSignal
    {
        private const float signalLightHndScaleFactor = 0.2f;
        private const float signalControllerScaleFactor = 0.3f;
        
        public static void RoadNetworkTrafficSignalLightCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    break;
                case EventType.Repaint:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    break;
            }
        }
        
        private static void RoadNetworkNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
                    break;
                case EventType.Repaint:
                    Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
                    break;
            }
        }

        private static void RoadNetworkLinkHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    //Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    Handles.DrawWireCube(position, new Vector3(size, size, size));
                    var subCubeSize = size * signalControllerScaleFactor;
                    Handles.DrawWireCube(position + Vector3.right * subCubeSize,
                        new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawWireCube(position + Vector3.left * subCubeSize,
                        new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawLine(position + Vector3.right * size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor,
                        position + Vector3.left * size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor);
                    break;
            }
        }

        private static void RoadNetworkLaneHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireCube(position, new Vector3(size, size, size));
                    Handles.DrawWireCube(position,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * signalControllerScaleFactor));
                    break;
            }
        }

        private static void RoadNetworkSplitLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * 0.15f));
                    Handles.DrawWireCube(position + Vector3.back * 0.07f,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * 0.15f));
                    break;
            }
        }
        

        private static void RoadNetworkRemoveLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * 0.15f));
                    break;
            }
        }
    }
}