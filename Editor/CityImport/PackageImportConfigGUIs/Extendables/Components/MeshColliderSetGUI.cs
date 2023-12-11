using PLATEAU.CityImport.Config.PackageImportConfigs;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components
{
    /// <summary>
    /// MeshColliderをセットするかどうかの設定GUIです。
    /// </summary>
    internal class MeshColliderSetGUI : ExtendableConfigGUIBase
    {

        public MeshColliderSetGUI(PackageImportConfigExtendable conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetMeshCollider =
                EditorGUILayout.Toggle("Mesh Collider をセットする", Conf.DoSetMeshCollider);
        }

    }
}