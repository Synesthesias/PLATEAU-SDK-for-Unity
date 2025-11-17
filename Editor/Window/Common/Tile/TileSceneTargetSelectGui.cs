using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using System;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common.Tile
{
    /// <summary>
    /// PLATEAUInstancedCityModel　またはPLATEAUTileManagerのどちらかを選択するGUI要素群
    /// </summary>
    internal class TileSceneTargetSelectGui : ElementGroup
    {
        public SceneTileChooserGui.ChooserType SelectedType => sceneTileChooser.SelectedType; // シーンオブジェクト/動的タイルの選択状態

        private SceneTileChooserGui sceneTileChooser;

        private Action<PLATEAUInstancedCityModel> onCityModelChanged;
        private Action<PLATEAUTileManager> onTileManagerChanged;
        private Action<SceneTileChooserGui.ChooserType> onChooserTypeChanged;

        private ObjectFieldElement<PLATEAUInstancedCityModel> ModelSelectElement;
        private ObjectFieldElement<PLATEAUTileManager> TileSelectElement;

        private string errorMessage = null;

        public TileSceneTargetSelectGui( string name, Action<PLATEAUInstancedCityModel> onCityModelChanged, Action<PLATEAUTileManager> onTileManagerChanged,  Action<SceneTileChooserGui.ChooserType> onChooserTypeChanged = null) :base(name, 0)
        {
            sceneTileChooser = new SceneTileChooserGui(OnChooserTypeChanged);
            Elements = new Element[] { 
                new ObjectFieldElement<PLATEAUInstancedCityModel>("scene", "変換対象", OnTargetModelChanged), 
                new ObjectFieldElement<PLATEAUTileManager>("tile", "変換対象", OnTargetTileChanged) };

            ModelSelectElement = this.Get<ObjectFieldElement<PLATEAUInstancedCityModel>>("scene");
            TileSelectElement = this.Get<ObjectFieldElement<PLATEAUTileManager>>("tile");
            TileSelectElement.IsVisible = false;

            this.onCityModelChanged = onCityModelChanged;
            this.onTileManagerChanged = onTileManagerChanged;
            this.onChooserTypeChanged = onChooserTypeChanged;
        }

        private void OnChooserTypeChanged(SceneTileChooserGui.ChooserType chooserType)
        {
            errorMessage = null;
            if (chooserType == SceneTileChooserGui.ChooserType.SceneObject)
            {
                ModelSelectElement.IsVisible = true;
                TileSelectElement.IsVisible = false;
            }
            else
            {
                ModelSelectElement.IsVisible = false;
                TileSelectElement.IsVisible = true;

                if(TileSelectElement.SelectedObject == null)
                {
                    // シーン内のTileManagerを自動でセット
                    var targetTileManager = GameObject.FindAnyObjectByType<PLATEAUTileManager>();
                    if ((targetTileManager != null))
                    {
                        TileSelectElement.SetSelectedObject(targetTileManager);
                        OnTargetTileChanged(targetTileManager);
                    }         
                }
            }
            onChooserTypeChanged?.Invoke(chooserType);
        }

        private void OnTargetModelChanged(PLATEAUInstancedCityModel selectedCityModel)
        {
            errorMessage = null;
            if (selectedCityModel == null)
            {
                onCityModelChanged?.Invoke(null);
                return;
            }
            if (selectedCityModel.transform.parent?.GetComponent<PLATEAUTileManager>() != null)
            {
                errorMessage = "動的タイルを対象とするには「調整対象の種類」を動的タイルにしてください。";
                ModelSelectElement.SetSelectedObject(null);
                onCityModelChanged?.Invoke(null);
                return;
            }

            onCityModelChanged?.Invoke(selectedCityModel);
        }

        private void OnTargetTileChanged(PLATEAUTileManager selectedTileManager)
        {
            onTileManagerChanged?.Invoke(selectedTileManager);
        }

        public override void Dispose()
        {
            this.onCityModelChanged = null;
            this.onTileManagerChanged = null;
            this.onChooserTypeChanged = null;
            ModelSelectElement = null;
            TileSelectElement = null;
            base.Dispose();
        }

        public override void DrawContent()
        {
            sceneTileChooser.Draw();
            base.DrawContent();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                PlateauEditorStyle.MultiLineLabelWithBox(errorMessage);
            }
        }

        public override void Reset()
        {
            base.Reset();
            OnChooserTypeChanged(sceneTileChooser.SelectedType);
        }
    }
}
