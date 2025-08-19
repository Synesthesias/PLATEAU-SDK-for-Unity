using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using PLATEAU.Util;
using UnityEngine.Rendering;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PrefabTextureResizer
    /// TextureImporterを使用して元画像の設定を変更し、解像度を変更した画像を新規に保存する
    /// </summary>
    public class PrefabTextureResizer
    {
        public static readonly int[] DENOMINATORS = { 1, 2, 4, 8, 16 };　//等倍サイズも含める場合
        private const TextureFormat TempTextureFormat = TextureFormat.RGBA32; // 非圧縮フォーマットを使うこと

        /// <summary>
        /// 保存先パス
        /// </summary>
        private string savePath;

        /// <summary>
        /// 既存のファイルを上書きするかどうか
        /// </summary>
        private bool overwriteExisting;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="savePath_">保存先パス</param>
        /// <param name="overwrite">上書きするかどうか</param>
        public PrefabTextureResizer(string savePath_, bool overwrite)
        {
            savePath = savePath_;
            overwriteExisting = overwrite;
        }

        /// <summary>
        /// Textureアセットを1/2,1/4,1/8,1/16の解像度にして保存
        /// </summary>
        /// <param name="texture">ソースTexture2D</param>
        /// <param name="largest">最大サイズ分母</param>
        /// <param name="smallest">最小サイズ分母</param>
        public Dictionary<int, (Texture2D, string)> CreateResizedTextures(Texture2D texture, int largest = 1, int smallest = 16)
        {
            Dictionary<int, (Texture2D,string)> results = new();

            foreach (var denominator in DENOMINATORS)
            {
                if (denominator < largest || denominator > smallest)
                    continue;

                var (newTexture, newPath) = ResizeAndSaveNewTexture(texture, denominator, denominator, savePath); // zoomLevelをdenominatorと同じ値に設定
                results[denominator] = (newTexture, newPath);
            }
            return results;
        }

        public Texture2D CreateSingleResizedTexture(Texture2D texture, int denominator, int zoomLevel)
        {
            var (newTexture, newPath) = ResizeAndSaveNewTexture(texture, denominator, zoomLevel, savePath);
            return newTexture;
        }

        /// <summary>
        /// 渡されたTextureを指定パーセントの解像度にして新規に保存
        /// </summary>
        /// <param name="sourceTexture">ソースTexture2D</param>
        /// <param name="denominator">サイズ比の分母</param>
        /// <param name="saveDirectory">保存フォルダ名</param>
        /// <returns>再読み込み後のTexture</returns>
        private (Texture2D, string) ResizeAndSaveNewTexture(Texture2D sourceTexture, int denominator, int zoomLevel, string saveDirectory)
        {
            //サイズ
            var proportion = 1f / denominator;
            var newWidth = Mathf.Max(1, Mathf.RoundToInt(sourceTexture.width * proportion));
            var newHeight = Mathf.Max(1, Mathf.RoundToInt(sourceTexture.height * proportion));

            //TextureImporter取得
            var textureImporter = GetTextureImporter(sourceTexture);
            if (textureImporter == null)
            {
                Debug.LogError($"TextureImporter is not suppoerted. {sourceTexture?.name}");
            }
            TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
            TextureImporterType textureType = TextureImporterType.Default;
            if (textureImporter != null)
            {
                compression = textureImporter.textureCompression;
                textureType = textureImporter.textureType;

                //設定変更
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.SaveAndReimport();
            }

            var newTexture = new Texture2D(newWidth, newHeight, TempTextureFormat, false);
            newTexture.name = sourceTexture.name + $"_{zoomLevel}";
            ResizeTextureAsync(sourceTexture, newTexture);

            // 保存先のディレクトリを作成 (解像度ごとに異なるフォルダに保存)
            string directoryPath = AssetPathUtil.GetFullPath(saveDirectory);
            AssetPathUtil.CreateDirectoryIfNotExist(directoryPath);

            string assetName = Path.GetFileName(textureImporter.assetPath);
            string fileExtension = Path.GetExtension(assetName).ToLower();

            var newAssetName = Path.GetFileNameWithoutExtension(sourceTexture.name) + $"_{zoomLevel}" + fileExtension;
            // 新しいファイルとして保存
            string newPath = Path.Combine(directoryPath, newAssetName);

            // 上書きする場合はAssetDatabaseのパスを指定
            if (overwriteExisting && textureImporter != null)
            {
                if(textureImporter == null)
                    Debug.LogError($"Overwriting Texture is not suppoerted without TextureImporter suppoert. {sourceTexture?.name}");
                else
                    newPath = AssetPathUtil.GetAssetPath(textureImporter.assetPath);
            }

            SaveTexture(newTexture, newPath, fileExtension.TrimStart('.'));

            if (textureImporter != null) 
            {
                //設定を元に戻す
                textureImporter.textureType = textureType;
                textureImporter.textureCompression = compression;
                textureImporter.SaveAndReimport();
            }

            //Texture再読み込み (AssetDatabaseにインポート)
            return (LoadTexture(newPath), newPath);
        }

        /// <summary>
        /// TextureImporterを取得
        /// </summary>
        /// <param name="texture">TextureImporterを取得するTexture2D</param>
        /// <returns>TextureImporter</returns>
        private TextureImporter GetTextureImporter(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                return textureImporter;
            }
            return null;
        }

        /// <summary>
        /// Texture2D をリサイズ
        /// </summary>
        /// <param name="source">ソースTexture2D</param>
        /// <param name="dest">変換用Texture2D</param>
        private void ResizeTextureAsync(Texture2D source, Texture2D dest)
        {
            RenderTexture rt = RenderTexture.GetTemporary(dest.width, dest.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            
            if (SystemInfo.supportsAsyncGPUReadback)
            {
                // GPU上にあるRenderTextureを取得します。
                var request = AsyncGPUReadback.Request(rt, 0, TempTextureFormat);
                // GPUにリクエストしたうえで成功を待たないと、下のLoadRawTextureDataに失敗する場合があります。
                request.WaitForCompletion();
                if (request.hasError)
                {
                    Debug.LogError("GPU Readback にエラーが発生しました。");
                    RenderTexture.ReleaseTemporary(rt);
                    return;
                }
                // データをTexture2Dにロードします。
                var data = request.GetData<byte>();
                if(data.Length != dest.width * dest.height * 4)
                {
                    Debug.LogError($"Data size mismatch: expected {dest.width * dest.height * 4} bytes, got {data.Length} bytes.");
                    RenderTexture.ReleaseTemporary(rt);
                    return;
                }
                dest.LoadRawTextureData(data);
            }
            else
            {
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                dest.ReadPixels(new Rect(0, 0, dest.width, dest.height), 0, 0);
                RenderTexture.active = prev;
            }
            
            dest.Apply();
            RenderTexture.ReleaseTemporary(rt);
        }

        /// <summary>
        /// 画像保存
        /// </summary>
        /// <param name="texture">保存するTexture2D</param>
        /// <param name="relativePath">保存先相対パス</param>
        /// <param name="fileExtension">拡張子(png/jpg)</param>
        private void SaveTexture(Texture2D texture, string relativePath, string fileExtension)
        {
            byte[] bytes = null;
            if (fileExtension == "jpg" || fileExtension == "jpeg")
            {
                // JPG形式の場合、JPEGエンコーダを使用
                int quality = 75; // 画質を指定（0〜100）
                bytes = texture.EncodeToJPG(quality);
            }
            else
            {
                // PNGエンコーダを使用
                bytes = texture.EncodeToPNG();
            }

            string path = AssetPathUtil.GetFullPath(relativePath);
            File.WriteAllBytes(path, bytes);

            Debug.Log($"Texture saved to: {path}");
        }

        /// <summary>
        /// 保存済みのTextureを再読み込みしてアセットデータベースにインポートすることで認識させる
        /// </summary>
        /// <param name="fullPath">Textureフルパス</param>
        /// <returns>再読み込みされたTexture</returns>
        private Texture2D LoadTexture(string fullPath)
        {
            var textureAssetPath = AssetPathUtil.GetAssetPath(fullPath);
            AssetDatabase.ImportAsset(textureAssetPath);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            return texture;
        }
    }
}
