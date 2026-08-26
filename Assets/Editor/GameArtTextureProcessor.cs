using UnityEditor;
using UnityEngine;

namespace GameRpg.EditorTools
{
    /// <summary>
    /// Applies pixel-art-appropriate import settings to the third-party Kenney assets
    /// under Assets/Art/ (feature 003) — point filtering, no compression, and (for the
    /// UI Pack button/panel sprites) a 9-slice border so DemoUiKit can stretch them to
    /// arbitrary button/panel sizes without visible seams.
    /// </summary>
    public class GameArtTextureProcessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var normalizedPath = assetPath.Replace('\\', '/');
            if (!normalizedPath.Contains("Assets/Art/"))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;

            if (normalizedPath.Contains("Assets/Art/Characters/"))
            {
                importer.spritePixelsPerUnit = 16f;
            }
            else if (normalizedPath.Contains("Assets/Art/UI/"))
            {
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = new Vector4(24f, 24f, 24f, 24f);
            }
        }
    }
}
