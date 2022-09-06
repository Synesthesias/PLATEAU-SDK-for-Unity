using System.IO;
using UnityEngine;

namespace PLATEAU.CityLoader
{
    internal class PLATEAUCityModelLoader : MonoBehaviour
    {
        [SerializeField] private string sourcePathBeforeImport;
        [SerializeField] private string sourcePathAfterImport; // TODO
        
        public static GameObject Create(string sourcePathBeforeImport)
        {
            string objName = Path.GetFileName(sourcePathBeforeImport);
            var obj = new GameObject(objName);
            var loader = obj.AddComponent<PLATEAUCityModelLoader>();
            loader.Init(sourcePathBeforeImport);
            return obj;
        }

        private void Init(string sourcePathBeforeImportArg)
        {
            this.sourcePathBeforeImport = sourcePathBeforeImportArg;
        }

        public string SourcePathBeforeImport => this.sourcePathBeforeImport;
        public string SourcePathAfterImport  => this.sourcePathAfterImport;

    }
}
