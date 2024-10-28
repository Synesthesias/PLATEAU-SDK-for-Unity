using NUnit.Framework;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using GenerateParameterFunc =
    System.Action<PLATEAU.Editor.RoadNetwork.RoadNetworkUIDoc,
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem.IRoadNetworkEditingSystem,
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditorAssets,
        UnityEngine.UIElements.VisualElement>;


namespace PLATEAU.Editor.RoadNetwork
{
    public enum RoadNetworkEditingResultType
    {
        _Undefind = 0xfffffff,
        Success = 0x0000000,
        InvalidNewValue = 1 << 0,      // 適切な変更値ではない
        InvalidTarget = 1 << 1,      // 適切な変更対象ではない(主に第1引数のLink,Laneなどについて)         
        CantApplyEditing = 1 << 2,      // 適用できない編集。リンクの幅員を小さくしすぎて一部のレーンの幅員が0になるなど
        InvalidArgs = 1 << 3,      // 適切な引数ではない。(主に第2引数以降　Wayのポイントを削除する際に渡したポイントが存在しないなど)
        _UndefindError = 1 << 256,
        //Faild // 失敗原因を明確にするために未使用
    }

    public struct RoadNetworkEditingResult
    {
        public readonly RoadNetworkEditingResultType Result;
        public readonly string Msg;

        public bool IsSuccess { get => Result == RoadNetworkEditingResultType.Success; }
        public RoadNetworkEditingResult(RoadNetworkEditingResultType result, string msg = "")
        {
            this.Result = result;
            this.Msg = msg;
        }
    }

    /// <summary>
    /// 道路ネットワークの交通規制を登録するための構造体ラッパ
    /// 削除予定　インターフェイスで引数で利用しているがそれぞれのクラスの引数を用意する
    /// </summary>
    public struct RoadNetworkRegulationElemet
    {
        // 信号制御器
        // 信号灯器
        // 一時停止線
        // 一時停止標識
        // 工事、事故、その他の規制
    }

    /// <summary>
    /// 道路ネットワークエディタのUIDocumentを扱うクラス
    /// インスペクター、エディタ、ランタイムでも利用できるように汎用化する
    /// (エディタ専用のUIを使用してるため現状はランタイム使用不可)
    /// 
    /// 未完成のクラス　大きく改装予定　役割は変えない
    /// </summary>
    public class RoadNetworkUIDoc
    {
        // RoadNetworkUIDoc内に隠したい, 依存部分をこのクラスの下層に配置する？
        //public RoadNetworkEditMode CurrentMode { get => (RoadNetworkEditMode)modeSelector.value; }
        public RoadNetworkEditMode CurrentMode
        {
            get => system.CurrentEditMode;
            set => system.CurrentEditMode = value;
        }

        private Toggle editableChangeToggle;
        private Button refreshButton;
        private EnumField modeSelector;
        private ScrollView parameterView;
        private ObjectField objSelecter;
        private DropdownField debugOperationMode;
        private UIDocBind.TrafficSignalLightControllerUIDoc trafficSignalLightControllerUIDoc;

        private readonly IRoadNetworkEditingSystem system;
        private readonly RoadNetworkEditorAssets assets;

        private readonly Dictionary<RoadNetworkEditMode, GenerateParameterFunc> createLayoutSet =
            new Dictionary<RoadNetworkEditMode, GenerateParameterFunc> {
            { RoadNetworkEditMode._EditLaneShape, CreateEditLaneShapeLayout },
            { RoadNetworkEditMode._EditLaneStructure, CreateEditLaneStructureLayout },
            { RoadNetworkEditMode.EditTrafficRegulation, CreateTrafficRegulationLayout },
            { RoadNetworkEditMode._AddLane, CreateAddLaneLayout },
            { RoadNetworkEditMode._AddLink, CreateAddLinkLayout },
            { RoadNetworkEditMode._AddNode, CreateAddNodeLayout },
            { RoadNetworkEditMode._EditLaneWidth, CreateEditLaneWidthLayout },
            { RoadNetworkEditMode.EditRoadStructure, CreateEditRoadStructureLayout },
        };
        // UIが無効化,切り替わる時にされるときに呼び出される　createLayoutSetで設定する
        private Action finalizeAction;

