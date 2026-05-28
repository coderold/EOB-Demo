using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Echoes.Game.Presentation
{
    public class BakuForgeUI : MonoBehaviour
    {
        public static BakuForgeUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("The floating text prompt that says 'Press [E] to Access Baku Forge'.")]
        [SerializeField] private GameObject interactionPrompt;
        
        [Tooltip("The main window panel holding the input field, buttons, and conversion numbers.")]
        [SerializeField] private GameObject conversionPanel;

        [Header("Controls & Input")]
        [SerializeField] private TMP_InputField perlasInputField;
        [SerializeField] private TextMeshProUGUI previewBakuText;
        [SerializeField] private Button convertButton;

        [Header("Conversion Rules")]
        [Tooltip("How many Perlas are needed to refine 1 Baku token.")]
        [SerializeField] private int perlasPerBakuToken = 10;

        private bool _isPlayerInForgeZone = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (conversionPanel != null) conversionPanel.SetActive(false);
            if (interactionPrompt != null) interactionPrompt.SetActive(false);

            if (perlasInputField != null)
            {
                perlasInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                perlasInputField.onValueChanged.AddListener(OnInputAmountChanged);
            }
            
            if (convertButton != null) 
                convertButton.onClick.AddListener(ExecuteForgeSmelt);

            ResetPreviewState();
        }

        private void Update()
        {
            if (_isPlayerInForgeZone && Input.GetKeyDown(KeyCode.E))
            {
                if (conversionPanel != null)
                {
                    bool nextState = !conversionPanel.activeSelf;
                    conversionPanel.SetActive(nextState);
                    
                    if (interactionPrompt != null) 
                        interactionPrompt.SetActive(!nextState);

                    if (nextState)
                    {
                        perlasInputField.text = "";
                        ResetPreviewState();
                        perlasInputField.ActivateInputField();
                    }
                }
            }
        }

        /// <summary>
        /// External method accessed by physical world triggers to establish player vicinity authorization.
        /// </summary>
        public void ToggleForgeZoneAccess(bool canAccess)
        {
            _isPlayerInForgeZone = canAccess;

            if (interactionPrompt != null) 
                interactionPrompt.SetActive(canAccess);

            if (!canAccess && conversionPanel != null) 
                conversionPanel.SetActive(false);
        }

        /// <summary>
        /// Instantly fires whenever a user updates, types, or backspaces values within the input text box.
        /// Includes validation rules for character checks, missing values, and wallet overdraw protection.
        /// </summary>
        private void OnInputAmountChanged(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput))
            {
                ResetPreviewState();
                return;
            }

            if (!int.TryParse(rawInput, out int perlasToConvert))
            {
                if (previewBakuText != null)
                    previewBakuText.text = "<color=red>Invalid numerical value</color>";
                
                if (convertButton != null) convertButton.interactable = false;
                return;
            }

            if (perlasToConvert <= 0)
            {
                ResetPreviewState();
                return;
            }

            int playerOwnedPerlas = CurrencyUIController.Instance.CurrentPerlas;
            if (perlasToConvert > playerOwnedPerlas)
            {
                if (previewBakuText != null)
                    previewBakuText.text = "<color=red>You don't have enough Perlas</color>";
                
                if (convertButton != null) convertButton.interactable = false;
                return;
            }

            if (perlasToConvert < perlasPerBakuToken)
            {
                if (previewBakuText != null)
                    previewBakuText.text = $"<color=yellow>Min. conversion rate is {perlasPerBakuToken} Perlas</color>";
                
                if (convertButton != null) convertButton.interactable = false;
                return;
            }

            int calculatedBaku = perlasToConvert / perlasPerBakuToken;
            
            if (previewBakuText != null)
                previewBakuText.text = $"<color=green>+ {calculatedBaku} BAKU</color>";

            if (convertButton != null) convertButton.interactable = true;
        }

        /// <summary>
        /// Resets the preview feedback layout elements to baseline configuration state.
        /// </summary>
        private void ResetPreviewState()
        {
            if (previewBakuText != null) previewBakuText.text = "+ 0 BAKU";
            if (convertButton != null) convertButton.interactable = false;
        }

        /// <summary>
        /// HANDLES CONVERT BUTTON CLICK:
        /// Processes transaction calculations and delegates state alterations directly back to your singleton.
        /// </summary>
        private void ExecuteForgeSmelt()
        {
            if (int.TryParse(perlasInputField.text, out int perlasToConvert))
            {
                int calculatedBakuGained = perlasToConvert / perlasPerBakuToken;
                int finalPerlasToDeduct = calculatedBakuGained * perlasPerBakuToken;

                if (finalPerlasToDeduct > 0 && CurrencyUIController.Instance.CurrentPerlas >= finalPerlasToDeduct)
                {
                    CurrencyUIController.Instance.ModifyBalances(-finalPerlasToDeduct, calculatedBakuGained);

                    perlasInputField.text = "";
                    ResetPreviewState();
                    
                    Debug.Log($"[Baku Forge] Successfully converted {finalPerlasToDeduct} Perlas into {calculatedBakuGained} Baku.");
                }
            }
        }
    }
}