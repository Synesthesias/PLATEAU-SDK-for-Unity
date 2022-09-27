namespace PLATEAU.Util
{
    public interface IProgressDisplay
    {
        public void SetProgress(string progressName, float percentage, string message);
    }
}
