using System;
using PlateauUnitySDK.Editor.FileConverter.Converters;

namespace PlateauUnitySDK.Editor.CityModelImportWindow;

[Serializable]
public class GmlSelectorConfig
{
    public string[] areaIds = {};
    public bool[] isAreaIdTarget = {};
    public GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

    public void SetAllAreaId(bool isTarget)
    {
        for (int i = 0; i < this.isAreaIdTarget.Length; i++)
        {
            this.isAreaIdTarget[i] = isTarget;
        }
    }
}