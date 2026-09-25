using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop.Editor
{
    // The JSON is the single layout source, also read by Tools/validate_level_layouts.py.
    public static class ThreeLevelSceneBuilder
    {
        private const string Root = "Assets/HatHop";
        private const string LayoutPath = Root + "/Editor/LevelData/ThreeLevels.json";
        private static readonly string[] Keys = { "Easy", "Medium", "Hard" };

        [Serializable] private sealed class LayoutSet { public Layout[] levels; }
        [Serializable] private sealed class Layout
        {
            public string key, title;
            public int sections;
            public float roomWidth, roomHeight, traversalSeconds, warningSeconds;
            public float[] accent;
            public Box[] platforms, hazards;
            public float exitX, exitFloorY, exitWidth, exitHeight;
        }
        [Serializable] private sealed class Box
        {
            public string name;
            public float x, y, width, height;
        }

        [MenuItem("Hat Hop/Create Menu and Three Levels")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before creating the levels.");
                return;
            }
            LayoutSet layouts;
            try
            {
                layouts = JsonUtility.FromJson<LayoutSet>(File.ReadAllText(LayoutPath));
                Validate(layouts);
            }
            catch (Exception error)
            {
                Debug.LogError("Level generation stopped before modifying scenes: " + error.Message);
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool replacing = File.Exists(SceneNavigation.MenuScenePath);
            foreach (string key in Keys) replacing |= File.Exists(ScenePath(key));
            if (replacing && !EditorUtility.DisplayDialog("Rebuild menu and three levels?",
                "This replaces MainMenu, Easy, Medium and Hard scenes and updates their LevelCatalog mappings. " +
                "Save or commit any manual edits first. GameplayTest and other test scenes are preserved.",
                "Rebuild", "Cancel")) return;

            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Settings");
            Directory.CreateDirectory(Root + "/Art");
            Directory.CreateDirectory(Root + "/Physics");
            AssetDatabase.Refresh();
            Sprite square = CreateSquare();
            PhysicsMaterial2D material = CreateMaterial();
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(MainMenuSceneBuilder.CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, MainMenuSceneBuilder.CatalogPath);
            }
            catalog.easyScenePath = ScenePath("Easy");
            catalog.mediumScenePath = ScenePath("Medium");
            catalog.hardScenePath = ScenePath("Hard");
            EditorUtility.SetDirty(catalog);
            for (int i = 0; i < layouts.levels.Length; i++) Build(layouts.levels[i], i, catalog, square, material);
            AssetDatabase.SaveAssets();
            if (!MainMenuSceneBuilder.Build(false))
            {
                Debug.LogError("Levels saved, but menu creation did not complete. Run Create Main Menu Scene before testing.");
                return;
            }
            Debug.Log("Menu and three levels created. MainMenu is open. Test every difficulty and Next Level. " +
                "Static layout checks are not a Unity playtest; verify Build Profile scene overrides before building.");
        }

        private static void Validate(LayoutSet set)
        {
            if (set?.levels == null || set.levels.Length != 3) throw new InvalidDataException("Expected three layouts.");
            for (int i = 0; i < 3; i++)
            {
                Layout l = set.levels[i];
                if (l.key != Keys[i] || l.platforms == null || l.platforms.Length < 2 ||
                    l.hazards == null || l.accent == null || l.accent.Length != 3 ||
                    l.roomWidth <= 0 || l.roomHeight <= 0 || l.sections < 1 || l.exitWidth <= 0 || l.exitHeight < 2 ||
                    l.traversalSeconds <= 0 || l.warningSeconds <= 0)
                    throw new InvalidDataException("Invalid layout: " + Keys[i]);
                foreach (Box p in l.platforms)
                    if (p.width < 0.7f || p.height <= 0 || Mathf.Abs(p.x) + p.width / 2 >= l.roomWidth / 2)
                        throw new InvalidDataException("Invalid landing: " + p.name);
            }
        }

        private static string ScenePath(string key) => Root + "/Scenes/" + key + ".unity";

        private static void Build(Layout layout, int index, LevelCatalog catalog, Sprite square, PhysicsMaterial2D material)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Color accent = new Color(layout.accent[0], layout.accent[1], layout.accent[2]);
            Color surface = new Color(0.73f, 0.80f, 0.87f);
            Color red = new Color(1f, 0.22f, 0.19f);
            Transform map = new GameObject("MapRoot").transform;
            BoxObject("Background", Vector2.zero, new Vector2(layout.roomWidth, layout.roomHeight), map,
                square, new Color(0.055f, 0.075f, 0.115f), false, -20);
            Transform decoration = new GameObject("Section Markers").transform;
            decoration.SetParent(map, false);
            for (int s = 0; s < layout.sections; s++)
            {
                Box p = layout.platforms[s * 4];
                float nextY = s + 1 < layout.sections ? layout.platforms[(s + 1) * 4].y : layout.exitFloorY;
                Color band = Color.Lerp(new Color(0.06f, 0.08f, 0.12f), accent, s % 2 == 0 ? 0.09f : 0.16f);
                BoxObject("Section " + (s + 1), new Vector2(0, (p.y + nextY) / 2),
                    new Vector2(layout.roomWidth - 0.5f, nextY - p.y), decoration, square, band, false, -19);
                BoxObject("Section Stripe " + (s + 1), new Vector2(-layout.roomWidth / 2 + 0.48f, p.y),
                    new Vector2(0.14f, 1.6f), decoration, square, accent, false, -18);
            }
            BoxObject("Left Wall", new Vector2(-layout.roomWidth / 2, 0),
                new Vector2(0.4f, layout.roomHeight + 1), map, square, surface);
            BoxObject("Right Wall", new Vector2(layout.roomWidth / 2, 0),
                new Vector2(0.4f, layout.roomHeight + 1), map, square, surface);
            Transform platforms = new GameObject("Platforms").transform;
            platforms.SetParent(map, false);
            for (int i = 0; i < layout.platforms.Length; i++)
            {
                Box p = layout.platforms[i];
                BoxObject(p.name, new Vector2(p.x, p.y), new Vector2(p.width, p.height),
                    platforms, square, i % 4 == 0 ? Color.Lerp(surface, accent, 0.55f) : surface);
            }
            // Two solid horizontal faces block a straight fall into the green trigger in either orientation.
            Transform exit = new GameObject("Exit Alcove").transform;
            exit.SetParent(map, false);
            BoxObject("Exit Floor", new Vector2(layout.exitX, layout.exitFloorY),
                new Vector2(layout.exitWidth, 0.28f), exit, square, accent);
            BoxObject("Exit Roof", new Vector2(layout.exitX, layout.exitFloorY + layout.exitHeight),
                new Vector2(layout.exitWidth, 0.28f), exit, square, accent);
            BoxObject("Exit Back Wall", new Vector2(layout.exitX + layout.exitWidth / 2, layout.exitFloorY + layout.exitHeight / 2),
                new Vector2(0.28f, layout.exitHeight + 0.28f), exit, square, accent);

            Box start = layout.platforms[0];
            Vector2 spawn = new Vector2(start.x, start.y + start.height / 2 + 0.42f);
            GameObject player = BoxObject("Player", spawn, new Vector2(0.6f, 0.8f), null, square, new Color(0.23f, 0.8f, 1f));
            player.GetComponent<Collider2D>().sharedMaterial = material;
            player.GetComponent<SpriteRenderer>().enabled = false;
            GameObject visual = BoxObject("Visual", Vector2.zero, Vector2.one, player.transform,
                square, new Color(0.23f, 0.8f, 1f), false, 5);
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            PlayerMotor2D motor = player.AddComponent<PlayerMotor2D>();
            SetFloat(motor, "horizontalSpeed", 4.5f);
            SetFloat(motor, "jumpSpeed", 8f);
            SetFloat(motor, "jumpBufferSeconds", 0.12f);
            SetFloat(motor, "coyoteSeconds", 0.08f);
            PlayerHopVisual hop = player.AddComponent<PlayerHopVisual>();
            hop.Configure(motor, visual.transform);
            SetFloat(hop, "hopHeight", 0.4f);

            GameObject systems = new GameObject("Gameplay Systems");
            RotationController rotation = systems.AddComponent<RotationController>();
            rotation.Configure(map, motor, true);
            SetFloat(rotation, "traversalSeconds", layout.traversalSeconds);
            SetFloat(rotation, "warningSeconds", layout.warningSeconds);
            LevelFlow flow = systems.AddComponent<LevelFlow>();
            flow.Configure(rotation, motor, map);
            // Includes the full rotating room radius; the old fixed 25-unit guard is insufficient for larger maps.
            SetFloat(flow, "outOfBoundsDistance", new Vector2(layout.roomWidth, layout.roomHeight).magnitude / 2 + 6);
            GameplayHUD hud = systems.AddComponent<GameplayHUD>();
            hud.Configure(flow);
            hud.ConfigureLevel(catalog, index, layout.key.ToUpperInvariant() + " / " + layout.title);

            Transform hazards = new GameObject("Hazards").transform;
            hazards.SetParent(map, false);
            Trigger("Bottom Hazard", new Vector2(0, -layout.roomHeight / 2),
                new Vector2(layout.roomWidth, 0.8f), hazards, square, red, LevelTrigger2D.Kind.Hazard, flow);
            Trigger("Top Hazard", new Vector2(0, layout.roomHeight / 2),
                new Vector2(layout.roomWidth, 0.8f), hazards, square, red, LevelTrigger2D.Kind.Hazard, flow);
            foreach (Box h in layout.hazards)
                Trigger(h.name, new Vector2(h.x, h.y), new Vector2(h.width, h.height),
                    hazards, square, red, LevelTrigger2D.Kind.Hazard, flow);
            Trigger("Exit", new Vector2(layout.exitX + 0.1f, layout.exitFloorY + layout.exitHeight / 2),
                new Vector2(0.65f, layout.exitHeight - 0.5f), exit, square,
                new Color(0.25f, 1f, 0.52f), LevelTrigger2D.Kind.Goal, flow);

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(spawn.x, spawn.y, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.07f);
            camera.gameObject.AddComponent<AudioListener>();
            camera.gameObject.AddComponent<PlayerFollowCamera>().Configure(motor, rotation, 10);
            if (!EditorSceneManager.SaveScene(scene, ScenePath(layout.key)))
                throw new IOException("Could not save " + layout.key + ". Stop and inspect the Console before rebuilding.");
        }

        private static void SetFloat(UnityEngine.Object target, string field, float value)
        {
            SerializedObject data = new SerializedObject(target);
            SerializedProperty property = data.FindProperty(field);
            if (property == null) throw new InvalidOperationException("Missing setting " + field + "; import the movement update first.");
            property.floatValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BoxObject(string name, Vector2 position, Vector2 size, Transform parent,
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

        private static void Trigger(string name, Vector2 position, Vector2 size, Transform parent,
            Sprite sprite, Color color, LevelTrigger2D.Kind kind, LevelFlow flow)
        {
            GameObject go = BoxObject(name, position, size, parent, sprite, color);
            go.AddComponent<LevelTrigger2D>().Configure(kind, flow);
        }

        private static PhysicsMaterial2D CreateMaterial()
        {
            const string path = Root + "/Physics/LevelPlayer.physicsMaterial2D";
            PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material == null)
            {
                material = new PhysicsMaterial2D("LevelPlayer");
                AssetDatabase.CreateAsset(material, path);
            }
            material.friction = 0;
            material.bounciness = 0;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Sprite CreateSquare()
        {
            const string path = Root + "/Art/LevelSquare.png";
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
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
