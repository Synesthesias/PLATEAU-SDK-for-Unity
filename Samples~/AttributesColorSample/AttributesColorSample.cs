using System.Collections;
using System.Collections.Generic;
using PLATEAU.CityInfo;
using UnityEngine;

/// <summary>
/// ランタイムで、PLATEAUの属性情報に応じて色を分けるサンプルです。
/// </summary>
public class AttributesColorSample : MonoBehaviour
{
    /// <summary> 色分けしたいターゲットを指定します。 </summary>
    [SerializeField] private Transform targetParent;
    private void Start()
    {
        // PLATEAUCityObjectGroupコンポーネントに属性情報が格納されており、ランタイムで読み込むことができます。
        var cityObjGroups = targetParent.GetComponentsInChildren<PLATEAUCityObjectGroup>();
        foreach(var cityObjGroup in cityObjGroups)
        {
            var target = cityObjGroup.transform;
            foreach (var cityObj in cityObjGroup.GetAllCityObjects())
            {
                // 属性情報（キーバリューペア）を取得します。
                var attributes = cityObj.AttributesMap;
                if (!attributes.TryGetValue("urf:function", out var landFuncAttr)) continue;
                string landFuncName = landFuncAttr.StringValue;
                var color = ColorByLandFuncName(landFuncName);
                ChangeMaterialByColor(target, color);
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
