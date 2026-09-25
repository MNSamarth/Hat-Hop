using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace HatHop.Editor
{
    public static class MainMenuSceneBuilder
    {
        public const string CatalogPath = "Assets/HatHop/Settings/LevelCatalog.asset";
        private static readonly Color Ink = new Color(0.055f, 0.065f, 0.11f);
        private static readonly Color Panel = new Color(0.09f, 0.11f, 0.18f);
        private static readonly Color White = new Color(0.92f, 0.95f, 1f);
        private static readonly Color Muted = new Color(0.64f, 0.7f, 0.8f);
        private static readonly Color Cyan = new Color(0.23f, 0.82f, 0.95f);
        private static readonly Color Gold = new Color(1f, 0.76f, 0.34f);
        private static Font font;

        [MenuItem("Hat Hop/Create Main Menu Scene")]
        public static void Create()
        {
            Build(true);
        }

        public static bool Build(bool confirmReplace)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before creating a menu scene.");
                return false;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (confirmReplace && File.Exists(SceneNavigation.MenuScenePath) && !EditorUtility.DisplayDialog("Replace main menu?",
                "This replaces MainMenu.unity. Your gameplay scenes and level mappings are preserved.", "Replace", "Cancel")) return false;
            Directory.CreateDirectory("Assets/HatHop/Scenes");
            Directory.CreateDirectory("Assets/HatHop/Settings");
            AssetDatabase.Refresh();
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.gameObject.AddComponent<AudioListener>();

            GameObject canvasObject = new GameObject("Menu Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            RectTransform stage = Rect("Menu Layout", canvas.transform, 0, 0, 1100, 620);
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(0.5f, 0.5f);
            stage.anchoredPosition = Vector2.zero;

            Box("Accent", stage, 40, 42, 48, 5, Gold);
            Label("Eyebrow", stage, "A WORLD THAT WON'T STAY STILL", 40, 70, 480, 30, 17, Gold);
            Label("Title", stage, "HAT HOP", 34, 105, 525, 95, 78, White, true);
            Label("Tagline", stage, "Find your footing.\nEscape the next flip.", 40, 205, 460, 86, 29, Muted);

            // Self-created silhouette made from UI rectangles; no imported art.
            Box("Lower Platform", stage, 42, 504, 170, 9, Muted);
            Box("Upper Platform", stage, 322, 358, 165, 9, Muted);
            RectTransform mascot = Rect("Hopping Rabbit", stage, 116, 404, 74, 100);
            Box("Body", mascot, 0, 40, 74, 60, Cyan);
            Box("Left Ear", mascot, 8, 0, 17, 48, Cyan);
            Box("Right Ear", mascot, 47, 7, 17, 41, Cyan);
            Box("Eye", mascot, 53, 53, 7, 7, Ink);
            Box("Hat Crown", stage, 364, 291, 76, 60, White);
            Box("Hat Band", stage, 364, 327, 76, 12, Gold);
            Box("Hat Brim", stage, 348, 343, 108, 9, White);
            Label("Footer", stage, "JUMP  /  ADAPT  /  ESCAPE", 40, 559, 470, 28, 16, Muted);
            Box("Card", stage, 580, 40, 480, 545, Panel);

            RectTransform home = Rect("Home", stage, 610, 69, 420, 454);
            Label("Heading", home, "TAKE THE LEAP", 0, 0, 420, 45, 29, White, true);
            Label("Description", home, "A small rabbit. A shifting world.", 0, 53, 420, 38, 19, Muted);
            Button play = MakeButton(home, "Play", "PLAY", 115, Cyan, Ink);
            Button levels = MakeButton(home, "Level Select", "LEVEL SELECT", 203, new Color(0.16f, 0.2f, 0.29f), White);
            Button controls = MakeButton(home, "Controls", "CONTROLS", 291, new Color(0.16f, 0.2f, 0.29f), White);
            Label("Hint", home, "Watch the warning. Prepare your landing.", 0, 387, 420, 48, 17, Muted);

            RectTransform selection = Rect("Level Select", stage, 610, 69, 420, 454);
            Label("Heading", selection, "CHOOSE YOUR CHALLENGE", 0, 0, 420, 45, 26, White, true);
            Label("Description", selection, "Three ways out. One shifting world.", 0, 49, 420, 38, 18, Muted);
            Button easy = MakeButton(selection, "Easy", "EASY   /   COMING SOON", 105, new Color(0.16f, 0.2f, 0.29f), White);
            Button medium = MakeButton(selection, "Medium", "MEDIUM   /   COMING SOON", 187, new Color(0.16f, 0.2f, 0.29f), White);
            Button hard = MakeButton(selection, "Hard", "HARD   /   COMING SOON", 269, new Color(0.16f, 0.2f, 0.29f), White);
            Button levelsBack = MakeButton(selection, "Back", "BACK", 370, new Color(0.16f, 0.2f, 0.29f), White);

            RectTransform help = Rect("Controls", stage, 610, 69, 420, 454);
            Label("Heading", help, "FIND YOUR FOOTING", 0, 0, 420, 45, 28, White, true);
            Label("Keys", help, "A / D      Move left / right\nSPACE    Jump\nR             Restart the run", 0, 70, 420, 117, 22, White);
            Label("Rules", help, "Collect stars for a 0-5 rating.\nGreen: exit. Red faces: danger.\nWatch warnings before each flip.\nFlip to enter golden star pockets.\nHard: weight an end, cross the seesaw,\nthen jump from its raised tip.", 0, 207, 420, 151, 19, Muted);
            Button controlsBack = MakeButton(help, "Back", "BACK", 370, new Color(0.16f, 0.2f, 0.29f), White);
            Text status = Label("Status", stage, "", 610, 535, 420, 40, 17, Gold);

            MainMenuController controller = canvasObject.AddComponent<MainMenuController>();
            controller.Configure(catalog, home.gameObject, selection.gameObject, help.gameObject,
                play, levels, controls, easy, medium, hard, levelsBack, controlsBack, status, mascot);
            selection.gameObject.SetActive(false);
            help.gameObject.SetActive(false);
            GameObject events = new GameObject("EventSystem", typeof(EventSystem));
            events.GetComponent<EventSystem>().firstSelectedGameObject = play.gameObject;
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
            if (!EditorSceneManager.SaveScene(scene, SceneNavigation.MenuScenePath)) return false;
            RefreshBuildScenes();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = canvasObject;
            Debug.Log("MainMenu saved. Play opens Easy when mapped, otherwise GameplayTest. " +
                "Open Build Profiles and verify MainMenu is first if your active profile overrides the global scene list.");
            return true;
        }

        [MenuItem("Hat Hop/Refresh Menu Build Scenes")]
        public static void RefreshBuildScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before updating the build scene list.");
                return;
            }
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null || !File.Exists(SceneNavigation.MenuScenePath))
            {
                Debug.LogWarning("Create the main menu scene first.");
                return;
            }
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            HashSet<string> added = new HashSet<string>();
            AddScene(scenes, added, SceneNavigation.MenuScenePath);
            AddScene(scenes, added, catalog.easyScenePath);
            AddScene(scenes, added, catalog.mediumScenePath);
            AddScene(scenes, added, catalog.hardScenePath);
            AddScene(scenes, added, catalog.prototypeScenePath);
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
                if (added.Add(existing.path)) scenes.Add(existing);
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Global scene list refreshed: MainMenu first, mapped scenes enabled, other entries preserved. " +
                "If a Build Profile uses Override Global Scene List, update that profile's scene list too.");
        }

        private static void AddScene(List<EditorBuildSettingsScene> scenes, HashSet<string> added, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!File.Exists(path))
            {
                Debug.LogWarning("Hat Hop: mapped scene does not exist: " + path);
                return;
            }
            if (added.Add(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        // All layout values use a top-left origin inside the centered reference stage.
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        private static Image Box(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            Image image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text Label(string name, Transform parent, string text, float x, float y,
            float w, float h, int size, Color color, bool bold = false)
        {
            Text label = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.color = color;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private static Button MakeButton(Transform parent, string name, string text, float y, Color color, Color foreground)
        {
            Image background = Box(name, parent, 0, y, 420, 65, color);
            background.raycastTarget = true;
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 0.95f, 1f);
            colors.selectedColor = new Color(0.7f, 0.9f, 1f);
            colors.pressedColor = new Color(0.6f, 0.75f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            button.colors = colors;
            Text label = Label("Label", background.transform, text, 22, 0, 378, 65, 20, foreground, true);
            label.alignment = TextAnchor.MiddleLeft;
            return button;
        }
    }
}
