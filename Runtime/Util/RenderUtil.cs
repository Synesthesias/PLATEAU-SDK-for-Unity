#if  !UNITY_EDITOR
using System;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.Util
{
    public static class RenderUtil
    {
        private static readonly int GlossinessPropertyId = Shader.PropertyToID("_Glossiness");

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

        public static Material CreateDefaultMaterial()
        {
            var mat =  new Material(RenderUtil.DefaultMaterial)
            {
                enableInstancing = true,
            };
            mat.SetFloat(GlossinessPropertyId, 0f); // 感覚的だが、航空写真が貼り付けられた土地などはSmoothnessが0のほうが、それらしい見た目になると思った
            return mat;
        }

        /// <summary>
        /// Material用シェーダー付きのOpaqueマテリアル
        /// </summary>
        public static Material PLATEAUX3DMaterial
        {
            get
            {
                #if UNITY_EDITOR
                return new Material(
                    (Material)AssetDatabase.LoadAssetAtPath(
                        PathUtil.SdkPathToAssetPath("Materials/PLATEAUX3DMaterial.mat"),
                        typeof(Material)));
                #else
                throw new NotImplementedException("This function is only supported in editor.");
                #endif
            }
        }

        /// <summary>
        /// Material用シェーダー付きのTransparentマテリアル
        /// </summary>
        public static Material PLATEAUX3DMaterial_Transparent
        {
            get
            {
                #if UNITY_EDITOR
                return new Material(
                    (Material)AssetDatabase.LoadAssetAtPath(
                        PathUtil.SdkPathToAssetPath("Materials/PLATEAUX3DMaterial_Transparent.mat"),
                        typeof(Material)));
                #else
                throw new NotImplementedException("This function is only supported in editor.");
                #endif
            }
        }

        /// <summary>
        /// CityGML.MaterialとRenderPipelineによりShaderを判別しMaterialを返します
        /// </summary>
        /// <param name="rawMaterial"></param>
        /// <returns></returns>
        public static Material GetPLATEAUX3DMaterialByCityGMLMaterial(PLATEAU.CityGML.Material rawMaterial)
        {
            float transparency = 1f - rawMaterial.Transparency; //(0で不透明、1で透明)

            var pipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (pipelineAsset == null) // Built-in Render Pipeline のとき 
            {
                //Specularに値が入っている場合ビルトインではStardard (Specular setup) Shader を使用
                if (rawMaterial.Specular.X > 0 || rawMaterial.Specular.Y > 0 || rawMaterial.Specular.Z > 0)
                {
                    var stdMat = new Material(Shader.Find("Standard (Specular setup)"));

                    if (transparency < 1f)
                    {
                        stdMat.SetInt("_Mode", 3);
                        stdMat.SetOverrideTag("RenderType", "Transparent");
                        stdMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        stdMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        stdMat.SetInt("_ZWrite", 0);
                        stdMat.DisableKeyword("_ALPHATEST_ON");
                        stdMat.EnableKeyword("_ALPHABLEND_ON");
                        stdMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        stdMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }

                    stdMat.SetColor("_Color", new Color(rawMaterial.Diffuse.X, rawMaterial.Diffuse.Y, rawMaterial.Diffuse.Z, transparency));
                    stdMat.SetColor("_SpecColor", new Color(rawMaterial.Specular.X, rawMaterial.Specular.Y, rawMaterial.Specular.Z));

                    if(rawMaterial.Emissive.X > 0 || rawMaterial.Emissive.Y > 0 || rawMaterial.Emissive.Z > 0 )
                        stdMat.EnableKeyword("_EMISSION");
                    stdMat.SetColor("_EmissionColor", new Color(rawMaterial.Emissive.X, rawMaterial.Emissive.Y, rawMaterial.Emissive.Z));

                    stdMat.SetFloat("_OcclusionStrength", rawMaterial.AmbientIntensity);
                    stdMat.SetFloat("_Glossiness", rawMaterial.Shininess);

                    return stdMat;
                }
            }

            var material = transparency < 1f ? PLATEAUX3DMaterial_Transparent : PLATEAUX3DMaterial;

            material.SetVector("_Diffuse", new Vector3(rawMaterial.Diffuse.X, rawMaterial.Diffuse.Y, rawMaterial.Diffuse.Z));
            material.SetVector("_Specular", new Vector3(rawMaterial.Specular.X, rawMaterial.Specular.Y, rawMaterial.Specular.Z));
            material.SetVector("_Emissive", new Vector3(rawMaterial.Emissive.X, rawMaterial.Emissive.Y, rawMaterial.Emissive.Z));

            material.SetFloat("_Transparency", transparency); //(0で不透明、1で透明)
            material.SetFloat("_AmbientIntensity", rawMaterial.AmbientIntensity);
            material.SetFloat("_Shininess", rawMaterial.Shininess);
            material.SetInt("_IsSmooth", rawMaterial.IsSmooth ? 1 : 0);

            if (pipelineAsset != null)
            {
                string pipelineName = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name;
                bool isHDRP = pipelineName == "HDRenderPipelineAsset";
                material.SetInt("_IsHDRP", isHDRP ? 1 : 0);
            }
            
            return material;
        }
    }
}