        private static void CreateEditLaneWidthLayout(RoadNetworkUIDoc doc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            var scale = new FloatField();
            scale.label = "比率";
            scale.name = "LaneWidthScale";
            // このレイアウトが削除されたらこのイベントも削除する
            system.OnChangedSelectRoadNetworkElement += (sender, e) =>
            {
                var lane = system.SelectedRoadNetworkElement as RnLane;
                if (lane == null)
                    return;

                scale.value = system.GetScale(lane);
            };
            scale.value = 1.0f;
            scale.RegisterValueChangedCallback((e) =>
            {
                var value = Mathf.Clamp(e.newValue, 0.01f, 2.0f);
                Debug.Log($"{value} 倍");
                var lane = system.SelectedRoadNetworkElement as RnLane;
                if (lane == null)
                    return;

                //var result = system.EditorInstance.EditLaneWidth(lane, value);
                //if (result.IsSuccess == false)
                //{
                //    Debug.LogError(result.Msg);
                //}
            });
            root.Add(scale);

            var applyBtn = new UnityEngine.UIElements.Button();
            applyBtn.text = "適用";
            applyBtn.clicked += () =>
            {
                var scaleValue = Mathf.Clamp(scale.value, 0.01f, 2.0f);

                var lane = system.SelectedRoadNetworkElement as RnLane;
                if (lane == null)
                    return;

                Debug.Log($"{scaleValue}倍 適用");

                var baseLane = system.GetBase(lane);
                if (baseLane == null)
                {
                    //baseLane = lane;
                    var leftWay = new RnWay(RnLineString.Create(lane.LeftWay));
                    var rightWay = new RnWay(RnLineString.Create(lane.RightWay));
                    var prevBorder = new RnWay(RnLineString.Create(lane.PrevBorder));
                    var nextBorder = new RnWay(RnLineString.Create(lane.NextBorder));
                    baseLane = new RnLane(leftWay, rightWay, prevBorder, nextBorder);
                }
                var leftLine = baseLane.GetInnerLerpSegments(0.5f - 0.5f * scaleValue);     // value==0.5 0.25, value==1 0
                var rightLine = baseLane.GetInnerLerpSegments(0.5f + 0.5f * scaleValue);    // value==0.5 0.75, value==1 1
                //IEnumerator<Vector2> newPos = null;
                //IEnumerator<RoadNetworkPoint> points = null;
                //points = lane.LeftWay.Points.GetEnumerator();   // 数値が違う？

                //List<RoadNetworkPoint> leftPoints = new List<RoadNetworkPoint>();

                //var leftFirstPt = lane.LeftWay.Points.First();
                //leftFirstPt.Vertex = leftLine.First().Xay(leftFirstPt.Vertex.y);
                //var leftLastPt = lane.LeftWay.Points.Last();
                //leftLastPt.Vertex = leftLine.Last().Xay(leftLastPt.Vertex.y);
                //var rightFirstPt = lane.RightWay.Points.First();
                //rightFirstPt.Vertex = rightLine.First().Xay(rightFirstPt.Vertex.y);
                //var rightLastPt = lane.RightWay.Points.Last();
                //rightLastPt.Vertex = rightLine.Last().Xay(rightLastPt.Vertex.y);
                //system.RegisterBase(lane, baseLane, scaleValue, null);

                //var newLane = new RoadNetworkLane(
                //    new RoadNetworkWay(RoadNetworkLineString.Create(leftLine.Select(v => v.Xay()))),
                //    new RoadNetworkWay(RoadNetworkLineString.Create(rightLine.Select(v => v.Xay()))),
                //    lane.PrevBorder, lane.NextBorder);
                //lane.Parent.ReplaceLane(lane, newLane);

                //system.RegisterBase(newLane, baseLane, scaleValue, lane);
                //system.SelectedRoadNetworkElement = newLane;    // 置き換えたので変更する必要がある

                //var leftLineE = leftLine.GetEnumerator();
                //var leftPtsE = lane.LeftWay.Points.GetEnumerator();
                //while (leftPtsE.MoveNext() && leftLineE.MoveNext())
                //{
                //    if (leftPtsE.Current == lane.LeftWay.Points.Last())
                //    {
                //        break;
                //    }
                //    leftPtsE.Current.Vertex = leftLineE.Current.Xay(leftPtsE.Current.Vertex.y);
                //}

                //var rightLineE = rightLine.GetEnumerator();
                //var rightPtsE = lane.RightWay.Points.GetEnumerator();
                //while (rightPtsE.MoveNext() && rightLineE.MoveNext())
                //{
                //    rightLineE.MoveNext();
                //    if (rightPtsE.Current == lane.RightWay.Points.Last())
                //    {
                //        break;
                //    }
                //    rightPtsE.Current.Vertex = rightLineE.Current.Xay(rightPtsE.Current.Vertex.y);
                //}
            };
            root.Add(applyBtn);
        }

