using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityImport.Load.Convert.MaterialConvert
{
    internal interface IDllSubMeshToUnityMaterialConverter
    {
        Task<Material> ConvertAsync(ConvertedMeshData meshData, int subMeshIndex, Material fallbackMaterial);
    }
}