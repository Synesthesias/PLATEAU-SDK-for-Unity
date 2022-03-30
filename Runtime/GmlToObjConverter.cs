using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LibPLATEAU.NET;
using UnityEngine;

namespace PlateauUnitySDK.Runtime {
    /// <summary>
    /// gmlファイルをobjファイルにコンバートします。
    /// </summary>
    public class GmlToObjConverter {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private ObjWriter objWriter = new ObjWriter();
        private CitygmlParserParams gmlParserParams;
        private bool mergeMeshFlg;
        private AxesConversion axesConversion;

        /// <summary>
        /// コンバートの設定を引数で渡します。
        /// </summary>
        /// <param name="optimizeFlg">trueのとき最適化します。</param>
        /// <param name="mergeMeshFlg">trueのとき、メッシュをマージしてオブジェクト数を削減します。</param>
        /// <param name="axesConversion">座標軸の向きです。Unityの場合は通常RUFです。</param>
        public GmlToObjConverter(bool optimizeFlg, bool mergeMeshFlg, AxesConversion axesConversion) {
            this.gmlParserParams.Optimize = optimizeFlg ? 1 : 0;
            this.mergeMeshFlg = mergeMeshFlg;
            this.axesConversion = axesConversion;
        }

        /// <summary>
        /// gmlファイルを変換しobjファイルを出力します。
        /// 成功時はtrue,失敗時はfalseを返します。
        /// </summary>
        public bool Convert(string gmlFilePath, string exportObjFilePath) {
            if (!File.Exists(gmlFilePath)) {
                Debug.LogError($"gml file is not found in the path:\n{gmlFilePath}");
                return false;
            }
            string extension = Path.GetExtension(gmlFilePath).ToLower();
            if (extension != ".gml") {
                Debug.LogError($"The file extension should be '.gml', but actual is {extension}");
                return false;
            }
            try {
                var cityModel = CityGml.Load(gmlFilePath, gmlParserParams);
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
