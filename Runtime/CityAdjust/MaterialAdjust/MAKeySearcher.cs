using System;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityInfo;
using PLATEAU.Util;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// マテリアル分けの基準となるキーを対象から探し、マテリアル設定を構築します。
    /// </summary>
    internal class MAKeySearcher
    {
        public MaterialCriterion Criterion { get; private set; }
        public IMaterialAdjustConf MaterialAdjustConf { get; private set; }
        public bool IsSearched { get; set; }

        public MAKeySearcher(MaterialCriterion criterion)
        {
            Criterion = criterion;
        }
        
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。成功したかどうかをboolで返します。 </summary>
        public bool Search(SearchArg searchArg)
        {
            ISearcher<object> searcher = Criterion switch
            {
                MaterialCriterion.ByAttribute => new AttrSearcher((SearchArgByArr)searchArg),
                MaterialCriterion.ByType => new TypeSearcher(searchArg),
                _ => throw new ArgumentException()
            };
            var searchResult = searcher.Search();
            if (searchResult.Length <= 0)
            {
                Dialogue.Display("対象が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                return false;
            }

            MaterialAdjustConf = searchResult switch
            {
                CityObjectTypeHierarchy.Node[] nodes => new MaterialAdjustConf<CityObjectTypeHierarchy.Node>(nodes),
                string[] strings => new MaterialAdjustConf<string>(strings),
                _ => throw new ArgumentException()
            };
            IsSearched = true;
            return true;
        }
    }
    
    /// <summary>
    /// マテリアル分けの基準です。
    /// </summary>
    internal enum MaterialCriterion
    {
        ByType, ByAttribute
    }
}