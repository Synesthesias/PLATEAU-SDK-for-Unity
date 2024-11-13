using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Tester;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PLATEAU.Editor.RoadNetwork;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using static PLATEAU.Editor.RoadNetwork.UIDocBind.SerializedScriptableRoadMdl;
using PLATEAU.Editor.Window.Common;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadEditPanel : RoadAdjustGuiPartBase, IScriptableRoadMdl
    {
        static readonly string name = "RoadNetwork_EditPanel";
        GameObject selfGameObject = null;
        public RoadNetworkEditingSystem EditorInterface { get; private set; }
        EventHandler SetupMethod = null;

        public bool IsSuccess => throw new NotImplementedException();

        public bool IsEditingDetailMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int NumLeftLane { get => self.Q<IntegerField>("LeftSide").value; set => self.Q<IntegerField>("LeftSide").value = value; }
        public int NumRightLane { get => self.Q<IntegerField>("RightSide").value; set => self.Q<IntegerField>("RightSide").value = value; }
        public bool EnableMedianLane { get => self.Q<Toggle>("EnableMedianLane").value; set => self.Q<Toggle>("EnableMedianLane").value = value; }
        public bool EnableLeftSideWalk { get => self.Q<Toggle>("EnableLeftSideWalk").value; set => self.Q<Toggle>("EnableLeftSideWalk").value = value; }
        public bool EnableRightSideWalk { get => self.Q<Toggle>("EnableRightSideWalk").value; set => self.Q<Toggle>("EnableRightSideWalk").value = value; }

        public RoadEditPanel() : base(name)
        {
        }

        public override void Init(VisualElement root)
        {
            base.Init(root);

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                Debug.LogError("RoadNetworkStructureModel is not found.");
                return;
            }
            selfGameObject = rnMdl.gameObject;


            // 初期値の設定

            // ボタン類の設定
            var editModeToggle = self.Q<Toggle>("EditModeButton");
            if (editModeToggle == null)
            {
                Debug.LogError("EditModeToggle is not found.");
                return;
            }
            editModeToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue)
                {
                    var system = InitSystem_(root);
                    system.EnableLimitSceneViewDefaultControl = true;
                }
                else
                {
                    var system = RoadNetworkEditingSystem.SingletonInstance.system;
                    system.EnableLimitSceneViewDefaultControl = false;
                    TerminateSystem_(root, system);
                }
            });

            IRoadNetworkEditingSystem InitSystem_(VisualElement root)
            {
                // バインドパスの設定
                self.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
                self.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
                self.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
                self.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
                self.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";


                // 編集システムの初期化
                EditorInterface =
                    RoadNetworkEditingSystem.TryInitalize(EditorInterface, root, new EditorInstance(root, this));

                // システムの取得
                var system = EditorInterface.system;
                system.CurrentEditMode = RoadNetworkEditMode.EditRoadStructure;

                system.RoadNetworkSimpleEditModule?.Init();

                SetupMethod = CreateSetup(this, system, root);
                system.OnChangedSelectRoadNetworkElement += SetupMethod;
                return system;
            }

            void TerminateSystem_(VisualElement root, IRoadNetworkEditingSystem system)
            {
                root.Unbind();
                system.OnChangedSelectRoadNetworkElement -= SetupMethod;
                RoadNetworkEditingSystem.TryTerminate(EditorInterface, root);
            }

            //// 適用ボタンの処理
            //var applyRoadButton = root.Q<Button>("ApplyRoadButton");
            //if (applyRoadButton == null)
            //{
            //    Debug.LogError("ApplyRoadButton is not found.");
            //    return;
            //}
            //applyRoadButton.RegisterCallback<MouseUpEvent>((evt) => 
            //{
            //    Debug.Log("clicked apply12");
            //});


        }

        public override void Terminate(VisualElement root)
        {
            //todoシステムが有効であるならTerminateを呼ぶ
            
            base.Terminate(root);
        }

        static EventHandler CreateSetup(RoadEditPanel panel, IRoadNetworkEditingSystem system, VisualElement element)
        {
            return (s, e) =>
            {
                var roadGroupEditorData = system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
                if (roadGroupEditorData != null)
                {
                    // 無ければ生成する あれば流用する
                    var mdl = panel.CreateOrGetRoadGroupData(panel, roadGroupEditorData);

                    var roadGroup = roadGroupEditorData.Ref;

                    // 既存のモデルオブジェクトを解除
                    element.Unbind();

                    // モデルのバインド
                    element.Bind(mdl);

                    // 適用ボタンの処理
                    var applyRoadButton = element.Q<Button>("ApplyRoadButton");
                    if (applyRoadButton == null)
                    {
                        Debug.LogError("ApplyRoadButton is not found.");
                        return;
                    }
                    applyRoadButton.clicked += () =>
                    {
                        mdl.Apply(system.RoadNetworkSimpleEditModule);
                    };

                }

                var intersectionData = system.SelectedRoadNetworkElement as EditorData<RnIntersection>;
                if (intersectionData != null)
                {
                    system.RoadNetworkSimpleEditModule?.Setup(intersectionData);

                    var applyIntersectionButton = element.Q<Button>("ApplyIntersectionButton");
                    if (applyIntersectionButton == null)
                    {
                        Debug.LogError("ApplyIntersectionButton is not found.");
                        return;
                    }
                    applyIntersectionButton.clicked += () =>
                    {
                        Debug.Log("clicked apply12");

                    };

                }

            };
        }

        private SerializedScriptableRoadMdl CreateOrGetRoadGroupData(IScriptableRoadMdl mdl1, EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.ReqSubData<ScriptableObjectFolder>();
            return mdl.Item;
        }

        public void Apply(RoadNetworkSimpleEditSysModule mod)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// このクラスは必要か？　責任の分担のために作った
        /// </summary>
