* [33m15cd0b0[m[33m ([m[1;36mHEAD -> [m[1;32mfeature/placement_config[m[33m, [m[1;31morigin/feature/placement_config[m[33m)[m 同様にIdToGmlTableの表示も見やすく
* [33mc2e898e[m スクロール可能な複数行ラベルを作って対象gmlファイル一覧を見やすくしました。
* [33m3984284[m 出力先フォルダ選択 GUIの見た目向上、Assetsフォルダ外を選択時エラーに
* [33m4150c49[m InputFolderSelectorGUI.cs の見た目を改善
* [33m9d9aec8[m GUIちょっと調整
* [33m328f915[m CopyPlateauSrcFiles.cs を整理中
* [33m52a19a8[m CopyPlateauSrcFiles.cs を整理中
* [33m90226a2[m 微修正
* [33mc4e85f3[m 微修正
* [33maa07c55[m シーン配置系のテストが過去のテストの影響を受けるバグを修正、テストコード整理
* [33m8372d00[m テストコード整理
* [33m8019c85[m テストコード整理
* [33m91c106d[m 同上
* [33m72f0539[m インポート設定で出力先が存在しないとき、変換ボタンを無効にしました。
* [33m6f11f12[m シーン配置設定のGUIを更新
* [33m75f19b9[m シーン配置設定の基礎を実装
* [33mb57528d[m[33m ([m[1;31morigin/main[m[33m, [m[1;31morigin/HEAD[m[33m, [m[1;32mmain[m[33m)[m 複数LODインポート、docfxが見やすいようにコード整理 (#7)
[31m|[m * [33m4eae006[m[33m ([m[1;31morigin/feature/doc_libplateau[m[33m, [m[1;32mfeature/doc_libplateau[m[33m)[m 今まで書いた全テストのAssertに説明メッセージを追加
[31m|[m * [33m10fb5dd[m バグ修正完了
[31m|[m * [33mcfb7a66[m テストを修正し、実はバグの原因は保存時にはなかったことが判明
[31m|[m * [33mcf7b6f5[m metaDataのcopySrcがコピー先ではなくコピー元になるバグを発見、バグをあぶり出す失敗テストを記述
[31m|[m * [33ma0b7a92[m 複数LODインポートのごり押し部分を修正
[31m|[m * [33mb61e708[m コード整理中
[31m|[m * [33mdacde8b[m インポート時にシーンにモデルを配置する処理を別クラスに分割
[31m|[m * [33mf9a7232[m コード整理中
[31m|[m * [33m881d3ce[m 同上
[31m|[m * [33m6b4f072[m インポート先に関する処理がAssetsパスかFullパスかで曖昧だったのを改善
[31m|[m * [33m68e8ee2[m 余計な部分を削除
[31m|[m * [33mec208c9[m 同上
[31m|[m * [33ma69234a[m 同上
[31m|[m * [33m3cb3fbe[m コード上、インポート元パスがコピー前を指すのかコピー後を指すのか曖昧だったのを改善
[31m|[m * [33m1ad981f[m tmp
[31m|[m * [33mc4e6300[m cityImporterの一部引数整理
[31m|[m * [33m1ba52db[m PlateauSourcePathにRootDirName,RootFullPathを実装
[31m|[m * [33m3c2f4fa[m PlateauSourcePath仮導入
[31m|[m * [33m129e491[m 微修正
[31m|[m * [33m695b0dc[m インポート時のインポート元コピー処理を別メソッドに分割
[31m|[m * [33m2845604[m CityImporterのプロパティを1つローカルなout引数に移動
[31m|[m * [33m7345a70[m Obsoleteなクラス、TestGmlToIdConverterを削除
[31m|[m * [33m863d0a4[m 古いインターフェイスを削除
[31m|[m * [33m2966fd5[m 古い機能である単品ファイル変換を削除
[31m|[m * [33mab7a998[m 余計な引数を削除
[31m|[m * [33m743adfd[m MeshConverterの出力先指定がファイルパスからディレクトリパスに変わったのに対応
[31m|[m * [33mcc5e4e6[m 微修正
[31m|[m * [33m81a370f[m TestCityImporter.csのコード整理
[31m|[m * [33mb8b9df5[m 複数のLODがシーンに配置されることのテストを記述
[31m|[m * [33md4c5c30[m LOD設定に合わせて複数のobjが出力されることのテストを記載、コードは荒削りだけどとりあえず出てくる
[31m|[m * [33m26d5ac9[m 荒削りだけど既存のテストをとりあえず通したというレベル
[31m|[m * [33mb65877d[m DLLの仕様変更に伴うテスト失敗の数をとりあえず減らしたというレベル
[31m|[m * [33m62b1684[m lod選択制導入中
[31m|[m * [33m63a9fcf[m 同上
[31m|[m * [33m73ce39b[m Editorコンテンツが良い感じにインデントするように調整中
[31m|[m * [33m4aa9f1c[m 見た目向上
[31m|[m * [33m9cb844c[m 見た目向上
[31m|[m * [33m8cd1350[m 地物タイプごとのLOD選択を（見た目だけ）作成中
[31m|[m * [33m414b119[m 新仕様に備えて GameObjectgNameParserを作成
[31m|[m * [33m839e513[m 同上
[31m|[m * [33mab84017[m publicからinternalにできるものをそのように変更
[31m|[m * [33m0511956[m docfx向けにコメント調整
[31m|[m * [33m90cdb29[m docfxでCityBehaviourのcrefが正しく見れないバグを修正
[31m|[m * [33m11097a4[m dll更新
[31m|[m * [33m0ed8585[m libplateauの名前空間変更に対応
[31m|[m * [33m883f894[m docfx向けにxmlコメント追記
[31m|[m * [33mf013803[m ISerializationCallbackReceiverの実装を隠蔽
[31m|[m * [33m8546f8d[m 名前空間変更
[31m|[m * [33m085dd52[m 名前空間変更中
[31m|[m * [33m3ec19cb[m 名前空間変更中
[31m|[m * [33m2ebf0c9[m docfxのためにxmlコメントを充実させています
[31m|[m * [33mc2e92ef[m SingleFileConvertWindowをinternalに
[31m|[m * [33mb9ec8a4[m libplateauのC#コメントがドキュメントに登場するようにしました。
[31m|[m * [33ma05a30a[m 今まではC#のlibplateauはdll形式だったのをソースコード形式に変更
[31m|[m[31m/[m  
* [33m788120a[m インポート方法のマニュアル作成、プログレスバー、ちょっとだけ高速化 (#6)
[32m|[m * [33mcc0d9c5[m[33m ([m[1;31morigin/gh-pages[m[33m)[m deploy: 788120ac0790497f1381583289257d3875cbd6dc
[32m|[m * [33m601c378[m deploy: ebdb2c42eff6f1d7ba0419f547426fa4e25eaa8f
[32m|[m * [33mc52b133[m deploy: aa4ba3763d2b13a1250c3b8dfc1cb656cf22abda
[32m|[m * [33m60e73c7[m deploy: 37886efe9c523f4bfb596efd4af64a7ecd91c3df
[32m|[m * [33ma033581[m deploy: 11840f8fa8ae8f7f12bbc24858970ee863952ae7
[32m|[m * [33m16e7a14[m[33m ([m[1;32mfeature/progress_bar[m[33m)[m 都市モデルインポート方法のマニュアルを作成
[32m|[m * [33m088807a[m dll更新
[32m|[m *   [33m86d4b92[m Merge branch 'main' into feature/progress_bar
[32m|[m [35m|[m[32m\[m  
[32m|[m [35m|[m[32m/[m  
[32m|[m[32m/[m[35m|[m   
* [35m|[m [33mebdb2c4[m Update Documentation.md
* [35m|[m [33maa4ba37[m Update Documentation.md
* [35m|[m [33m37886ef[m ドキュメント追加 (#5)
[36m|[m * [33m90f4074[m dll更新
[36m|[m * [33m80752f0[m dll更新、ポリゴンがないときの削除処理をdllに任せるように
[36m|[m * [33m49b49eb[m 変換成功数に関するテストを追記
[36m|[m * [33m323920a[m インポート時のidtoGmlTableのシリアライズ回数を減らすことで高速化しました（手元のテストケースではあまり効果確認できず）
[36m|[m * [33m016a3d5[m インポートのgml変換時にプログレスバーを表示
[36m|[m * [33me5de636[m インポート時にgmltypeが正しくないバグを修正、コピー時にプログレスバー表示
[36m|[m * [33m9d92524[m GmlFileNameParserの使い勝手向上
[36m|[m[36m/[m  
* [33mdbf5982[m インポート時に元データのうち選択したものだけをStreamingAssetsにコピー、シーンにモデル自動配置 (#4)
[36m|[m * [33md657022[m[33m ([m[1;32mfeature/auto_place_objs[m[33m)[m 同上
[36m|[m * [33mc90efba[m コメント追記
[36m|[m * [33m941055a[m gmlFileNameParserがstringの代わりにGmlType型を返すように
[36m|[m * [33mdea84de[m コメント追記、areaIdをstringからintに
[36m|[m * [33md702d4e[m Mkdir関数でパス途中のフォルダも作るようにした
[36m|[m * [33m0e427db[m コピー時のフォルダ作成の複雑な部分を共通化して簡潔に
[36m|[m * [33m700117d[m PLATEAUという名前のフォルダがなくても動作するようにした
[36m|[m * [33md185ca4[m cityModelをDisposeするようにした
[36m|[m * [33m1e1d502[m 同上
[36m|[m * [33m0e92b6a[m Gmlsコピー機能をCopyGmlsに抽出
[36m|[m * [33m7f444c5[m コメントを追記
[36m|[m * [33mb878137[m インポート画面で選択したgmlに関連するものだけをコピーする機能
[36m|[m * [33md51e2cd[m 途中だけどバックアップのコミット
[36m|[m * [33m7cab251[m 作りかけだけどメモのためにコミット
[36m|[m * [33mee7b0e5[m テストコード整理
[36m|[m * [33mad1ee42[m コードの警告に対処
[36m|[m * [33mb12764d[m publicをinternalに変更
[36m|[m * [33md515dae[m publicをinternalに直し中
[36m|[m * [33m25f9858[m 名前空間整理
[36m|[m * [33m5155f3a[m コード整理中
[36m|[m * [33m7ed246a[m 長すぎるクラス名を短縮中
[36m|[m * [33m1f297fd[m ビルドエラー修正
[36m|[m * [33mc678e15[m シーン配置時に古いモデルが消えないバグを修正
[36m|[m * [33mdbd3261[m 変換後の配置親オブジェクトにCityMapBehaviourを設定
[36m|[m * [33m87cc5a9[m 変換後のオブジェクトを自動で配置するように
[36m|[m * [33m316304c[m StreamingAssetsフォルダ外を選択すると「コピーされます」というメッセージが出るようにしました。
[36m|[m * [33mc07ccea[m パス情報をPlateauPathに切り出し
[36m|[m * [33m2d4215a[m 同上
[36m|[m * [33m59d302b[m 複数変換のインポート元が StreamingAssetsの外である場合は StreamingAssts/PLATEAU にコピーするようにしました。
[36m|[m * [33m5f8411b[m リネーム
[36m|[m * [33mf9bb14d[m Merge branch 'feature/multi_convert_config' into feature/auto_place_objs
[36m|[m[36m/[m[1;33m|[m 
[36m|[m * [33m5bcb1c2[m[33m ([m[1;32mfeature/multi_convert_config[m[33m)[m コード整理
[36m|[m * [33m50128ac[m 変換時の粒度設定が記録されないバグを修正
[36m|[m * [33m2d62225[m 地域IDのチェック設定が記憶されないバグを修正
[36m|[m * [33md477ed1[m コード整理中
[36m|[m * [33m8360d84[m コード整理中
[36m|[m * [33mf8bd74d[m コード整理中
[36m|[m * [33m5ec2190[m 変換時の設定で地域・地物のチェックを覚えておくようにしました。
[36m|[m * [33mbd96abf[m 地域IDと地物選択も保存するようにしました。
[36m|[m * [33me2e0a4c[m 同上
[36m|[m * [33m5f82e71[m GmlSelectorGUIの設定部分を他のクラスに切り出し
[36m|[m * [33md38f045[m コード整理
[36m|[m * [33mc514c54[m コード整理
[36m|[m * [33mfc45e9b[m foldout追加
[36m|[m * [33m2edcda9[m MetaDataのインスペクタから直接変換できるGUIを表示
[36m|[m * [33m5860ec9[m MetaDataの再変換ボタンを押すと設定を一部引き継いだ変換ウィンドウが出るようにしました。
[36m|[m * [33m5457d13[m MetaDataに再変換ボタンを配置中
[36m|[m * [33me6f781b[m 変換元のパスを覚えておくようにしました。
[36m|[m * [33ma3cd15f[m 複数変換の設定クラスをCityMapMetaDataでも使うようにして処理を一元化
[36m|[m * [33m5f430c0[m テスト修正
[36m|[m * [33m123f2dc[m MetaDataのIdToGmlFileの記録を、変換時のオブジェクトの粒度に合わせるようにした。
[36m|[m * [33m4572bb0[m 複数変換時は以前のIdTableをリセットするように
[36m|[m * [33m81bfc67[m 複数変換でMeshGranularityを記録する機能のテストを書きました。
[36m|[m * [33m6770d54[m 変換時のMeshGranularityをCityMapInfoに保存するようにしました。
[36m|[m * [33m5271ced[m リファクタリング
[36m|[m * [33m9cae341[m リファクタリング
[36m|[m * [33m98f9c9f[m リファクタリング
[36m|[m * [33m39750e6[m 複数変換のReferencePoint設定のユニットテストを書きました。
[36m|[m * [33m869a7f9[m 複数変換時のReferencePointをCityMapInfoに記録するようにしました
[36m|[m * [33mc94c892[m 複数変換時のReferencePointを最初のものに合わせるようにしました。
[36m|[m * [33mcdc1cc8[m GmlToObjFileConverterのConfigを別クラスに分けた
[36m|[m * [33m27f40ba[m ビルド通らない問題を修正
[36m|[m * [33m834dd22[m dll更新
[36m|[m * [33m8e076df[m IdToGmlFileをCityMapInfoに置き換え
[36m|[m * [33mbf4eebd[m IdToGmlFileからCityMapInfoへの置き換え、とりあえず単体Table変換が動くレベル
[36m|[m * [33m9bcde26[m リネーム
[36m|[m * [33m6a48150[m obj変換してもメッシュがなければ変換後ファイルを削除して変換失敗とする機能を実装(Assetsフォルダ内のみ)
[36m|[m * [33m9984305[m 単品ファイル変換でもログレベルを設定できるように
[36m|[m * [33m6f18f68[m objファイル変換ですでに存在するobjファイルを上書きしたとき、メッシュの名前が変わってもそれがUnityに反映されないバグを修正
[36m|[m * [33m137fd5c[m udxインポートウィンドウで最適化選択とメッシュ分け粒度選択を実装
* [1;33m|[m [33mc85e73c[m gml複数変換: gmlの絞り込み設定、粒度設定、設定の記録と再変換 (#3)
[1;33m|[m[1;33m/[m  
* [33m79cb183[m udxフォルダ内のgmlを地物の種類と地域IDから絞りこんで複数変換する機能を実装、他 (#2)
[1;33m|[m * [33maf659a0[m[33m ([m[1;32mfix/string_pass[m[33m)[m 効率化してMultiGmlConverterの処理時間を半減
[1;33m|[m * [33m3fe7069[m MultiGmlConverterのテストを実装
[1;33m|[m * [33mb25d5cb[m udxフォルダ指定したときのパス表示が反映されないバグを修正
[1;33m|[m * [33m276791d[m 各プラットフォーム向けにDLL更新
[1;33m|[m * [33m2828887[m テストデータ luse_66682を修正
[1;33m|[m * [33me66d2c5[m パース時のDLL内のログをUnityで見れるようにしました。
[1;33m|[m * [33me67f077[m 新DLLLoggerに対応
[1;33m|[m * [33m3339a81[m 例外処理を追加、容量の小さいテストデータを作成
[1;33m|[m * [33m848bc27[m リファクタリング
[1;33m|[m * [33mc065452[m リファクタリング
[1;33m|[m * [33m5eb963d[m リファクタリング
[1;33m|[m * [33mb0e66ea[m リファクタリング
[1;33m|[m * [33mccead91[m リファクタリング
[1;33m|[m * [33m642af29[m リファクタリング
[1;33m|[m * [33mcebeeaf[m リファクタリング
[1;33m|[m * [33m8c5d5ec[m いちおう最低限、複数のgmlから複数のobjと1つのIdToFileTableを生成するというところまではできた。（設定変更機能がないとかエラーがけっこうでてるとかの問題はともかく。）
[1;33m|[m * [33m81da9a6[m 地物の種別によりgmlファイルを絞り込んで表示する機能を実装
[1;33m|[m * [33m4f4a998[m 地域コードに該当するgmlファイル一覧を表示できるようにしました。
[1;33m|[m * [33mf6829a8[m GUIでudxフォルダを選択したら、そこに含まれる地域IDを検索して表示する機能を実装しました。
[1;33m|[m * [33m9d95bd3[m フォルダリネーム
[1;33m|[m * [33mad7ee03[m 東京の一部を切り出したテストデータを導入。GmlFileSearcherでgmlファイル目録を作成。
[1;33m|[m * [33m81fd211[m 同上
[1;33m|[m * [33m4f7dc80[m ObjWriterのバグ原因を解決
[1;33m|[m * [33m812bff7[m DLL側のパス区切り文字の都合でうまく変換されない問題を修正
[1;33m|[m * [33m4a2128e[m objWriterのWrite時にDLLからログメッセージを受け取るようにしました。
[1;33m|[m * [33me5c967b[m 一時フォルダ生成忘れバグ修正、コメント追記
[1;33m|[m * [33m003bcd4[m 変換後のobjファイルがプロセス使用中になって削除できない問題を、ConverterにIDisposableを導入することで解決
[1;33m|[m * [33m995cf0d[m テスト追記: MeshGranularity.PerCityModelArea
[1;33m|[m * [33m57d34bd[m メソッドリネーム
[1;33m|[m * [33m827c1df[m メッシュのオブジェクト分けの粒度を3択から選択するようにしました。
[1;33m|[m * [33m19c1365[m DLLにstringが正しく渡せないバグの修正
[1;33m|[m[1;33m/[m  
* [33mcb75352[m IdToFileTableConverterのテストを追加、テストコード整理
* [33mc2440a1[m IdToFileTable作成、gui整理、Linux対応 (#1)
[1;34m|[m * [33m94017de[m[33m ([m[1;32mfeature/id_table[m[33m)[m dll更新:
[1;34m|[m * [33mefabbf4[m テストコードを整理
[1;34m|[m * [33m0843e02[m TestFilePathValidatorのコードを整理
[1;34m|[m * [33m8d76016[m linux向けにユニットテストを追記
[1;34m|[m * [33m19c018e[m Linuxでテストを動かし中
[1;34m|[m * [33m2f32fe2[m dll更新
[1;34m|[m * [33mb8d8aad[m dll更新
[1;34m|[m * [33m9c36dbb[m 同上
[1;34m|[m * [33m74af99d[m 同上
[1;34m|[m * [33m3fab769[m 同上
[1;34m|[m * [33mb2f3e85[m 同上
[1;34m|[m * [33m88c294e[m 同上
[1;34m|[m * [33m8c24d9b[m 同上
[1;34m|[m * [33m176a3b1[m ネイティブプラグインのlinux,mac版を用意
[1;34m|[m * [33m987a9fe[m dll更新
[1;34m|[m * [33m8a48bb7[m dll更新
[1;34m|[m * [33mbcb7782[m dll更新
[1;34m|[m * [33ma15aafc[m 同上
[1;34m|[m * [33m6c3fd99[m IdからCityModelを読んでCityObjectを取得する機能の試作を作成したところ、日本語を含む文字がnullになる問題を発見
[1;34m|[m * [33m64001f2[m 見出し番号が正しくないバグを修正
[1;34m|[m * [33m454293a[m GUIの見出し番号をカウントしつつ描画するHeaderDrawerを試作しました。
[1;34m|[m * [33md1f3a0e[m GMLから2つのファイルに同時に変換するウィンドウでもIdFileTableの出力先指定を2通りの方法から選べるようにしまいした。
[1;34m|[m * [33m14c5ac0[m GML to IDFileTable 変換タブで、対象ファイルをNewFileかExistingFileか選択できるようにしました。
[1;34m|[m * [33mc43c720[m Model File Converter Windowの初期画面の描画でエラーがないことをテスト
[1;34m|[m * [33m0b728eb[m EditorWindowのテストを少し追加
[1;34m|[m * [33me62c777[m GMLからOBJとIDテーブルをいっぺんに変換するGUIの基礎を作成
[1;34m|[m * [33md046098[m ConvertFileSelectorGUIUtil.PrintConvertButtonの処理のボタン押下時の処理の汎用性をUP
[1;34m|[m * [33m282c9b2[m 同上
[1;34m|[m * [33m7fa1d56[m GmlToObjAndIdTableConvertTabを一部作成
[1;34m|[m * [33mf644505[m ModelFileConverterWindowの余計な番号表示を削除
[1;34m|[m * [33m8a0257d[m ConvertFileSelectorGUIUtil.DefualtPathのコードを整理して簡潔に
[1;34m|[m * [33m8cfc863[m リネーム
[1;34m|[m * [33m869e2f1[m コード整理
[1;34m|[m * [33mdd23b5d[m 入力ファイル選択、出力ファイル選択の処理の共通化
[1;34m|[m * [33m809d549[m ConvertFileSelectorGUIUtilをstaticクラスに整理
[1;34m|[m * [33m1040190[m コメント整備
[1;34m|[m * [33m284a30f[m コード整理
[1;34m|[m * [33m586b8bb[m さっきの変更を元に戻す
[1;34m|[m * [33m32bc8eb[m 自動テストの記述をDevからプラグイン本体に移動
[1;34m|[m * [33m1dd726d[m IdToFileTableの基礎的な部分を実装
[1;34m|[m * [33ma3cf23c[m TestObjToFbxFileConverterを記載
[1;34m|[m * [33m8f55ef7[m dll更新
[1;34m|[m * [33m1649b6c[m TestGmlToObjFileConverter.csを作成
[1;34m|[m * [33mc2a659b[m dll更新
[1;34m|[m * [33m13e092c[m ConvertTabBaseに移行
[1;34m|[m * [33ma95b148[m ConverterTabBaseを実装中
[1;34m|[m * [33ma7c16a5[m ScrollableEditorWindowContentsを作成
[1;34m|[m * [33m56f05fd[m FileConverterのディレクトリ整理
[1;34m|[m [1;35m|[m * [33m875f99c[m[33m ([m[1;35mrefs/stash[m[33m)[m WIP on (no branch): b4cffb3 フォーマット一括変更
[1;34m|[m [1;35m|[m[1;35m/[m[31m|[m 
[1;34m|[m [1;35m|[m * [33ma4bb931[m index on (no branch): b4cffb3 フォーマット一括変更
[1;34m|[m [1;35m|[m[1;35m/[m  
[1;34m|[m * [33mb4cffb3[m フォーマット一括変更
[1;34m|[m * [33m60e3e01[m 変更前のバックアップ
[1;34m|[m[1;34m/[m  
* [33m5bbecc6[m[33m ([m[1;32mfeature/introduce_test_framework[m[33m)[m コード整理しました
* [33m892b674[m asmdefを整理、ReflectionUtilクラスを作成
* [33mf815af7[m テスト専用のコードを実装用csファイルからテスト用csファイルに移動
* [33ma345019[m テストコード整理
* [33m7ab0e88[m テストを書いて、クラス FilePathValidator のカバー率を96.4%にしました。
* [33m8e887bf[m 上記メソッドの例外が出ることをテスト
* [33mc766414[m CodeCoverageの解析を開始
* [33ma0b30bd[m フルパスからAssetsパスへ変換する機能の正常系テストを書きました。
* [33mbcda999[m Test Frameworkを導入
* [33m6235d5c[m[33m ([m[1;32mfeature/fbx_export[m[33m)[m コード整理しました。
* [33m1dcd60d[m タブ機能をPlateauEditorStylesにまとめました。
* [33mb930460[m 変換タイプごとに異なるウィンドウだったのを、ウィンドウ統合してタブで分けるようにしています
* [33me2076e9[m VerticalScopeのGUIデザインに関するコードを整理しました。
* [33m44947b8[m コード整理しました。
* [33ma753ce8[m 不用になったプロトタイプ実装を削除しました。
* [33me97c471[m objファイルからfbxファイルへの設定でfbxフォーマットをバイナリかAsciiか指定できるようにしました。
* [33m9fea248[m 入出力ファイルがAssetsフォルダ内にあるべきなのか、Assetsの外でも良いのかをはっきりさせ、それに関するエラー処理を実装しました。
* [33m4920951[m GmlToObjでファイル入出力のエラーハンドリングを実装
* [33m9fbca6e[m obj->fbx変換ウィンドウのプロトタイプ第2段
* [33m51275f0[m コード整理
* [33mfe9b6c3[m 変換ウィンドウのうち、他でも使える機能を別クラスにまとめました。
* [33m9bc27ff[m fbx出力のプロトタイプを作成
* [33m2efcc5a[m fbx exporterへの依存をpackage.jsonに明示
* [33mcca7338[m 微修正
* [33mf011ee6[m Optimize設定をintからboolに変更しました。
* [33mdd56555[m dll移動、EditorWindow用スタイルクラス整備
* [33m406d0fc[m GML Convertr Window で変換設定を指定できるようにしました。Optimize Level, Merge Mesh, Axes Conversion を指定できます。
* [33m7fde07c[m obj変換先指定をディレクトリとファイル名一括で行うようにしました。
* [33m0d1e844[m 変換部分を別クラスに切り出してgmlファイル処理の例外処理を追加しました。
* [33m42cb0f1[m dll更新
* [33m70da0a2[m dll更新
* [33m897f860[m モデルが原点から極めて遠い位置になる問題を修正、EditorWindow用のスタイルを少々作成
* [33me4eb671[m GML Convert Windowの試作
* [33ma9aec4e[m initial commit
