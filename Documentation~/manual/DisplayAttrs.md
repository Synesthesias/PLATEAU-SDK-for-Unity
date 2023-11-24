# 属性情報を表示する
![](../resources/manual/displayAttrs/screenShot.png)
このサンプルを再生して地物をクリックすると、その属性情報が画面に表示されます。  

## サンプルの開き方

このサンプルは次の場所にあります：  
```(PLATEAU SDKのサンプルディレクトリ)/AttributesDisplaySample/AttributesDisplaySample.unity```
  
サンプルスクリプトは次の場所にあります：  
```(PLATEAU SDKのサンプルディレクトリ)/AttributesDisplaySample/Scripts```  
- ClickToShowAttributes.cs
  - クリックされた地物の情報を取得し、UIに送ります。
  - クリックされた地物に付与されている`PLATEAUCityObjectGroup`から`CityObject`を取得し、`cityObj.DebugString()`で文字列にします。
- AttributesDisplay.cs
  - 情報表示のUIを制御します。  

## サンプルスクリプト解説
### サンプルスクリプト全文

**ClickToShowAttributes.cs**
```csharp
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
            var cityObjectList = cityObjGroup.CityObjects;
            // 複数の PLATEAU 地物が結合されて1つのゲームオブジェクトになっている場合があるため、
            // 1つのゲームオブジェクトから取得できる cityObjects の数は1つまたは複数になっています。
            var rootCityObjects = cityObjectList.rootCityObjects;
            var attributesSb = new StringBuilder();
            foreach (var cityObj in rootCityObjects)
            {
                // 属性情報を見やすい形式の文字列にします。
                attributesSb.Append(cityObj.DebugString());
                attributesSb.Append("\n\n");
            }
            

            // 最大文字数の範囲内で属性情報を表示します。
            string displayText = attributesSb.Length > CharacterLimit
                ? attributesSb.ToString()[..CharacterLimit] + "\n\n(最大文字数に達しました)"
                : attributesSb.ToString();
            this.display.AttributesText = displayText;

            // クリックしたゲームオブジェクトの、最初の主要地物のGmlIDと高さと住所をヘッダーUIに表示します。
            var headerSb = new StringBuilder();
            var firstPrimaryObj = cityObjGroup.PrimaryCityObjects.FirstOrDefault();
            if (firstPrimaryObj != null)
            {
                // 主要地物のGmlIDを取得します。
                headerSb.Append(firstPrimaryObj.GmlID);
                headerSb.Append("\n");
                // 高さを取得します。
                var attributesMap = firstPrimaryObj.AttributesMap;
                // 高さは、PLATEAUの属性情報の中では "bldg:measuredheight" というキーで格納されています。
                if (attributesMap.TryGetValue("bldg:measuredheight", out var attribute))
                {
                    // 属性は keyとvalue のペアですが、valueの型は string,double,attribute(入れ子)などいくつかパターンがあります。
                    // attribute.Type で型を判別し、型に応じたゲッターを使って値を取得できます。例えば valueの型が文字列に変換可能であれば attribute.StringValue で値を取得できます。
                    headerSb.Append($"高さ: {Convert.ToString(attribute.StringValue)}"); 
                };
                // 住所を取得します。
                // 住所は、PLATEAUの属性情報では "uro:buildingIDAttribute"というキーの中で、キーバリュー辞書が入れ子になっている中の "uro:city" というキーで取得できます。
                if (attributesMap.TryGetValue("uro:buildingIDAttribute", out var buildingAttr)) // 入れ子キーの外側です
                {
                    // 属性のキーバリューペア(Attributes)で入れ子になっているものを取得します。
                    // キー uro:buildingIDAttribute の中に入れ子で0個以上のキーバリューペアがあり、
                    // そのキー uro:city からバリュー（住所）を取得します。
                    if(buildingAttr.AttributesMapValue.TryGetValue("uro:city", out var addressAttr)) // 入れ子キーの内側です
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


```

**AttributesDisplay.cs**
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace PLATEAU.Samples.Scripts
{
    public class AttributesDisplay : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text attributesText;

        public string TitleText
        {
            set => this.titleText.text = value;
        }

        public string AttributesText
        {
            set => this.attributesText.text = value;
        }

        private void Awake()
        {
            if (this.titleText == null)
            {
                Debug.LogError($"{nameof(this.titleText)} が null です。インスペクタから設定してください。");
            }

            if (this.attributesText == null)
            {
                Debug.LogError($"{nameof(this.attributesText)} が null です。インスペクタから設定してください。");
            }
        }
    }
}

