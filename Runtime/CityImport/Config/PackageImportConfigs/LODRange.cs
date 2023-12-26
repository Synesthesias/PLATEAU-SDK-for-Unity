using System;

namespace PLATEAU.CityImport.Config.PackageImportConfigs
{
    public struct LODRange
    {
        /// <summary> ユーザーが選択したLOD範囲の下限 </summary>
        public int MinLOD { get; }

        /// <summary> ユーザーが選択したLODの上限 </summary>
        public int MaxLOD { get; }

        /// <summary> ユーザーが選択したデータのなかで存在するLODの最大値 </summary>
        public int AvailableMaxLOD { get; }

        public LODRange(int minLOD, int maxLOD, int availableMaxLOD)
        {
            if (minLOD > maxLOD) throw new ArgumentException("Condition minLOD <= maxLOD does not meet.");
            MinLOD = minLOD;
            MaxLOD = maxLOD;
            AvailableMaxLOD = availableMaxLOD;
        }
    }
}