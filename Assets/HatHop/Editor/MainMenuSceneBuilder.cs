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

        [MenuItem("Leap of Faith/Create Main Menu Scene")]
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
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
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
            PlayerSettings.productName = "Leap of Faith";
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

            Box("Accent", stage, 40, 70, 48, 5, Gold);
            Label("Title", stage, "LEAP OF\nFAITH", 34, 112, 525, 145, 62, White, true);
            // Abstract platforms and a square player; no character or narrative art.
            Box("Lower Platform", stage, 42, 504, 170, 9, Muted);
            Box("Middle Platform", stage, 210, 426, 124, 9, Muted);
            Box("Upper Platform", stage, 354, 348, 132, 9, Muted);
            Box("Player Square", stage, 112, 462, 30, 38, Cyan);
            RectTransform mascot = null;
            Box("Card", stage, 580, 40, 480, 545, Panel);

            RectTransform home = Rect("Home", stage, 610, 69, 420, 454);
            Label("Heading", home, "PLAY", 0, 0, 420, 45, 29, White, true);
            Button play = MakeButton(home, "Play", "START", 115, Cyan, Ink);
            Button levels = MakeButton(home, "Level Select", "LEVEL SELECT", 203, new Color(0.16f, 0.2f, 0.29f), White);
            Button controls = MakeButton(home, "Controls", "CONTROLS", 291, new Color(0.16f, 0.2f, 0.29f), White);

            RectTransform selection = Rect("Level Select", stage, 610, 69, 420, 454);
            Label("Heading", selection, "LEVEL SELECT", 0, 0, 420, 45, 26, White, true);
            Button prologue = MakeButton(selection, "Prologue", "BEGINNER", 92, Cyan, Ink);
            Button easy = MakeButton(selection, "Easy", "EASY   /   COMING SOON", 167, new Color(0.16f, 0.2f, 0.29f), White);
            Button medium = MakeButton(selection, "Medium", "MEDIUM   /   COMING SOON", 242, new Color(0.16f, 0.2f, 0.29f), White);
            Button hard = MakeButton(selection, "Hard", "HARD   /   COMING SOON", 317, new Color(0.16f, 0.2f, 0.29f), White);
            Button levelsBack = MakeButton(selection, "Back", "BACK", 392, new Color(0.16f, 0.2f, 0.29f), White);

            RectTransform help = Rect("Controls", stage, 610, 69, 420, 454);
            Label("Heading", help, "CONTROLS", 0, 0, 420, 45, 28, White, true);
            Label("Keys", help, "A / D or LEFT / RIGHT    Move\nSPACE or UP                    Jump\nR                                        Restart", 0, 70, 420, 117, 22, White);
            Label("Rules", help, "Collect stars for a 0-5 rating.\nGreen: exit. Red faces: danger.\nWatch warnings before each flip.\nFlip to enter golden star pockets.\nHard: light both outer pads to open exit.\nUse raised seesaw tips to climb.", 0, 207, 420, 151, 19, Muted);
            Button controlsBack = MakeButton(help, "Back", "BACK", 392, new Color(0.16f, 0.2f, 0.29f), White);
            Text status = Label("Status", stage, "", 610, 535, 420, 40, 17, Gold);

            MainMenuController controller = canvasObject.AddComponent<MainMenuController>();
            controller.Configure(catalog, home.gameObject, selection.gameObject, help.gameObject,
                play, levels, controls, easy, medium, hard, prologue, levelsBack, controlsBack, status, mascot);
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
            Debug.Log("MainMenu saved. Play opens Prologue when mapped, otherwise Easy or GameplayTest. " +
                "Open Build Profiles and verify MainMenu is first if your active profile overrides the global scene list.");
            return true;
        }

        [MenuItem("Leap of Faith/Refresh Menu Build Scenes")]
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
            AddScene(scenes, added, catalog.prologueScenePath);
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
                Debug.LogWarning("Leap of Faith: mapped scene does not exist: " + path);
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
