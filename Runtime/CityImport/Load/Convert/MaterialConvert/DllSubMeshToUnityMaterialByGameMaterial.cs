using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Load.Convert.MaterialConvert
{
    /// <summary>
    /// 共通ライブラリのSubMeshをUnityのマテリアルに変換する方法の1つを提供します。
    /// その方法とは、事前に記録したUnityのマテリアルをインデックスから復元します。
    /// </summary>
    internal class DllSubMeshToUnityMaterialByGameMaterial : IDllSubMeshToUnityMaterialConverter
    {
        /// <summary>
        /// 共通ライブラリに変換したときの情報を元にマテリアルを復元します。
        /// </summary>
        private readonly UnityMeshToDllSubMeshWithGameMaterial toDllMatConverter;

        public DllSubMeshToUnityMaterialByGameMaterial(UnityMeshToDllSubMeshWithGameMaterial toDllMatConverter)
        {
            this.toDllMatConverter = toDllMatConverter;
        }
        
        public async Task<Material> ConvertAsync(ConvertedMeshData meshData, int subMeshIndex, Material fallbackMaterial)
        {
            if (subMeshIndex < 0 || subMeshIndex >= meshData.GameMaterialIDs.Count)
            {
                return RenderUtil.CreateDefaultMaterial();
            }
            int gameMaterialID = meshData.GameMaterialIDs[subMeshIndex];
            
            if (gameMaterialID < 0)
            {
                return RenderUtil.CreateDefaultMaterial();
            }
            return toDllMatConverter.GameMaterials[gameMaterialID];
        }
    }
}