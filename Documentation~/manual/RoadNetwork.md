# 道路ネットワークの利用

PLATEAU SDKの機能によってインポートされた3D都市モデルから道路ネットワークの自動生成や編集ができます。 
このページでその方法を説明します。

## 道路ネットワークの機能を確認する(PLATEAU SDKのエディタ機能)
PLATEAU SDKウィンドウの「道路調整」タブを開くと、道路ネットワークに対して利用できる機能が表示されます。  
![](../resources/manual/roadnetwork/generate_panel.png)

- `生成`：道路ネットワークの自動生成ができます。
- `編集`：生成済みの道路ネットワークの編集ができます。
- `追加`：生成済みの道路ネットワークに新しい道路や交差点を追加できます。

### 道路ネットワークの生成
PLATEAU SDKの機能によって`インポートされた3D都市モデル`から`道路ネットワークを自動で生成`します。  
パラメータの設定が完了したら生成ボタンをクリックして、しばらくすると道路ネットワークが生成されます。  
道路ネットワークは新しく生成されたゲームオブジェクトのコンポーネントに保持しています。  
`PLATEAURnStructureModel`コンポーネント
![](../resources/manual/roadnetwork/generate_panel.png)

- 生成パラメータ  
  - `車線幅（メートル）`：3D都市モデルの道路幅とこれを基準に車線が生成されます。車線数や形状に影響します。  
  - `歩道生成時の歩道幅`：歩道が生成可能な場合にこれを基準に歩道が生成されます。  
  - `道路LOD3の歩道情報を利用`：LOD3の歩道情報を利用して歩道を生成します。LOD3が無い場合は元の自動生成ロジックを利用します。  
  - `中央分離帯を作成`：可能であれば中央分離帯を作成します。  
  - `信号制御器を自動配置`：信号制御器、信号機を自動で生成します。信号の制御はデフォルト値が適用されています。  

- 詳細設定  
  - `頂点結合のセルサイズ（メートル）`：道路の頂点を結合する際の参照範囲に関係する値です。  
  - `結合頂点の距離（セル数）`：参照範囲内の頂点を結合するかを判断する閾値です。  
  - `同一直線に近い中間点を削除する距離`：3つの頂点が同一直線上に並んでいるかを判断する閾値です。同一直線上にある際には中央の頂点は削除されます。  
  - `行き止まりの線の拡大許容角度`：行き止まり検出判定時に同一直線と判断する角度の総和です。  
  - `高速道路を対象外にする`：高速道路を自動生成の対象外にします。  

### 道路ネットワークの編集
生成済みの道路ネットワークを編集することが出来ます。ここでは既にある道路と交差点を編集します。  
![](../resources/manual/roadnetwork/edit_panel.png)

編集モードを有効にするとシーンビュー上に道路と交差点の2種類のアイコンが表示されます。  
編集したい対象をクリックすることで編集対象の選択と切替ができます。
![](../resources/manual/roadnetwork/scene_view_on_editing.png)

#### 道路編集
道路が編集対象の時には道路の車線数や歩道の有無を設定できます。  
設定後には編集内容を確定ボタンをクリックしてください。  
また、詳細編集モード時には形状や頂点の編集が可能です。  
![](../resources/manual/roadnetwork/scene_view_on_editing_road.png)

- `車線数`：道路の車線数を変更します。
- `中央分離帯の有無`：中央分離帯の有無を変更します。
- `歩道の有無`：歩道の有無を変更します。

- `詳細編集モード`：詳細編集モードを起動します。（このボタンは後ほど削除されます。ショートカットキーでの詳細編集モードの切り替えを行います。）

##### 詳細編集モード(道路)
詳細編集モードでは各線を形成する頂点の位置に操作可能なハンドルが表示されます。  
以下はシーンビュー上で可能な操作です。  
現バージョンでは端の頂点に対して追加、削除は行わないでください。（移動は可能です。）

- `頂点の移動`：ハンドルをドラッグアンドドロップすることで頂点の位置を変更できます。
- `頂点の追加`：Ctrlキーを押している間、アイコンが変化します。この状態でクリックすることで新しい頂点を追加できます。
- `頂点の削除`：Ctrl+Shiftキーを押している間、アイコンが変化します。この状態でクリックすることで頂点を削除できます。

#### 交差点編集
交差点が編集対象の時には交差点の内の車の経路や歩道の有無、形状などを編集できます。

### 道路ネットワークへ交差点や道路の追加
開発中の機能です。  
![](../resources/manual/roadnetwork/add_panel.png)

  