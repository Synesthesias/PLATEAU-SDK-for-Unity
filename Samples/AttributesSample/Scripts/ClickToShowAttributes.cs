using System.IO;
using System.Text;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Samples.Scripts
{
    /// <summary>
    /// クリック位置にレイキャストして、当たった都市オブジェクトの属性を Canvas に表示します。
    /// </summary>
    public class ClickToShowAttributes : MonoBehaviour
    {
        [SerializeField] private AttributesDisplay display;
        private Camera mainCamera;

        private void Start()
        {
            this.mainCamera = Camera.main;
            if (this.mainCamera == null)
            {
                Debug.LogError("メインカメラがありません。");
            }

            string samplePath = PathUtil.SdkPathToAssetPath("Samples");
            if (!Directory.Exists(samplePath)) samplePath = PathUtil.SdkPathToAssetPath("Samples~/");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // 左クリックで
            {
                var ray = this.mainCamera.ScreenPointToRay(Input.mousePosition); // クリック位置に
                if (Physics.Raycast(ray, out var hit)) // レイキャストして当たったとき
                {
                    var cityObjGroup = hit.transform.GetComponent<PLATEAUCityObjectGroup>(); // その都市オブジェクトを取得します。
                    if (cityObjGroup != null)
                    {
                        PrintAttributesToCanvas(cityObjGroup);
                    }
                }
            }
        }

        /// <summary>
        /// 都市オブジェクトの属性を取得して表示します。
        /// </summary>
        private void PrintAttributesToCanvas(PLATEAUCityObjectGroup cityObjGroup)
        {
            // インポートされた各ゲームオブジェクトには PLATEAUCityObjectGroup コンポーネントがアタッチされており、
            // その中に属性情報が json で保存されています。 json をデシリアライズします。
            var deserializedObjs = cityObjGroup.DeserializedCityObjects;
            // 複数の PLATEAU 地物が結合されて1つのゲームオブジェクトになっている場合があるため、
            // 1つのゲームオブジェクトから取得できる cityObjects の数は1つまたは複数になっています。
            var cityObjParams = deserializedObjs.cityObjects;
            var attributesSb = new StringBuilder();
            foreach (var cityObjParam in cityObjParams)
            {
                attributesSb.Append(cityObjParam.DebugString());
                attributesSb.Append("\n");
            }

            this.display.AttributesText = attributesSb.ToString();


            // // 住所の市を取得します。
            // // AttributesMap.GetValueOrNull を使うと、属性のキーに対応する値である AttributeValue を取得できます。
            // // AttributeValue は、値として 文字列 または 子のAttributesMap のどちらか1つを保持します。
            // // AsAttrSet で 子AttributesMap を取得します。 AsString で文字列を取得します。
            // // AsDouble, AsInt というメソッドもあります。これは内部的には文字列であるものをパースしたものを返します。
            // string cityName = cityObj
            //     .AttributesMap // 属性の辞書を取得します。
            //     .GetValueOrNull("uro:buildingDetails") // 辞書のうち キーに対応する AttributeValue を取得します。 
            //     ?.AsAttrSet // AttributeValue のデータの実体は AsString か AsAttrSet のどちらかで取得できます。今回は子の属性辞書を取得します。
            //     ?.GetValueOrNull("uro:city") // 子AttributesMap について、キーに対応する AttributeValue を取得します。
            //     ?.AsString; // 値として市の名称が入っているので取得します。
            //
            // // 属性から高さを取得します。
            // double? height = cityObj.AttributesMap
            //     .GetValueOrNull("高さ")
            //     ?.AsDouble; // AttributeValue.AsString を double にパースしたものを返します。
            //
            // // 取得した属性値を表示します。
            // this.display.TitleText = $"[{cityName ?? "No data"}]   高さ: {height?.ToString() ?? "No data"}\nID: {cityObjID}";
            //
            //
            // // AttributeValue には 文字列 または 子のAttributesMap のどちらか1つが入っていることを上述しました。
            // // ではどちらを取得すれば良いのかというと、 AttributeValue.Type で判別できます。
            // // Type が AttributeSet であれば AsAttrSet で取得でき、それ以外であれば AsString で取得できます。
            // var detailAttr = cityObj.AttributesMap.GetValueOrNull("uro:buildingDetails");
            // if (detailAttr != null)
            // {
            //     Assert.AreEqual(AttributeType.AttributeSet ,detailAttr.Type);
            // }
            //
            // return true;
        }

        private void NotifyLoadFailure()
        {
            this.display.TitleText = "";
            this.display.AttributesText = "読込失敗";
        }

        private void Awake()
        {
            if (this.display == null)
            {
                Debug.LogError($"{nameof(AttributesDisplay)} が null です。インスペクタから設定してください。");
            }
        }
    }
}

