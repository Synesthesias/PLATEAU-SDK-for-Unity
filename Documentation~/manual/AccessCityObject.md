# 都市情報へのアクセス

C# APIによって都市オブジェクトの情報を取得できます。  
このページでその方法を説明します。

## サンプル
クリックした建物の情報を表示するサンプルを用意しています。  
次の方法で確認できます。
- 下図のボタンをクリックして `Attributes Sample` をインポートします。
  ![](../resources/manual/accessCityObject/importSample.png)  
　　図は Package Manager の Import ボタンを押している様子です。
- シーン AttributesSample を開きます。
- Playボタンで再生します。
- クリックした建物の情報が画面に表示されます。  
  ![](../resources/manual/accessCityObject/attributeDisplay.png)
- ソースコード `ClickToShowAttributes.cs` の中に、この実装とコメントでの説明が記載されています。

## ロードした都市モデルの情報を表示

PlateauSDKでインポートした都市オブジェクトには属性等の情報が含まれています。  
情報にアクセスするには、都市オブジェクトであるGameObjectにアタッチされた`PLATEAUCityObjectGroup`コンポーネントを使用します。  
`PLATEAUCityObjectGroup`コンポーネントには都市オブジェクトの持つ個別の情報がJson形式で表示されています。  

  ![](../resources/manual/accessCityObject/cityObjectGroup.png)

このJsonの内容は、`PLATEAUCityObjectGroup.CityObjects`から利用可能なデータ`CityObject`として取得できます。  
`CityObject.cityObjects`リストには、通常は１つの`CityObjectParam`が格納されていて、`CityObjectParam`の属性を取得することで地物（建物など）の情報を取得できます。  
インポート時のメッシュ粒度設定が `地域単位`だった場合のみ`CityObject.cityObjects`リストに複数の`CityObjectParam`が格納されます。  
`CityObjectParam.Children`には、都市オブジェクトの階層の子に当たる`CityObjectParam`が格納されます。  

## 属性とは

都市オブジェクトの情報は「属性」として取得できます。  
属性は例えば  
  
```text
(String) 大字・町コード => 42,
(String) 防火及び準防火地域 => 準防火地域
```
  
のように、キーと値のペアからなる辞書形式の情報です。  
属性辞書`Attributes`は `CityObjectParam.AttributesMap` メソッドで取得できます。  
`Attributes.DebugString()` をコールすると、属性情報をすべて文字列にして返します。　　
`Attributes.TryGetValue("key", out value)` によってキーに対応する`Attributes.Value` を取得できます。  
`Attributes.Value` の具体的な値は文字列型として取得できるか、または  
子の属性（属性は入れ子になることもあります）として取得できるかのいずれかです。  
属性が入れ子になっている例は次のとおりです。

```text
 (AttributeSet) 多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模） => 
    [ { (String) 浸水ランク => 2 }
    { (Measure) 浸水深 => 0.990 }
    { (Measure) 継続時間 => 0.68 }
    { (String) 規模 => L2 }  
]}
```

上の例において、(括弧)内の文字は属性の型を示します。  
属性値は次の型があります。:  
`AttributeSet, String, Double, Integer, Data, Uri, Measure`  
AttributeSet以外の型はすべて内部的には文字列型であり、  
`Attributes.Value.StringValue` で値を取得できます。  
入れ子AttributeSetの値は `StringValue` ではなく `Attributes.Value.AttributesMapValue`で取得できます。  
属性値の型は `Attributes.Value.Type` で取得でき、この値が `AttributeSet` である場合は　　
`Attributes.Value.AttributesMapValue`で子の `Attributes` を取得できます。  
`Attributes.Value.Type` がそれ以外 (String, Doubleなど) である場合は `Attributes.Value.StringValue` で文字列を取得できます。

