using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Import.Convert.MaterialConvert
{
    /// <summary>
    /// 共通ライブラリのSubMeshをUnityのマテリアルに変換する方法の1つを提供します。
    /// その方法とは、<see cref="GameMaterialIDRegistry"/>とゲームマテリアルIDからマテリアルを探して復元んします。
    /// </summary>
    internal class RecoverFromGameMaterialID : IDllSubMeshToUnityMaterialConverter
    {
        /// <summary>
        /// 共通ライブラリに変換したときの情報を元にマテリアルを復元します。
        /// </summary>
        private readonly GameMaterialIDRegistry toDllMatConverter;

        public RecoverFromGameMaterialID(GameMaterialIDRegistry toDllMatConverter)
        {
            this.toDllMatConverter = toDllMatConverter;
        }
        
        public Task<Material> ConvertAsync(ConvertedMeshData meshData, int subMeshIndex, Material fallbackMaterial)
        {
            // このメソッドは同期的に実行可能ですが、インターフェイスの戻り値型に合わせるために戻り値をTaskにします。
            
            if (subMeshIndex < 0 || subMeshIndex >= meshData.GameMaterialIDs.Count)
            {
                return Task.FromResult(RenderUtil.CreateDefaultMaterial());
            }
            int gameMaterialID = meshData.GameMaterialIDs[subMeshIndex];
            
            if (gameMaterialID < 0)
            {
                return Task.FromResult(RenderUtil.CreateDefaultMaterial());
            }
            return Task.FromResult(toDllMatConverter.GameMaterials[gameMaterialID]);
        }
    }
}