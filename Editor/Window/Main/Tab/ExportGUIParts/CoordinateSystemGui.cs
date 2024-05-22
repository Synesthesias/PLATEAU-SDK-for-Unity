using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityExport;
using PLATEAU.Geometries;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGUIParts
{
    /// <summary>
    /// エクスポートにおいて、座標軸の向きを選択するGUIです。
    /// 選択中のファイルフォーマットが変わった場合は生成し直す必要があります。
    /// </summary>
    public class CoordinateSystemGui
    {
        private readonly List<CoordinateSystem> meshAxisChoices;
            
        public CoordinateSystem SelectedCoordinateSystem { get; private set; }
        private readonly string[] meshAxisDisplay;


        /// <summary>
        /// 引数のファイルフォーマットに対応する選択肢を生成します。
        /// 選択の初期状態は、引数のinitialSelectedとしますが、それが選択肢にない場合は
        /// 代わりに最初の選択肢を選択します。
        /// </summary>
        public CoordinateSystemGui(MeshFileFormat format, CoordinateSystem initialSelected)
        {
            // ファイルフォーマットに応じて、選択可能な座標軸の選択肢を変えます。
            switch (format)
            {
                case MeshFileFormat.OBJ:
                case MeshFileFormat.FBX:
                    // obj, fbxは、仕様に従うと右手座標系で扱うべきなので、左手座標系を選択肢から除外します。
                    meshAxisChoices = ((CoordinateSystem[])Enum.GetValues(typeof(CoordinateSystem)))
                        .Where(sys => sys.IsRightHanded())
                        .ToList();
                    break;
                case MeshFileFormat.GLTF:
                    // gltfの座標系は仕様で決まっているので、選択肢は1つだけです。
                    meshAxisChoices = new List<CoordinateSystem>() { CoordinateSystem.WUN };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format));
            }
            meshAxisDisplay = meshAxisChoices.Select(axis => axis.ToNaturalLanguage()).ToArray();

            // 選択の初期値を設定します。
            SelectedCoordinateSystem = meshAxisChoices.Contains(initialSelected) ? initialSelected : meshAxisChoices[0];
        }
        
        
        public void Draw()
        {
            switch (meshAxisChoices.Count)
            {
                case > 1:
                    SelectedCoordinateSystem = meshAxisChoices[EditorGUILayout.Popup("座標軸", meshAxisChoices.IndexOf(this.SelectedCoordinateSystem), meshAxisDisplay)];
                    break;
                case 1:
                    // 選択肢が1つだけなら、表示する意味がないので何もしません
                    break;
                default:
                    throw new InvalidOperationException("No coordinate system is available.");
            }
        }
    }
}