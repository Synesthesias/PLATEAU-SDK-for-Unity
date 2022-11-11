using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.Util
{
    public static class RenderUtil
    {
        public static Material DefaultMaterial
        {
            get
            {
                var pipelineAsset = GraphicsSettings.renderPipelineAsset;
                var defaultMat = pipelineAsset == null ?
                    new Material(Shader.Find("Standard")) : // Built-in Render Pipeline のとき 
                    pipelineAsset.defaultMaterial; // Universal Render Pipeline または High Definition Render Pipeline のとき
                return defaultMat;
            }

        }
    }
}
