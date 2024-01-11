using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.Samples.AttributesColorSample
{
    /// <summary>
    /// ランタイムで、PLATEAUの属性情報に応じて色を分けるサンプルです。
    /// </summary>
    public class AttributesColorSample : MonoBehaviour
    {
        /// <summary> 色分けしたいターゲットを指定します。 </summary>
        [SerializeField] private Transform targetParent;
        private void Awake()
        {
            // PLATEAUCityObjectGroupコンポーネントに属性情報が格納されており、ランタイムで読み込むことができます。
            var cityObjGroups = targetParent.GetComponentsInChildren<PLATEAUCityObjectGroup>();
            foreach(var cityObjGroup in cityObjGroups)
            {
                var target = cityObjGroup.transform;
                foreach (var cityObj in cityObjGroup.GetAllCityObjects())
                {
                    // 属性情報（キーバリューペアが集まったもの）を取得します。
                    var attributes = cityObj.AttributesMap;
                
                    // 属性情報のうち、土地計画上の区分を取得して色分けします。
                    if (attributes.TryGetValue("urf:function", out var landFuncAttr))
                    {
                        string landFuncName = landFuncAttr.StringValue;
                        var color = ColorByLandFuncName(landFuncName);
                        ChangeMaterialByColor(target, color);
                    }
                
                    // 属性情報のうち、水害時の想定浸水高さを取得します。
                    if (attributes.TryGetValue("uro:floodingRiskAttribute", out var disasterRiskAttr))
                    {
                        if (disasterRiskAttr.AttributesMapValue.TryGetValue("uro:rank", out var depthValue))
                        {
                            var rank = depthValue.StringValue;
                            var color = ColorByFloodingRank(rank);
                            ChangeMaterialByColor(target, color);
                        }
                    }
                
                }
            }
        }

        private Color ColorByLandFuncName(string landFuncName)
        {
            Color matColor = Color.white;
        
            if (landFuncName.Contains("第1種住居地域"))
            {
                matColor = new Color(0.1f, 0.8f, 0.1f);
            }
            else if (landFuncName.Contains("第2種住居地域"))
            {
                matColor = new Color(0.3f, 0.7f, 0.9f);
            }
            else if (landFuncName.Contains("商業地域"))
            {
                matColor = new Color(0.8f, 0.8f, 0.5f);
            }
            else if (landFuncName.Contains("高度地区"))
            {
                matColor = new Color(0.8f, 0.5f, 0.5f);
            }
            else if (landFuncName.Contains("準工業地域"))
            {
                matColor = new Color(0.8f, 0.5f, 0f);
            }
            else if (landFuncName.Contains("準防火地域"))
            {
                matColor = new Color(0.9f, 0.1f, 0.4f);
            }
            else if (landFuncName.Contains("防火地域"))
            {
                matColor = new Color(0.9f, 0.2f, 0.2f);
            }

            return matColor;
        }

        private Color ColorByFloodingRank(string rank)
        {
            Color matColor = Color.white;
            if (rank == "0.5m未満")
            {
                matColor = new Color(0f, 0f, 1f);
            }
            else if (rank == "0.5m以上3m未満")
            {
                matColor = new Color(1.0f, 1.0f, 0f);
            }
            else if (rank == "3m以上5m未満")
            {
                matColor = new Color(1.0f, 0f, 0f);
            }

            return matColor;
        }

        private void ChangeMaterialByColor(Transform target, Color color)
        {
            var meshRenderer = target.GetComponent<MeshRenderer>();
            if (meshRenderer == null) return;
            var material = new Material(meshRenderer.material)
            {
                color = color
            };
            meshRenderer.material = material;
        }
    

    }
}
