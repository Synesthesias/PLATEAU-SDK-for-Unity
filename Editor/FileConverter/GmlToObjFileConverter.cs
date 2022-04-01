using System;
using LibPLATEAU.NET;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    /// <summary>
    /// gmlファイルをobjファイルにコンバートします。
    /// </summary>
    public class GmlToObjFileConverter : IFileConverter {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private readonly ObjWriter objWriter = new ObjWriter();
        private CitygmlParserParams gmlParserParams;
        private bool mergeMeshFlg;
        private AxesConversion axesConversion;

        

        /// <summary>
        /// コンバートの設定を引数で渡します。
        /// </summary>
        /// <param name="optimizeFlg">trueのとき最適化します。</param>
        /// <param name="mergeMeshFlgArg">trueのとき、メッシュをマージしてオブジェクト数を削減します。</param>
        /// <param name="axesConversionArg">座標軸の向きです。Unityの場合は通常RUFです。</param>
        public void SetConfig(bool optimizeFlg, bool mergeMeshFlgArg, AxesConversion axesConversionArg) {
            this.gmlParserParams.Optimize = optimizeFlg ? 1 : 0;
            this.mergeMeshFlg = mergeMeshFlgArg;
            this.axesConversion = axesConversionArg;
        }

        /// <summary>
        /// gmlファイルを変換しobjファイルを出力します。
        /// 成功時はtrue,失敗時はfalseを返します。
        /// </summary>
        public bool Convert(string gmlFilePath, string exportObjFilePath) {
            if (!FilePathValidator.IsValidInputFilePath(gmlFilePath, "gml", false)) return false;
            if (!FilePathValidator.IsValidOutputFilePath(exportObjFilePath, "obj")) return false;

            try {
                var cityModel = CityGml.Load(gmlFilePath, this.gmlParserParams);
                this.objWriter.SetValidReferencePoint(cityModel);
                this.objWriter.SetMergeMeshFlg(this.mergeMeshFlg);
                this.objWriter.SetDestAxes(this.axesConversion);
                this.objWriter.Write(exportObjFilePath, cityModel, gmlFilePath);
            }
            catch (Exception e) {
                Debug.LogError($"gml convert is failed.\n{e}");
                return false;
            }
            return true;
        }

    }
}
