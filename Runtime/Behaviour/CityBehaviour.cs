using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// ゲームオブジェクトからPlateauの <see cref="CityGML.CityObject"/> を返す MonoBehaviour です。
    /// 実行のために <see cref="CityMetadata"/> と <see cref="CityModelLoader"/> を保持します。
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
        /// ゲームオブジェクト から <see cref="CityGML.CityObject"/> を返します。
        /// </summary>
        /// <param name="gameObj"><see cref="CityObject"/>に対応するゲームオブジェクト</param>
        /// <returns>引数に対応する<see cref="CityGML.CityObject"/></returns>
        public CityObject LoadCityObject(GameObject gameObj)
        {
            return this.loader.Load(gameObj, this.cityMetadata);
        }

        /// <summary>
        /// 再帰的にヒエラルキーの子を探索し、 <see cref="CityObject"/> に対応付けできるものをすべて <see cref="CityObject"/> にして
        /// IEnumerable で返します。
        /// 順番は DFS (深さ優先探索)です。
        /// </summary>
        public IEnumerable<CityObject> ChildCityObjsOnHierarchyDfs()
        {
            // minDepth = 2 である理由 : 自身はルートなのでスキップし、直近の子は gmlファイル名 が付いた空のゲームオブジェクトなのでスキップします。
            var cityObjs = CityHierarchyEnumerator.ChildrenDfsAsCityObjects(transform, 2, this.loader, CityMetadata);
            foreach (var co in cityObjs)
            {
                yield return co;
            }
        }

        /// <summary>
        /// 再帰的にヒエラルキーの子を列挙します。
        /// 順番は DFS (深さ優先探索)です。
        /// </summary>
        public IEnumerable<GameObject> ChildGameObjsOnHierarchyDfs()
        {
            var gameObjs = CityHierarchyEnumerator.ChildrenDfsAsTransform(transform, 0)
                .Select(trans => trans.gameObject);
            foreach (var go in gameObjs)
            {
                yield return go;
            }
        }

    }
}