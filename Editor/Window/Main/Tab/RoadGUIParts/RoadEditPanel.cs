﻿using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
using static PLATEAU.Editor.RoadNetwork.EditingSystem.RoadNetworkEditingSystem;
using PLATEAU.Editor.RoadNetwork.UIDocBind;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    internal class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        GameObject selfGameObject = null;
        public RoadNetworkEditingSystem EditorInterface { get; private set; }
        EventCallback<ChangeEvent<bool>> activateEditSystemMethod;
        EventHandler setupMethod = null;

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
            editModeToggle.value = RoadNetworkEditingSystem.SingletonInstance?.system != null;
            activateEditSystemMethod = CreateActivateEditSystemMethod(root);
            editModeToggle.RegisterCallback<ChangeEvent<bool>>(activateEditSystemMethod);

            IRoadNetworkEditingSystem InitSystem_(VisualElement root)
            {
                // バインドパスの設定
                self.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
                self.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
                self.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
                self.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
                self.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";
                var d = self.Q<Toggle>("DetailEditMode");
                if (d != null)
                    d.bindingPath = "isEditingDetailMode";

                // 編集システムの初期化
                EditorInterface =
                    RoadNetworkEditingSystem.TryInitalize(EditorInterface, root, new EditorInstance(root, this));

                // システムの取得
                var system = EditorInterface.system;
                system.CurrentEditMode = RoadNetworkEditMode.EditRoadStructure;

                system.RoadNetworkSimpleEditModule?.Init();

                setupMethod = CreateSetup(this, system, root);
                system.OnChangedSelectRoadNetworkElement += setupMethod;
                return system;
            }

            EventCallback<ChangeEvent<bool>> CreateActivateEditSystemMethod(VisualElement root)
            {
                return (evt) =>
                {
                    if (evt.newValue)
                    {
                        var system = InitSystem_(root);
                        system.EnableLimitSceneViewDefaultControl = true;
                    }
                    else
                    {
                        var system = RoadNetworkEditingSystem.SingletonInstance?.system;
                        if (system != null)
                        {
                            system.EnableLimitSceneViewDefaultControl = false;
                            TerminateSystem_(root, system);
                        }
                    }
                };
            }

        }

        public override void Terminate(VisualElement root)
        {
            var editModeToggle = self.Q<Toggle>("EditModeButton");
            if (editModeToggle != null)
            {
                editModeToggle.UnregisterCallback<ChangeEvent<bool>>(activateEditSystemMethod);
                activateEditSystemMethod = null;
            }

            var sys = RoadNetworkEditingSystem.SingletonInstance?.system;
            if (sys != null)
                TerminateSystem_(root, sys);
            base.Terminate(root);
        }

        void TerminateSystem_(VisualElement root, IRoadNetworkEditingSystem system)
        {
            root.Unbind();
            system.OnChangedSelectRoadNetworkElement -= setupMethod;
            setupMethod = null;
            RoadNetworkEditingSystem.TryTerminate(EditorInterface, root);
        }

        static EventHandler CreateSetup(RoadEditPanel panel, IRoadNetworkEditingSystem system, VisualElement element)
        {
            return (s, e) =>
            {
                var roadGroupEditorData = system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
                if (roadGroupEditorData != null)
                {
                    // 無ければ生成する あれば流用する
                    var mdl = panel.CreateOrGetRoadGroupData(roadGroupEditorData);

                    var roadGroup = roadGroupEditorData.Ref;

                    // 既存のモデルオブジェクトを解除
                    element.Unbind();

                    // 仮　詳細編集モード
                    element.TrackSerializedObjectValue(mdl, (se) =>
                    {
                        var mod = system.RoadNetworkSimpleEditModule;
                        var obj = se as IScriptableRoadMdl;
                        if (mod.CanSetDtailMode())
                        {
                            if (mod.IsDetailMode() != obj.IsEditingDetailMode)
                            {
                                mod.SetDetailMode(obj.IsEditingDetailMode);
                            }
                        }
                    });

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

                    //var applyIntersectionButton = element.Q<Button>("ApplyIntersectionButton");
                    //if (applyIntersectionButton == null)
                    //{
                    //    Debug.LogError("ApplyIntersectionButton is not found.");
                    //    return;
                    //}
                    //applyIntersectionButton.clicked += () =>
                    //{
                    //    Debug.Log("clicked apply12");

                    //};

                }

            };
        }

        private SerializedScriptableRoadMdl CreateOrGetRoadGroupData(EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.ReqSubData<ScriptableObjectFolder>();
            return mdl.Item;
        }

        public void Apply(RoadNetworkSimpleEditSysModule mod)
        {
            throw new NotImplementedException();
        }

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
