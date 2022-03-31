using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;

// 仮実装、あとで消す

namespace PlateauUnitySDK.Editor.FileConverter {
    public class ObjToFbxFileConverter : IFileConverter {


        public void SetConfig() {
            // TODO
        }
        
        public bool Convert(string srcFilePath, string dstFilePath) {
            // TODO srcFilePath の拡張子が obj でない場合のエラーハンドリング (gmlも同様)
            
            // UnityWebRequest request = UnityWebRequest.Get("file://" + srcFilePath);
            // request.SendWebRequest();
            // int loopCount = 0;
            // while (!request.isDone) {
            //     Thread.Sleep(50);
            //     if (++loopCount > 100) {
            //         request.Abort();
            //         Debug.LogError("Loading obj file is timed out.");
            //         return false;
            //     }
            // }
            // if (request.result != UnityWebRequest.Result.Success) {
            //     Debug.LogError($"Loading obj file resulted {request.result}");
            //     return false;
            // }
            //
            // request.downloadHandler.data

            // TODO このロジック、パスにUnityのAssetsより前にAssetsフォルダがあったら機能しない、警告もある
            int assetsPathStart = srcFilePath.IndexOf("Assets");
            string srcAssetPath = srcFilePath.Substring(assetsPathStart, srcFilePath.Length - assetsPathStart);
            Debug.Log(srcAssetPath);
            var objMesh = AssetDatabase.LoadAssetAtPath<Object>(srcAssetPath);
            string exportPath = Path.Combine(Application.dataPath, "GitIgnored/exportedFbx.fbx");

            // ASCIIかBinaryか変更するにはProjectSettingsから行えるはずだが行えない？
            // ModelExporter.ExportObject(exportPath, objMesh);
            // TODO 設定を反映
            ExportBinaryFBX(exportPath, objMesh);
            return true;

            // 参考: https://docs.unity3d.com/Packages/com.unity.formats.fbx@4.1/api/index.html
            //
            // using (FbxManager fbxManager = FbxManager.Create()) {
            //     fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));
            //     using (FbxExporter exporter = FbxExporter.Create(fbxManager, "ObjToFbxExporter")) {
            //         bool status = exporter.Initialize(exportPath, -1, fbxManager.GetIOSettings());
            //         
            //     }
            //     
            // }

        }
        
        
        // 参考 : https://forum.unity.com/threads/fbx-exporter-binary-export-doesnt-work-via-editor-scripting.1114222/#post-7277590
        private static void ExportBinaryFBX (string filePath, UnityEngine.Object singleObject)
        {
            // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
            Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");
 
            // Instantiate a settings object instance
            MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            object optionsInstance = optionsProperty.Invoke(null, null);
 
            // Change the export setting from ASCII to binary
            FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            exportFormatField.SetValue(optionsInstance, 1);
 
            // Invoke the ExportObject method with the settings param
            MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
            exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
        }
        
    }
}