//        private class RoadEditMdl
//        {
//            private EditorData<RnRoadGroup> editorData;
//            private RnRoadGroup road { get => editorData.Ref; }
//            private IScriptableRoadMdl mdl;


//            public bool IsSuccess => throw new NotImplementedException();

//            private ScriptableRoadMdlData cache;

//            public RoadEditMdl(EditorData<RnRoadGroup> roadGroupEditorData, IScriptableRoadMdl mdl)
//            {
//                editorData = roadGroupEditorData;
//                this.mdl = mdl;
//            }

//            /// <summary>
//            /// ToDo　privateでよいが　古い仕様のインターフェイスでpublicで定義されているため public
//            /// 古い仕様を整備した時に対応する
//            /// </summary>
//            public void Apply()
//            {
//                if (this.road == null)
//                {
//                    Debug.Log("編集対象のLinkGroupが設定されていない");
//                    return;
//                }

//                bool isChanged = false;

//                if (cache.numLeftLane != mdl.NumLeftLane)
//                {
//                    Notify(mdl.NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
//                    cache.numLeftLane = mdl.NumLeftLane;
//                    isChanged = true;
//                    road.SetLeftLaneCount(mdl.NumLeftLane);
//                    editorData.ClearSubData();
//                }
//                if (cache.numRightLane != mdl.NumRightLane)
//                {
//                    Notify(mdl.NumRightLane, cache.numRightLane, nameof(NumRightLane));
//                    cache.numRightLane = mdl.NumRightLane;
//                    isChanged = true;
//                    road.SetRightLaneCount(mdl.NumRightLane);
//                    editorData.ClearSubData();
//                }
//                if (cache.enableMedianLane != mdl.EnableMedianLane)
//                {
//                    Notify(mdl.EnableMedianLane, cache.enableMedianLane, nameof(EnableMedianLane));
//                    cache.enableMedianLane = mdl.EnableMedianLane;
//                    isChanged = true;

