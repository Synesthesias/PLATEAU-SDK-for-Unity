using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.SemanticsLoad;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// ゲームオブジェクトの名称からPlateauの <see cref="CityGML.CityObject"/> を返す MonoBehaviour です。
    /// 実行には <see cref="CityMeta.CityMetaData"/> を保持する必要があります。
    /// </summary>
    public class CityBehaviour : MonoBehaviour
    {
        [SerializeField] private CityMetaData cityMetaData;
        private readonly SemanticsLoader loader = new SemanticsLoader();

        /// <summary> ゲームオブジェクトとgmlファイルを対応付けるために必要な <see cref="CityMeta.CityMetaData"/> です。 </summary>
        public CityMetaData CityMetaData
        {
            get => this.cityMetaData;
            set => this.cityMetaData = value;
        }

        /// <summary>
        /// ゲームオブジェクト名 → <see cref="CityGML.CityObject"/>
        /// </summary>
        /// <param name="gameObjName">ゲームオブジェクト名</param>
        /// <returns>引数に対応する<see cref="CityGML.CityObject"/></returns>
        public CityObject LoadSemantics(string gameObjName)
        {
            return this.loader.Load(gameObjName, this.cityMetaData);
        }
    }
}