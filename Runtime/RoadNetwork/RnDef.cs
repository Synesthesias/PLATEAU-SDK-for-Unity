namespace PLATEAU.RoadNetwork
{

    // 左右を表すenum
    public enum RnDir
    {
        /// <summary>
        /// 左
        /// </summary>
        Left,
        /// <summary>
        /// 右
        /// </summary>
        Right
    }

    public static class RnDirEx
    {
        /// <summary>
        /// 反対を取得
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static RnDir GetOpposite(this RnDir dir)
        {
            return dir == RnDir.Left ? RnDir.Right : RnDir.Left;
        }
    }
}