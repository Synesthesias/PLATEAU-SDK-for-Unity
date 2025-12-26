using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace PLATEAU.Util
{

    public static class EditorAsync
    {
        // 1) 次のエディタフレーム（n回）まで返す
        public static async Task YieldToEditorAsync(CancellationToken cancelToken, int frames = 1)
        {
            for (int i = 0; i < frames; i++)
            {
                cancelToken.ThrowIfCancellationRequested();
                await Task.Yield(); // UnitySynchronizationContext 経由でメインスレ継続
            }
                
        }

        // 2) エディタがアイドル(=インポート/再コンパイルが終わる)まで待つ
        public static Task WaitUntilEditorIdleAsync(CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            void Tick()
            {
                if (ct.IsCancellationRequested)
                {
                    EditorApplication.update -= Tick;
                    tcs.TrySetCanceled(ct);
                    return;
                }

                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                EditorApplication.update -= Tick;
                tcs.TrySetResult(true);
            }

            EditorApplication.update += Tick;
            return tcs.Task;
        }

        // 3) IEnumerator を “エディタフレームで” 完走させて Task 化
        public static Task RunCoroutineAsync(IEnumerator routine, CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            void Step()
            {
                if (ct.IsCancellationRequested)
                {
                    EditorApplication.update -= Step;
                    tcs.TrySetCanceled(ct);
                    return;
                }

                try
                {
                    if (routine != null && routine.MoveNext()) return;
                }
                catch (Exception ex)
                {
                    EditorApplication.update -= Step;
                    tcs.TrySetException(ex);
                    return;
                }

                EditorApplication.update -= Step;
                tcs.TrySetResult(true);
            }

            EditorApplication.update += Step;
            return tcs.Task;
        }

        // 4) ImportPackageをTask化（完了/失敗イベント待ち ＋ アイドル待ち）
        public static async Task<bool> ImportPackageAsync(string path, CancellationToken ct, bool interactive = false,
            int extraYieldFrames = 1)
        {
            var tcs = new TaskCompletionSource<bool>();
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                Debug.LogError($"ImportPackageAsync: package not found. path={path}");
                await YieldToEditorAsync(ct, extraYieldFrames);
                return false;
            }

            var cancelled = false;
            using var ctr = ct.Register(() =>
            {
                cancelled = true;
                Unsub();
                tcs.TrySetCanceled(ct);
            });

            void Done(string _)
            {
                Unsub();
                tcs.TrySetResult(true);
            }

            void Fail(string _, string __)
            {
                Unsub();
                tcs.TrySetResult(false);
            }

            void Cancel(string _)
            {
                Unsub();
                tcs.TrySetResult(false);
            }

            void Unsub()
            {
                AssetDatabase.importPackageCompleted -= Done;
                AssetDatabase.importPackageFailed -= Fail;
                AssetDatabase.importPackageCancelled -= Cancel;
            }

            AssetDatabase.importPackageCompleted += Done;
            AssetDatabase.importPackageFailed += Fail;
            AssetDatabase.importPackageCancelled += Cancel;

            // UIや内部ステートと競合しないよう、いったんエディタに返してから実行
            EditorApplication.delayCall += () =>
            {
                if (cancelled) return;
                AssetDatabase.ImportPackage(path, interactive);
            };

            var ok = await tcs.Task; // 完了/失敗を待つ
            await WaitUntilEditorIdleAsync(ct); // Importキュー/再コンパイルの完了待ち
            await YieldToEditorAsync(ct, extraYieldFrames); // 念のため1フレームほど返す
            return ok;
        }
    }
}