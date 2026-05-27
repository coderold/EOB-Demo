using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Quest Assignment")]
    [SerializeField] private string questName = "Help the Old Man";
    
    [Header("Quest Objective")]
    [SerializeField] private GameObject targetEnemyObject; 
    private SkellyAI _targetQuestScript;
    
    [Header("UI & Camera")]
    [SerializeField] private TMP_Text floatingText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private CinemachineCamera dialogueCamera;
    [SerializeField] private MonoBehaviour playerMovementScript;
    
    private bool _playerIsClose = false;
    private bool _isTalking = false;
    private bool _awaitingDecision = false;
    private int _currentLineIndex = 0;
    private bool _questIsActive = false;

    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player")) 
        {
            _playerIsClose = true;
            if (_questIsActive)
            {
                if (floatingText != null) floatingText.text = "Please defeat the target!";
            }
            else
            {
                if (floatingText != null) floatingText.text = "Press E to Interact";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerIsClose = false;
            _isTalking = false;
            _awaitingDecision = false;
            if (floatingText != null) floatingText.text = "Old Man";
        }
    }

    void Update()
    {
        if (!_playerIsClose) return;

        if (_awaitingDecision)
        {
            if (Input.GetKeyDown(KeyCode.E)) QuestAccept();
            else if (Input.GetKeyDown(KeyCode.Q)) QuestReject();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (QuestManager.Instance && QuestManager.Instance.HasQuest(questName))
            {
                if (floatingText) 
                {
                    floatingText.text = "Please finish my task, traveler.";
                }
                return; 
            }

       
            if (!_isTalking) StartDialogue();
            else AdvanceDialogue();
        }
    }

    void StartDialogue()
    {
        _isTalking = true;
        _awaitingDecision = false;
        _currentLineIndex = 0;
        
        if (playerMovementScript) playerMovementScript.enabled = false;
        if (dialogueCamera) 
        {
            dialogueCamera.enabled = true;
            dialogueCamera.Priority = 20; 
        }        
        DisplayCurrentLine();
    }
    
    void AdvanceDialogue()
    {
        _currentLineIndex++;

        if (_currentLineIndex < dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            _awaitingDecision = true;
            if (floatingText)
            {
                floatingText.text = "[E] Accept   |   [Q] Reject";
            }
        }
    }

    void DisplayCurrentLine()
    {
        if (floatingText)
        {
            floatingText.text = dialogueLines[_currentLineIndex];
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void QuestAccept()
    {
        _isTalking = false;
        _awaitingDecision = false;
        _questIsActive = true;

        if (floatingText) floatingText.text = "Go defeat the monster!";

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AcceptQuest(questName);
        }

        if (targetEnemyObject != null)
        {
            _targetQuestScript = targetEnemyObject.GetComponent<SkellyAI>();
            if (_targetQuestScript != null)
            {
                _targetQuestScript.OnTargetDestroyed += OnQuestComplete;
            }
        }
        ResetCameraAndPlayer();

    }

    void QuestReject()
    {
        _isTalking = false;
        _awaitingDecision = false;
        _currentLineIndex = 0;
        
        if (floatingText)
        {
            floatingText.text = "Old Man";
          
        }

        ResetCameraAndPlayer();
    }


    private void ResetCameraAndPlayer()
    {
        if (dialogueCamera) 
        {
            dialogueCamera.Priority = 0;
            dialogueCamera.enabled = false;
        }
        if (playerMovementScript) 
        {
            playerMovementScript.enabled = true;
        }

        if (floatingText)
        {
            floatingText.text = "Old Man";
        }
    }
    
    void OnQuestComplete()
    {
        Debug.Log("Objective monster defeated! Completing quest...");
        
        if (_targetQuestScript != null)
        {
            _targetQuestScript.OnTargetDestroyed -= OnQuestComplete;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(questName);
        }

        if (floatingText != null)
        {
            floatingText.text = "Old Man: Wonderful job!";
        }

        _questIsActive = false;
    }
    
}