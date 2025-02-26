using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PLATEAU.RoadNetwork.Util.RnDebugDrawerBase;

namespace PLATEAU.RoadNetwork.Util
{
    /// <summary>
    /// 基本の描画オプション
    /// </summary>
    [Serializable]
    public class DrawOption
    {
        public bool visible = true;
        public Color color = Color.white;

        public DrawOption() { }

        public DrawOption(bool visible, Color color)
        {
            this.visible = visible;
            this.color = color;
        }
    }

    public class RnDebugDrawerBase
    {
        [Flags]
        public enum VisibleType
        {
            Empty = 0,
            // 選択されていないもの
            NonSelected = 1 << 0,
            // シーンで選択されたGameObjectに紐づくもの
            SceneSelected = 1 << 1,
            // EditorWindowで選択されたもの
            GuiSelected = 1 << 2,
            // 有効なもの
            Valid = 1 << 3,
            // 不正なもの
            InValid = 1 << 4,
            // デバッグメモが表示されている
            DebugMemo = 1 << 5,
            // 全て
            All = ~0
        }
    }

    public class RnDebugDrawerModel<TModel>
    {
        /// <summary>
        /// Drawの最初でリセットされるフレーム情報
        /// </summary>
        public class DrawFrameWork
        {
            public HashSet<object> Visited { get; } = new();

            public TModel Model { get; set; }

            public VisibleType visibleType = RnDebugDrawerBase.VisibleType.Empty;

            public DrawFrameWork(TModel model)
            {
                Model = model;
            }

            public bool IsVisited(object obj)
            {
                var ret = Visited.Contains(obj);
                if (ret == false)
                    Visited.Add(obj);
                return true;
            }

            public VisibleType GetVisibleType(object obj, IEnumerable<PLATEAUCityObjectGroup> cityObjects)
            {
                var ret = VisibleType.Empty;

                if (IsGuiSelected(obj))
                    ret |= VisibleType.GuiSelected;

                if (cityObjects != null && cityObjects.Any(RnEx.IsEditorSceneSelected))
                    ret |= VisibleType.SceneSelected;

                if (ret == VisibleType.Empty)
                    ret = VisibleType.NonSelected;

                // デバッグメモが設定されている
                if (obj is ARnPartsBase parts)
                {
                    if (string.IsNullOrEmpty(parts.DebugMemo) == false)
                        ret |= VisibleType.DebugMemo;
                }
                return ret;
            }

            public virtual bool IsGuiSelected(object obj)
            {
                return false;
            }
        }

        public class Drawer<TWork, T> : RnDebugDrawerBase where TWork : DrawFrameWork
        {
            // 表示非表示設定
            public bool visible = false;

            // 表示対象設定
            public VisibleType showVisibleType = VisibleType.All;

            protected virtual bool DrawImpl(TWork work, T self) { return true; }

            // 子Drawer
            protected virtual IEnumerable<Drawer<TWork, T>> GetChildDrawers() => Enumerable.Empty<Drawer<TWork, T>>();

            public bool Draw(TWork work, T self, VisibleType visibleType)
            {
                if (visible == false)
                    return false;

                if (self == null)
                    return false;

                var lastVisibleType = visibleType;
                try
                {
                    if (GetTargetGameObjects(self)?.Any(RnEx.IsEditorSceneSelected) ?? false)
                    {
                        visibleType |= VisibleType.SceneSelected;
                        visibleType &= ~VisibleType.NonSelected;
                    }

                    if (work.IsVisited(self) == false)
                        return false;

                    static bool Exec(Drawer<TWork, T> drawer, TWork work, T obj)
                    {
                        if (drawer.visible == false)
                            return false;

                        var visibleType = work.visibleType;
                        if (drawer.IsValid(obj))
                        {
                            visibleType |= VisibleType.Valid;
                            visibleType &= ~VisibleType.InValid;
                        }
                        else
                        {
                            visibleType |= VisibleType.InValid;
                            visibleType &= ~VisibleType.Valid;
                        }

                        if ((visibleType & drawer.showVisibleType) == 0)
                            return false;

                        if (drawer.DrawImpl(work, obj) == false)
                            return false;

                        foreach (var child in drawer.GetChildDrawers() ?? new List<Drawer<TWork, T>>())
                        {
                            Exec(child, work, obj);
                        }

                        return true;
                    }
                    work.visibleType = visibleType;
                    return Exec(this, work, self);
                }
                finally
                {
                    work.visibleType = lastVisibleType;
                }
            }

            public virtual IEnumerable<PLATEAUCityObjectGroup> GetTargetGameObjects(T self) => null;

            public virtual bool IsShowTarget(TWork work, T self)
            {
                return true;
            }

            // 有効な物かどうか
            public virtual bool IsValid(T self) => true;
        }
    }



}