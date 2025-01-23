# 道路ネットワークのエクスポートの仕様

道路ネットワークのエクスポート機能で生成されるファイルの仕様をこのページに記します。  
下記のgeojsonファイルが生成されます。

## roadnetwork_node.geojson
道路ネットワーク内の交差点をノード情報として格納します。

#### 属性情報
- ID:交差点の識別子

## roadnetwork_link.geojson
ノード（交差点）間の接続情報をリンク情報として格納します。

#### 属性情報
- ID:道路の識別子
- UPNODE:上流ノードの識別子
- DOWNNODE: 下流ノードの識別子
- LENGTH:リンクの長さ（m）
- LANENUM: 車線数
- RLANENUM: 右折（付加）レーンの数
- RLANELENGTH: 右折（付加）レーンの長さ（m）
- LLANENUM: 左折（付加）レーンの数
- LLANELENGTH: 左折（付加）レーンの長さ（m）
- PROHIBIT: 通行禁止情報
- TURNCONFIG: 進行可能な方向の設定
- TYPECONFIG: 進行可能な車両の種類

## roadnetwork_lane.geojson
各リンクに属するの車線情報をレーン情報として格納します。

#### 属性情報
- ID:車線の識別子
- LINKID: 車線が属するリンクの識別子
- LANEPOS: 車線の位置を示す番号（左からの順番）
- LENGTH:車線の長さ（m）
- WIDTH:車線の幅（m）


## roadnetwork_track.geojson
各交差点の通行可能な経路情報をトラック情報として格納します。

#### 属性情報
- ID:軌跡の識別子
- ORDER: 軌跡の順序を示す値
- UPLINKID: 上流道路の識別子
- UPLANEPOS: 上流車線の位置を示す番号（左からの順番）
- UPDISTANCE: 上流道路からの距離（m）
- DOWNLINKID: 下流道路の識別子
- DOWNLANEPOS: 下流車線の位置を示す番号（左からの順番）
- DOWNDISTANCE: 下流道路からの距離（m）
- LENGTH: 軌跡の長さ（m）
- TURNCONFIG: 進行可能な方向の設定
- TYPECONFIG: 進行可能な車両の種類
下記手順を設定のうえ、出力手順を行うと信号情報も併せて出力される。

## 信号情報について
上記の道路・交差点情報に加えて、別途プラグインSandbox Toolkitで交通シミュレーションを利用している場合、下記の信号情報も出力されます。


## roadnetwork_signalcontroler.geojson
各交差点の信号制御情報を格納します。

#### 属性情報
- ID:信号制御器の識別子
- ALLOCNODE:設置先交差点の識別子
- SIGLIGHT: 制御対象の制御信号灯器識別子（:で連結可）
- OFFSETBASESIGID: オフセット基準信号制御器の識別子
- NUMOFPATTERN: 時間帯別信号制御パターン数
- PATTERNID: 使用する制御パターン識別子（:で連結可）
- INITCYCLE: 制御サイクル長（秒）
- PHASENUM: 現示数
- OFFSETTYPE: オフセットタイプ
- OFFSET: オフセット値（秒）
- STARTTIME: 制御パターン開始時刻（:で連結可）

## roadnetwork_signallight.geojson
各信号制御器に属する信号灯火器情報を格納します。

#### 属性情報
- ID:信号灯火器の識別子
- SIGNALID: 属する信号制御器の識別子
- LINKID: 設置対象の道路の識別子
- LANETYPE: 規制対象車線種別
- LANEPOS: 規制対象車線番号（-1:全車線）
- kDISTANCE: 設置位置（停止線からの距離）

## roadnetwork_signalstep.geojson
各信号制御器の信号現示階梯（信号表示切替毎の信号現示のパターン）を格納します。

#### 属性情報
- ID: 信号現示階梯の識別子
- SIGNALID: 対象の信号制御器の識別子
- PATTERNID: 制御パターン番号
- ORDER: 階梯順番
- DURATION: 階梯の持続時間（スプリット）（秒）
- SIGLIGHT: 対象の制御信号灯器の識別子
- TYPEMASK: 進入許可車種規制
- GREEN: 青現示方向道路ペア（これらは"->"で繋がれ，道路ペアは":"で区切られる）
- YELLOW: 黄現示方向道路ペア（これらは"->"で繋がれ，道路ペアは":"で区切られる）
- RED: 赤現示方向道路ペア（これらは"->"で繋がれ，道路ペアは":"で区切られる） 

