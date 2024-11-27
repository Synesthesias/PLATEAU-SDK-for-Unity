using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.Editor.Window.Main.Tab.RoadGuiParts;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_AddPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadAddPanel : RoadAdjustGuiPartBase, IAddPanelMdl
    {
        static readonly string name = "RoadNetwork_AddPanel";

        GameObject selfGameObject = null;
        public RoadNetworkEditingSystem EditorInterface { get; private set; }
        EventHandler SetupMethod = null;


        public RoadAddPanel() : base(name)
        {
        }

        public override void Init(VisualElement root)
        {
            base.Init(root); var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                Debug.LogError("RoadNetworkStructureModel is not found.");
                return;
            }
            selfGameObject = rnMdl.gameObject;


            var system = InitSystem_(self);
            system.EnableLimitSceneViewDefaultControl = true;

            // 初期値の設定

            // ボタン類の設定
            IRoadNetworkEditingSystem InitSystem_(VisualElement root)
            {
                // バインドパスの設定

                // 編集システムの初期化
                EditorInterface =
                    RoadNetworkEditingSystem.TryInitalize(EditorInterface, root, new EditorInstance(root, this));

                // システムの取得
                var system = EditorInterface.system;
                system.CurrentEditMode = RoadNetworkEditMode.AddRoadStructure;

                system.RoadNetworkSimpleEditModule?.Init();

                SetupMethod = CreateSetup(this, system, root);
                system.OnChangedSelectRoadNetworkElement += SetupMethod;
                return system;
            }

        }
        public override void Terminate(VisualElement root)
        {
            var sys = RoadNetworkEditingSystem.SingletonInstance?.system;
            if (sys != null)
                TerminateSystem_(root, sys);
            base.Terminate(root);
        }

        void TerminateSystem_(VisualElement root, IRoadNetworkEditingSystem system)
        {
            root.Unbind();
            system.OnChangedSelectRoadNetworkElement -= SetupMethod;
            system.EnableLimitSceneViewDefaultControl = true;
            RoadNetworkEditingSystem.TryTerminate(EditorInterface, root);
        }

        static EventHandler CreateSetup(RoadAddPanel panel, IRoadNetworkEditingSystem system, VisualElement element)
        {
            return (s, e) =>
            {
                var roadGroupEditorData = system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
                if (roadGroupEditorData != null)
                {
                    //// 無ければ生成する あれば流用する
                    //var mdl = panel.CreateOrGetRoadGroupData(panel, roadGroupEditorData);

                    //var roadGroup = roadGroupEditorData.Ref;

                    //// 既存のモデルオブジェクトを解除
                    //element.Unbind();

                    //// 仮　詳細編集モード
                    //element.TrackSerializedObjectValue(mdl, (se) =>
                    //{
                    //    var mod = system.RoadNetworkSimpleEditModule;
                    //    var obj = se as IScriptableRoadMdl;
                    //    if (mod.CanSetDtailMode())
                    //    {
                    //        if (mod.IsDetailMode() != obj.IsEditingDetailMode)
                    //        {
                    //            mod.SetDetailMode(obj.IsEditingDetailMode);
                    //        }
                    //    }
                    //});

                    //// モデルのバインド
                    //element.Bind(mdl);

                    //// 適用ボタンの処理
                    //var applyRoadButton = element.Q<Button>("ApplyRoadButton");
                    //if (applyRoadButton == null)
                    //{
                    //    Debug.LogError("ApplyRoadButton is not found.");
                    //    return;
                    //}
                    //applyRoadButton.clicked += () =>
                    //{
                    //    mdl.Apply(system.RoadNetworkSimpleEditModule);
                    //};

                }

                var intersectionData = system.SelectedRoadNetworkElement as EditorData<RnIntersection>;
                if (intersectionData != null)
                {
                    //system.RoadNetworkSimpleEditModule?.Setup(intersectionData);

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

        private SerializedScriptableRoadMdl CreateOrGetRoadGroupData(IAddPanelMdl mdl1, EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.ReqSubData<ScriptableObjectFolder>();
            return mdl.Item;
        }


        private class EditorInstance : RoadNetworkEditingSystem.ISystemInstance
        {
            public EditorInstance(VisualElement root, RoadAddPanel editor)
            {
                this.root = root;
                this.editor = editor;
            }
            VisualElement root;
            RoadAddPanel editor;

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
