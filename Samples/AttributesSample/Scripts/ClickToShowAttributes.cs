using System;
using System.IO;
using System.Linq;
using System.Text;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using UnityEngine;
using CityObject = PLATEAU.CityInfo.CityObject;

namespace PLATEAU.Samples.Scripts
{
    /// <summary>
    /// クリック位置にレイキャストして、当たった都市オブジェクトの属性を Canvas に表示します。
    /// </summary>
    public class ClickToShowAttributes : MonoBehaviour
    {
        [SerializeField] private AttributesDisplay display;
        private Camera mainCamera;

        /// <summary> これ以上の文字数は省略して表示します </summary>
        private const int CharacterLimit = 15000;

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
                bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                if (Physics.Raycast(ray, out var hit) & !isOverUI) // レイキャストして当たったとき
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
            var deserializedObjs = cityObjGroup.CityObjects;
            // 複数の PLATEAU 地物が結合されて1つのゲームオブジェクトになっている場合があるため、
            // 1つのゲームオブジェクトから取得できる cityObjects の数は1つまたは複数になっています。
            var cityObjParams = deserializedObjs.cityObjects;
            var attributesSb = new StringBuilder();
            foreach (var cityObjParam in cityObjParams)
            {
                attributesSb.Append(cityObjParam.DebugString());
                attributesSb.Append("\n\n");
            }
            

            // 最大文字数の範囲内で属性情報を表示します。
            string displayText = attributesSb.Length > CharacterLimit
                ? attributesSb.ToString()[..CharacterLimit] + "\n\n(最大文字数に達しました)"
                : attributesSb.ToString();
            this.display.AttributesText = displayText;

            // クリックしたゲームオブジェクトの、最初の主要地物の情報をヘッダーとして表示します。
            var headerSb = new StringBuilder();
            var firstPrimaryObj = cityObjGroup.PrimaryCityObjects.FirstOrDefault();
            if (firstPrimaryObj != null)
            {
                // 主要地物のGmlIDを取得します。
                headerSb.Append(firstPrimaryObj.GmlID);
                headerSb.Append("\n");
                // 高さを取得します。
                var attributesMap = firstPrimaryObj.AttributesMap;
                if (attributesMap.TryGetValue("bldg:measuredheight", out var attribute))
                {
                    // TODO このコード、Unity設定で Api Conpatibility Level が .NET Standard 2.1 だと動かなくて、 .NET Framework にしてようやく動く。
                    // TODO ユーザーにそのような設定を強制させたくはない。attribute.value をstringで取得する良い方法は？　そもそも本当に dynamic型を利用すべき？ 静的型システムの範囲内でなんとかならないか？
                    // TODO PLATEAUのAttributeMapでは、メンバー変数に string と AttributeMap があってどっちか片方に値が入っているという方法をとっていた。 
                    //headerSb.Append($"高さ: {Convert.ToString(attribute.value)}"); // valueは動的型であり、具体的な型は Attribute(入れ子), string, double などがあります。
                    headerSb.Append($"高さ: {Convert.ToString(attribute.StringValue)}"); // valueは動的型であり、具体的な型は Attribute(入れ子), string, double などがあります。
                };
                // 住所を取得します。
                if (attributesMap.TryGetValue("uro:buildingDetails", out var buildingAttr))
                {
                    // TODO uro:buildingDetails 属性に入れ子構造になっている uro:city 属性を取得したいが、そのような手軽な方法がないことに気づいた。Attributeを直接Dictionaryのように扱うことができれば、入れ子構造でも簡単にアクセスできる。
                    // TODO 住所 (uro:buildingDetails/uro:city) を取得して headerSBにAppendする処理をここに書く
                }

            }

            this.display.TitleText = headerSb.ToString();



            // TODO 上記のTODOがうまくいったら以下の古いコードを消す
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

