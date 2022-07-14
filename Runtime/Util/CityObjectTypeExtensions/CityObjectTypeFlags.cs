using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Util.CityObjectTypeExtensions
{
    /// <summary>
    /// <see cref="CityObjectType"/> をビット列（フラグ群）と解釈する機能を提供します。
    /// </summary>
    internal static class CityObjectTypeFlags
    {
        /// <summary>
        /// 引数をビット列（フラグの集まり）として見ます。
        /// 右のフラグから順に確認し、立っているフラグに対応する CityObjectType を順番に返します。
        /// </summary>
        public static IEnumerable<CityObjectType> ForEachTypes(this CityObjectType typeFlags)
        {
            ulong flags = (ulong)typeFlags;
            int shiftCount = 0;
            while (flags != 0)
            {
                if ((flags & 1) == 1)
                {
                    yield return (CityObjectType)(1ul << shiftCount);
                }
                flags >>= 1;
                shiftCount++;
                if (shiftCount > 99999)
                {
                    throw new Exception("無限ループのフェイルセーフが発動しました。");
                }
            }
        }

        /// <summary>
        /// 引数をビット列（フラグの集まり）として見ます。
        /// 右のフラグから順に確認し、立っているフラグに対応する CityObjectType を配列で返します。
        /// </summary>
        public static CityObjectType[] ToTypeArray(this CityObjectType typeFlags)
        {
            return typeFlags.ForEachTypes().ToArray();
        }
    }

    #if UNITY_EDITOR
    internal static class CityObjectTypeFlagsEditor
    {
        /// <summary>
        /// <see cref="CityObjectType"/> のうちの候補を0個以上選択するGUIを作ります。
        /// 候補は <see cref="CityObjectType"/> のうちの一部分であり、31個以下である必要があります。
        /// 候補のうちGUIで選択されていないものは、対応するビットが 0 になり、選択されているものは 1 になります。
        /// 候補でないものは、対応するビットが 1 になります。（候補でカバーしきれないものはとりあえず対象としないと、対象にするすべがないので。）
        /// </summary>
        /// <param name="candidateFlags">選択の候補をフラグ群（ビット列）で表現したものです。</param>
        /// <param name="label">ラベルです。</param>
        /// <param name="currentSelectedTypeFlags">現在選択中のタイプのフラグ群です。</param>
        /// <returns>選択されたタイプをフラグ群として返します。</returns>
        public static CityObjectType FlagField(this CityObjectType candidateFlags, string label, ulong currentSelectedTypeFlags)
        {
            return FlagFieldInner(candidateFlags, currentSelectedTypeFlags,
                (currentSubsetFlags, subsetDisplays) =>
                    EditorGUILayout.MaskField(label, currentSubsetFlags, subsetDisplays));
        }

        public delegate int SubsetFlagsSelector(int currentSubsetFlags, string[] subsetDisplays); 
        
        /// <summary>
        /// 詳しい説明は <see cref="FlagField"/> をご覧ください。
        /// </summary>
        public static CityObjectType FlagFieldInner(
            CityObjectType candidateFlags,
            ulong currentSelectedTypeFlags,
            SubsetFlagsSelector subsetFlagsSelector // テストできるようにGUI部分を置き換え可能にします。
            )
        {
            // 実装方針 :
            // CityObjectType のうち、候補だけを取り出した集合を subset と呼びます。
            // 対照的に CityObjectType 全体の集合を type と呼びます。
            // subsetFlagsとは、int型であり、 (subsetFlags の 2進数のi桁目 (1の位を i=0 とする)) = (subset[i] を対象とするとき 1, そうでなければ 0)で定義されます。
            // typeFlags は、CityObjectType全体のフラグ群で ulong型です。2進数で見たフラグ群です。
            // 2つのフラグを相互に変換しながら実装します。 
            
            // この配列内の各要素はフラグが1つしか立っていないことが前提です。
            var subset = candidateFlags.ToTypeArray();
            
            // Unityの MaskField に渡すフラグ型は int(符号付き32bit) なので、フラグの個数は 31 までしか対応できません。
            if (subset.Length > 31)
            {
                throw new Exception($"Num of candidates must be smaller than 32. num={subset.Length}");
            }
            var subsetDisplays = subset.Select(type => type.ToDisplay()).ToArray();
            int currentSubsetFlags = TypeToSubsetFlags(subset, currentSelectedTypeFlags);
            
            // GUIで選択
            int selectedSubsetFlags = subsetFlagsSelector(currentSubsetFlags, subsetDisplays);
            
            // subsetFlags を typeFlags に変換
            var typeFlags = SubsetFlagsToType(subset, selectedSubsetFlags);
            
            // 候補でないものは、対応する bit を 1 にします。
            typeFlags |= ~(ulong)candidateFlags;
            
            return (CityObjectType)typeFlags;
        }

        private static int TypeToSubsetFlags(CityObjectType[] subset, ulong typeFlags)
        {
            // GUIで Everything が選択されたとき、int型の全ビットを立てます。
            if (typeFlags == ~0ul)
            {
                return ~0;
            }
            // Everything 以外のとき、typeFlags に対応する subsetFlags のビットを立てます。
            int subsetFlags = 0;
            for (int i = 0; i < subset.Length; i++)
            {
                if ((typeFlags & (ulong)subset[i]) > 0)
                {
                    subsetFlags |= 1 << i;
                }
            }

            return subsetFlags;
        }

        private static ulong SubsetFlagsToType(CityObjectType[] subset, int subsetFlags)
        {
            // GUIで Everything が選択されたとき、CityObjectType (ulong) の全ビットを立てます。
            if (subsetFlags == ~0)
            {
                return ~0ul;
            }
            // Everything 以外のとき、subsetFlags に対応する typeFlags のビットを立てます。
            ulong typeFlags = 0;
            int loopCount = 0;
            for(int flags = subsetFlags; flags != 0; flags >>=1)
            {
                if (loopCount >= subset.Length) break;
                if ((flags & 1) == 1)
                {
                    typeFlags |= (ulong)subset[loopCount];
                }
                loopCount++;
            }
            // Debug.Log($"typeFlags = {typeFlags}");
            return typeFlags;
        }
    }
    #endif
}