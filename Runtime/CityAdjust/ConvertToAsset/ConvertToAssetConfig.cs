using System.IO;
using System.Linq;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityAdjust.ConvertToAsset
{
    /// <summary>
    /// シーン内の都市モデルをAssets以下に抽出するときの設定です。
    /// </summary>
    public class ConvertToAssetConfig
    {
        /// <summary> 設定値: 対象となるシーン内のオブジェクトです。 </summary>
        public GameObject SrcGameObj { get; set; }
        /// <summary> 設定値: FBXの出力先であるAssetsパスです。 </summary>
        public string AssetPath { get; set; }

        // Tile Rebuild時にtrueに設定
        public bool ConvertFromFbx { get; set; } = false;

        // Tile Rebuild時にtrueに設定
        public bool ConvertTerrain { get; set; } = false;

        public ConvertToAssetConfig(GameObject srcGameObj, string assetPath)
        {
            SrcGameObj = srcGameObj;
            AssetPath = assetPath;
        }

        public static ConvertToAssetConfig DefaultValue
        {
            get
            {
                return new ConvertToAssetConfig(null, "Assets/");
            }
        }

        /// <summary>
        /// フルパスから<see cref="AssetPath"/>を設定します。
        /// ただし、フルパスがAssets以下でない場合は設定を中止してエラーダイアログを表示します。
        /// </summary>
        public void SetByFullPath(string fullPath)
        {
            try
            {
                AssetPath = PathUtil.FullPathToAssetsPath(fullPath);
            }
            catch (IOException)
            {
                Dialogue.Display("Assets外のフォルダが指定されました。\nUnityプロジェクトのAssets以下の空のフォルダを指定してください。", "OK");
            }
            
        }

        /// <summary>
        /// <see cref="SrcGameObj"/>の設定値が有効かどうかを返します。
        /// 無効の場合、エラーメッセージをout引数で返します。
        /// </summary>
        public bool ValidateSrcGameObj(out string errorMessage)
        {
            errorMessage = "";
            if (SrcGameObj == null)
            {
                errorMessage = "都市オブジェクトを指定してください。";
                return false;
            }

            if (!SrcGameObj.GetComponentsInChildren<Renderer>(false).Any(r => r.gameObject.activeInHierarchy))
            {
                errorMessage = "ActiveなRendererが見つかりません。都市モデルが非表示状態になっていないか確認してください。";
                return false;
            }
            return true;
        }

        /// <summary>
        /// <see cref="AssetPath"/>の設定値が有効かどうかを返します。
        /// 無効の場合、エラーメッセージをout引数で返します。
        /// </summary>
        public bool ValidateAssetPath(out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrEmpty(AssetPath))
            {
                errorMessage = "出力先フォルダを指定してください。Assets以下の空のフォルダである必要があります。";
                return false;
            }

            if (!PathUtil.IsSubDirectoryOfAssets(AssetPath))
            {
                errorMessage = "Assets以下のフォルダを指定してください。現在、Assets外のフォルダが指定されています。";
                return false;
            }

            if (Directory.GetFileSystemEntries(Path.GetFullPath(AssetPath)).Length > 0)
            {
                errorMessage = "出力先は空のフォルダを指定してください。現在、空でないフォルダが指定されています。";
                return false;
            }

            return true;
        }
    }
}