using PLATEAU.DynamicTile;
using UnityEditor.SceneManagement;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイル生成のうち、エディタに関する操作を担当します。
    /// </summary>
    public class TileGenEditorProcedure : IOnTileGenerateStart, IBeforeTileAssetBuild, IAfterTileAssetBuild, IOnTileGenerationCancelled, IOnTileBuildFailed
    {
        public bool OnTileGenerateStart()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = true; // タイル生成中フラグを設定
            return true;
        }

        // 処理順の都合上、他のBeforeTileAssetBuildの最後にしてください。
        public bool BeforeTileAssetBuild()
        {
            // アセットバンドルのビルド時に「シーンを保存しますか」とダイアログが出てくるのがうっとうしいので前もって保存して抑制します。
            // 保存については処理前にダイアログでユーザーに了承を得ています。
            EditorSceneManager.SaveOpenScenes();
            return true;
        }

        // 処理順の都合上、他のBeforeTileAssetBuildの最後にしてください。
        public bool AfterTileAssetBuild()
        {
            // 上で自動保存しておてメタアドレスを保存しないのは中途半端なのでここでも保存します。
            EditorSceneManager.SaveOpenScenes();
            
            // タイル生成中フラグを設定
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            return true;
        }

        public void OnTileGenerationCancelled()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
        }
        
        public void OnTileGenerateStartFailed()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
        }

        public void OnTileBuildFailed()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            EditorSceneManager.SaveOpenScenes();
        }
    }
}