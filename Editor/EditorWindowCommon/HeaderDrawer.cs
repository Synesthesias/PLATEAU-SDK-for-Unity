using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindowCommon
{
    /// <summary>
    /// 見出しを表示しつつ、その見出し番号 (例: 2-1-1.)を管理します。
    /// 使い方は EditorWindow の最初で <see cref="Reset"/> を呼び、
    /// <see cref="Draw"/> で見出し番号の深さに応じたヘッダーを表示しつつ、見出し番号を数えます。
    /// <see cref="IncrementDepth"/>, <see cref="DecrementDepth"/> で番号の深さを変えます。
    /// </summary>
    internal static class HeaderDrawer
    {
        /// <summary> 例えば現在の見出し番号が 2-1-1. であれば、このリストは {2,1,1} になります。 </summary>
        private static readonly List<int> currentHeaderNum;

        static HeaderDrawer()
        {
            currentHeaderNum = new List<int>();
            Reset();
        }

        public static void Reset()
        {
            // 最初の見出し番号 {1} にします。
            currentHeaderNum.Clear();
            IncrementDepth(false);
        }

        /// <summary>
        /// 見出しの深さです。
        /// 例 : 現在の見出し番号が 5-6-7-8. であれば、 Depth は 4 です。
        /// </summary>
        public static int Depth => currentHeaderNum.Count;

        /// <summary>
        /// 見出し番号を深くします。
        /// 例 : 現在の見出し番号が 2. であれば、 2-1. にします。 
        /// </summary>
        public static void IncrementDepth(bool doGoPrev = true)
        {
            if(doGoPrev) Prev();
            currentHeaderNum.Add(1);
        }

        /// <summary>
        /// 見出し番号を浅くします。
        /// 例 : 現在の見出し番号が 1-1-1. であれば、 1-2. にします。
        /// </summary>
        public static void DecrementDepth(bool doGoNext = true)
        {
            if(Depth > 0) currentHeaderNum.RemoveAt(Depth-1);
            if (doGoNext) Next();
        }

        /// <summary>
        /// 次の見出し番号にします。
        /// 例 : 現在の見出し番号が 1-1. であれば、 1-2. にします。
        /// </summary>
        private static void Next()
        {
            currentHeaderNum[Depth - 1]++;
        }

        /// <summary>
        /// 前の見出し番号にします。
        /// </summary>
        private static void Prev()
        {
            int index = Math.Max(0, Depth - 1);
            int num = Math.Max(1, currentHeaderNum[index] - 1);
            currentHeaderNum[index] = num;
        }

        private static string HeaderNumToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Depth; i++)
            {
                sb.Append(currentHeaderNum[i]);
                if (i < Depth - 1) sb.Append("-");
            }

            sb.Append(".");
            return sb.ToString();
        }

        /// <summary>
        /// 現在の見出し番号の深さに応じた見出しを表示します。
        /// その後、見出し番号を進めます。
        /// </summary>
        public static void Draw(string text)
        {
            string headerText = $"{HeaderNumToString()} {text}";
            if (Depth == 1)
            {
                PlateauEditorStyle.Heading1(headerText);
            }
            else if (Depth == 2)
            {
                PlateauEditorStyle.Heading2(headerText);
            }
            else if (Depth == 3)
            {
                PlateauEditorStyle.Heading3(headerText);
            }
            else
            {
                EditorGUILayout.LabelField(headerText);
            }
            Next();
        }

    }
}