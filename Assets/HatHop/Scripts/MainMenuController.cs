using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace HatHop
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject levelsPanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Button levelsBack;
        [SerializeField] private Button controlsBack;
        [SerializeField] private Text status;
        [SerializeField] private RectTransform rabbit;
        private Vector2 rabbitRest;

        public void Configure(LevelCatalog levels, GameObject home, GameObject select,
            GameObject controls, Button play, Button choose, Button help, Button easy,
            Button medium, Button hard, Button backFromLevels, Button backFromControls,
            Text message, RectTransform mascot)
        {
            catalog = levels;
            homePanel = home;
            levelsPanel = select;
            controlsPanel = controls;
            playButton = play;
            levelsButton = choose;
            controlsButton = help;
            easyButton = easy;
            mediumButton = medium;
            hardButton = hard;
            levelsBack = backFromLevels;
            controlsBack = backFromControls;
            status = message;
            rabbit = mascot;
        }

        private void Start()
        {
            if (catalog == null || homePanel == null || levelsPanel == null || controlsPanel == null ||
                playButton == null || levelsButton == null || controlsButton == null ||
                easyButton == null || mediumButton == null || hardButton == null ||
                levelsBack == null || controlsBack == null || status == null)
            {
                Debug.LogError("Main menu references are incomplete. Recreate it with Hat Hop > Create Main Menu Scene.", this);
                enabled = false;
                return;
            }
#if ENABLE_INPUT_SYSTEM
            // Recreate defaults if an Editor-generated transient action reference was not saved.
            if (EventSystem.current != null)
            {
                InputSystemUIInputModule module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
                if (module != null && module.actionsAsset == null) module.AssignDefaultActions();
            }
#endif
            if (rabbit != null) rabbitRest = rabbit.anchoredPosition;
            playButton.interactable = SceneNavigation.CanLoad(catalog.PlayScenePath);
            BindLevel(easyButton, "EASY", catalog.easyScenePath, 0);
            BindLevel(mediumButton, "MEDIUM", catalog.mediumScenePath, 1);
            BindLevel(hardButton, "HARD", catalog.hardScenePath, 2);
            playButton.onClick.AddListener(() => Open(catalog.PlayScenePath));
            levelsButton.onClick.AddListener(() => Show(levelsPanel, levelsBack));
            controlsButton.onClick.AddListener(() => Show(controlsPanel, controlsBack));
            levelsBack.onClick.AddListener(ShowHome);
            controlsBack.onClick.AddListener(ShowHome);
            ShowHome();
            if (!playButton.interactable) status.text = "No playable level is available yet.";
        }

        private void BindLevel(Button button, string label, string path, int index)
        {
            bool ready = SceneNavigation.CanLoad(path);
            button.interactable = ready;
            button.GetComponentInChildren<Text>().text = label + (ready ? $"   /   BEST {LevelStars.BestFor(index)}/5" : "   /   COMING SOON");
            button.onClick.AddListener(() => Open(path));
        }

        private void ShowHome() => Show(homePanel, playButton.interactable ? playButton : levelsButton);

        private void Show(GameObject panel, Button focus)
        {
            homePanel.SetActive(panel == homePanel);
            levelsPanel.SetActive(panel == levelsPanel);
            controlsPanel.SetActive(panel == controlsPanel);
            status.text = "";
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(focus.gameObject);
            }
        }

        private void Open(string path)
        {
            if (SceneNavigation.IsLoading) return;
            if (SceneNavigation.TryLoad(path, out string error))
            {
                status.text = "Loading...";
                foreach (Button button in GetComponentsInChildren<Button>(true)) button.interactable = false;
            }
            else status.text = error;
        }

        private void Update()
        {
            if (rabbit != null)
                rabbit.anchoredPosition = rabbitRest + Vector2.up * (Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 22f);
        }
    }
}