//                    if (mdl.EnableMedianLane == false)
//                    {
//                        road.RemoveMedian();
//                        editorData.ClearSubData();
//                    }
//                    else
//                    {

//                        var isSuc = road.CreateMedianOrSkip();
//                        if (isSuc == false)
//                        {
//                            Debug.Log("CreateMedianOrSkip() : 作成に失敗");
//                        }
//                        editorData.ClearSubData();

//                        // ToDo ここで作成したMedianに対してeditorDataで所持している値を適用する
//                        //...
//                    }
//                }
//                road.GetSideWalkGroups(out var leftSideWalks, out var rightSideWalks);

//                if (cache.enableLeftSideWalk != mdl.EnableLeftSideWalk)
//                {
//                    Notify(mdl.EnableLeftSideWalk, cache.enableLeftSideWalk, nameof(EnableLeftSideWalk));
//                    cache.enableLeftSideWalk = mdl.EnableLeftSideWalk;

//                    if (mdl.EnableLeftSideWalk)
//                    {
//                        var c = editorData.GetSubData<WayEditorDataLeft>().sideWalkGroup;
//                        if (c != null)
//                            road.AddSideWalks(c);
//                    }
//                    else
//                    {
//                        road.RemoveSideWalks(leftSideWalks);
//                        //　削除前にデータ保持しておく
//                        //editorData.;
//                        editorData.ClearSubData();
//                        editorData.TryAdd(new WayEditorDataLeft() { sideWalkGroup = leftSideWalks });
//                    }
//                }
//                if (cache.enableRightSideWalk != mdl.EnableRightSideWalk)
//                {
//                    Notify(mdl.EnableRightSideWalk, cache.enableRightSideWalk, nameof(EnableRightSideWalk));
//                    cache.enableRightSideWalk = mdl.EnableRightSideWalk;

//                    if (mdl.EnableRightSideWalk)
//                    {
//                        var c = editorData.GetSubData<WayEditorDataRight>().sideWalkGroup;
//                        if (c != null)
//                            road.AddSideWalks(c);
//                    }
//                    else
//                    {
//                        road.RemoveSideWalks(rightSideWalks);
//                        //　削除前にデータ保持しておく
//                        //editorData.;
//                        editorData.ClearSubData();
//                        editorData.TryAdd(new WayEditorDataRight() { sideWalkGroup = rightSideWalks });
//                    }

//                }


//                if (isChanged)
//                {

//                }
//            }

//            public void ResetCache()
//            {
//                cache.Reset(mdl);
//            }

//            private static void Notify<_T>(in _T post, in _T pre, in string name)
//where _T : IEquatable<_T>
//            {
//                var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
//                Debug.Log(s);
//            }

//            public struct ScriptableRoadMdlData
//            {
//                public bool isEditingDetailMode;
//                public int numLeftLane;
//                public int numRightLane;
//                public bool enableMedianLane;
//                public bool enableLeftSideWalk;
//                public bool enableRightSideWalk;

//                public void Reset(IScriptableRoadMdl mdl)
//                {
//                    isEditingDetailMode = mdl.IsEditingDetailMode;
//                    numLeftLane = mdl.NumLeftLane;
//                    numRightLane = mdl.NumRightLane;
//                    enableMedianLane = mdl.EnableMedianLane;
//                    enableLeftSideWalk = mdl.EnableLeftSideWalk;
//                    enableRightSideWalk = mdl.EnableRightSideWalk;

//                }

//            }

//        }




        private class EditorInstance : RoadNetworkEditingSystem.ISystemInstance
        {
            public EditorInstance(VisualElement root, RoadEditPanel editor)
            {
                this.root = root;
                this.editor = editor;
            }
            VisualElement root;
            RoadEditPanel editor;

            public void RequestReinitialize()
            {
                editor.Init(root);
                ReInitialize();
            }

            public void ReInitialize()
            {
            }
        }

    }
}
