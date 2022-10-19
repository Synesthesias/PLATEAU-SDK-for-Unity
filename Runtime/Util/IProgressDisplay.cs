namespace PLATEAU.Util
{
    public interface IProgressDisplay
    {
        public void SetProgress(string progressName, float percentage, string message);
    }
    
    public class DisplayedProgress
    {
        public string Name;
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
    
}
