namespace PLATEAU.RoadNetwork.Util
{
    /// <summary>
    /// 主にVector2の引数にVector3渡そうとしたときに暗黙の型変換でおかしくなるのを回避するための対応
    /// 引数をRnExplicit&lt;Vector2&gt;にすることで、Vector3を渡すときに暗黙の型変換ができないようにする
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RnExplicit<T>
    {
        public T V { get; set; }

        private RnExplicit(T v)
        {
            V = v;
        }

        public static implicit operator RnExplicit<T>(T v)
        {
            return new RnExplicit<T>(v);
        }

        public static implicit operator T(RnExplicit<T> v)
        {
            return v.V;
        }
    }
}