using PLATEAU.Behaviour;
using PLATEAU.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// インポートされた都市の3Dモデルファイルをロードし、シーンに配置します。
    /// </summary>
    internal static class CityMeshPlacerToScene
    {
        /// <summary>
        /// <paramref name="objAssetPath"/> の3Dモデルをロードし、
        /// <paramref name="parentGameObjName"/> の子オブジェクトとしてシーンに配置します。
        /// 親ゲームオブジェクトには <see cref="CityBehaviour"/> をアタッチし、与えられた<see cref="CityMetaData"/> をリンクします。
        /// </summary>
        public static void Place(string objAssetPath, string parentGameObjName, CityMetaData metaData)
        {
            // 親を配置
            var parent = GameObject.Find(parentGameObjName);
            if (parent == null)
            {
                parent = new GameObject(parentGameObjName);
            }
            
            // 親に CityBehaviour をアタッチしてメタデータをリンク
            var cityBehaviour = parent.GetComponent<CityBehaviour>();
            if ( cityBehaviour == null)
            {
                cityBehaviour = parent.AddComponent<CityBehaviour>();
            }
            cityBehaviour.CityMetaData = metaData;

            var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);
            if (assetObj == null)
            {
                // TODO Errorのほうがいい（ユニットテストが通るなら）
                Debug.LogWarning($"Failed to load '.obj' file.\nobjAssetPath = {objAssetPath}");
                return;
            }
            
            // 古い同名の GameObject を削除
            var oldObj = FindRecursive(parent.transform, assetObj.name);
            if (oldObj != null)
            {
                Object.DestroyImmediate(oldObj.gameObject);
            }

            // 変換後モデルの配置
            var placedObj = (GameObject)PrefabUtility.InstantiatePrefab(assetObj);
            placedObj.name = assetObj.name;
            placedObj.transform.parent = parent.transform;
        }
        
        
        private static Transform FindRecursive(Transform target, string name)
        {
            if (target.name == name) return target;
            foreach (Transform child in target)
            {
                Transform found = FindRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}