using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector
{
    public class AreaSelectorBehaviour : MonoBehaviour
    {
        [SerializeField] private string prevScenePath;

        public void Init(string prevScenePathArg)
        {
            this.prevScenePath = prevScenePathArg;
        }
        
        private void EndAreaSelection()
        {
            var testAreaSelectResult = new List<int> { 123 };
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            EditorApplication.playModeStateChanged += (stat) => AreaSelectorDataPass.Exec(stat, this.prevScenePath, testAreaSelectResult);
            EditorApplication.ExitPlaymode();
        }
        
        public void OnCancelButtonPushed()
        {
            EndAreaSelection();
        }
    }
}