        private static void CreateEditRoadStructureLayout(RoadNetworkUIDoc doc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            system.RoadNetworkSimpleEditModule?.Init();

            // UIの作成
            var element = assets.GetAsset(RoadNetworkEditorAssets.RoadNetworkEditingRoadPanel).Instantiate();

            //// UIへバインドするモデルオブジェクトの生成
            //var testObj = ScriptableObject.CreateInstance<ScriptableRoadMdl>();
            //testObj.Construct(null);
            //var test = new SerializedScriptableRoadMdl(testObj);

            ////Bindingの設定
            ////var bp = element.BindProperty(test);
            ////element.BindProperty(bp);
            //element.TrackSerializedObjectValue(test, (se) =>
            //{
            //    Debug.Log("changed");
            //    //var obj = se as SerializedScriptableRoadMdl;
            //    //obj.Apply();
            //});
            //element.Bind(test);
            //if (element.Q<Button>("ApplyButton") is var btn)
            //{
            //    btn.clicked += () =>
            //    {
            //        test.Apply();
            //    };
            //}
            ////element.Unbind();

            // UIの追加
            root.Add(element);
            // 選択オブジェクト変更時のイベント設定 モデルオブジェクトの再設定を行う
            system.OnChangedSelectRoadNetworkElement += CreateSetup(doc, system, element);

            doc.finalizeAction = () =>
            {
                root.Remove(element);
                element.Unbind();
                system.OnChangedSelectRoadNetworkElement -= CreateSetup(doc, system, element);    

            };

            static EventHandler CreateSetup(RoadNetworkUIDoc doc, IRoadNetworkEditingSystem system, TemplateContainer element)
            {
                return (s, e) =>
                {
                    var linkGroupEditorData = system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
                    if (linkGroupEditorData != null)
                    {

                        // 無ければ生成する あれば流用する
                        var mdl = doc.CreateOrGetLinkGroupData(linkGroupEditorData);

                        // 既存のモデルオブジェクトを解除
                        element.Unbind();

                        //Bindingの設定
                        //var bp = element.BindProperty(test);
                        //element.BindProperty(bp);
                        element.TrackSerializedObjectValue(mdl, (se) =>
                        {
                            Debug.Log("changed");

                            var mod = system.RoadNetworkSimpleEditModule;
                            var obj = se as IScriptableRoadMdl;
                            if (mod.CanSetDtailMode())
                            {
                                if (mod.IsDetailMode() != obj.IsEditingDetailMode)
                                {
                                    mod.SetDetailMode(obj.IsEditingDetailMode);
                                }
                            }

                            //obj.Apply();
                        });
                        element.Bind(mdl);
                        if (element.Q<Button>("ApplyButton") is var btn)
                        {
                            btn.clicked += () =>
                            {
                                mdl.Apply();
                            };
                        }
                        //element.Unbind();

                        //linkGroupEditorData.LinkGroup;
                        //scale.value = system.GetScale(lane);
                    }

                    var intersectionData = system.SelectedRoadNetworkElement as EditorData<RnIntersection>;
                    if (intersectionData != null)
                    {
                        system.RoadNetworkSimpleEditModule?.Setup(intersectionData);
                    }

                };
            }
        }

        private SerializedScriptableRoadMdl CreateOrGetLinkGroupData(EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.GetSubData<SerializedScriptableRoadMdl>();
            if (mdl == null)
            {
                // UIへバインドするモデルオブジェクトの生成
                var testObj = ScriptableObject.CreateInstance<ScriptableRoadMdl>();
                testObj.Construct(linkGroupEditorData.Ref);
                mdl = new SerializedScriptableRoadMdl(testObj, linkGroupEditorData, system.RoadNetworkSimpleEditModule);

                // 参照を持たせる
                linkGroupEditorData.TryAdd(mdl);
            }

            return mdl;
        }

        private static void CreateAddLinkLayout(RoadNetworkUIDoc doc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement element)
        {
            system.RoadNetworkSimpleLinkGenerateModule.Init();
        }

        private static void CreateAddNodeLayout(RoadNetworkUIDoc doc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement element)
        {
            system.RoadNetworkSimpleNodeGenerateModule.Init();
        }

