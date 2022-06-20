# ドキュメント作成

## ローカルでの確認

1 [DocFXをインストール](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool)する。
2 ビルドしてサーバーを起動する。
```
$ docfx Documentation~/docfx.json --serve
```
3 ブラウザで`http://localhost:8080/`を開く。

## マニュアルの追加
1. [toc.yml](../Documentation~/manual/toc.yml)
```
Documentation~
├─ manual/
    └─ ~.md               // 
└─ mkdocs.yml           // mkdocs config.
```
Create one directory per document. For example, the directory structure of this "Documentation" page might look like this.
```
E2ESimulator
└─ docs/                            // Root of all documents
    └─ DeveloperGuide               // Category
        └─ Documentation            // Root of each document
            ├─ Documentation.md     // Markdown file
            └─ image_0.png          // Images used in markdown file
```

## デプロイ
`main`ブランチが更新されたタイミングでWebページが更新される。
