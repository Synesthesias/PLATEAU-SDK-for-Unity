using System;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// gmlファイルをobjファイルにコンバートします。
    /// </summary>
    public class GmlToObjFileConverter : IFileConverter
    {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private readonly ObjWriter objWriter = new ObjWriter();

        private CitygmlParserParams gmlParserParams;
        private MeshGranularity meshGranularity;
        private AxesConversion axesConversion;


        /// <summary>
        /// コンバートの設定を引数で渡します。
        /// </summary>
        /// <param name="optimizeFlg">trueのとき最適化します。</param>
        /// <param name="meshGranularityArg">メッシュのオブジェクト分けの粒度です。</param>
        /// <param name="axesConversionArg">座標軸の向きです。Unityの場合は通常RUFです。</param>
        public void SetConfig(bool optimizeFlg, MeshGranularity meshGranularityArg, AxesConversion axesConversionArg)
        {
            this.gmlParserParams = new CitygmlParserParams(optimizeFlg);
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
    }
}