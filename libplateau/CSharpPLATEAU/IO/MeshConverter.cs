using PLATEAU.CityGML;
using PLATEAU.Interop;
using System;
using System.Threading;

namespace PLATEAU.IO
{
    /// <summary>
    /// GMLファイルをメッシュファイルに変換する機能を提供します。
    /// </summary>
    public class MeshConverter : IDisposable
    {
        private readonly IntPtr handle;
        private int disposed;

        private MeshConvertOptions options = null;

        /// <summary>
        /// メッシュ変換オプションを取得または設定します。
        /// </summary>
        public MeshConvertOptions Options
        {
            get
            {
                if (this.options == null)
                {
                    APIResult result = NativeMethods.plateau_mesh_converter_get_options(this.handle, out var value);
                    DLLUtil.CheckDllError(result);
                    this.options = new MeshConvertOptions(value);
                }

                return this.options;
            }
            set
            {
                this.options = value;
                ApplyOptions();
            }
        }

        /// <summary>
        /// <see cref="MeshConverter"/>クラスのインスタンスを初期化します。
        /// </summary>
        public MeshConverter()
        {
            APIResult result = NativeMethods.plateau_create_mesh_converter(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
        }

        ~MeshConverter()
        {
            Dispose();
        }

        /// <summary>
        /// セーフハンドルを取得します。
        /// </summary>
        public IntPtr Handle => this.handle;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_delete_mesh_converter(this.handle);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// GMLファイルをメッシュファイルに変換します。<br/>
        /// 
        /// 変換後のメッシュファイル(OBJもしくはglTF)は<paramref name="destinationDirectory"/>に<em>LOD{LODの値}_{gmlファイル名}.{拡張子}</em>という名前で格納され、
        /// .gmlから参照されるテクスチャファイル一式は<paramref name="destinationDirectory"/>にコピーされます。<br/>
        /// <paramref name="cityModel"/>は以下を満たしている必要があります。<br/>
        /// - <see cref="CitygmlParserParams.Tessellate"/>オプションがtrueでパースされていること<br/>
        /// - <paramref name="gmlPath"/>がパースされた都市モデルであること
        /// </summary>
        /// <param name="destinationDirectory">出力先ディレクトリ</param>
        /// <param name="gmlPath">入力GMLファイル</param>
        /// <param name="cityModel"><paramref name="gmlPath"/>がパースされた都市モデル。nullの場合は内部でパースされます。</param>
        /// <param name="logger">内部ログ受け取り用の<see cref="DllLogger"/>インスタンス</param>
        public string[] Convert(string destinationDirectory, string gmlPath, CityModel cityModel = null, DllLogger logger = null)
        {
            ApplyOptions();

            var cityModelHandle = cityModel?.Handle ?? IntPtr.Zero;
            var loggerHandle = logger?.Handle ?? IntPtr.Zero;
            APIResult result = NativeMethods.plateau_mesh_converter_convert(this.handle, destinationDirectory, gmlPath, cityModelHandle, loggerHandle);
            DLLUtil.CheckDllError(result);
            string[] exportedFileNames = DLLUtil.GetNativeStringArrayByPtr(this.handle,
                NativeMethods.plateau_mesh_converter_get_last_exported_model_file_names_count,
                NativeMethods.plateau_mesh_converter_get_last_exported_model_file_names);
            return exportedFileNames;
        }

        private void ApplyOptions()
        {
            if (this.options == null)
                return;

            APIResult result = NativeMethods.plateau_mesh_converter_set_options(this.handle, this.options.Data);
            DLLUtil.CheckDllError(result);
        }
    }
}
