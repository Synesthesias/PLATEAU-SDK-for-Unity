using System;
using UnityEditor;
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

        /// <summary>
        /// Material用シェーダー付きのOpaqueマテリアル
        /// </summary>
        public static Material PLATEAUX3DMaterial
        {
            get
            {
                return new Material(
                    (Material)AssetDatabase.LoadAssetAtPath(
                        PathUtil.SdkPathToAssetPath("Materials/PLATEAUX3DMaterial.mat"),
                        typeof(Material)));
            }
        }

        /// <summary>
        /// Material用シェーダー付きのTransparentマテリアル
        /// </summary>
        public static Material PLATEAUX3DMaterial_Transparent
        {
            get
            {
                return new Material(
                    (Material)AssetDatabase.LoadAssetAtPath(
                        PathUtil.SdkPathToAssetPath("Materials/PLATEAUX3DMaterial_Transparent.mat"),
                        typeof(Material)));
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
                    stdMat.SetColor("_EmissionColor", new Color(rawMaterial.Emissive.X, rawMaterial.Emissive.Y, rawMaterial.Emissive.Z));

                    stdMat.SetFloat("_OcclusionStrength", rawMaterial.AmbientIntensity);
                    stdMat.SetFloat("_Glossiness", rawMaterial.Shininess);

                    return stdMat;
                }
            }

            var material = transparency < 1f ? PLATEAUX3DMaterial_Transparent : PLATEAUX3DMaterial;
            /*
            var material = PLATEAUX3DMaterial; 
            if( transparency < 1f)
            {
                material.SetOverrideTag("RenderType", "Transparent");

                //Builtin
                material.SetFloat("_BUILTIN_Surface", 1f);
                material.SetInt("_BUILTIN_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_BUILTIN_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_BUILTIN_ZWrite", 0);
                material.DisableKeyword("_BUILTIN_ALPHATEST_ON");
                material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                //URP
                material.SetFloat("_Surface", 1f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
          
                //HDRP
                material.SetFloat("_SurfaceType", 1f);
                material.SetInt("_AlphaSrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_AlphaDstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_TransparentZWrite", 0);
                material.SetFloat("_RenderQueueType", 5);
                material.SetFloat("_BlendMode", 0);
                material.SetFloat("_AlphaCutoffEnable", 0);
                material.SetFloat("_ZTestDepthEqualForOpaque", 4f);  
                material.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
                //material.EnableKeyword("_BLENDMODE_ALPHA");
                //material.DisableKeyword("_BLENDMODE_ADD");
                //material.DisableKeyword("_BLENDMODE_PRE_MULTIPLY");

                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            */

            material.SetVector("_Diffuse", new Vector3(rawMaterial.Diffuse.X, rawMaterial.Diffuse.Y, rawMaterial.Diffuse.Z));
            material.SetVector("_Specular", new Vector3(rawMaterial.Specular.X, rawMaterial.Specular.Y, rawMaterial.Specular.Z));
            material.SetVector("_Emissive", new Vector3(rawMaterial.Emissive.X, rawMaterial.Emissive.Y, rawMaterial.Emissive.Z));

            material.SetFloat("_Transparency", transparency); //(0で不透明、1で透明)
            material.SetFloat("_AmbientIntensity", rawMaterial.AmbientIntensity);
            material.SetFloat("_Shininess", rawMaterial.Shininess);
            material.SetInt("_IsSmooth", rawMaterial.IsSmooth ? 1 : 0);

            return material;
        }
    }
}
