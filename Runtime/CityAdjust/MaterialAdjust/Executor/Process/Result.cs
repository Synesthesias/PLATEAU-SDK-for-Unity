namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// 何か処理をしたとして、処理の結果となるデータと、処理の成否をまとめて返すためのクラスです。
    /// </summary>
    public class Result<T>
    {
        public bool IsSucceed { get; }
        public T Get { get; }
        
        public Result(bool isSucceed, T returnValue)
        {
            IsSucceed = isSucceed;
            Get = returnValue;
        }
    }
}