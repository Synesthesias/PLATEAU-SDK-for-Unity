using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector
{
    [ExecuteInEditMode]
    public class AreaSelectorBehaviour : MonoBehaviour
    {
        [SerializeField] private MeshRenderer mapPlane;
        [SerializeField] private string prevScenePath;
        private readonly List<Material> mapMaterials = new List<Material>();

        public void Init(string prevScenePathArg)
        {
            this.prevScenePath = prevScenePathArg;
        }

        private void Start()
        {
            AreaSelectorGUI.Enable(this);
            var photoLoadTask = GSIPhotoLoader.Load("seamlessphoto", 10, 909, 403, this.mapPlane, this.mapMaterials);
            photoLoadTask.ContinueWithErrorCatch();
        }
        
        private void EndAreaSelection()
        {
            AreaSelectorGUI.Disable();
            DestroyMaterials();
            var testAreaSelectResult = new List<int> { 123 };
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            AreaSelectorDataPass.Exec(this.prevScenePath, testAreaSelectResult);
        }
        
        public void OnCancelButtonPushed()
        {
            EndAreaSelection();
        }

        private void DestroyMaterials()
        {
            foreach (var mat in this.mapMaterials)
            {
                DestroyImmediate(mat);
            }
        }
    }
}
