using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.Window.Common;

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
                PlateauEditorStyle.Toggle("Mesh Collider をセットする", Conf.DoSetMeshCollider);
        }

    }
}