using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// ゲームオブジェクトの名称からPlateauの <see cref="CityGML.CityObject"/> を返す MonoBehaviour です。
    /// 実行には <see cref="CityMetadata"/> を保持する必要があります。
    /// </summary>
    public class CityBehaviour : MonoBehaviour
    {
        [SerializeField] private CityMetadata cityMetadata;
        private readonly CityModelLoader loader = new CityModelLoader();

        /// <summary> ゲームオブジェクトとgmlファイルを対応付けるために必要な <see cref="CityMetadata"/> です。 </summary>
        public CityMetadata CityMetadata
        {
            get => this.cityMetadata;
            set => this.cityMetadata = value;
        }

        /// <summary>
        /// ゲームオブジェクト名 から <see cref="CityGML.CityObject"/> を返します。
        /// </summary>
        /// <param name="gameObjName">ゲームオブジェクト名</param>
        /// <returns>引数に対応する<see cref="CityGML.CityObject"/></returns>
        public CityObject LoadCityObject(string gameObjName)
        {
            return this.loader.Load(gameObjName, this.cityMetadata);
        }
    }
}