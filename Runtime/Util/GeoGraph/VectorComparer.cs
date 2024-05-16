using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{

    public class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            var dx = x.x.CompareTo(y.x);
            if (dx != 0)
            {
                return dx;
            }

            var dy = x.y.CompareTo(y.y);
            if (dy != 0)
            {
                return dy;
            }

            return 0;
        }
    }
    public class Vector2IntComparer : IComparer<Vector2Int>
    {
        public int Compare(Vector2Int x, Vector2Int y)
        {
            var dx = x.x.CompareTo(y.x);
            if (dx != 0)
            {
                return dx;
            }

            var dy = x.y.CompareTo(y.y);
            if (dy != 0)
            {
                return dy;
            }

            return 0;
        }
    }

    public class Vector3Comparer : IComparer<Vector3>
    {
        public int Compare(Vector3 x, Vector3 y)
        {
            var dx = x.x.CompareTo(y.x);
            if (dx != 0)
            {
                return dx;
            }

            var dy = x.y.CompareTo(y.y);
            if (dy != 0)
            {
                return dy;
            }

            var dz = x.z.CompareTo(y.z);
            if (dz != 0)
            {
                return dz;
            }
            return 0;
        }
    }

    public class Vector3IntComparer : IComparer<Vector3Int>
    {
        public int Compare(Vector3Int x, Vector3Int y)
        {
            var dx = x.x.CompareTo(y.x);
            if (dx != 0)
            {
                return dx;
            }

            var dy = x.y.CompareTo(y.y);
            if (dy != 0)
            {
                return dy;
            }

            var dz = x.z.CompareTo(y.z);
            if (dz != 0)
            {
                return dz;
            }
            return 0;
        }
    }
}