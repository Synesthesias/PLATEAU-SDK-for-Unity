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
        public string ServerUrl { get; private set; } = "";
        public string ServerToken { get; private set; } = "";

        public LoadStatusEnum LoadStatus { get; private set; } = LoadStatusEnum.NotStarted;
        public NativeVectorDatasetMetadataGroup DatasetGroups { get; private set; }
        
        public enum LoadStatusEnum{ Success, Failure, Loading, NotStarted}

        private bool foldOutServerConf = false;

        public ServerDatasetFetchGUI(UnityEditor.EditorWindow parentWindow)
        {
            this.parentWindow = parentWindow;
        }

        private bool isFirstDraw = true;
        
        public void Draw()
        {
            // GUIを表示した地点で、デフォルトURLへ接続を試みます。
            if (this.isFirstDraw)
            {
                LoadDatasetAsync().ContinueWithErrorCatch();
            }
            
            this.foldOutServerConf = EditorGUILayout.Foldout(this.foldOutServerConf, "接続先設定");
            if (this.foldOutServerConf)
            {
                PlateauEditorStyle.RightAlign(() =>
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (PlateauEditorStyle.MiniButton("デフォルトのURLにする", 150))
                        {
                            GUI.FocusControl(""); // 入力欄にフォーカスがあると変更がGUI上に反映されないため
                            ServerUrl = "";
                            ServerToken = "";
                        }
                    }

                });

                // GUIで空文字が入力されている時、接続先はデフォルトになるので空文字の代わりに デフォルト と表示します。
                ServerUrl = PlateauEditorStyle.TextFieldWithDefaultValue(
                    "サーバーURL", ServerUrl, "", "(デフォルトURL)");
                ServerToken = PlateauEditorStyle.TextFieldWithDefaultValue(
                    "認証トークン", ServerToken, "", "(デフォルトトークン)");
                switch (LoadStatus)
                {
                    case LoadStatusEnum.NotStarted:
                    case LoadStatusEnum.Failure:
                        if (PlateauEditorStyle.MainButton("接続"))
                        {
                            LoadDatasetAsync().ContinueWithErrorCatch();
                        }
                        break;
                    case LoadStatusEnum.Success:
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                            PlateauEditorStyle.LabelSizeFit(new GUIContent("接続OK")));
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        {
                            if (PlateauEditorStyle.MiniButton("再接続", 60))
                            {
                                LoadDatasetAsync().ContinueWithErrorCatch();
                            }
                        });

                        break;
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

            this.isFirstDraw = false;
        }

        private async Task LoadDatasetAsync()
        {
            await LoadDatasetAsyncInner().ContinueWith(
                _ => this.parentWindow.Repaint(),
                TaskScheduler.FromCurrentSynchronizationContext()
            );
        }
        
        /// <summary>
        /// どのようなデータセットが利用可能であるかをサーバーに問い合わせます。
        /// </summary>
        private async Task LoadDatasetAsyncInner()
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
