using UnityEngine;
using UnityEditor;
using System.IO;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Texture processor for batch texture operations including compression, atlas generation, and format conversion.
    /// </summary>
    public class TextureProcessor
    {
        [MenuItem("Tools/Frontier/Process Selected Textures")]
        public static void ProcessSelectedTextures()
        {
            string[] guids = Selection.assetGUIDs;
            if (guids.Length == 0)
            {
                Debug.LogWarning("[TextureProcessor] No textures selected");
                return;
            }

            int processedCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ProcessTexture(path))
                {
                    processedCount++;
                }
            }

            Debug.Log($"[TextureProcessor] Processed {processedCount} textures");
        }

        public static bool ProcessTexture(string assetPath)
        {
            if (!assetPath.EndsWith(".png") && !assetPath.EndsWith(".jpg") && !assetPath.EndsWith(".tga"))
            {
                return false;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;

            // Detect texture type from path or name
            TextureType detectedType = DetectTextureType(assetPath);
            
            switch (detectedType)
            {
                case TextureType.Albedo:
                    SetupAlbedoTexture(importer);
                    break;
                case TextureType.NormalMap:
                    SetupNormalMap(importer);
                    break;
                case TextureType.Metallic:
                    SetupMetallicTexture(importer);
                    break;
                case TextureType.Height:
                    SetupHeightMap(importer);
                    break;
                case TextureType.OCCLUSION:
                    SetupOcclusionMap(importer);
                    break;
                default:
                    SetupGenericTexture(importer);
                    break;
            }

            AssetDatabase.ImportAsset(assetPath);
            return true;
        }

        private enum TextureType
        {
            Albedo,
            NormalMap,
            Metallic,
            Height,
            OCCLUSION,
            Generic
        }

        private static TextureType DetectTextureType(string path)
        {
            string lowerPath = path.ToLower();
            
            if (lowerPath.Contains("_albedo") || lowerPath.Contains("_diffuse") || lowerPath.Contains("_color"))
                return TextureType.Albedo;
            if (lowerPath.Contains("_normal") || lowerPath.Contains("_nrm"))
                return TextureType.NormalMap;
            if (lowerPath.Contains("_metallic") || lowerPath.Contains("_metal"))
                return TextureType.Metallic;
            if (lowerPath.Contains("_height") || lowerPath.Contains("_displacement") || lowerPath.Contains("_disp"))
                return TextureType.Height;
            if (lowerPath.Contains("_occlusion") || lowerPath.Contains("_ao"))
                return TextureType.OCCLUSION;
            
            return TextureType.Generic;
        }

        private static void SetupAlbedoTexture(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.textureShape = TextureImporterShape.Texture2D;
            
            SetCompressionSettings(importer, TextureImporterCompression.Auto, 2048);
        }

        private static void SetupNormalMap(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.convertToNormalmap = false;
            importer.normalMapFilter = TextureImporterNormalFilter.Standard;
            
            SetCompressionSettings(importer, TextureImporterCompression.Uncompressed, 2048);
        }

        private static void SetupMetallicTexture(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.NoAlpha;
            importer.grayscaleToAlpha = false;
            
            SetCompressionSettings(importer, TextureImporterCompression.HighQuality, 1024);
        }

        private static void SetupHeightMap(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Heightmap;
            importer.heightmapScale = 0.02f;
            
            SetCompressionSettings(importer, TextureImporterCompression.Uncompressed, 2048);
        }

        private static void SetupOcclusionMap(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.NoAlpha;
            
            SetCompressionSettings(importer, TextureImporterCompression.HighQuality, 1024);
        }

        private static void SetupGenericTexture(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            
            SetCompressionSettings(importer, TextureImporterCompression.Auto, 2048);
        }

        private static void SetCompressionSettings(TextureImporter importer, TextureImporterCompression compression, int maxSize)
        {
            importer.maxTextureSize = maxSize;
            importer.textureCompression = compression;
            importer.crunchedCompression = false;
            importer.compressionQuality = 50;
        }

        public static void GenerateTextureAtlas(string outputPath, string[] texturePaths, int atlasSize)
        {
            // Placeholder for texture atlas generation
            Debug.Log($"[TextureProcessor] Generating atlas at {outputPath} with {texturePaths.Length} textures");
        }
    }
}
