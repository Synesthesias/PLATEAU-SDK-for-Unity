namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// マテリアル分けのための検索機能を提供するインターフェースです。
    /// </summary>
    internal interface ISearcher<out KeyT>
    {
        public KeyT[] Search();
    }
}