```

### ソースコード解説
上のソースコードを解説します。  
まずクリックした箇所の地物を取得するためにレイを飛ばします。  
ここでは、レイを検出するために地物にはコライダーが付いていることを前提とします。  
また、クリックしたのが地物であるかを検出するため、地物にはPLATEAUCityObjectGroupが付いていることを前提とします。
```csharp
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
```

地物に付いているコンポーネント`PLATEAUCityObjectGroup`から情報を取得します。  
地物の情報をデバッグ目的で確認するなら、`cityObject.DebugString()`を使って  
地物の情報を整形されたテキストにまとめることができます。  
今回は、DebugStringを画面に表示することにします。

```csharp
// インポートされた各ゲームオブジェクトには PLATEAUCityObjectGroup コンポーネントがアタッチされており、
// その中に属性情報が json で保存されています。 json をデシリアライズします。
var cityObjectList = cityObjGroup.CityObjects;
// 複数の PLATEAU 地物が結合されて1つのゲームオブジェクトになっている場合があるため、
// 1つのゲームオブジェクトから取得できる cityObjects の数は1つまたは複数になっています。
var rootCityObjects = cityObjectList.rootCityObjects;
var attributesSb = new StringBuilder();
foreach (var cityObj in rootCityObjects)
{
    // 属性情報を見やすい形式の文字列にします。
    attributesSb.Append(cityObj.DebugString());
    attributesSb.Append("\n\n");
}
```

上記のとおり、`DebugString()`でも属性情報を確認できますが、次に具体的な属性情報にアクセスしてみます。  
今回のサンプルでは、建物を主要地物単位でインポートしたものを利用しています。  
そのため、ゲームオブジェクトは1つの建物ごとの粒度で分かれているため、  
`cityObjectGroup.PrimaryCityObjects`で1つの建物の情報(CityObject)を取得できます。  
`cityObject.AttributesMap`で属性情報の組を取得でき、  
`attributesMap.TryGetValue`で属性情報のキーを元に値を取得できます。  
下の例では、高さのキー`bldg.measuredheight`から高さを取得します。  
また、住所を取得するために、キー`uro:buildingIDAttribute`の中に入れ子で入っている  
属性情報マップのキー`uro:city`から値を取得します。

```csharp
// クリックしたゲームオブジェクトの、最初の主要地物のGmlIDと高さと住所をヘッダーUIに表示します。
var headerSb = new StringBuilder();
var firstPrimaryObj = cityObjGroup.PrimaryCityObjects.FirstOrDefault();
if (firstPrimaryObj != null)
{
    // 主要地物のGmlIDを取得します。
    headerSb.Append(firstPrimaryObj.GmlID);
    headerSb.Append("\n");
    // 高さを取得します。
    var attributesMap = firstPrimaryObj.AttributesMap;
    // 高さは、PLATEAUの属性情報の中では "bldg:measuredheight" というキーで格納されています。
    if (attributesMap.TryGetValue("bldg:measuredheight", out var attribute))
    {
        // 属性は keyとvalue のペアですが、valueの型は string,double,attribute(入れ子)などいくつかパターンがあります。
        // attribute.Type で型を判別し、型に応じたゲッターを使って値を取得できます。例えば valueの型が文字列に変換可能であれば attribute.StringValue で値を取得できます。
        headerSb.Append($"高さ: {Convert.ToString(attribute.StringValue)}"); 
    };
    // 住所を取得します。
    // 住所は、PLATEAUの属性情報では "uro:buildingIDAttribute"というキーの中で、キーバリュー辞書が入れ子になっている中の "uro:city" というキーで取得できます。
    if (attributesMap.TryGetValue("uro:buildingIDAttribute", out var buildingAttr)) // 入れ子キーの外側です
    {
        // 属性のキーバリューペア(Attributes)で入れ子になっているものを取得します。
        // キー uro:buildingIDAttribute の中に入れ子で0個以上のキーバリューペアがあり、
        // そのキー uro:city からバリュー（住所）を取得します。
        if(buildingAttr.AttributesMapValue.TryGetValue("uro:city", out var addressAttr)) // 入れ子キーの内側です
        {
            headerSb.Append($"  住所: {addressAttr.StringValue}");
        }
    }

}

this.display.TitleText = headerSb.ToString();
```