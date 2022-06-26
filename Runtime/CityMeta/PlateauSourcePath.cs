using System;
using System.IO;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// Plateauの元データのパスです。
    /// 2つの役割があります。
    /// ・インポート時の設定として <see cref="CityMetaData"/> に 元データパスを保持、記録されること
    /// ・udxフォルダのパスから、関連パスを取得できるようにすること。
    /// </summary>
    [Serializable]
    internal class PlateauSourcePath
    {
        /// <summary>
        /// Plateau元データのパスの記録です。
        /// </summary>
        public string udxFullPath;

        public PlateauSourcePath(string udxFullPath)
        {
            this.udxFullPath = udxFullPath;
        }
        
        /// <summary>
        /// 元データのRoot (= udxの親) フォルダの名前です。
        /// </summary>
        public string RootDirName
        {
            get
            {
                string root = RootFullPath;
                return Path.GetFileName(Path.GetDirectoryName(root));
            }
            
        }
        
        /// <summary>
        /// 元データのRoot (= udxの親) フォルダのパスです。
        /// </summary>
        public string RootFullPath => Path.GetFullPath(Path.Combine(this.udxFullPath, "../"));

        /// <summary>
        /// udxフォルダからの相対パスを受け取り、そのフルパスを返します。
        /// </summary>
        public string RelativeToFullPath(string relativePathFromUdx)
        {
            return Path.GetFullPath(Path.Combine(this.udxFullPath, relativePathFromUdx));
        }
    }
}