using System;
using System.IO;
using System.Threading;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.Util;
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
        private MeshGranularity meshGranularity;
        private AxesConversion axesConversion;
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
        /// <param name="meshGranularityArg">メッシュのオブジェクト分けの粒度です。</param>
        /// <param name="axesConversionArg">座標軸の向きです。Unityの場合は通常RUFです。</param>
        /// <param name="optimizeFlg">trueのとき最適化します。</param>
        public void SetConfig(MeshGranularity meshGranularityArg, AxesConversion axesConversionArg = AxesConversion.RUF,
            bool optimizeFlg = true)
        {
            this.gmlParserParams.Optimize = optimizeFlg;
            this.gmlParserParams.Tessellate = true; // true でないと、頂点の代わりに LinearRing が生成されてしまい 3Dモデルには不適になります。
            this.meshGranularity = meshGranularityArg;
            this.axesConversion = axesConversionArg;
        }

        /// <summary>
        /// gmlファイルを変換しobjファイルを出力します。
        /// 成功時はtrue,失敗時はfalseを返します。
        /// </summary>
        public bool Convert(string gmlFilePath, string exportObjFilePath)
        {
            if (!FilePathValidator.IsValidInputFilePath(gmlFilePath, "gml", false)) return false;
            if (!FilePathValidator.IsValidOutputFilePath(exportObjFilePath, "obj")) return false;
            try
            {
                // DLL側の実装の都合上、ここではパス区切りはスラッシュとします。 
                gmlFilePath = gmlFilePath.Replace('\\', '/');
                exportObjFilePath = exportObjFilePath.Replace('\\', '/');

                var cityModel = CityGml.Load(gmlFilePath, this.gmlParserParams, DllLogCallback.UnityLogCallbacks, this.logLevel);
                this.objWriter.SetValidReferencePoint(cityModel);
                this.objWriter.SetMeshGranularity(this.meshGranularity);
                this.objWriter.SetDestAxes(this.axesConversion);
                this.objWriter.Write(exportObjFilePath, cityModel, gmlFilePath);
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