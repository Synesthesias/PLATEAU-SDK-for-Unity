using System;
using System.IO;
using System.Linq;
using System.Text;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using UnityEngine;

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
                    // 属性は keyとvalue のペアですが、valueの型は string,double,attribute(入れ子)などいくつかパターンがあります。
                    // attribute.Type で型を判別し、型に応じたゲッターを使って値を取得できます。例えば valueの型が文字列に変換可能であれば attribute.StringValue で値を取得できます。
                    headerSb.Append($"高さ: {Convert.ToString(attribute.StringValue)}"); 
                };
                // 住所を取得します。
                if (attributesMap.TryGetValue("uro:buildingIDAttribute", out var buildingAttr))
                {
                    // 属性のキーバリューペア(Attributes)で入れ子になっているものを取得します。
                    // キー uro:buildingIDAttribute の中に入れ子で0個以上のキーバリューペアがあり、
                    // そのキー uro:city からバリュー（住所）を取得します。
                    if(buildingAttr.AttributesMapValue.TryGetValue("uro:city", out var addressAttr))
                    {
                        headerSb.Append($"  住所: {addressAttr.StringValue}");
                    }
                }

            }

            this.display.TitleText = headerSb.ToString();
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

