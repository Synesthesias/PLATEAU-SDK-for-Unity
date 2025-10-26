using PLATEAU.CityInfo;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 主要地物単位のPLATEAUCityObjectGroupのまとまりを表すためのキー
    /// 最小地物の歩道/道路がもともと所属していた主要地物のGmlキーを持つ
    /// </summary>
    [Serializable]
    public struct RnCityObjectGroupKey: IEquatable<RnCityObjectGroupKey>

    {
        [field:SerializeField]
        public string GmlId { get; set; }
        
        public bool IsValid => !string.IsNullOrEmpty(GmlId);

        public RnCityObjectGroupKey(string gmlId)
        {
            GmlId = gmlId;
        }
        
        /// <summary>
        /// other.GmlIDと一致するかどうか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(CityObjectList.CityObject other)
        {
            return other != null && other.GmlID == GmlId;
        }

        /// <summary>
        /// otherのPrimaryCityObjectsのどれかと一致するかどうか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualAny(PLATEAUCityObjectGroup other)
        {
            if (!other)
                return false;
            foreach (var c in other.PrimaryCityObjects)
            {
                if (Equals(c))
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// RnCityObjectGroupKey 同士の等価比較（GmlId の厳密一致）
        /// </summary>
        public bool Equals(RnCityObjectGroupKey other)
        {
            return string.Equals(GmlId, other.GmlId, StringComparison.Ordinal);
        }

        /// <summary>
        /// object からの等価比較
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is RnCityObjectGroupKey other && Equals(other);
        }

        /// <summary>
        /// ハッシュコード（GmlId を基に計算）
        /// </summary>
        public override int GetHashCode()
        {
            return GmlId != null ? StringComparer.Ordinal.GetHashCode(GmlId) : 0;
        }

        public static bool operator ==(RnCityObjectGroupKey left, RnCityObjectGroupKey right) => left.Equals(right);
        public static bool operator !=(RnCityObjectGroupKey left, RnCityObjectGroupKey right) => !left.Equals(right);

        /// <summary>
        /// bool への暗黙的変換（IsValid を返す）
        /// if (key) などの条件式で使用できます
        /// </summary>
        public static implicit operator bool(RnCityObjectGroupKey key) => key.IsValid;


        /// <summary>
        /// cogが主要地物をただ一つだけ持つ場合に、その主要地物をキーとしてRnCityObjectGroupKeyを作る.
        /// 主要地物を持っていなかったり、複数ある場合はdefaultを返す
        /// </summary>
        /// <param name="cog"></param>
        /// <returns></returns>
        public static RnCityObjectGroupKey CreateFromPrimaryPLATEAUCityObjectGroup(PLATEAUCityObjectGroup cog)
        {
            if (!cog)
                return default;
            // 主要地物が0 or 2以上あってはダメ
            var obj = cog.PrimaryCityObjects.FirstOrDefault();
            if (obj == null || cog.PrimaryCityObjects.Skip(1).Any())
                return default;
            
            return new RnCityObjectGroupKey(obj.GmlID);
        }
    }
}