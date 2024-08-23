![](Documentation~/resources/index/eyecatch.png)

# PLATEAU SDK for Unity
PLATEAU SDK for Unityは、[PLATEAU](https://www.mlit.go.jp/plateau/)の3D都市モデルデータをUnityで扱うためのツールキットであり、主に以下の機能を提供しています。

- CityGMLの直感的なインポート
  - 地図上での範囲選択による3D都市モデルの抽出
  - PLATEAUのサーバーで提供されるCityGMLデータへのアクセス
  - 地形3Dモデルへの航空写真および地図の貼り付け
  - テクスチャの自動結合
- 3D都市モデルに含まれる地物のフィルタリング
- 3D都市モデルの3Dファイル形式へのエクスポート
- 3D都市モデルの属性にアクセスするためのC# API、およびエディタ上での確認
- 3D都市モデルに含まれる地物の分割・結合
- 地物型によるマテリアル分割

![](Documentation~/resources/index/sdk_outline.png)

PLATEAU SDK for Unityを利用することで、実世界を舞台にしたアプリケーションの開発や、PLATEAUの豊富なデータを活用したシミュレーションを簡単に行うことができます。

◆PLATEAU SDK for Unityは利用者アンケートを実施しています。
今後の開発の参考にするため、ユーザーの皆様の忌憚ないご意見をお聞かせください。
[アンケートはこちら](https://docs.google.com/forms/d/e/1FAIpQLSdEU_CjR6-wT9cpvusvqX0bFkPIOE1J-UlJ-oF2JLOLAJoYNQ/viewform?usp=sf_link)

◆PLATEAU SLackコミュニティはどなたでもご参加いただけます。
参加希望の方は、[お問い合わせページ](https://www.mlit.go.jp/plateau/contact/)よりお気軽にお問い合わせください。

# サンプルプロジェクト
本SDKを使用して作成されたサンプルプロジェクトを配布しています。
- [2024年のGISサンプル](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity-GISSample)
  - 都市の情報を視覚的に表示するサンプルです。
- [2024年のゲームサンプル](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity-GameSample)
  - 都市を舞台に遊ぶゲームのサンプルです。
- [2023年のサンプル](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity-Samples)
  - 都市の情報を表示するサンプルと、都市を車で走行するサンプルです。2023年のサンプルなので情報が古い箇所があります。

<img src="Documentation~/resources/index/gissample.png" width="48%" />&nbsp;
<img src="Documentation~/resources/index/gamesample.png" width="48%" /></a>&nbsp;

# 動作環境
- Windows（x86_64）
- MacOS（ARM）
- MacOS (Intel) (v2.2.0-alphaから対応)
- Android、iOS
  - モバイル向けには、一部の機能のみ（緯度経度と直交座標の相互変換など）をサポートしています。

# 利用手順
- SDKの最新版は[Releaseページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)からダウンロードしてください。
- 詳しい利用方法については、こちらの[マニュアル](https://Project-PLATEAU.github.io/PLATEAU-SDK-for-Unity/index.html) をご覧ください。
- [PLATEAUで好きな都市を爆走するミニゲームを作るハンズオン動画](https://www.youtube.com/watch?v=jXWqIb2nGtk)  
[![動画リンク](https://img.youtube.com/vi/jXWqIb2nGtk/0.jpg)](https://www.youtube.com/watch?v=jXWqIb2nGtk)

# 注意点
- 現在、この SDKとドキュメントは日本語のみ対応しています。

## ライセンス
- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発は株式会社シナスタジアが行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

## 注意事項
- 本リポジトリの内容は予告なく変更・削除する可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
