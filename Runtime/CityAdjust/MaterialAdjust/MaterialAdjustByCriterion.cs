using System;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// マテリアル分けの対象検索と設定、実行を行います。
    /// 検索の基準が地物型であるか、属性情報であるかの違いを吸収するため、
    /// コンストラクタで注入された型によって処理を分けます。具体的には:
    /// 地物型で検索する場合はコンストラクタに<see cref="MaterialAdjustExecutorByType"/>を渡し、
    /// 属性情報で検索する場合はコンストラクタに<see cref="MaterialAdjustExecutorByAttr"/>を渡します。
    /// </summary>
    internal class MaterialAdjustByCriterion
    {
        public IMaterialAdjustExecutor AdjustExecutor { get; }
        public IMaterialAdjustConf MaterialAdjustConf { get; private set; }
        public bool IsSearched { get; set; }

        public MaterialAdjustByCriterion(IMaterialAdjustExecutor adjustExecutor)
        {
            this.AdjustExecutor = adjustExecutor;
        }
        
        /// <summary> 対象を選択して「検索」ボタンを押したときの処理です。成功したかどうかをboolで返します。 </summary>
        public bool Search(SearchArg searchArg)
        {
            ISearcher<object> searcher = AdjustExecutor switch
            {
                MaterialAdjustExecutorByAttr => new AttrSearcher((SearchArgByArr)searchArg),
                MaterialAdjustExecutorByType => new TypeSearcher(searchArg),
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
}