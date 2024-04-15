using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// マテリアル分けのキーを検索するための条件です。
    /// </summary>
    internal class SearchArg
    {
        public SearchArg(IReadOnlyCollection<GameObject> targetObjs)
        {
            TargetObjs = targetObjs;
        }
        public IReadOnlyCollection<GameObject> TargetObjs;
    }

    /// <summary>
    /// <see cref="SearchArg"/>の属性情報を基準とする版です。
    /// </summary>
    internal class SearchArgByArr : SearchArg
    {
        public SearchArgByArr(IReadOnlyCollection<GameObject> targetObjs, string attrKey)
            : base(targetObjs)
        {
            AttrKey = attrKey;
        }
        public string AttrKey;
    }
}