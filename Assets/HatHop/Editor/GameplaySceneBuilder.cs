using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop.Editor
{
    public static class GameplaySceneBuilder
    {
        private const string Root = "Assets/HatHop";
        private const string ScenePath = Root + "/Scenes/GameplayTest.unity";

        [MenuItem("Hat Hop/Create Gameplay Test Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before creating the gameplay scene.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Replace gameplay test?",
                "This replaces GameplayTest.unity only. Commit any scene edits you want to keep first.",
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
            camera.orthographicSize = 10;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.085f);
            camera.gameObject.AddComponent<AudioListener>();

            Transform map = new GameObject("MapRoot").transform;
            Color grey = new Color(0.73f, 0.77f, 0.84f);
            MakeBox("Background", Vector2.zero, new Vector2(12, 12), map, square,
                new Color(0.10f, 0.15f, 0.22f), false, -10);
            MakeBox("Orientation Marker", new Vector2(-4.5f, 5.1f), new Vector2(2, 0.16f), map,
                square, new Color(1f, 0.74f, 0.22f), false, -9);
            MakeBox("Left Wall", new Vector2(-6, 0), new Vector2(0.5f, 13), map, square, grey);
            MakeBox("Right Wall", new Vector2(6, 0), new Vector2(0.5f, 13), map, square, grey);
            Transform platforms = new GameObject("Platforms").transform;
            platforms.SetParent(map, false);
            MakeBox("Start Platform", new Vector2(-4.4f, -4.8f), new Vector2(2.4f, 0.3f), platforms, square, grey);
            Vector2[] steps = {
                new Vector2(-2f, -3.6f), new Vector2(0.4f, -2.4f),
                new Vector2(2.8f, -1.2f), new Vector2(0.4f, 0f),
                new Vector2(-2f, 1.2f), new Vector2(0.4f, 2.4f)
            };
            for (int i = 0; i < steps.Length; i++)
                MakeBox("Step " + (i + 1), steps[i], new Vector2(2.2f, 0.3f), platforms, square, grey);
            MakeBox("Exit Ledge", new Vector2(4.4f, 3.6f), new Vector2(2.2f, 0.3f), platforms, square, grey);
            MakeBox("Exit Alcove Roof", new Vector2(4.5f, 5.1f), new Vector2(2.2f, 0.3f), platforms, square, grey);

            GameObject player = MakeBox("Player", new Vector2(-4.7f, -4.05f), new Vector2(0.6f, 0.8f),
                null, square, new Color(0.2f, 0.78f, 1f));
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            PlayerMotor2D motor = player.AddComponent<PlayerMotor2D>();
            GameObject systems = new GameObject("Gameplay Systems");
            RotationController rotation = systems.AddComponent<RotationController>();
            rotation.Configure(map, motor, true);
            LevelFlow flow = systems.AddComponent<LevelFlow>();
            flow.Configure(rotation, motor, map);
            systems.AddComponent<GameplayHUD>().Configure(flow);

            Transform hazards = new GameObject("Hazards").transform;
            hazards.SetParent(map, false);
            Color red = new Color(1f, 0.22f, 0.18f);
            MakeTrigger("Bottom Hazard", new Vector2(0, -6.1f), new Vector2(12.5f, 1.2f),
                hazards, square, red, LevelTrigger2D.Kind.Hazard, flow);
            MakeTrigger("Top Hazard", new Vector2(0, 6.1f), new Vector2(12.5f, 1.2f),
                hazards, square, red, LevelTrigger2D.Kind.Hazard, flow);
            MakeTrigger("Exit", new Vector2(3.85f, 4.35f), new Vector2(0.9f, 1f),
                map, square, new Color(0.25f, 1f, 0.52f), LevelTrigger2D.Kind.Goal, flow);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = systems;
            Debug.Log("Gameplay test saved. Reach green, avoid red. R restarts in every phase. Play Mode checks are pending.");
        }

        private static void MakeTrigger(string name, Vector2 position, Vector2 size, Transform parent,
            Sprite sprite, Color color, LevelTrigger2D.Kind kind, LevelFlow flow)
        {
            GameObject go = MakeBox(name, position, size, parent, sprite, color);
            go.AddComponent<LevelTrigger2D>().Configure(kind, flow);
        }

        private static GameObject MakeBox(string name, Vector2 position, Vector2 size, Transform parent,
            Sprite sprite, Color color, bool solid = true, int order = 0)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            if (solid) go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }

        private static Sprite CreateSquare()
        {
            const string path = Root + "/Art/GameplaySquare.png";
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
