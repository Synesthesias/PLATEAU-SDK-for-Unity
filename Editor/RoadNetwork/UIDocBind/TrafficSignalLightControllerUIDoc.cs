﻿using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    /// <summary>
    /// 交通信号灯制御器関連のUIDocumentのバインドや挙動の定義を行うクラス
    /// RoadNetworkUIDoc.csのprivate static void CreateTrafficRegulationLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)の処理を個々に移植する
    /// </summary>
    internal class TrafficSignalLightControllerUIDoc
    {

        public TrafficSignalLightControllerUIDoc(TrafficSignalEditor trafficSignalEditor, RoadNetworkEditTarget editTarget, RoadNetworkEditorAssets assets, VisualElement root)
        {
            this.assets = assets;
            this.root = root;
            this.editTarget = editTarget;
            this.trafficSignalEditor = trafficSignalEditor;
        }

        private RoadNetworkEditorAssets assets;
        private VisualElement root;
        private RoadNetworkEditTarget editTarget;
        private TrafficSignalEditor trafficSignalEditor;

        private TrafficSignalLightPatternUIDoc trafficSignalLightPatternUIDoc;
        private VisualElement trafficPatternPanelRoot;

        /// <summary>
        /// 交通規制レイアウトを作成します。
        /// </summary>
        public void CreateTrafficRegulationLayout()
        {
            // 交通規制パネルの取得
            var panel = assets.GetAsset(RoadNetworkEditorAssets.TrafficRegulationPanel);

            // パネルのインスタンス化
            var panelInst = panel.Instantiate();

            // デプロオブジェクトセレクタの取得
            var deploObjectSelecter = panelInst.Q<DropdownField>("DeploObjectSelecter");
            deploObjectSelecter.RegisterCallback<ChangeEvent<string>>((e) =>
            {

            });

            // パターン追加ボタンの取得とクリックイベントの設定
            var patternAddBtn = panelInst.Q<Button>("Add");
            patternAddBtn.clicked += () =>
            {
                // 選択された交通信号灯制御器の取得
                var trafficLightController = editTarget.SelectedRoadNetworkElement as TrafficSignalLightController;
                if (trafficLightController != null)
                {
                    // 新しい交通信号制御パターンの追加
                    trafficLightController.SignalPatterns.Add(new TrafficSignalControllerPattern());
                    // パターンリストの同期
                    SyncTrafficLightControlPatternList(assets, panelInst, trafficLightController);
                }
            };

            // パターン削除ボタンの取得とクリックイベントの設定
            var patternRemoveBtn = panelInst.Q<Button>("Remove");
            patternRemoveBtn.clicked += () =>
            {
                // 選択された交通信号灯制御器の取得
                var trafficLightController = editTarget.SelectedRoadNetworkElement as TrafficSignalLightController;
                if (trafficLightController != null)
                {
                    // パターンが存在する場合、最後のパターンを削除
                    if (trafficLightController.SignalPatterns.Count > 0)
                    {
                        trafficLightController.SignalPatterns.RemoveAt(trafficLightController.SignalPatterns.Count - 1);
                        // パターンリストの同期
                        SyncTrafficLightControlPatternList(assets, panelInst, trafficLightController);
                    }
                }
            };

            // パターン編集ボタンの取得とクリックイベントの設定
            var patternEditBtn = panelInst.Q<Button>("Edit");
            patternEditBtn.clicked += () =>
            {
                // 選択された交通信号灯制御器の取得
                var trafficLightController = editTarget.SelectedRoadNetworkElement as TrafficSignalLightController;
                if (trafficLightController != null)
                {
                    if (trafficSignalEditor.SelectedSignalControllerPattern != null)
                    {
                        // 交通信号灯制御パターンのUIを作成
                        trafficSignalLightPatternUIDoc = new TrafficSignalLightPatternUIDoc(trafficSignalEditor,editTarget, assets, trafficPatternPanelRoot);
                    }
                }
            };

            // 選択された交通信号灯制御器の取得
            var trafficLightController = editTarget.SelectedRoadNetworkElement as TrafficSignalLightController;
            // 信号制御器が持つパターンリストを元にUIを同期する
            SyncTrafficLightControlPatternList(assets, panelInst, trafficLightController);

            // パネルをルートに追加
            root.Add(panelInst);

            // 交通信号灯制御パターンパネルのルートを作成
            trafficPatternPanelRoot = new VisualElement();
            root.Add(trafficPatternPanelRoot);
        }

        /// <summary>
        /// 信号制御パターンリストを同期します。
        /// </summary>
        /// <param name="assets">RoadNetworkEditorAssets</param>
        /// <param name="panelInst">パネルのインスタンス</param>
        /// <param name="trafficLightController">交通信号灯制御器</param>
        private void SyncTrafficLightControlPatternList(RoadNetworkEditorAssets assets, TemplateContainer panelInst, TrafficSignalLightController trafficLightController)
        {
            // 信号制御パターンリストの取得
            var radioBtnGroup = panelInst.Q<RadioButtonGroup>("TrafficSignalControlPatternList");
            if (trafficLightController != null)
            {
                var radioBtnAsset = assets.GetAsset(RoadNetworkEditorAssets.RadioButton);
                // 信号制御パターンリストの同期
                SyncTrafficLightControlPatternList(trafficLightController.SignalPatterns, radioBtnGroup, radioBtnAsset);
            }
            else
            {
                radioBtnGroup.Clear();
            }
        }

        /// <summary>
        /// 信号制御パターンリストを同期します。
        /// </summary>
        /// <param name="patterns">信号制御パターンリスト</param>
        /// <param name="radioBtnGroup">ラジオボタングループ</param>
        /// <param name="radioBtnAsset">ラジオボタンのアセット</param>
        private void SyncTrafficLightControlPatternList(List<TrafficSignalControllerPattern> patterns, RadioButtonGroup radioBtnGroup, VisualTreeAsset radioBtnAsset)
        {
            // ラジオボタングループ内のラジオボタンをループし、パターンにリンクされていないものを削除する
            var children = radioBtnGroup.Children().ToArray();
            foreach (var radioButton in children)
            {
                if (!patterns.Contains(radioButton.userData))
                {
                    radioBtnGroup.Remove(radioButton);
                }
            }

            // パターンをループする
            foreach (var item in patterns)
            {
                bool isContains = false;

                // ラジオボタングループ内のラジオボタンをループする
                foreach (var radioButton in radioBtnGroup.Children())
                {
                    // ラジオボタンが現在のパターンに既にリンクされているかをチェックする
                    if (radioButton.userData == item)
                    {
                        isContains = true;
                        break;
                    }
                }

                // パターンがどのラジオボタンにもリンクされていない場合、新しいラジオボタンを作成し、パターンにリンクする
                if (!isContains)
                {
                    var inst = radioBtnAsset.Instantiate();
                    var radioBtn = inst.Q<RadioButton>("RadioButton");
                    var refSignalLight = "none";
                    var Seconds = 0.0f;
                    if (item.OffsetTrafficLight != null)
                    {
                        refSignalLight = item.OffsetTrafficLight.ToString();
                        Seconds = item.OffsetSeconds;
                    }

                    radioBtn.label =
                        String.Format("{0} {1} {2} {3} {4}",
                        item.StartOffsets.ToShortDateString(),
                        item.ControlPatternId,
                        "未計算",
                        refSignalLight,
                        Seconds);

                    radioBtn.RegisterValueChangedCallback((e) =>
                    {
                        var userData = UIDocBindHelper.GetUserData(e) as TrafficSignalControllerPattern;
                        var v = e.target as VisualElement;
                        Debug.Assert(userData != null);
                        trafficSignalEditor.SelectedSignalControllerPattern = userData;
                    });
                    radioBtn.userData = item;
                    radioBtnGroup.Add(inst);
                }
            }
        }
    }
}
