using UnityEngine;
using TMPro;

namespace Echoes.Game.Presentation
{
    public class CurrencyUIController : MonoBehaviour
    {
        public static CurrencyUIController Instance { get; private set; }

        [Header("TextMeshPro UI References")]
        [SerializeField] private TextMeshProUGUI perlasText;
        [SerializeField] private TextMeshProUGUI bakuText;

        [Header("Starting Demo Values")]
        [SerializeField] private int initialPerlas = 0;
        [SerializeField] private int initialBaku = 0;

        private int _currentPerlas;
        private int _currentBaku;

        public int CurrentPerlas => _currentPerlas;
        public int CurrentBaku => _currentBaku;

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
            _currentPerlas = initialPerlas;
            _currentBaku = initialBaku;
            
            UpdateCurrencyVisuals();
        }

        /// <summary>
        /// Adds a specified amount of Perlas to the total wallet loop and refreshes the text layout.
        /// </summary>
        public void AddPerlas(int amount)
        {
            _currentPerlas += amount;
            UpdateCurrencyVisuals();
        }

        /// <summary>
        /// Changes your Perlas into Baku tokens at the Baku Forge. Pass a negative value to deduct perlas.
        /// </summary>
        public void ModifyBalances(int perlasChange, int bakuChange)
        {
            _currentPerlas += perlasChange;
            _currentBaku += bakuChange;
            
            if (_currentPerlas < 0) _currentPerlas = 0;
            if (_currentBaku < 0) _currentBaku = 0;

            UpdateCurrencyVisuals();
        }

        /// <summary>
        /// Updates the raw strings on your canvas interface with proper string formatting.
        /// </summary>
        private void UpdateCurrencyVisuals()
        {
            if (perlasText != null)
            {
                perlasText.text = $"{_currentPerlas}";
            }

            if (bakuText != null)
            {
                bakuText.text = $"{_currentBaku}";
            }
        }
    }
}