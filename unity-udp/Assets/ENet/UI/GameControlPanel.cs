using UnityEngine;
using UnityEngine.UI;

namespace MENet.UI
{
    public class GameControlPanel : MonoBehaviour
    {
        [Header("按鈕引用")]
        public Button playButton;
        public Button playRecordButton;

        private ENetManager netManager;
        private bool isPlaying = false;
        private bool isPlayingRecord = false;

        private void Start()
        {
            netManager = FindObjectOfType<ENetManager>();
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClick);
            }
            
            if (playRecordButton != null)
            {
                playRecordButton.onClick.AddListener(OnPlayRecordButtonClick);
            }

            UpdateButtonStates();
        }

        private void Update()
        {
            if (netManager != null)
            {
                isPlaying = netManager.IsPlaying();
                isPlayingRecord = netManager.IsPlayingRecord();
                UpdateButtonStates();
            }
        }

        private void OnPlayButtonClick()
        {
            if (!isPlaying && !isPlayingRecord)
            {
                netManager.StartGame();
                Debug.Log("[GameControl] 開始遊戲");
            }
            UpdateButtonStates();
        }

        private void OnPlayRecordButtonClick()
        {
            netManager.PlayRecord();
            Debug.Log("[GameControl] 開始回放");
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (playButton != null)
            {
                playButton.interactable = !isPlayingRecord;
            }
            
            if (playRecordButton != null)
            {
                playRecordButton.interactable = true;
            }
        }
    }
} 