using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

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
    public interface IScriptableRoadMdl
    {
        public void Apply();

        // 処理の成否を返す
        // 値の設定後に確認するようにする
        public bool IsSuccess { get; }
        public bool IsEditingDetailMode { get; set; }
        public int NumLeftLane { get; set; }
        public int NumRightLane { get; set; }
        public bool EnableMedianLane { get; set; }

        //public bool ChangeLeftLaneCount(int count);
        //public bool ChangeRightLaneCount(int count);
        //public bool SetEnableSideWalk(bool isEnable);

        public bool EnableLeftSideWalk { get; set; }
        public bool EnableRightSideWalk { get; set; }

    }

    public struct ScriptableRoadMdlData
    {
        public bool isEditingDetailMode;
        public int numLeftLane;
        public int numRightLane;
        public bool enableMedianLane;
        public bool enableLeftSideWalk;
        public bool enableRightSideWalk;

        public void Reset(IScriptableRoadMdl mdl)
        {
            isEditingDetailMode = mdl.IsEditingDetailMode;
            numLeftLane = mdl.NumLeftLane;
            numRightLane = mdl.NumRightLane;
            enableMedianLane = mdl.EnableMedianLane;
            enableLeftSideWalk = mdl.EnableLeftSideWalk;
            enableRightSideWalk = mdl.EnableRightSideWalk;

        }

    }


    public class ScriptableRoadMdl : ScriptableObject, IScriptableRoadMdl
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
        public void Apply()
        {
            throw new NotImplementedException();
        }
        public bool IsSuccess { get; set; }

        public bool IsEditingDetailMode 
        {
            get => isEditingDetailMode;
            set => SetPropety(value, ref isEditingDetailMode, nameof(isEditingDetailMode)); 
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

    public class SerializedScriptableRoadMdl : SerializedObject, IScriptableRoadMdl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mdl"></param>
        /// <param name="editorData">ScriptableRoadMdlで扱っているRnRoadGroupと対象が同じである必要</param>
        /// <param name="mod"></param>
        public SerializedScriptableRoadMdl(ScriptableRoadMdl mdl, EditorData<RnRoadGroup> editorData, RoadNetworkSimpleEditSysModule mod)
            : base(mdl)
        {
            Assert.IsNotNull(mdl);
            Assert.IsNotNull(mod);
            Assert.IsTrue(mdl.road == editorData.Ref);
            //serializedObject = mdl;
            this.mod = mod;

            this.editorData = editorData;

            //            public float _leftSideWalkWidth = 0.0f;
            //public float _rightSideWalkWidth = 0.0f;
            //public float _laneWidth = 0.0f;
            //public bool _isApply = false;
            road = FindProperty("road");
            isEditingDetailMode = FindProperty("isEditingDetailMode");
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

        public RoadNetworkSimpleEditSysModule mod;

        public EditorData<RnRoadGroup> editorData;
        public SerializedProperty road;

        public SerializedProperty isEditingDetailMode;
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

        public bool ChangeLaneWidth(float width)
        {
            throw new NotImplementedException();
        }

        public void ResetCache()
        {
            cache.Reset(this);
        }


        public void Apply()
        {
            if (this.road == null)
            {
                Debug.Log("編集対象のLinkGroupが設定されていない");
                return;
            }

            bool isChanged = false;
            var roadObj = targetObject as ScriptableRoadMdl;
            var road = roadObj.road;
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

            if (cache.numLeftLane != NumLeftLane)
            {
                Notify(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
                cache.numLeftLane = NumLeftLane;
                isChanged = true;
                road.SetLeftLaneCount(NumLeftLane);
                editorData.ClearSubData();
            }
            if (cache.numRightLane != NumRightLane)
            {
                Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
                isChanged = true;
                road.SetRightLaneCount(NumRightLane);
                editorData.ClearSubData();
            }
            if (cache.enableMedianLane != EnableMedianLane) 
            {
                Notify(EnableMedianLane, cache.enableMedianLane, nameof(EnableMedianLane));
                cache.enableMedianLane = EnableMedianLane;
                isChanged = true;

                if (EnableMedianLane == false)
                {
                    road.RemoveMedian();
                    editorData.ClearSubData();
                }
                else
                {

                    var isSuc = road.CreateMedianOrSkip();
                    if (isSuc == false)
                    {
                        Debug.Log("CreateMedianOrSkip() : 作成に失敗");
                    }
                    editorData.ClearSubData();

                    // ToDo ここで作成したMedianに対してeditorDataで所持している値を適用する
                    //...
                }
            }
            road.GetSideWalkGroups(out var leftSideWalks, out var rightSideWalks);

            if (cache.enableLeftSideWalk != EnableLeftSideWalk)
            {
                Notify(EnableLeftSideWalk, cache.enableLeftSideWalk, nameof(EnableLeftSideWalk));
                cache.enableLeftSideWalk = EnableLeftSideWalk;

                if (EnableLeftSideWalk)
                {
                    var c = editorData.GetSubData<WayEditorDataLeft>().sideWalkGroup;
                    if (c != null)
                        road.AddSideWalks(c);
                }
                else
                {
                    road.RemoveSideWalks(leftSideWalks);
                    //　削除前にデータ保持しておく
                    //editorData.;
                    editorData.ClearSubData();
                    editorData.TryAdd(new WayEditorDataLeft() { sideWalkGroup = leftSideWalks });
                }
            }
            if (cache.enableRightSideWalk != EnableRightSideWalk)
            {
                Notify(EnableRightSideWalk, cache.enableRightSideWalk, nameof(EnableRightSideWalk));
                cache.enableRightSideWalk = EnableRightSideWalk;

                if (EnableRightSideWalk)
                {
                    var c = editorData.GetSubData<WayEditorDataRight>().sideWalkGroup;
                    if (c != null)
                        road.AddSideWalks(c);
                }
                else
                {
                    road.RemoveSideWalks(rightSideWalks);
                    //　削除前にデータ保持しておく
                    //editorData.;
                    editorData.ClearSubData();
                    editorData.TryAdd(new WayEditorDataRight() { sideWalkGroup = rightSideWalks });
                }

            }


            if (isChanged)
            {

            }
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
            where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            Debug.Log(s);
        }

        public class WayEditorDataBase
        {
            public IReadOnlyCollection<RnRoadGroup.RnSideWalkGroup> sideWalkGroup;
        }
        public class WayEditorDataLeft : WayEditorDataBase
        {

        }
        public class WayEditorDataRight : WayEditorDataBase
        {

        }
    }
}
