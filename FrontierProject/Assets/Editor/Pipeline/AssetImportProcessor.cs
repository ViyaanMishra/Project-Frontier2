using UnityEngine;
using UnityEditor;
using System.IO;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Handles batch import settings and validation for all asset types.
    /// </summary>
    public class AssetImportProcessor : AssetPostprocessor
    {
        private static readonly string[] supportedExtensions = { ".fbx", ".obj", ".png", ".jpg", ".wav", ".mp3" };
        
        private void OnPreprocessModel()
        {
            ModelImporter importer = assetImporter as ModelImporter;
            if (importer == null) return;

            // Optimize import settings
            importer.importBlendShapes = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.Import;
            importer.importAnimation = true;
            importer.importCameras = false;
            importer.importLights = false;
            
            // Enable rig if humanoid
            if (IsHumanoidRig(importer))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            }
        }

        private void OnPreprocessTexture()
        {
            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null) return;

            // Set texture type based on path
            if (assetPath.Contains("/NormalMaps/"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else if (assetPath.Contains("/HeightMaps/"))
            {
                importer.textureType = TextureImporterType.Heightmap;
            }
            else if (assetPath.Contains("/Sprites/"))
            {
                importer.textureType = TextureImporterType.Sprite;
            }

            // Compression settings
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Auto;
        }

        private void OnPreprocessAudio()
        {
            AudioImporter importer = assetImporter as AudioImporter;
            if (importer == null) return;

            // Default audio settings
            importer.loadInBackground = true;
            importer.preloadAudioData = false;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool needsRefresh = false;

            foreach (string asset in importedAssets)
            {
                if (ShouldProcessAsset(asset))
                {
                    Debug.Log($"[AssetImportProcessor] Processing: {asset}");
                    needsRefresh = true;
                }
            }

            if (needsRefresh)
            {
                AssetDatabase.Refresh();
            }
        }

        private bool IsHumanoidRig(ModelImporter importer)
        {
            // Check for humanoid rig indicators
            return importer.sourceFilePath.ToLower().Contains("human") ||
                   importer.sourceFilePath.ToLower().Contains("character");
        }

        private static bool ShouldProcessAsset(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            foreach (string supported in supportedExtensions)
            {
                if (ext == supported) return true;
            }
            return false;
        }
    }
}
