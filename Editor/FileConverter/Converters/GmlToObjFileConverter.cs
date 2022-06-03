using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// gmlファイルをobjファイルに変換します。
    /// 変換後は <see cref="Dispose"/> を呼ぶか、usingステートメントを使ってobjファイルを解放してください。
    /// </summary>
    public class GmlToObjFileConverter : IFileConverter, IDisposable
    {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private readonly ObjWriter objWriter;

        private CitygmlParserParams gmlParserParams;
        private MeshGranularity meshGranularity;
        private AxesConversion axesConversion;
        private static readonly LogCallbackFuncType logCallback = (ptr) => Debug.Log(Marshal.PtrToStringAnsi(ptr));

        private int disposed;

        public GmlToObjFileConverter()
        {
            this.objWriter = new ObjWriter();
            this.objWriter.SetLogCallback(logCallback);
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
                
                var cityModel = CityGml.Load(gmlFilePath, this.gmlParserParams);
                this.objWriter.SetValidReferencePoint(cityModel);
                this.objWriter.SetMeshGranularity(this.meshGranularity);
                this.objWriter.SetDestAxes(this.axesConversion);
                this.objWriter.Write(exportObjFilePath, cityModel, gmlFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"gml convert is failed.\n{e}");
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