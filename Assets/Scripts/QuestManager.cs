using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text questListText;

    // A collection list tracking all currently accepted quests
    private readonly List<string> _activeQuests = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UpdateQuestTrackerUI();
    }

    public void AcceptQuest(string questName)
    {
        if (!_activeQuests.Contains(questName))
        {
            _activeQuests.Add(questName);
            UpdateQuestTrackerUI();
            Debug.Log($"Quest '{questName}' added to active tracker list.");
        }
    }

    // Call this from an NPC or Combat script when a quest is turned in
    public void CompleteQuest(string questName)
    {
        if (_activeQuests.Contains(questName))
        {
            _activeQuests.Remove(questName);
            UpdateQuestTrackerUI();
            Debug.Log($"Quest '{questName}' removed from active tracker list.");
        }
    }

    // Helper method to check if the player currently holds a specific quest
    public bool HasQuest(string questName)
    {
        return _activeQuests.Contains(questName);
    }

    // Loops through the active list and formats them cleanly into lines of text
    private void UpdateQuestTrackerUI()
    {
        if (questListText == null) return;

        if (_activeQuests.Count == 0)
        {
            questListText.text = "";
            return;
        }

        string formattedText = "";

        foreach (string quest in _activeQuests)
        {
            formattedText += $"- {quest}\n";
        }

        questListText.text = formattedText;
    }
}