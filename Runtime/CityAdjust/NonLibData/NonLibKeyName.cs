using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary> 変換の前後でゲームオブジェクトを特定するための情報であり、ゲームオブジェクト名とその親の名前をキーとします。 </summary>
    internal class NonLibKeyName
    {
        public string ObjName { get; }
        public string ParentName { get; }

        public NonLibKeyName(Transform obj)
        {
            ObjName = obj.name;
            ParentName = obj.parent == null ? "" : obj.parent.name;
        }

        public override bool Equals(object obj)
        {
            if (obj is NonLibKeyName other)
            {
                return ObjName == other.ObjName && ParentName == other.ParentName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        
        public override string ToString()
        {
            return $"{ParentName}/{ObjName}";
        }
    }
}