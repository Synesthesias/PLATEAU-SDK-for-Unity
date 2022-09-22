using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector.SceneObjs
{
    /// <summary>
    /// 自身の transform に応じた箱型のギズモを表示します。
    /// </summary>
    public class BoxGizmoDrawer : HandlesBase
    {
        public Color BoxColor { get; set; } = Color.white;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var prevColor = Gizmos.color;
            Gizmos.color = this.BoxColor;
            var trans = transform;
            var centerPos = trans.position;
            var size = trans.localScale;
            Gizmos.DrawWireCube(centerPos, size);
            Gizmos.color = prevColor;
        }
#endif
        protected override void OnSceneGUI(SceneView sceneView)
        {
            
        }
        
        public static Vector3 AreaMax(Vector3 center, Vector3 size)
        {
            return center + size / 2.0f;
        }

        public static Vector3 AreaMin(Vector3 center, Vector3 size)
        {
            return center - size / 2.0f;
        }

        /// <summary>
        /// Y軸の値は無視して、XとZの値で箱同士が重なる箇所があるかどうかを bool で返します。
        /// </summary>
        public bool IsBoxIntersectXZ(BoxGizmoDrawer other)
        {
            var trans = transform;
            var pos = trans.position;
            var size = trans.localScale;

            var otherTrans = other.transform;
            var otherPos = otherTrans.position;
            var otherSize = otherTrans.localScale;

            return
                Math.Abs(pos.x - otherPos.x) <= (Math.Abs(size.x) + Math.Abs(otherSize.x)) * 0.5 &&
                Math.Abs(pos.z - otherPos.z) <= (Math.Abs(size.z) + Math.Abs(otherSize.z)) * 0.5;
        }
    }
}
