using PLATEAU.Udx;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector.SceneObjs
{
    /// <summary>
    /// <see cref="BoxGizmoDrawer"/> の <see cref="MeshCode"/> を保持する版です。
    /// </summary>
    public class MeshCodeGizmoDrawer : BoxGizmoDrawer
    {
        public MeshCode MeshCode { get; set; }

        protected override float SizeMultiplier => 0.98f;
        private const float crossHairSizeMultiplier = 0.2f;

        protected override void AdditionalGizmo()
        {
            #if UNITY_EDITOR
            // 追加でボックスの中心にクロスヘア（十字マーク）を描きます。
            var trans = transform;
            var center = trans.position;
            var crossHairLength = trans.localScale * crossHairSizeMultiplier;
            Gizmos.DrawLine(
                center + Vector3.left    *  crossHairLength.x * 0.5f,
                center + Vector3.right   * crossHairLength.x * 0.5f);
            Gizmos.DrawLine(
                center + Vector3.forward * crossHairLength.z * 0.5f,
                center + Vector3.back    * crossHairLength.z * 0.5f);
            #endif
        }
    }
}
