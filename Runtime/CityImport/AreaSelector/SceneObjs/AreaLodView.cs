using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.AreaSelector
{
    public class AreaLodView
    {
        private PackageLod[] packageLods = null;
        private Vector3 position;
        private static readonly Color textColor = Color.black;

        public AreaLodView(IEnumerable<PackageLod> packageLods, Vector3 position)
        {
            this.packageLods = packageLods.ToArray();
            this.position = position;
        }

        public void DrawHandles()
        {
            if (this.packageLods == null) return;
            var sb = new StringBuilder();
            foreach (var packageLod in this.packageLods)
            {
                if (packageLod.Lods.Count == 0) continue;
                sb.Append($"{packageLod.Package} => ");
                foreach(var lod in packageLod.Lods)
                {
                    sb.Append($"{lod}, ");
                }

                sb.Append("\n");
            }

            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = textColor;
            Handles.Label(this.position, sb.ToString(), style);
        }
    }
}
