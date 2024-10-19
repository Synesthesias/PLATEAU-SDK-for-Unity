using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプラインによる道路編集機能を提供する
    /// </summary>
    public class RnSplineEditting
    {
        public RnSplineEditting()
        {
        }

        /// <summary>
        /// 処理フローのサンプル
        /// 想定している処理フローは以下の通り
        /// </summary>
        private static void SampleFlow()
        {
            // インスタンス生成
            var instance = new RnSplineEditting();

            // 初期化
            instance.Initialize();

            // 道路を選択
            var selectRoadGroup = (EditorData<RnRoadGroup>)null;
            var parameter = (IScriptableRoadMdl)null;

            // スプライン機能を起動
            instance.Enable(parameter, selectRoadGroup);

            // 別の道路を選択
            var newSelectRoadGroup = (EditorData<RnRoadGroup>)null;
            var newParameter = (IScriptableRoadMdl)null;

            // 現在のスプラインを終了
            instance.Disable();

            // 新しいスプラインを開始
            instance.Enable(newParameter, newSelectRoadGroup);

            // スプラインの編集結果を道路に適用
            instance.Apply();


            // スプライン機能を完全に終了 
            instance.Terminate();


        }

        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// 道路の編集パラメータ
        /// スプラインのパラメータもここに含む
        /// </summary>
        private IScriptableRoadMdl parameter;
        
        /// <summary>
        /// 編集対象の道路グループ
        /// </summary>
        private EditorData<RnRoadGroup> edittingTarget;
        private RnRoadGroup RoadGroup => edittingTarget.Ref;

        /// <summary>
        /// 初期化時に呼び出す
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="edittingTarget"></param>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Terminate();
            }

            Debug.Log("Initialize()");
            Assert.IsFalse(IsInitialized, "既に初期化されています。");
            IsInitialized = true;

            parameter = null;
            edittingTarget = null;
        }


        /// <summary>
        /// 終了時に呼び出す
        /// </summary>
        public void Terminate()
        {
            Debug.Log("Terminate()");
            Assert.IsTrue(IsInitialized, "初期化されていない");
            IsInitialized = false;
        }

        /// <summary>
        /// スプライン編集機能を有効化
        /// </summary>
        public void Enable(IScriptableRoadMdl parameter, EditorData<RnRoadGroup> edittingTarget)
        {
            Debug.Log("Enable()");
            Assert.IsTrue(IsInitialized, "初期化されていない");

            this.parameter = parameter;
            this.edittingTarget = edittingTarget;

        }

        /// <summary>
        /// スプライン編集機能を無効化
        /// </summary>
        public void Disable()
        {
            Debug.Log("Disable()");
            Assert.IsTrue(IsInitialized, "初期化されていない");
        }

        /// <summary>
        /// スプラインの編集結果を道路に適用
        /// </summary>
        public void Apply()
        {
            Debug.Log("Apply()");
            Assert.IsTrue(IsInitialized, "初期化されていない");

            // RoadGroup.Roads.Clear();
            // CreateLanes(parameter.NumLeftLane, paremeter.NumRightLane);

        }

    }
}
