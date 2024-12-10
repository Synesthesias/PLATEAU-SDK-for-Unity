using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.Editor.RoadNetwork.EditingSystem.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    /// <summary>
    /// Node間を繋ぐLaneの繋がり
    /// １つ以上のLaneで構成する
    /// </summary>
    public interface INodeConnectionLane
    {
        public List<RnLane> lanes { get; }
    }

    /// <summary>
    /// 道路を編集する際に利用するデータモデルのインターフェイス
    /// クラス内でのLink、LaneはNode間を繋ぐLinkリスト、Laneリストを指す
    /// </summary>
    internal interface IScriptableRoadMdl
    {
        /// <summary> 適用します。これにより変更点があったかどうかを返します。</summary>
        public bool Apply(RoadNetworkSimpleEditSysModule mod);

        // 処理の成否を返す
        public bool IsSuccess { get; }
        public bool IsEditingDetailMode { get; set; }
        public bool IsSplineEditMode { get; }
        public int NumLeftLane { get; set; }
        public int NumRightLane { get; set; }
        public bool EnableMedianLane { get; set; }

        //public bool ChangeLeftLaneCount(int count);
        //public bool ChangeRightLaneCount(int count);
        //public bool SetEnableSideWalk(bool isEnable);

        public bool EnableLeftSideWalk { get; set; }
        public bool EnableRightSideWalk { get; set; }

    }

    internal struct ScriptableRoadMdlData
    {
        public bool isEditingDetailMode;
        public bool isSplineEditMode;
        public int numLeftLane;
        public int numRightLane;
        public bool enableMedianLane;
        public bool enableLeftSideWalk;
        public bool enableRightSideWalk;

        public void Reset(IScriptableRoadMdl mdl)
        {
            isEditingDetailMode = mdl.IsEditingDetailMode;
            isSplineEditMode = mdl.IsSplineEditMode;
            numLeftLane = mdl.NumLeftLane;
            numRightLane = mdl.NumRightLane;
            enableMedianLane = mdl.EnableMedianLane;
            enableLeftSideWalk = mdl.EnableLeftSideWalk;
            enableRightSideWalk = mdl.EnableRightSideWalk;

        }

    }


    internal class ScriptableRoadMdl : ScriptableObject, IScriptableRoadMdl
    {
        public ScriptableRoadMdl()
        {
        }

        public void Construct(RnRoadGroup road)
        {
            //Assert.IsNotNull(road);
            this.road = road;

            numLeftLane = this.road.GetLeftLaneCount();
            numRightLane = this.road.GetRightLaneCount();
            enableMedianLane = this.road.HasMedians();
            this.road.GetSideWalkGroups(out var leftSideWalkGroup, out var rightSideWalkGroup);
            enableLeftSideWalk = leftSideWalkGroup.Count > 0;
            enableRightSideWalk = rightSideWalkGroup.Count > 0;
            ResetCache();
        }

        public RnRoadGroup road;

        public bool isEditingDetailMode;
        public bool isSplineEditMode;
        public int numLeftLane = 3;
        public int numRightLane = 3;
        public bool enableMedianLane = true;
        public bool enableLeftSideWalk = false;
        public bool enableRightSideWalk = false;

        ScriptableRoadMdlData cache;


        public void ResetCache()
        {
            cache.Reset(this);
        }

        // cacheとの比較を行い、変更があれば変更を通知する
        public bool Apply(RoadNetworkSimpleEditSysModule mod)
        {
            throw new NotImplementedException();
        }
        public bool IsSuccess { get; set; }

        public bool IsEditingDetailMode 
        {
            get => isEditingDetailMode;
            set => SetPropety(value, ref isEditingDetailMode, nameof(isEditingDetailMode)); 
        }

        public bool IsSplineEditMode
        {
            get => isSplineEditMode;
            set => SetPropety(value, ref isSplineEditMode, nameof(isSplineEditMode));
        }
        public int NumLeftLane
        {
            get => numLeftLane;
            set => SetPropety(value, ref numLeftLane, nameof(NumLeftLane));
        }
        public int NumRightLane
        {
            get => numRightLane;
            set => SetPropety(value, ref numRightLane, nameof(NumRightLane));
        }

        public bool EnableLeftSideWalk
        {
            get => enableLeftSideWalk;
            set => SetPropety(value, ref enableLeftSideWalk, nameof(EnableLeftSideWalk));
        }

        public bool EnableRightSideWalk
        {
            get => enableRightSideWalk;
            set => SetPropety(value, ref enableRightSideWalk, nameof(EnableRightSideWalk));
        }
        public bool EnableMedianLane
        { 
            get => enableMedianLane; 
            set => SetPropety(value, ref enableMedianLane, nameof(enableMedianLane)); 
        }

        private static void SetPropety<_T>(in _T v, ref _T current, in string name)
            where _T : IEquatable<_T>
        {
            if (v.Equals(current))
            {
                return;
            }
            var pre = current;
            current = v;
            Notify(v, pre, name);
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
    where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            Debug.Log(s);
        }

    }

    internal class SerializedScriptableRoadMdl : SerializedObject, IScriptableRoadMdl
    {
        private const bool EnableDebugLog = false;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mdl"></param>
        /// <param name="editorData">ScriptableRoadMdlで扱っているRnRoadGroupと対象が同じである必要</param>
        /// <param name="mod"></param>
        public SerializedScriptableRoadMdl(ScriptableRoadMdl mdl, EditorData<RnRoadGroup> editorData)
            : base(mdl)
        {
            Assert.IsNotNull(mdl);
            Assert.IsTrue(mdl.road == editorData.Ref);
            //serializedObject = mdl;

            this.editorData = editorData;

            //            public float _leftSideWalkWidth = 0.0f;
            //public float _rightSideWalkWidth = 0.0f;
            //public float _laneWidth = 0.0f;
            //public bool _isApply = false;
            road = FindProperty("road");
            isEditingDetailMode = FindProperty("isEditingDetailMode");
            isSplineEditMode = FindProperty("isSplineEditMode");
            numLeftLane = FindProperty("numLeftLane");
            numRightLane = FindProperty("numRightLane");

            enableMedianLane = FindProperty("enableMedianLane");
            enableLeftSideWalk = FindProperty("enableLeftSideWalk");
            enableRightSideWalk = FindProperty("enableRightSideWalk");

            isApply = FindProperty("_isApply");
            //_numLeftLane = serializedObject.FindProperty("_numLeftLane");
            //_numRightLane = serializedObject.FindProperty("_numRightLane");
            //_enableSideWalk = serializedObject.FindProperty("_enableSideWalk");
            //_leftSideWalkWidth = serializedObject.FindProperty("_leftSideWalkWidth");
            //_rightSideWalkWidth = serializedObject.FindProperty("_rightSideWalkWidth");
            //_isApply = serializedObject.FindProperty("_isApply");

            ResetCache();
        }

        public EditorData<RnRoadGroup> editorData;
        public SerializedProperty road;

        public SerializedProperty isEditingDetailMode;
        public SerializedProperty isSplineEditMode;
        public SerializedProperty numLeftLane;
        public SerializedProperty numRightLane;
        public SerializedProperty enableMedianLane;
        public SerializedProperty enableLeftSideWalk;
        public SerializedProperty enableRightSideWalk;

        public SerializedProperty isApply;
        //public UIDocBindHelper.IAccessor<int> _numLeftLane;
        //public UIDocBindHelper.IAccessor<int> _numRightLane;
        //public UIDocBindHelper.IAccessor<bool> _enableSideWalk ;
        //public UIDocBindHelper.IAccessor<float> _leftSideWalkWidth;
        //public UIDocBindHelper.IAccessor<float> _rightSideWalkWidth;
        //public UIDocBindHelper.IAccessor<bool> _isApply;

        ScriptableRoadMdlData cache;
        public bool IsSuccess => throw new NotImplementedException();

        public int NumLeftLane { get => numLeftLane.intValue; set => numLeftLane.intValue = value; }
        public int NumRightLane { get => numRightLane.intValue; set => numRightLane.intValue = value; }
        public bool EnableMedianLane { get => enableMedianLane.boolValue; set => enableMedianLane.boolValue = value; }
        public bool EnableLeftSideWalk { get => enableLeftSideWalk.boolValue; set => enableLeftSideWalk.boolValue = value; }
        public bool EnableRightSideWalk { get => enableRightSideWalk.boolValue; set => enableRightSideWalk.boolValue = value; }

        public bool IsEditingDetailMode { get => isEditingDetailMode.boolValue; set => isEditingDetailMode.boolValue = value; }
        public bool IsSplineEditMode { get => isSplineEditMode.boolValue; set => isSplineEditMode.boolValue = value; }

        public ScriptableRoadMdl TargetScriptableRoadMdl
        {
            get
            {
                return targetObject as ScriptableRoadMdl;
            }
        }

        public bool ChangeLaneWidth(float width)
        {
            throw new NotImplementedException();
        }

        public void ResetCache()
        {
            cache.Reset(this);
        }


        
        public bool Apply(RoadNetworkSimpleEditSysModule mod)
        {
            if (this.road == null)
            {
                Debug.Log("編集対象のLinkGroupが設定されていない");
                return false;
            }

            bool isChanged = false;
            var road = TargetScriptableRoadMdl.road;
            if (cache.isEditingDetailMode != IsEditingDetailMode)
            {
                //if (mod.CanSetDtailMode())
                //{
                //    Notify(IsEditingDetailMode, cache.isEditingDetailMode, nameof(IsEditingDetailMode));
                //    cache.isEditingDetailMode = IsEditingDetailMode;
                //    mod.SetDetailMode(IsEditingDetailMode);
                //    isChanged = true;
                //}
            }

            if (cache.isSplineEditMode != IsSplineEditMode)
            {
                Log(IsSplineEditMode, cache.isSplineEditMode, nameof(IsSplineEditMode));
                cache.isSplineEditMode = IsSplineEditMode;
            }

            bool isChangedLane = false;
            if (cache.numLeftLane != NumLeftLane)
            {
                Log(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
                cache.numLeftLane = NumLeftLane;
                isChangedLane = true;
                editorData.ClearSubData<WayEditorDataList>();
            }
            if (cache.numRightLane != NumRightLane)
            {
                Log(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
                isChangedLane = true;
                editorData.ClearSubData<WayEditorDataList>();
            }
            if (isChangedLane)
            {
                road.SetLaneCount(NumLeftLane, NumRightLane);
            }

            isChanged |= isChangedLane;

            if (cache.enableMedianLane != EnableMedianLane) 
            {
                Log(EnableMedianLane, cache.enableMedianLane, nameof(EnableMedianLane));
                cache.enableMedianLane = EnableMedianLane;
                isChanged = true;

                if (EnableMedianLane == false)
                {
                    road.RemoveMedian();
                    editorData.ClearSubData<WayEditorDataList>();
                }
                else
                {
                    var isSuc = road.CreateMedianOrSkip();
                    if (isSuc == false)
                    {
                        Debug.Log("CreateMedianOrSkip() : 作成に失敗");
                    }
                    editorData.ClearSubData<WayEditorDataList>();

                    // ToDo ここで作成したMedianに対してeditorDataで所持している値を適用する
                    //...
                }
            }
            road.GetSideWalkGroups(out var leftSideWalks, out var rightSideWalks);

            bool isChangedLeftSideWalk = cache.enableLeftSideWalk != EnableLeftSideWalk;
            bool isChangedRightSideWalk = cache.enableRightSideWalk != EnableRightSideWalk;
            bool isChangedSideWalk = isChangedLeftSideWalk || isChangedRightSideWalk;
            if (isChangedLeftSideWalk)
            {
                Log(EnableLeftSideWalk, cache.enableLeftSideWalk, nameof(EnableLeftSideWalk));
                cache.enableLeftSideWalk = EnableLeftSideWalk;
                UpdateSideWalk<CacheLeftSideWalkGroupEditorData>(EnableLeftSideWalk, road, editorData);

            }
            if (isChangedRightSideWalk)
            {
                Log(EnableRightSideWalk, cache.enableRightSideWalk, nameof(EnableRightSideWalk));
                cache.enableRightSideWalk = EnableRightSideWalk;
                UpdateSideWalk<CacheRightSideWalkGroupEditorData>(EnableRightSideWalk, road, editorData);
            }
            if (isChangedSideWalk)
            {
                // 編集用のWayデータの更新も必要なため
               editorData.ClearSubData<WayEditorDataList>();
            }

            isChanged |= isChangedSideWalk;

            if (isChanged)
            {
                
            }

            static void UpdateSideWalk<_CacheSideWalkData>(bool enable, RnRoadGroup road, EditorData<RnRoadGroup> editorData)
                where _CacheSideWalkData : CacheSideWalkGroupEditorData, new()
            {
                if (enable)
                {
                    var data = editorData.GetSubData<_CacheSideWalkData>();
                    var group = data?.Group;

                    // 前のデータあれば追加する
                    if (group != null)
                    {
                        road.AddSideWalks(group);

                        // 追加したので古いデータは削除
                        editorData.ClearSubData<_CacheSideWalkData>();
                    }
                    else
                    {
                        // 仮　無い場合は作成しない

                        //// 無ければ車線から複製する
                        //var ways = road.GetEdgeWays(RnDir.Left);

                        //// 車線をスライドして複製

                        //// 複製した車線を元に歩道を作成
                        //var sideWalks = new List<RnSideWalk>();
                        //road.AddSideWalks(
                        //    new List<RnRoadGroup.RnSideWalkGroup> { new RnRoadGroup.RnSideWalkGroup(sideWalks) });
                    }

                }
                else
                {
                    // 削除前のデータはeditorDataで保持 (システム的にあり得ないが既にある場合はそのデータを再利用)
                    var data = editorData.ReqSubData<_CacheSideWalkData>();
                    Assert.IsNotNull(data);
                    var group = data.Group;

                    // そのまま削除 
                    road.RemoveSideWalks(group);
                }
            }

            return isChanged;
        }

        public void ApplySplineEditMode(RoadNetworkSimpleEditSysModule mod)
        {
            var target = new EditorData<RnRoadGroup>((targetObject as ScriptableRoadMdl)?.road);
            if (IsSplineEditMode)
            {
                mod.SplineEditorMod.Enable(this, target);
            }
            else
            {
                mod.SplineEditorMod.Apply();
                mod.SplineEditorMod.Disable();
            }
        }

        private static void Log<_T>(in _T post, in _T pre, in string name)
            where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            if (EnableDebugLog) Debug.Log(s);
        }

    }
}
