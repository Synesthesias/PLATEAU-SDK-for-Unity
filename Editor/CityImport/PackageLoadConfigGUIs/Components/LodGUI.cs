using System;
using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Components
{
    /// <summary>
    /// LOD範囲の設定GUIです。
    /// </summary>
    internal class LodGUI : PackageLoadConfigGUIComponent
    {
        public LodGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            var predefined = CityModelPackageInfo.GetPredefined(Conf.Package);
            int minLOD = Conf.LODRange.MinLOD;
            int maxLOD = Conf.LODRange.MaxLOD;
            int availableMaxLOD = Conf.LODRange.AvailableMaxLOD;
            PlateauEditorStyle.LODSlider("LOD描画設定", ref minLOD, ref maxLOD,
                Math.Min(predefined.minLOD, availableMaxLOD), availableMaxLOD);
            Conf.LODRange = new LODRange(minLOD, maxLOD, availableMaxLOD);
        }
    }
}