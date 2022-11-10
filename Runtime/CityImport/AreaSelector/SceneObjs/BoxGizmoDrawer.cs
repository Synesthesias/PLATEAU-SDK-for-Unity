﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 箱型のギズモを表示します。
    /// </summary>
    internal class BoxGizmoDrawer
    {
        protected Vector3 CenterPos;
        protected Vector3 Size;
        protected Color BoxColor { get; set; } = Color.white;
        protected float LineWidth { get; set; } = 1f;
        public int Priority { get; set; } = 0;

        protected void Init(Vector3 centerPosArg, Vector3 sizeArg)
        {
            this.CenterPos = centerPosArg;
            this.Size = sizeArg;
        }

        public static void DrawWithPriority(IEnumerable<BoxGizmoDrawer> drawers)
        {
            var sorted = drawers.OrderBy(d => d.Priority);
            foreach (var d in sorted)
            {
                d.DrawGizmos();
            }
        }
        

        public virtual void DrawGizmos()
        {
#if UNITY_EDITOR
            var prevColor = Gizmos.color;
            Gizmos.color = this.BoxColor;
            var max = AreaMax(this.CenterPos, this.Size);
            var min = AreaMin(this.CenterPos, this.Size);
            var p1 = new Vector3(min.x, max.y, min.z);
            var p2 = new Vector3(min.x, max.y, max.z);
            var p3 = new Vector3(max.x, max.y, max.z);
            var p4 = new Vector3(max.x, max.y, min.z);
            DrawThickLine(p1, p2, LineWidth);
            DrawThickLine(p2, p3, LineWidth);
            DrawThickLine(p3, p4, LineWidth);
            DrawThickLine(p4, p1, LineWidth);
            AdditionalGizmo();
            Gizmos.color = prevColor;
#endif
        }

        public virtual void DrawSceneGUI()
        {
            
        }

        /// <summary>
        /// サブクラスで描画ギズモを増やしたい場合に実装します。
        /// </summary>
        protected virtual void AdditionalGizmo()
        {
            
        }

        protected static Vector3 AreaMax(Vector3 center, Vector3 size)
        {
            return center + size / 2.0f;
        }

        protected static Vector3 AreaMin(Vector3 center, Vector3 size)
        {
            return center - size / 2.0f;
        }

        /// <summary>
        /// Y軸の値は無視して、XとZの値で箱同士が重なる箇所があるかどうかを bool で返します。
        /// </summary>
        public bool IsBoxIntersectXZ(MeshCodeGizmoDrawer other)
        {
            var otherPos = other.CenterPos;
            var otherSize = other.Size;

            return
                Math.Abs(this.CenterPos.x - otherPos.x) <= (Math.Abs(this.Size.x) + Math.Abs(otherSize.x)) * 0.5 &&
                Math.Abs(this.CenterPos.z - otherPos.z) <= (Math.Abs(this.Size.z) + Math.Abs(otherSize.z)) * 0.5;
        }


        /// <summary>
        /// 太さのある線を描画します。
        /// 参考 : <see href="http://answers.unity.com/answers/1614973/view.html"/>
        /// </summary>
        private void DrawThickLine(Vector3 p1, Vector3 p2, float width)
        {
            int count = 1 + Mathf.CeilToInt(width); // 必要な線の数
            if (count == 1)
            {
                Gizmos.DrawLine(p1, p2);
            }
            else
            {
                Camera c = Camera.current;
                if (c == null)
                {
                    Debug.LogError("Camera.current is null");
                    return;
                }
                var scp1 = c.WorldToScreenPoint(p1);
                var scp2 = c.WorldToScreenPoint(p2);
 
                Vector3 v1 = (scp2 - scp1).normalized; // 線の方向
                Vector3 n = Vector3.Cross(v1, Vector3.forward); // 法線ベクトル
 
                for (int i = 0; i < count; i++)
                {
                    Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                    Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                    Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                    Gizmos.DrawLine(origin, destiny);
                }
            }
        }
    }
}
