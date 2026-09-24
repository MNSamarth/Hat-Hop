using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop.Editor
{
    public static class MovementSceneBuilder
    {
        private const string Root = "Assets/HatHop";
        private const string ScenePath = Root + "/Scenes/MovementTest.unity";

        [MenuItem("Hat Hop/Create Movement Test Scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Replace movement test?",
                "This replaces only MovementTest.unity. Commit any edits you want to keep first.",
                "Replace", "Cancel")) return;

            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Art");
            AssetDatabase.Refresh();
            Sprite square = CreateSquare();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
            camera.gameObject.AddComponent<AudioListener>();

            Transform map = new GameObject("MapRoot").transform;
            MakeBox("Ground", new Vector2(0, -3), new Vector2(12, 0.5f), map, square);
            MakeBox("Step 1", new Vector2(-2.2f, -2), new Vector2(2, 0.3f), map, square);
            MakeBox("Step 2", new Vector2(0, -1), new Vector2(2, 0.3f), map, square);
            MakeBox("Step 3", new Vector2(2.2f, 0), new Vector2(2, 0.3f), map, square);
            GameObject player = MakeBox("Player", new Vector2(-4, -2), new Vector2(0.6f, 0.8f), null, square);
            player.GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.8f, 1f);
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            player.AddComponent<PlayerMotor2D>();
            player.AddComponent<MovementTestSession>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = player;
            Debug.Log("Movement test created. A/D steer, Space buffers one big hop, R resets. Play Mode validation is pending.");
        }

        private static GameObject MakeBox(string name, Vector2 position, Vector2 size, Transform parent, Sprite sprite)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.75f, 0.78f, 0.83f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }

        private static Sprite CreateSquare()
        {
            const string path = Root + "/Art/GreyboxSquare.png";
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 2;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
