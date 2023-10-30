using PLATEAU.CityImport.Config.PackageLoadConfigs;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables.Components
{
    /// <summary>
    /// MeshColliderをセットするかどうかの設定GUIです。
    /// </summary>
    internal class MeshColliderSetGUI : ExtendableConfigGUIBase
    {

        public MeshColliderSetGUI(PackageLoadConfigExtendable conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetMeshCollider =
                EditorGUILayout.Toggle("Mesh Collider をセットする", Conf.DoSetMeshCollider);
        }

    }
}