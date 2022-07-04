# 都市情報へのアクセス

C#プログラムによって都市情報にアクセスする手順は、おおまかには次のとおりです。  
- GameObject から CityObject を取得  
- CityObject から 属性(AttributesMap) を取得
- 属性から値を取得  

以下で詳しく解説します。  
まずサンプルを提示し、次に都市情報を取得するために知っておくべきことを解説します。

## サンプル
シーンに配置された都市の3Dモデルから、都市情報の1つである属性を取得できます。  
そのサンプルとして、```PlateauLogAttributes``` シーンをご覧ください。  
シーンを開いてPlayボタンを押すと、建物の情報がUnityコンソールに表示されます。  
そのコードのサンプルが ```PlateauAttributesLogger.cs``` になります。


## 属性とは
取得できる都市情報の1つに「属性」があります。  
属性は例えば  
```text
 (String) 大字・町コード => 42,
 (String) 防火及び準防火地域 => 準防火地域
```
のように、キーと値のペアからなる辞書形式の情報です。  
属性の値は文字列型として取得できるか、または  
子の属性（属性は入れ子になることもあります）として取得できるかのいずれかです。  
属性が入れ子になっている例は次のとおりです（サンプルシーンの実行結果から抜粋）。
```text
 (AttributeSet) 多摩水系多摩川、浅川、大栗川洪水浸水想定区域（想定最大規模） => 
    [ { (String) 浸水ランク => 2 }
    { (Measure) 浸水深 => 0.990 }
    { (Measure) 継続時間 => 0.68 }
    { (String) 規模 => L2 }  
]}
```
上の例において、(括弧)内の文字は属性の型を示します。  
属性は次の型があります:  
```AttributeSet, String, Double, Integer, Data, Uri, Measure```  
AttributeSet以外の型はすべて内部的には文字列型であり、  
```attributeValue.AsString``` で値を取得できます。  
入れ子AttributeSetの値は ```AsString``` では取得できず、```attributeValue.AsAttrSet```で取得できます。


## 属性の取得

シーンの GameObject から都市情報の属性を取得する方法については  
サンプルの ```PlateauAttributesLogger.cs``` が例になります。  
その補足として、方法の概要を以下に記します。


### シーンのヒエラルキー
PlateauデータをUnityにインポートすると、  
サンプルシーンにあるとおり、次の階層構造でオブジェクトが配置されます。

```text
 都市モデルルート( CityBehaviour がアタッチされます )
   → 子 : 3Dモデルファイルのインスタンス ( プレハブとして配置されます )
             → 子 : CityObject に対応する GameObject
```

すなわち、 CityBehaviour の孫にあたる GameObject が CityObject と対応します。

### CityObject の取得
GameObject の名称から CityObject を取得するには次のようにします。  
```cs
var cityObj = cityBehaviour.LoadCityObject(gameObj.name);
```   
そうすると CityBehaviour は自身の参照するメタデータを利用し、  
GameObjectの名称からどの gml ファイルをロードするべきか検索します。  
そして gmlファイルから CityModel ロードして（ロード済みの場合はキャッシュが使われます）、  
CityModel のうち 該当する CityObject を返します。  

**補足:**  
ここで利用するメタデータとは、  
インポート時に生成される CityMetaData型の Scriptable Object です。  
CityMetaDataは、インポート時に GameObject名と gmlファイル名の対応表を記録します。

### CityObject の属性の取得
CityObject から属性を取得するには、  
```cityObject.AttributesMap``` を利用します。  
属性については上述の通りです。