using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace PLATEAU.CityImport.Import.Convert.MaterialConvert
{
    /// <summary>
    /// Unityのマテリアルと、DLLのSubMesh情報を相互変換します。
    /// 注意：シーン内蔵のテクスチャについては、C++にテクスチャのファイルパスを渡す目的で、実行時に一時ファイルに書き出します。
    ///      Disposeパターンでクリーンアップして一時ファイルを削除する必要があります。
    /// </summary>
    internal class MaterialConverter : IDisposable
    {
        
        /// <summary> 一時的に書き出したテクスチャファイルを覚えておき、クリーンアップメソッドで削除する用です。スレッドセーフに配慮します。 </summary>
        private readonly ConcurrentBag<string> temporaryTextureFiles = new ConcurrentBag<string>();
        
        /// <summary> 同じテクスチャを何度も書き出さないように覚えておく用です。スレッドセーフに配慮する用です。 </summary>
        private readonly ConcurrentDictionary<int, string> textureFileCache = new ConcurrentDictionary<int, string>();
        
        /// <summary>
        /// Unityのマテリアルを、DLLのSubMeshのTexturePathに変換します。
        /// </summary>
        public string MaterialToSubMeshTexturePath(Material mat)
        {
            if (mat == null) return "";
            if (!mat.HasMainTextureAttribute()) return "";
            var tex = mat.mainTexture;
            if (tex == null)
            {
                if(mat.GetTexturePropertyNames().Contains("_BaseMap"))
                    tex = mat.GetTexture("_BaseMap");
            }
            if (tex == null) return "";
            
            return GetTextureFilePath(tex);
        }
        
        /// <summary>
        /// テクスチャから実際のファイルパスを取得します。
        /// シーン内蔵テクスチャの場合は一時ファイルに書き出します。
        /// </summary>
        private string GetTextureFilePath(UnityEngine.Texture tex)
        {
            if (tex == null) return "";
            
#if UNITY_EDITOR
            // まずAssetDatabaseでパスを取得を試行
            string texAssetPath = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(texAssetPath))
            {
                return Path.GetFullPath(texAssetPath);
            }
#endif
            
            // シーン内蔵テクスチャの場合、一時ファイルに書き出し
            return CreateTemporaryTextureFile(tex);
        }
        
        /// <summary>
        /// シーン内蔵テクスチャを一時ファイルに書き出し、パスを返します。
        /// 同じテクスチャは一度だけ書き出し、以降はキャッシュされたパスを返します。
        /// </summary>
        private string CreateTemporaryTextureFile(UnityEngine.Texture tex)
        {
            // Texture2Dでない場合は処理できない
            if (!(tex is Texture2D tex2D))
            {
                Debug.LogWarning($"テクスチャ '{tex.name}' はTexture2Dではないため、一時ファイルに書き出せません。");
                return "";
            }
            
            int textureInstanceId = tex.GetInstanceID();
            
            // キャッシュをチェックし、すでに書き出し済みのテクスチャの場合はそれを返す
            if (textureFileCache.TryGetValue(textureInstanceId, out string cachedPath))
            {
                // ファイルがまだ存在するかチェック
                if (File.Exists(cachedPath))
                {
                    return cachedPath;
                }
                else
                {
                    // ファイルが削除されていればキャッシュからも削除
                    textureFileCache.TryRemove(textureInstanceId, out _);
                }
            }
            
            try
            {
                // 一時ディレクトリとファイル名を生成
                string tempDir = Path.Combine(Application.temporaryCachePath, "PLATEAU_TextureExport");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }
                
                // 以前はUnityのテクスチャ名がテクスチャパスであるという前提でエクスポートしていました。
                // その名残で、このままエクスポートするとファイル名が長くなりすぎてしまいます。
                // これを防ぐためにテクスチャ名からパスのファイル名部分のみを取得し、長さを制限します。
                string textureName = tex.name;
                if (textureName.Contains("/") || textureName.Contains("\\"))
                {
                    textureName = Path.GetFileNameWithoutExtension(textureName);
                }
                
                // ファイル名の長さを制限
                const int maxNameLength = 100; // InstanceIDと拡張子分を考慮した安全な長さ
                if (textureName.Length > maxNameLength)
                {
                    textureName = textureName.Substring(0, maxNameLength);
                }
                
                string fileName = $"{textureName}_{textureInstanceId}.png";
                // 不正なファイル名文字を置換
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(c, '_');
                }
                
                string tempFilePath = Path.Combine(tempDir, fileName);
                
                // テクスチャをPNGファイルに書き出し
                WriteTextureToPNG(tex2D, tempFilePath);
                
                // キャッシュに追加
                textureFileCache.TryAdd(textureInstanceId, tempFilePath);
                
                // 一時ファイルのリストに追加
                temporaryTextureFiles.Add(tempFilePath);
                
                return tempFilePath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"テクスチャ '{tex.name}' の一時ファイル書き出しに失敗しました: {ex.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// Texture2DをPNGファイルに書き出します。
        /// </summary>
        private static void WriteTextureToPNG(Texture2D tex2D, string filePath)
        {
            // EncodeToPngに対応していない場合は、対応するテクスチャを作成
            Texture2D readableTexture = tex2D;
            bool needsCleanup = false;
            
            if (!tex2D.isReadable || IsCompressedFormat(tex2D.format))
            {
                readableTexture = CreateReadableTexture(tex2D);
                needsCleanup = true;
            }
            
            try
            {
                byte[] pngData = readableTexture.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);
            }
            finally
            {
                if (needsCleanup && readableTexture != tex2D)
                {
                    UnityEngine.Object.DestroyImmediate(readableTexture);
                }
            }
        }
        
        /// <summary>
        /// テクスチャフォーマットが圧縮フォーマットかどうかを判定します。
        /// </summary>
        private static bool IsCompressedFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 読み取り不可能なテクスチャなど、EncodeToPngに対応していないテクスチャをから対応テクスチャを作成します。
        /// </summary>
        private static Texture2D CreateReadableTexture(Texture2D source)
        {
            // 元のテクスチャの色空間設定を参照して適切なReadWriteモードを選択
            var readWrite = GraphicsFormatUtility.IsSRGBFormat(source.graphicsFormat) ? 
                RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
            RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, readWrite);
            Graphics.Blit(source, renderTex);
            
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            
            // 非圧縮フォーマット（RGB24）で作成してEncodeToPNGに対応
            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
            readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTexture.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            
            return readableTexture;
        }
        
        /// <summary>
        /// 作成された一時テクスチャファイルをすべて削除します。
        /// </summary>
        public void CleanupTemporaryTextureFiles()
        {
            foreach (string filePath in temporaryTextureFiles)
            {
                try
                {
                    if (File.Exists(filePath)) // 重複ケース配慮のチェック
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"一時テクスチャファイル '{filePath}' の削除に失敗しました: {ex.Message}");
                }
            }
            // ConcurrentBagを空にします
            while (!temporaryTextureFiles.IsEmpty)
            {
                temporaryTextureFiles.TryTake(out _);
            }
            textureFileCache.Clear();
        }
        
        /// <summary>
        /// IDisposable実装: 一時テクスチャファイルを自動的にクリーンアップします。
        /// </summary>
        public void Dispose()
        {
            CleanupTemporaryTextureFiles();
        }
        
    }

    internal static class MaterialExtension
    {
        /// <summary>
        /// シェーダーによっては、material.mainTexture が失敗することがあります。
        /// mainTextureの実行前に、これの成功に必要な属性をシェーダーが有しているかをチェックします。
        /// OKならtrue、NGならfalseを返します。
        /// </summary>
        public static bool HasMainTextureAttribute(this Material mat)
        {
            // [MainTexture]属性がシェーダーで指定されているかどうかをチェックします。
            // privateメソッドの利用が必要なのでリフレクションで無理やり実行します。
            var matType = mat.GetType();
            var propIdGetMethod = matType.GetMethod("GetFirstPropertyNameIdByAttribute",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (propIdGetMethod == null)
            {
                throw new Exception("Something is wrong");
            }
            int nameIdByAttribute = (int)propIdGetMethod.Invoke(mat, new object[] { ShaderPropertyFlags.MainTexture });
            if (nameIdByAttribute >= 0) return true;
            
            // プロパティ名でチェックします。
            return mat.HasProperty("_MainTex") || mat.HasProperty("_BaseMap");
        }
    }
}