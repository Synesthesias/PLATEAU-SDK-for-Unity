# インストール

このページでは PLATEAU SDK for Unity のインストール方法を説明します。

## 対応Unityバージョンのインストール
- PLATEAU SDK for Unity で想定するUnityバージョンが[リリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)に記載されています。  
  そのバージョンがインストールされていない場合は、次の手順でインストールしてください。
  - [Unity Hub をこちらからインストールします](https://unity3d.com/jp/get-unity/download)。
  - Unity Hub とは、Unityのお好きなバージョンをインストールして起動することのできるソフトウェアです。
  - Unity Hubを起動し、左のサイドバーから`インストール` → 右上のボタンから`エディターをインストール` をクリックします。
![](../resources/manual/installation/unityHub.png)
  - SDK対応のUnityバージョンを選択し、`インストール`を押します。
![](../resources/manual/installation/unityHubSelectVersion.png)

## Unityプロジェクトの作成
- Unity Hub の左サイドバーの`プロジェクト` を押し、右上の`新しいプロジェクト`ボタンをクリックします。
![](../resources/manual/installation/unityHubNewProjectVersion.png)
- 新しいプロジェクトの設定画面で、次のように設定します。
  - 画面上部の `エディターバージョン` を、[リリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)に記載のバージョンにします。
  - 画面中部の`テンプレート` は `3D` を選択します。
  - 画面右下のプロジェクト名をお好みのものに設定します。
  - `プロジェクトを作成`ボタンを押します。
  ![](../resources/manual/installation/unityHubnewProjectConfig.png)
  
## PLATEAU Unity SDK の導入
PLATEAU SDK for Unity の導入方法は2通りあります。

- 1つは、配布の tgz ファイルから導入する方法
- 2つ目に、gitのURL指定で導入する方法です。  

どちらか1つを行ってください。

### tgzファイルから導入する方法
- [PLATEAU SDK for Unity のリリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases) から tgzファイルをダウンロードします。
  - ファイルは解凍せず、そのまま利用します。
- Unityのメニューバーから `Window` → `Package Manager` を選択します。
- Package Manager ウィンドウの左上の＋ボタンから `Add pacakge from tarball...` を選択します。
  ![](../resources/manual/installation/addPackageFromTarball.png)
- ウィンドウのパッケージ一覧に `Plateau Unity SDK` が表示されたら完了です。
  ![](../resources/manual/installation/packageInstalled.png)

### GitのURL指定で導入する方法
- Gitを用いて導入するには、Git LFSがお使いのコンピュータにインストールされている必要があります。
  - Git LFSのインストール方法については[こちら](https://docs.github.com/ja/repositories/working-with-files/managing-large-files/installing-git-large-file-storage)を参照してください。
- 導入したいバージョンを確認するため、 [PLATEAU SDK for Unity のリリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases) にアクセスします。
- リリースのリンクをクリックし、導入したいバージョンのタグ（画像の箇所）をメモします。
  ![](../resources/manual/installation/releaseTag.png)
- Unityのメニューバーから `Window` → `Package Manager` を選択します。
- Package Manager ウィンドウの左上の＋ボタンから `Add pacakge from git URL...` を選択します。
- 入力欄に次を入力します。
  - GithubリポジトリのURLに、`#(先ほどメモしたタグ名)` を付けたものを入力してください。例えば次のとおりです：
  - `https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity.git#v1.0.0`
    ![](../resources/manual/installation/packageManagerGitUrl.png)
  - なお、`#(タグ名)`を省略するとデフォルトブランチになりますが、  
    デフォルトブランチは内容が予告なく変更されるので推奨しません。  
    タグ名を記載することを推奨します。
- `Add` ボタンを押します。
- ウィンドウのパッケージ一覧に `Plateau Unity SDK` が表示されたら完了です。
  ![](../resources/manual/installation/packageInstalled.png)

### Android向けにビルドする場合
Android向けにビルドする場合、次のようにUnityプロジェクトを設定してください。
- メニューバーから Edit → Project Settingsを選びます。
- 表示されたウィンドウの左パネルからPlayerを選びます。
- Other Settingsを開きます。
- Scripting BackendをIL2CPPにします。
- Target ArchitecturesはARM64だけがチェックが入っているようにします。

### トラブルシューティング
うまく導入できない場合、次のことをご確認ください。
- お使いのウイルス対策ソフトによっては、SDKに含まれるバイナリファイルが削除されることがあります。ウィルス対策ソフトによってSDKのファイルが削除されないよう設定をお願いします。

