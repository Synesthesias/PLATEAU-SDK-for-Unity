# インストール

このページでは PLATEAU SDK for Unity のインストール方法を説明します。

## 対応Unityバージョンのインストール
- PLATEAU SDK for Unity は、Unityバージョン `2020.3` を想定しています。  
  そのバージョンがインストールされていない場合は、次の手順でインストールしてください。
  - [Unity Hub をこちらからインストールします](https://unity3d.com/jp/get-unity/download)。
  - Unity Hub とは、Unityのお好きなバージョンをインストールして起動することのできるソフトウェアです。
  - Unity Hubを起動し、左のサイドバーから`インストール` → 右上のボタンから`エディターをインストール` をクリックします。
![](../resources/manual/installation/unityHub.png)
  - Unity 2020.3 で始まるバージョンを選択し、`インストール`を押します。
![](../resources/manual/installation/unityHubSelectVersion.png)

## Unityプロジェクトの作成
- Unity Hub の左サイドバーの`プロジェクト` を押し、右上の`新しいプロジェクト`ボタンをクリックします。
![](../resources/manual/installation/unityHubNewProjectVersion.png)
- 新しいプロジェクトの設定画面で、次のように設定します。
  - 画面上部の `エディターバージョン` を `2020.3` で始まる番号にします。
  - 画面中部の`テンプレート` は `3D` を選択します。
  - 画面右下のプロジェクト名をお好みのものに設定します。
  - `プロジェクトを作成`ボタンを押します。
  ![](../resources/manual/installation/unityHubnewProjectConfig.png)
  
## PLATEAU Unity SDK の導入
PLATEAU SDK for Unity は配布の tgz ファイルから導入できます。   
- [PLATEAU SDK for Unity のリリースページ](https://github.com/Synesthesias/PLATEAU-SDK-for-Unity/releases) から tgzファイルをダウンロードします。
- Unityのメニューバーから `Window` → `Package Manager` を選択します。
- Package Manager ウィンドウの左上の＋ボタンから `Add pacakge from tarball...` を選択します。
  ![](../resources/manual/installation/addPackageFromTarball.png)
- ウィンドウのパッケージ一覧に `Plateau Unity SDK` が表示されたら完了です。
  ![](../resources/manual/installation/packageInstalled.png)