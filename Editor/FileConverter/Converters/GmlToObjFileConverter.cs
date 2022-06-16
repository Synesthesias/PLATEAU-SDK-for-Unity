using System;
using System.IO;
using System.Linq;
using System.Threading;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// gmlファイルをobjファイルに変換します。
    /// 使い方は <see cref="Config"/> をセットしてから <see cref="Convert"/> を呼んで <see cref="Dispose"/> します。
    /// <see cref="Dispose"/> を忘れると、変換後もファイルが使用中となり外部から変更できなくなります。
    /// usingステートメントを使うことで暗黙的に <see cref="Dispose"/> を呼ぶことができます。
    /// </summary>
    public class GmlToObjFileConverter : IFileConverter, IDisposable
    {
        /// <summary> ObjWriter は変換処理をC++のDLLに委譲します。 </summary>
        private readonly ObjWriter objWriter;

        private GmlToObjFileConverterConfig config;
        private CitygmlParserParams gmlParserParams;

        private int disposed;

        public GmlToObjFileConverter()
        {
            this.objWriter = new ObjWriter();
            this.config = new GmlToObjFileConverterConfig();
            this.gmlParserParams = new CitygmlParserParams();
            ApplyConfig(this.config, ref this.gmlParserParams);
        }

        private void ApplyConfig(GmlToObjFileConverterConfig conf, ref CitygmlParserParams parserParams)
        {
            this.objWriter.SetMeshGranularity(conf.MeshGranularity);
            var logger = this.objWriter.GetDllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            logger.SetLogLevel(conf.LogLevel);
            parserParams.Optimize = conf.OptimizeFlag;
            parserParams.Tessellate = true; // true でないと、頂点の代わりに LinearRing が生成されてしまい 3Dモデルには不適になります。
            this.objWriter.SetDestAxes(conf.AxesConversion);
        }
        
        /// <summary> コンバートの設定です。setで設定を変えます。 </summary>
        public GmlToObjFileConverterConfig Config
        {
            set
            {
                this.config = value;
                ApplyConfig(value, ref this.gmlParserParams);
            }
            get => this.config;
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
        /// 設定で <see cref="gmlParserParams"/> はロード後は変更できませんが、
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
        /// null でなければ、ファイルロードを省略して代わりに渡された <see cref="CityModel"/> を変換します。
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
                    cityModel = CityGml.Load(gmlFilePath, this.gmlParserParams, DllLogCallback.UnityLogCallbacks, this.config.LogLevel);
                }
                
                // 出力先が Assets フォルダ内 かつ すでに同名ファイルが存在する場合、古いファイルを消します。
                // そうしないと上書きによって obj のメッシュ名が変わっても Unity に反映されないことがあるためです。
                if (PathUtil.IsSubDirectoryOfAssets(exportObjFilePath))
                {
                    string assetPath = PathUtil.FullPathToAssetsPath(exportObjFilePath);
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.Refresh();
                }
                
                // ReferencePointを設定します。
                if (this.config.DoAutoSetReferencePoint)
                {
                    this.objWriter.SetValidReferencePoint(cityModel);
                }
                else
                {
                    var referencePoint = this.config.ManualReferencePoint;
                    if (referencePoint == null)
                        throw new Exception($"{nameof(this.config.ManualReferencePoint)} is null.");
                    this.objWriter.ReferencePoint = VectorConverter.ToPlateauVector(referencePoint.Value);
                }
                
                // 変換してファイルに書き込みます。
                this.objWriter.Write(exportObjFilePath, cityModel, gmlFilePath);

                // 出力先が Assets フォルダ内なら、それをUnityに反映させます。
                if (PathUtil.IsSubDirectoryOfAssets(exportObjFilePath))
                {
                    string assetPath = PathUtil.FullPathToAssetsPath(exportObjFilePath);
                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();
                }
                
                // 変換後、3Dメッシュが1つもなければ、ファイルを削除して変換失敗とします。
                // ただし、このチェックは出力パスが Assets フォルダ内である時のみ機能します。
                // TODO この処理は C#側ではなく C++側で実装したい
                if (PathUtil.IsSubDirectoryOfAssets(exportObjFilePath))
                {
                    string assetPath = PathUtil.FullPathToAssetsPath(exportObjFilePath);
                    // AssetDatabase は Unityプロジェクトの外のパスでは動かないので、不本意ながら「Assetsフォルダ内にあるときのみチェック」という条件ができてしまいます。
                    var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    if (!assets.OfType<Mesh>().Any())
                    {
                        Debug.LogError($"No mesh found. Deleting output obj file.\ngml path = {gmlFilePath}");
                        AssetDatabase.DeleteAsset(assetPath);
                        return false;
                    }
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
            if (!PathUtil.IsValidInputFilePath(gmlFilePath, "gml", false)) return false;
            if (!PathUtil.IsValidOutputFilePath(exportObjFilePath, "obj")) return false;
            return true;
        }


        /// <summary> objWriterに渡すパスの区切り文字は '/' である必要があるので '/' にします。 </summary>
        private static void SlashPath(ref string path)
        {
            path = path.Replace('\\', '/');
        }

        /// <summary>
        /// <paramref name="cityModel"/> に適する ReferencePointを計算し、
        /// その座標を変換の基準点として設定し、その座標を返します。
        /// </summary>
        public Vector3 SetValidReferencePoint(CityModel cityModel)
        {
            this.objWriter.SetValidReferencePoint(cityModel);
            var plateauCoord = this.objWriter.ReferencePoint;
            var coord = VectorConverter.ToUnityVector(plateauCoord);
            this.config.ManualReferencePoint = coord;
            return coord;
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