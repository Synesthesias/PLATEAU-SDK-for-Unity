
using System;
using System.IO;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    public static class CityGml
    {
        /// <summary>
        /// DLLの機能によって gmlファイルをパースし、CityModelを生成します。
        /// </summary>
        /// <param name="gmlPath">gmlファイルのパスです。</param>
        /// <param name="parserParams">変換の設定です。</param>
        /// <param name="logCallbacks">ログを受け取るコールバックです。省略または null の場合は C# の標準出力にログを転送します。</param>
        /// <param name="logLevel">ログの詳細度です。</param>
        public static CityModel Load(
            string gmlPath, CitygmlParserParams parserParams,
            LogCallbacks logCallbacks = null,
            DllLogLevel logLevel = DllLogLevel.Error
        )
        {
            if (logCallbacks == null)
            {
                logCallbacks = LogCallbacks.StdOut;
            }
            APIResult result = NativeMethods.plateau_load_citygml(
                gmlPath, parserParams, out IntPtr cityModelHandle,
                logLevel, logCallbacks.LogErrorFuncPtr, logCallbacks.LogWarnFuncPtr, logCallbacks.LogInfoFuncPtr);
            if (result == APIResult.ErrorLoadingCityGml)
            {
                throw new FileLoadException(
                    $"Loading gml failed.\nPlease check codelist xml files are located in (gmlFolder)/../../codelists\nAND gml file is located at {gmlPath}\nand ");
            }
            DLLUtil.CheckDllError(result);
            return new CityModel(cityModelHandle);
        }
    }
}
