namespace PLATEAU.Util
{
    public interface IProgressDisplay
    {
        /// <summary>
        /// 進捗情報をセットします。
        /// <paramref name="progressName"/> をキーとし、キーがすでにあれば進捗を更新、
        /// なければ追加します。
        /// 別スレッドから呼ばれることがあります。
        /// </summary>
        public void SetProgress(string progressName, float percentage, string message);
    }
    
    public class DisplayedProgress
    {
        public readonly string Name;
        public float Percentage;
        public string Message;
        public string PercentageStr => this.Percentage.ToString("00") + "%";

        public DisplayedProgress(string name, float percentage, string message)
        {
            this.Name = name;
            this.Percentage = percentage;
            this.Message = message;
        }
    }
    
    public class DummyProgressDisplay : IProgressDisplay
    {
        public void SetProgress(string progressName, float percentage, string message){}
    }
    
}
