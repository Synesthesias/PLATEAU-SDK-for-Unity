using PLATEAU.Dataset;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// パッケージ種を複数選択する<see cref="Element"/>です。
    /// </summary>
    internal class MultiPackageSelectElement : Element
    {
        public SortedSet<PredefinedCityModelPackage> Choices { get; private set; } = new ();
        public Dictionary<PredefinedCityModelPackage, bool> SelectedDict { get; private set; }= new();
        private string label;
        private bool defaultValue;

        public HashSet<PredefinedCityModelPackage> SelectedPackages
        {
            get
            {
                return SelectedDict.Where(pair => pair.Value).Select(pair => pair.Key).ToHashSet();
            }
        }

        public MultiPackageSelectElement(string name, string label, bool defaultValue) : base(name)
        {
            this.label = label;
            this.defaultValue = defaultValue;
        }
        
        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField(label);
                using (PlateauEditorStyle.VerticalScopeLevel2())
                {
                    foreach (var c in Choices)
                    {
                        SelectedDict[c] = EditorGUILayout.ToggleLeft(c.ToJapaneseName(), SelectedDict[c]);
                    }
                }
            }
        }

        public void SetChoices(IEnumerable<PredefinedCityModelPackage> packages)
        {
            Choices.Clear();
            SelectedDict.Clear();
            foreach (var p in packages)
            {
                Choices.Add(p);
            }

            foreach (var c in Choices)
            {
                SelectedDict.Add(c, defaultValue);
            }
        }

        public override void Reset() { }
        public override void Dispose()
        {
        }
    }
}