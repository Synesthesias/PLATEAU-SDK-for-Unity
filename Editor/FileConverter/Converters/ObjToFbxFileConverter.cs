using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;


namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// Objファイルをfbxファイルに変換します。
    /// </summary>
    public class ObjToFbxFileConverter : IFileConverter
    {
        private FbxFormat fbxFormat = FbxFormat.Binary;

        /// <summary>
        /// Fbxのフォーマットには Binary と Ascii があります。
        /// Binaryは ファイルサイズが小さく、多くのソフトウェアで利用できます。
        /// Asciiは Blenderで非対応(v2.93時点)となりますが、ファイルの中身を文字列として編集したいという特殊な場合に利用できます。
        /// </summary>
        public enum FbxFormat
        {
            Binary,
            Ascii
        }

        /// <summary>
        /// 出力するfbxファイルに関する設定をします。
        /// </summary>
        public void SetConfig(FbxFormat nextFbxFormat)
        {
            this.fbxFormat = nextFbxFormat;
        }

        /// <summary>
        /// objファイルを読み込みfbxファイルを出力します。
        /// </summary>
        public bool Convert(string srcFilePath, string dstFilePath)
        {
            if (!FilePathValidator.IsValidInputFilePath(srcFilePath, "obj", true)) return false;
            if (!FilePathValidator.IsValidOutputFilePath(dstFilePath, "fbx")) return false;


            var srcAssetPath = FilePathValidator.FullPathToAssetsPath(srcFilePath);
            var objMesh = AssetDatabase.LoadAssetAtPath<Object>(srcAssetPath);

            switch (this.fbxFormat)
            {
                case FbxFormat.Binary:
                    ExportBinaryFBX(dstFilePath, objMesh);
                    break;
                case FbxFormat.Ascii:
                    ExportAsciiFBX(dstFilePath, objMesh);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        /// <summary>
        /// AsciiフォーマットのFBXを出力します。
        /// </summary>
        private void ExportAsciiFBX(string exportPath, Object objMesh)
        {
            // ModelExporterのデフォルト設定が Asciiフォーマットなので、
            // 普通に下記のメソッドを実行すれば AsciiフォーマットのFBXファイルができます。
            ModelExporter.ExportObject(exportPath, objMesh);
        }


        /// <summary>
        /// BinaryフォーマットのFBXを出力します。 
        /// </summary>
        private static void ExportBinaryFBX(string filePath, Object singleObject)
        {
            // Unityの FBX Exporter の機能を利用し、 ExportModelSetting.cs 中の
            // ExportOptionsSettingsSerializeBase クラスを使って FBXのフォーマットをBinaryに設定したいところです。
            // しかし残念ながらそのクラスは internal であり直接設定を操作することができません。
            // そこで次善の策として C# のリフレクション機能を使って internal のメソッドを呼び出すことで
            // FBXフォーマットをバイナリに設定してファイル出力します。
            // 参考 : https://forum.unity.com/threads/fbx-exporter-binary-export-doesnt-work-via-editor-scripting.1114221/#post-7277590

            // Unity.Formats.Fbx.Editor のアセンブリから関連する型を取得します。
            var types = AppDomain.CurrentDomain.GetAssemblies().First(x =>
                    x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
                )
                .GetTypes();
            var optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            var optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

            // 設定用オブジェクトをインスタンス化します。
            var propertyInfo =
                typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic);
            if (propertyInfo == null)
            {
                LogAbort();
                return;
            }

            var optionsProperty = propertyInfo.GetGetMethod(true);
            var optionsInstance = optionsProperty.Invoke(null, null);

            // エクスポート設定をASCIIからBinaryに変更します。
            var exportFormatField =
                optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            if (exportFormatField == null)
            {
                LogAbort();
                return;
            }

            exportFormatField.SetValue(optionsInstance, 1);

            // 設定を指定しつつfbxファイルをエクスポートします。
            var exportObjectMethod = typeof(ModelExporter).GetMethod(
                "ExportObject",
                BindingFlags.Static | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[] { typeof(string), typeof(Object), optionsInterfaceType },
                null
            );
            if (exportObjectMethod == null)
            {
                LogAbort();
                return;
            }

            exportObjectMethod.Invoke(null, new[] { filePath, singleObject, optionsInstance });
        }

        /// <summary> "Aborting." というメッセージでエラーログを出します。</summary>
        private static void LogAbort()
        {
            Debug.LogError($"Exporting fbx failed. Aborting.");
        }
    }
}