using System;
using System.IO;
using System.Threading;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// gmlファイルをobjファイルに変換します。
    /// 使い方は <see cref="SetConfig"/> してから <see cref="Convert"/> を呼んで <see cref="Dispose"/> します。
    /// <see cref="Dispose"/> を忘れると、変換後もファイルが使用中となり外部から変更できなくなります。
    /// usingステートメントを使うことで暗黙的に <see cref="Dispose"/> を呼ぶことができます。
    /// </summary>
    public class GmlToObjFileConverter : IFileConverter, IDisposable
    {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private readonly ObjWriter objWriter;

        private CitygmlParserParams gmlParserParams;
        // private MeshGranularity meshGranularity;
        // private AxesConversion axesConversion;
        private DllLogLevel logLevel;

        private int disposed;

        public GmlToObjFileConverter(DllLogLevel logLevel = DllLogLevel.Error)
        {
            this.objWriter = new ObjWriter();
            var logger = this.objWriter.GetDllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            logger.SetLogLevel(logLevel);
            this.logLevel = logLevel;
            this.gmlParserParams = new CitygmlParserParams(true, true);
        }


        /// <summary>
        /// コンバートの設定を引数で渡します。
        /// </summary>
        /// <param name="meshGranularity">メッシュのオブジェクト分けの粒度です。</param>
        /// <param name="axesConversion">座標軸の向きです。Unityの場合は通常RUFです。</param>
        /// <param name="optimizeFlg">trueのとき最適化します。</param>
        public void SetConfig(MeshGranularity meshGranularity, AxesConversion axesConversion = AxesConversion.RUF,
            bool optimizeFlg = true)
        {
            this.gmlParserParams.Optimize = optimizeFlg;
            this.gmlParserParams.Tessellate = true; // true でないと、頂点の代わりに LinearRing が生成されてしまい 3Dモデルには不適になります。
            this.objWriter.SetMeshGranularity(meshGranularity);
            this.objWriter.SetDestAxes(axesConversion);
        }

        /// <summary>
        /// gmlファイルをロードし、変換してobjファイルを出力します。
        /// 成功時はtrue,失敗時はfalseを返します。
        /// </summary>
        public bool Convert(string gmlFilePath, string exportObjFilePath)
        {
            return ConvertInner(gmlFilePath, exportObjFilePath, null);
        }

        /// <summary>
        /// gmlファイルのロードを省略し、代わりにロード済みの cityModel を使って変換します。
        /// 成否を bool で返します。
        /// <see cref="SetConfig"/> による設定で <see cref="gmlParserParams"/> はロード後は変更できませんが、
        /// meshGranularity, axesConversion の設定は反映されます。
        /// </summary>
        public bool ConvertWithoutLoad(CityModel cityModel, string gmlFilePath, string exportObjFilePath)
        {
            if (cityModel == null)
            {
                Debug.LogError("cityModel is null.");
                return false;
            }

            return ConvertInner(gmlFilePath, exportObjFilePath, cityModel);
        }
        
        /// <summary>
        /// 変換のインナーメソッドです。
        /// 引数の <paramref name="cityModel"/> が null なら gmlファイルをロードして変換します。
        /// null でない <paramref name="cityModel"/> が渡されたら、ロードを省略してそのモデルを変換します。
        /// 成否をboolで返します。
        /// </summary>
        private bool ConvertInner(string gmlFilePath, string exportObjFilePath, CityModel cityModel)
        {
            if (!IsPathValid(gmlFilePath, exportObjFilePath)) return false;
            try
            {
                SlashPath(ref gmlFilePath);
                SlashPath(ref exportObjFilePath);

                if (cityModel == null)
                {
                    cityModel = CityGml.Load(gmlFilePath, this.gmlParserParams, DllLogCallback.UnityLogCallbacks, this.logLevel);
                }
                
                // 出力先が Assets フォルダ内 かつ すでに同名ファイルが存在する場合、古いファイルを消します。
                // そうしないと上書きによって obj のメッシュ名が変わっても Unity に反映されないことがあるためです。
                if (FilePathValidator.IsSubDirectoryOfAssets(exportObjFilePath))
                {
                    string assetPath = FilePathValidator.FullPathToAssetsPath(exportObjFilePath);
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.Refresh();
                }
                
                this.objWriter.SetValidReferencePoint(cityModel);
                this.objWriter.Write(exportObjFilePath, cityModel, gmlFilePath);
                
                // 出力先が Assets フォルダ内なら、それをUnityに反映させます。
                if (FilePathValidator.IsSubDirectoryOfAssets(exportObjFilePath))
                {
                    string assetPath = FilePathValidator.FullPathToAssetsPath(exportObjFilePath);
                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();
                }
            }
            catch (FileLoadException e)
            {
                Debug.LogError($"Failed to load gml file.\n gml path = {gmlFilePath}\n{e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Gml to obj convert is Failed.\ngml path = {gmlFilePath}\n{e}");
                return false;
            }

            return true;
        }

        private static bool IsPathValid(string gmlFilePath, string exportObjFilePath)
        {
            if (!FilePathValidator.IsValidInputFilePath(gmlFilePath, "gml", false)) return false;
            if (!FilePathValidator.IsValidOutputFilePath(exportObjFilePath, "obj")) return false;
            return true;
        }

        /// <summary> objWriterに渡すパスの区切り文字は '/' である必要があるので '/' にします。 </summary>
        private static void SlashPath(ref string path)
        {
            path = path.Replace('\\', '/');
        }
        
        /// <summary>
        /// 変換後の obj ファイルを開放したい時に呼びます。
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                this.objWriter.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        ~GmlToObjFileConverter()
        {
            Dispose();
        }
    }
}