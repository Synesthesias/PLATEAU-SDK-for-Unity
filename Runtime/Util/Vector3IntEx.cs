using UnityEngine;

namespace PLATEAU.Util
{
    public static class Vector3IntEx
    {
        /// <summary> 各要素をmin, maxでクランプ </summary>
        public static Vector3Int Clamp(this Vector3Int x, int min, int max)
        {
            return new Vector3Int(Mathf.Clamp(x.x, min, max), Mathf.Clamp(x.y, min, max), Mathf.Clamp(x.z, min, max));
        }

        /// <summary> 各要素をmin,maxの各要素でクランプ </summary>
        public static Vector3Int Clamp(Vector3Int x, Vector3Int min, Vector3Int max)
        {
            return new Vector3Int(Mathf.Clamp(x.x, min.x, max.x), Mathf.Clamp(x.y, min.y, max.y), Mathf.Clamp(x.x, min.x, max.x));
        }

        /// <summary> xを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutX(this Vector3Int self, int x)
        {
            self.x = x;
            return self;
        }

        /// <summary> xyを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutXy(this Vector3Int self, int x, int y)
        {
            self.x = x;
            self.y = y;
            return self;
        }

        /// <summary> xzを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutXz(this Vector3Int self, int x, int z)
        {
            self.x = x;
            self.z = z;
            return self;
        }

        /// <summary> yを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutY(this Vector3Int self, int y)
        {
            self.y = y;
            return self;
        }

        /// <summary> yzを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutYz(this Vector3Int self, int y, int z)
        {
            self.y = y;
            self.z = z;
            return self;
        }

        /// <summary> zを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutZ(this Vector3Int self, int z)
        {
            self.z = z;
            return self;
        }

        /// <summary> xzを差し替えたVector3Int返す。非破壊 </summary>
        public static Vector3Int PutXy(this Vector3Int self, Vector2Int xy)
        {
            self.x = xy.x;
            self.y = xy.y;
            return self;
        }

        /// <summary>
        /// new Vector2Int(self.x, self.y);
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2Int Xy(this Vector3Int self)
        {
            return new Vector2Int(self.x, self.y);
        }

        /// <summary>
        /// new Vector2Int(self.x, self.z);
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2Int Xz(this Vector3Int self)
        {
            return new Vector2Int(self.x, self.z);
        }


        /// <summary>
        /// new Vector2Int(self.x, self.z);
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2Int Yz(this Vector3Int self)
        {
            return new Vector2Int(self.y, self.z);
        }

        /// <summary>
        /// Mathf.Min(self.z, Mathf.Min(self.x, self.y))
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int Min(this Vector3Int self)
        {
            return Mathf.Min(self.z, Mathf.Min(self.x, self.y));
        }

        /// <summary>
        /// Mathf.Max(self.z, Mathf.Max(self.x, self.y))
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int Max(this Vector3Int self)
        {
            return Mathf.Max(self.z, Mathf.Max(self.x, self.y));
        }

        /// <summary>
        /// self.x + self.y
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int Sum(this Vector3Int self)
        {
            return self.x + self.y + self.z;
        }

        /// <summary>
        /// new Vector3Int(Mathf.Abs(self.x), Mathf.Abs(self.y), Mathf.Abs(self.z))
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector3Int Abs(this Vector3Int self)
        {
            self.x = Mathf.Abs(self.x);
            self.y = Mathf.Abs(self.y);
            self.z = Mathf.Abs(self.z);
            return self;
        }

        /// <summary>
        /// new Vector3Int(self.x * mul.x, self.y * mul.y, self.z * mul.z)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="mul"></param>
        /// <returns></returns>
        public static Vector3Int Scaled(this Vector3Int self, Vector3Int mul)
        {
            self.x *= mul.x;
            self.y *= mul.y;
            self.z *= mul.z;
            return self;
        }

        /// <summary>
        /// new Vector3Int(self.x / div.x, self.y / div.y, self.z / div.z)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static Vector3Int RevScaled(this Vector3Int self, Vector3Int div)
        {
            self.x /= div.x;
            self.y /= div.y;
            self.z /= div.z;
            return self;
        }

        /// <summary> Vector3Int.Scaleの逆数版 a / b </summary>
        public static Vector3Int RevScale(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 ToVector3(this Vector3Int self)
        {
            return new Vector3(self.x, self.y, self.z);
        }
    }
}
