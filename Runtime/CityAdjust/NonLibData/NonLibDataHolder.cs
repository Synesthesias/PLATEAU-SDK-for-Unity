using PLATEAU.CityAdjust.AlignLand;
using PLATEAU.Util;
using System.Collections.Generic;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// モデル修正（CityAdjust）の機能の多くは、次のような流れで行われます:
    /// ・UnityのゲームオブジェクトををC++のModelに変換して渡す
    /// ・C++でModelに対して変換処理をする
    /// ・C++のModelをUnityのゲームオブジェクトに変換する
    /// 
    /// しかし、中にはC++には渡さないが覚えておきたいデータがあります。
    /// 例えば<see cref="AlignLandExecutor"/>では、属性情報の内容はC++には渡りませんが、
    /// 属性情報を処理前から処理後にコピーするために覚えておく必要があります。
    /// このように、変換処理のコアロジックとは違うものの、覚えておく必要があるデータを総称して
    /// NonLibData と呼ぶことにし、<see cref="INonLibData"/>を複数覚えておくためのクラスをこのクラスとします。
    /// </summary>
    public class NonLibDataHolder
    {
        private INonLibData[] nonLibs;
        public NonLibDataHolder(params INonLibData[] nonLibs)
        {
            this.nonLibs = nonLibs;
        }

        /// <summary>
        /// 引数のTransformから保持すべきデータを集めて記憶します
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            foreach (var nl in nonLibs)
            {
                nl.ComposeFrom(src);
            }
        }

        /// <summary>
        /// 記憶したデータを引数のTransformに対して復元します
        /// </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            foreach (var nl in nonLibs)
            {
                nl.RestoreTo(target);
            }
        }

    
        /// <summary>
        /// <see cref="INonLibData"/>を型で検索して返します
        /// </summary>
        public T Get<T>() where T : INonLibData
        {
            foreach (var nl in nonLibs)
            {
                if (nl is T found)
                {
                    return found;
                }
            }
            throw new KeyNotFoundException();
        }
    }

    /// <summary>
    /// 上述のNonLibDataです。
    /// 主に変換前のゲームオブジェクトから<see cref="ComposeFrom"/>でデータを集め、
    /// 主に変換後のゲームオブジェクトへ<see cref="RestoreTo"/>でデータを復元します。
    /// </summary>
    public interface INonLibData
    {
        public void ComposeFrom(UniqueParentTransformList src);
        public void RestoreTo(UniqueParentTransformList target);
    }
}