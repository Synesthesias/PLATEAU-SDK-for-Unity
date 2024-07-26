using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// 変換の前後でゲームオブジェクトを特定するための情報であり、
    /// ある基準Transformからのパス(スラッシュでつないだもの)をキーとします。
    /// </summary>
    internal class NonLibKeyName
    {
        public string Path { get; }
        public string ObjName { get; }

        public NonLibKeyName(Transform obj, Transform[] baseTransforms)
        {
            Path = MakeKey(obj, baseTransforms);
            ObjName = obj.name;
        }

        public override bool Equals(object obj)
        {
            if (obj is NonLibKeyName other)
            {
                return Path == other.Path;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private string MakeKey(Transform obj, Transform[] baseTransforms)
        {
            List<string> pathes = new();
            var current = obj;
            while (current != null && !baseTransforms.Contains(current))
            {
                pathes.Add(current.name);
                current = current.parent;
            }

            var sb = new StringBuilder();
            for (int i = pathes.Count - 1; i >= 0; i--)
            {
                sb.Append(pathes[i]);
                if(i != 0) sb.Append('/');
            }

            return sb.ToString();
        }
        
        public override string ToString()
        {
            return Path;
        }
    }
}