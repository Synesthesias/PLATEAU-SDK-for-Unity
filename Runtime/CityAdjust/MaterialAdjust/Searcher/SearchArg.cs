using PLATEAU.Util;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// マテリアル分けのキーを検索するための条件です。
    /// </summary>
    public class SearchArg
    {
        public SearchArg(UniqueParentTransformList targetTransforms)
        {
            TargetTransforms = targetTransforms;
        }
        public UniqueParentTransformList TargetTransforms;
    }

    /// <summary>
    /// <see cref="SearchArg"/>の属性情報を基準とする版です。
    /// </summary>
    internal class SearchArgByArr : SearchArg
    {
        public SearchArgByArr(UniqueParentTransformList targetTransforms, string attrKey)
            : base(targetTransforms)
        {
            AttrKey = attrKey;
        }
        public string AttrKey;
    }
}