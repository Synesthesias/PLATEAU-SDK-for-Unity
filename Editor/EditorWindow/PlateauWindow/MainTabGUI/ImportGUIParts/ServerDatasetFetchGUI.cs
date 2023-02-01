using System;
using System.Threading.Tasks;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Native;
using PLATEAU.Network;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// サーバーURLとトークンを指定するGUIを描画し、
    /// そのURLに対して利用可能なデータセットを問い合わせます。
    /// </summary>
    public class ServerDatasetFetchGUI
    {
        private readonly UnityEditor.EditorWindow parentWindow;
        public string ServerUrl { get; private set; } = NetworkConfig.DefaultApiServerUrl;
        public string ServerToken { get; private set; } = NetworkConfig.DefaultApiToken;

        public LoadStatusEnum LoadStatus { get; private set; } = LoadStatusEnum.NotStarted;
        public NativeVectorDatasetMetadataGroup DatasetGroups { get; private set; }
        
        public enum LoadStatusEnum{ Success, Failure, Loading, NotStarted}

        public ServerDatasetFetchGUI(UnityEditor.EditorWindow parentWindow)
        {
            this.parentWindow = parentWindow;
        }
        
        public void Draw()
        {
            PlateauEditorStyle.RightAlign(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("デフォルトのURLにする", 150))
                    {
                        GUI.FocusControl(""); // 入力欄にフォーカスがあると変更がGUI上に反映されないため
                        this.ServerUrl = NetworkConfig.DefaultApiServerUrl;
                        this.ServerToken = NetworkConfig.DefaultApiToken;
                    }
                    if(PlateauEditorStyle.MiniButton("(開発者向け)\nモックサーバー", 100))
                    {
                        GUI.FocusControl("");
                        this.ServerUrl = NetworkConfig.MockServerUrl;
                        this.ServerToken = "";
                    }
                }

            });
            
            ServerUrl = EditorGUILayout.TextField("サーバーURL", this.ServerUrl);
            const string defaultTokenDisplay = "(デフォルト トークン)";
            string tokenDisplay = ServerToken == NetworkConfig.DefaultApiToken ? defaultTokenDisplay : ServerToken;
            string tokenInputted = EditorGUILayout.TextField("トークン", tokenDisplay);
            ServerToken = tokenInputted == defaultTokenDisplay ? NetworkConfig.DefaultApiToken : tokenInputted;

            if (LoadStatus != LoadStatusEnum.Loading)
            {
                if (PlateauEditorStyle.MainButton("接続"))
                {
                    LoadDatasetAsync()
                        .ContinueWith(_ => this.parentWindow.Repaint(), TaskScheduler.FromCurrentSynchronizationContext())
                        .ContinueWithErrorCatch();
                }
            }
            
            // ロードのステータスを表示します。
            string loadStatusMessage = LoadStatus switch
            {
                LoadStatusEnum.Loading => "サーバーに問い合わせ中です...",
                LoadStatusEnum.Failure => "サーバーへの問い合わせに失敗しました。",
                _ => ""
            };
            if (!string.IsNullOrEmpty(loadStatusMessage))
            {
                PlateauEditorStyle.CenterAlignHorizontal(
                    () =>
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel2())
                        {
                            EditorGUILayout.LabelField(loadStatusMessage);
                        }
                    });
            }
        }
        
        /// <summary>
        /// どのようなデータセットが利用可能であるかをサーバーに問い合わせます。
        /// </summary>
        private async Task LoadDatasetAsync()
        {
            LoadStatus = LoadStatusEnum.Loading;
            try
            {
                DatasetGroups = await Task.Run(() =>
                {
                    var client = Client.Create(this.ServerUrl, this.ServerToken);
                    var ret = client.GetDatasetMetadataGroup();
                    client.Dispose();
                    return ret;
                });
            }
            catch (Exception e)
            {
                Debug.Log($"データセットの問い合わせに失敗しました:\n{e.Message}\n{e.StackTrace}");
                LoadStatus = LoadStatusEnum.Failure;
                return;
            }

            if (DatasetGroups.Length == 0)
            {
                LoadStatus = LoadStatusEnum.Failure;
                return;
            }
            
            LoadStatus = LoadStatusEnum.Success;
        }
    }
}
