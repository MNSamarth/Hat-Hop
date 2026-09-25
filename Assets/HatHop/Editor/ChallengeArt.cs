using System.IO;
using UnityEditor;
using UnityEngine;

namespace HatHop.Editor
{
    // Rasterize original geometric UI/game icons. No external art dependencies.
    public static class ChallengeArt
    {
        public static Sprite CreateStar()
        {
            Vector2[] vertices = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = (90 + i * 36) * Mathf.Deg2Rad;
                vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? 0.47f : 0.22f);
            }
            return Make("CollectibleStar", point => Inside(point, vertices), new Color(1f, 0.8f, 0.16f));
        }
        public static Texture2D CreateArrow() => Make("ExitArrow", p =>
            (p.y > 0 && p.y < 0.46f && Mathf.Abs(p.x) < 0.46f - p.y) ||
            (p.y <= 0.1f && p.y > -0.43f && Mathf.Abs(p.x) < 0.11f), Color.white).texture;

        private static bool Inside(Vector2 p, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
                if ((polygon[i].y > p.y) != (polygon[j].y > p.y) &&
                    p.x < (polygon[j].x - polygon[i].x) * (p.y - polygon[i].y) /
                    (polygon[j].y - polygon[i].y) + polygon[i].x) inside = !inside;
            return inside;
        }
        private static Sprite Make(string name, System.Func<Vector2, bool> mask, Color color)
        {
            string path = "Assets/HatHop/Art/" + name + ".png";
            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                pixels[y * 64 + x] = mask(new Vector2((x + 0.5f) / 64 - 0.5f, (y + 0.5f) / 64 - 0.5f)) ? color : Color.clear;
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