        private static void CreateDebugNodeLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            var element = assets.GetAsset(RoadNetworkEditorAssets.Vec3Field).Instantiate();
            element.Q<Vector3Field>().label = "座標";
            root.Add(element);
        }

        private static void CreateDebugLinkLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

            Func<VisualElement> createDataIDField = () =>
            {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDField).Instantiate().Q<IntegerField>();
                dataID.label = "";
                dataID.value = -1;
                return dataID;
            };
            CreateParamterBox(assets, root, "本線レーン", createDataIDField);
            CreateParamterBox(assets, root, "右折レーン", createDataIDField);
            CreateParamterBox(assets, root, "左折レーン", createDataIDField);

            var floatFieldAsset = assets.GetAsset(RoadNetworkEditorAssets.FloatFieldAsset);
            if (floatFieldAsset.Instantiate() is var linkCap)
            {
                var slider = linkCap.Q<Slider>();
                {
                    slider.label = "リンク容量(pcu/時/車線)";
                    slider.highValue = 100.0f;
                    slider.value = 0.0f;
                }
                root.Add(linkCap);
            }

            if (floatFieldAsset.Instantiate() is var freeSpd)
            {
                var slider = freeSpd.Q<Slider>();
                {
                    slider.label = "自由流速度(km/時)";
                    slider.highValue = 300.0f;
                    slider.value = 60.0f;
                }
                root.Add(freeSpd);
            }
            if (floatFieldAsset.Instantiate() is var jamDens)
            {
                var slider = jamDens.Q<Slider>();
                {
                    slider.label = "ジャム密度(pcu/km/車線)";
                    slider.highValue = 100.0f;
                    slider.value = 0.0f;
                }
                root.Add(jamDens);
            }
        }

        private static void CreateDebugLaneLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            Func<VisualElement> createVec3Field = () =>
            {
                var vecField = assets.GetAsset(RoadNetworkEditorAssets.Vec3Field).Instantiate().Q<Vector3Field>();
                vecField.label = "";
                return vecField;
            };
            CreateParamterBox(assets, root, "道形状の設定(左側Way)", createVec3Field);
            CreateParamterBox(assets, root, "道形状の設定(右側Way)", createVec3Field);

            Func<VisualElement> createDataIDField = () =>
            {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDField).Instantiate().Q<IntegerField>();
                dataID.label = "";
                dataID.value = -1;
                return dataID;
            };
            CreateParamterBox(assets, root, "連結元レーン(上流側)", createDataIDField);
            CreateParamterBox(assets, root, "連結先レーン(下流側)", createDataIDField);
        }


        private static void CreateParamterBox(RoadNetworkEditorAssets assets, VisualElement root, string labelText, Func<VisualElement> createElemetFunc)
        {
            var parameterBoxAsset = assets.GetAsset(RoadNetworkEditorAssets.ParameterBoxAssetName);

            var parameterBox = parameterBoxAsset.Instantiate();

            var label = parameterBox.Q<Label>("GroupBoxLabel");
            var addBtn = parameterBox.Q<Button>("AddElementBtn");
            var removeBtn = parameterBox.Q<Button>("RemoveElementBtn");
            var elementFolder = parameterBox.Q<ScrollView>("ElementFolder");

            label.text = labelText;

            addBtn.clicked += () =>
            {
                elementFolder.Add(createElemetFunc());
            };
            removeBtn.clicked += () =>
            {
                if (elementFolder.childCount <= 0)
                    return;
                elementFolder.RemoveAt(elementFolder.childCount - 1);
            };

            root.Add(parameterBox);
        }

        private static void CreateEditLaneShapeLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        private static void CreateEditLaneStructureLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        private static void CreateTrafficRegulationLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            roadNetworkUIDoc.trafficSignalLightControllerUIDoc = new UIDocBind.TrafficSignalLightControllerUIDoc(system, assets, root);
            roadNetworkUIDoc.trafficSignalLightControllerUIDoc.CreateTrafficRegulationLayout();

            //var testVec3Field = new Vector3Field("controller position");
            //var testObj = new TrafficSignalLightController("test", null, Vector3.zero);
            //var acc = UIDocBind.Helper.A(testObj).GetOrCreate(testObj.Position, nameof(testObj.Position));
            //UIDocBind.Helper.Bind(testVec3Field, acc, testObj);
            //root.Add(testVec3Field);

            return;
        }

        private static void CreateAddLaneLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            system.RoadNetworkSimpleLaneGenerateModule.Init();
        }

        public RoadNetworkUIDoc(IRoadNetworkEditingSystem system, VisualElement editorRoot, in RoadNetworkEditorAssets assets)
        {
            // 正当性チェック 最低限の初期化
            Assert.IsNotNull(system);
            Assert.IsNotNull(editorRoot);
            Assert.IsNotNull(assets);

            this.system = system;
            this.assets = assets;

            editableChangeToggle = editorRoot.Q<Toggle>("EditableChangeToggle");
            Assert.IsNotNull(editableChangeToggle);
            refreshButton = editorRoot.Q<Button>("RefreshBtn");
            Assert.IsNotNull(refreshButton);
            modeSelector = editorRoot.Q<EnumField>("ModeSelector");
            Assert.IsNotNull(modeSelector);

            objSelecter = editorRoot.Q<ObjectField>("EditingTargetSelecter");
            Assert.IsNotNull(objSelecter);

            parameterView = editorRoot.Q<ScrollView>("ParameterView");
            Assert.IsNotNull(parameterView);

            debugOperationMode = editorRoot.Q<DropdownField>("DebugOperationMode");
            Assert.IsNotNull(debugOperationMode);

            debugOperationMode.choices = new List<string>
            {
                "no-op",
                nameof(IRoadNetworkEditOperation.AddPoint),
                nameof(IRoadNetworkEditOperation.RemovePoint),
                nameof(IRoadNetworkEditOperation.AddMainLane),
                nameof(IRoadNetworkEditOperation.RemoveMainLane),
                nameof(IRoadNetworkEditOperation.AddLink),
                nameof(IRoadNetworkEditOperation.RemoveLink),
                nameof(IRoadNetworkEditOperation.AddNode),
                nameof(IRoadNetworkEditOperation.RemoveNode),
            };

            debugOperationMode.RegisterValueChangedCallback((evt) =>
            {
                var nextOperation = evt.newValue;
                var operation = system.EditOperation;
                system.OperationMode = nextOperation;
            });

        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            //　parameterの変更は必ずシステムを介して行う

            editableChangeToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue != system.EnableLimitSceneViewDefaultControl)
                {
                    system.EnableLimitSceneViewDefaultControl = evt.newValue;
                }
            });

            refreshButton.RegisterCallback<MouseUpEvent>((evt) =>
            {
                system.EditorInstance.RequestReinitialize();
            });

            system.OnChangedRoadNetworkObject += SelectRoadNetwrokObject;
            system.OnChangedEditMode += UpdateMode;

            modeSelector.RegisterCallback<ChangeEvent<Enum>>((evt) =>
            {
                var mode = (RoadNetworkEditMode)evt.newValue;
                CurrentMode = (RoadNetworkEditMode)modeSelector.value;
            });

            system.OnChangedSelectRoadNetworkElement -= SelectRoadNetwrokElement;
            system.OnChangedSelectRoadNetworkElement += SelectRoadNetwrokElement;
        }

        public void PostInitialize()
        {
            CurrentMode = (RoadNetworkEditMode)modeSelector.value;
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Terminate()
        {

        }

        private void UpdateMode(object _, EventArgs _1)
        {
            var mode = system.CurrentEditMode;

            // 既存の生成済みのUIの破棄処理
            finalizeAction?.Invoke();
            finalizeAction = null;

            parameterView.Clear();

            // UIの生成処理
            createLayoutSet[mode](this, system, assets, parameterView);
        }

        private void SelectRoadNetwrokObject(object _, EventArgs _1)
        {
            objSelecter.value = system.RoadNetworkObject;
            UpdateMode(null, null);
        }

        private void SelectRoadNetwrokElement(object _, EventArgs _1)
        {
            if (system.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation)
            {
                var intersection = system.SelectedRoadNetworkElement as RnIntersection;
                if (intersection != null)
                {
                    if (Event.current.shift && intersection.SignalController == null)
                    {
                        // 信号制御器の作成、信号機の作成
                        var trafficController = new TrafficSignalLightController("SignalController" + intersection.DebugMyId, intersection, intersection.GetCenterPoint());
                        intersection.SignalController = trafficController;
                        var lights = TrafficSignalLight.CreateTrafficLights(intersection);
                        trafficController.TrafficLights.AddRange(lights);
                    }else if (Event.current.shift && intersection.SignalController != null)
                    {
                        // 信号制御器の削除、信号機の削除
                        var lights = intersection.SignalController.TrafficLights;
                        lights?.Clear();
                        intersection.SignalController = null;
                    }
                }
            }
        }

    }

}
