using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using RPG.Core;
using RPG.Quest;

namespace RPG.UI
{
    public class UIController : MonoBehaviour
    {
        private UIDocument uiDocumentCmp;
        public VisualElement root;
        public List<Button> buttons = new List<Button>();
        public VisualElement mainMenuContainer;
        public VisualElement playerInfoContainer;
        public Label healthLabel;
        public Label potionsLabel;
        private VisualElement questItemIcon;
        public AudioClip gameOverAudio;
        public AudioClip victoryAudio;
        [NonSerialized] public AudioSource audioSourceCmp;

        public UIBaseState currentState;
        public UIMainMenuState mainMenuState;
        public UIDialogueState dialogueState;
        public UIQuestItemState questItemState;
        public UIVictoryState victoryState;
        public UIGameOverState gameOverState;
        public UIPauseState pauseState;
        public UIUnpausedState unpausedState;

        public int currentSelection = 0;
        public bool canPause = true;

        private void Awake()
        {
            audioSourceCmp = GetComponent<AudioSource>();
            uiDocumentCmp = GetComponent<UIDocument>();
            root = uiDocumentCmp.rootVisualElement;

            mainMenuContainer = root.Q<VisualElement>("main-menu-container");
            playerInfoContainer = root.Q<VisualElement>("player-info-container");
            healthLabel = playerInfoContainer.Q<Label>("health-label");
            potionsLabel = playerInfoContainer.Q<Label>("potions-label");
            questItemIcon = playerInfoContainer.Q<VisualElement>("quest-item-icon");

            mainMenuState = new UIMainMenuState(this);
            dialogueState = new UIDialogueState(this);
            questItemState = new UIQuestItemState(this);
            victoryState = new UIVictoryState(this);
            gameOverState = new UIGameOverState(this);
            pauseState = new UIPauseState(this);
            unpausedState = new UIUnpausedState(this);
        }

        private void OnEnable()
        {
            EventManager.OnChangePlayerHealth += HandleChangePlayerHealth;
            EventManager.OnChangePlayerPotions += HandleChangePlayerPotions;
            EventManager.OnInitiateDialogue += HandleInitiateDialogue;
            EventManager.OnTreasureChestUnlocked += HandleTreasureChestUnlocked;
            EventManager.OnVictory += HandleVictory;
            EventManager.OnGameOver += HandleGameOver;
        }

        // Start is called before the first frame update
        void Start()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (sceneIndex == 0)
            {
                currentState = mainMenuState;
                currentState.EnterState();
            }
            else
            {
                playerInfoContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void OnDisable()
        {
            EventManager.OnChangePlayerHealth -= HandleChangePlayerHealth;
            EventManager.OnChangePlayerPotions -= HandleChangePlayerPotions;
            EventManager.OnInitiateDialogue -= HandleInitiateDialogue;
            EventManager.OnTreasureChestUnlocked -= HandleTreasureChestUnlocked;
            EventManager.OnVictory -= HandleVictory;
            EventManager.OnGameOver -= HandleGameOver;
        }

        public void HandleInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            currentState.SelectButton();
        }

        public void HandleNavigate(InputAction.CallbackContext context)
        {
            if (!context.performed || buttons.Count == 0) return;

            buttons[currentSelection].RemoveFromClassList("active");

            Vector2 input = context.ReadValue<Vector2>();
            currentSelection += input.x > 0 ? 1 : -1;
            currentSelection = Mathf.Clamp(
                currentSelection, 0, buttons.Count - 1
            );

            buttons[currentSelection].AddToClassList("active");
        }

        private void HandleChangePlayerHealth(float newHealthPoints)
        {
            healthLabel.text = newHealthPoints.ToString();
        }

        private void HandleChangePlayerPotions(int newPotionCount)
        {
            potionsLabel.text = newPotionCount.ToString();
        }

        private void HandleInitiateDialogue(TextAsset inkJSON, GameObject npc)
        {
            currentState = dialogueState;
            currentState.EnterState();

            (currentState as UIDialogueState).SetStory(inkJSON, npc);
        }

        private void HandleTreasureChestUnlocked(QuestItemSO item, bool showUI)
        {
            questItemIcon.style.display = DisplayStyle.Flex;

            if (!showUI) return;

            currentState = questItemState;
            currentState.EnterState();

            (currentState as UIQuestItemState).SetQuestItemLabel(item.itemName);
        }

        private void HandleVictory()
        {
            currentState = victoryState;
            currentState.EnterState();
        }

        private void HandleGameOver()
        {
            currentState = gameOverState;
            currentState.EnterState();
        }

        public void HandlePause(InputAction.CallbackContext context)
        {
            if (!context.performed || !canPause) return;

            currentState = currentState == pauseState ?
                unpausedState :
                pauseState;
            currentState.EnterState();
        }
    }
}