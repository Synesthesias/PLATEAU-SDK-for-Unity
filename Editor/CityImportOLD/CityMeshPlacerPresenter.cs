using PLATEAU.CityMeta;

namespace PLATEAU.Editor.CityImportOLD
{
    internal class CityMeshPlacerPresenter
    {
        private CityMetadata metadata;

        public void Draw(CityMetadata metadataArg)
        {
            this.metadata = metadataArg;
            var importConf = this.metadata.cityImportConfig;
            CityMeshPlacerView.Draw(this, importConf.cityMeshPlacerConfig, importConf.generatedObjFiles);
        }

        public void Place()
        {
            CityMeshPlacerModel.Place(this.metadata.cityImportConfig.cityMeshPlacerConfig, this.metadata, null);
        }
    }
}