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
    public class Prototype_ObjToFbxConverter {
        public void ConvertFile() {
            var objMesh = AssetDatabase.LoadAssetAtPath<Object>("Assets/GitIgnored/exported.obj");
            string exportPath = Path.Combine(Application.dataPath, "GitIgnored/exportedFbx.fbx");
            Debug.Log($"objMesh: {objMesh}\nexportPath: {exportPath}");

            // ASCIIかBinaryか変更するにはProjectSettingsから行えるはずだが行えない？
            // ModelExporter.ExportObject(exportPath, objMesh);
            ExportBinaryFBX(exportPath, objMesh);

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
