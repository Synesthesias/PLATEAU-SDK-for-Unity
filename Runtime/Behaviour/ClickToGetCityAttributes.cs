using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    public class ClickToGetCityAttributes : MonoBehaviour
    {
        [SerializeField] private Transform gridParent;
        private Camera mainCamera;

        private void Awake()
        {
            InitMembers();
            AttachMeshCollidersToCity();
        }
        
        private void Update()
        {
            if (Input.GetMouseButton(0)) // マウスクリック時
            {
                if (CastMouseRay(out var uv2))
                {
                    Debug.Log(uv2);
                }
            }
        }

        private void InitMembers()
        {
            if (this.gridParent == null)
            {
                Debug.LogError($"{nameof(this.gridParent)} is null.");
            }

            this.mainCamera = Camera.main;
            if (this.mainCamera == null)
            {
                Debug.LogError("main camera is not found.");
            }
        }

        private void AttachMeshCollidersToCity()
        {
            int numChild = this.gridParent.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var child = this.gridParent.GetChild(i);
                GameObjectUtil.AssureComponent<MeshCollider>(child.gameObject);
            }
        }
        
        /// <summary>
        /// マウスカーソルの位置にレイを飛ばし、ヒット情報を返します。
        /// 当たったかどうかをboolで返し、当たった場合はその位置の UV を out引数で返します。
        /// </summary>
        private bool CastMouseRay(out Vector2 hitUv2)
        {
            Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                hitUv2 = hit.textureCoord2;
                return true;
            }

            hitUv2 = Vector2.negativeInfinity;
            return false;
        }
    }
}