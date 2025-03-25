using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataVisualizer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Resets level saves - only the first level is unlocked (takes priority over unlock)")]
    public bool resetUnlockedLevels;

    [Tooltip("Overrides the level save - all levels are unlocked (reset takes priority)")]
    public bool unlockAllLevels;
    
    [Tooltip("Resets unlocked badges - no badges are unlocked (takes priority over unlock)")]
    public bool resetUnlockedBadges;
    
    [Tooltip("Overrides the badges save - all badges are unlocked (reset takes priority)")]
    public bool unlockAllBadges;

    // Singleton instance
    public static GameManager Instance;

    public GameTextsLoader gameTextsLoader;
    public GameSavesLoader gameSavesLoader;

    // Data to pass between scenes
    public GameTextData gameTextData;
    public GameSaveData gameSaveData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeData();
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeLevelUnlockStates(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLevelUnlockStates()
    {
        // Ensure first level is unlocked by default if no data exists
        if (!PlayerPrefs.HasKey("Level1Unlocked"))
        {
            PlayerPrefs.SetInt("Level1Unlocked", 1); // Assuming 1 is the unlocked state
            PlayerPrefs.Save();
        }
        // Call to update level locks based on PlayerPrefs
        SetLevelLocks();
    }

    private void InitializeData()
    {
        // Load game texts before save files - saves include information based on the level names in text data
        gameTextsLoader.LoadGameTextData();
        gameSavesLoader.LoadGameSaveData(gameTextData);
    }

    public void UnlockNextLevel(string completedLevelName)
    {
        int currentLevelIndex = gameSaveData.levelOrder.GetValueOrDefault(completedLevelName, -1);
        int nextLevelIndex = currentLevelIndex + 1;
        int minLevelIndex = gameSaveData.levelOrder.Min(levelOrderInfo => levelOrderInfo.Value);
        int maxLevelIndex = gameSaveData.levelOrder.Max(levelOrderInfo => levelOrderInfo.Value);

        var isIndexInRange = nextLevelIndex >= minLevelIndex && nextLevelIndex <= maxLevelIndex;
        if (isIndexInRange)
        {
            var nextLevel = gameSaveData.levelOrder.First(levelOrderInfo => levelOrderInfo.Value == nextLevelIndex);
            gameSaveData.isUnlockedLevel[nextLevel.Key] = true;
            gameSavesLoader.SaveGameData();
        }
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenuScene")
        {
            if (resetUnlockedLevels)
            {
                gameSavesLoader.ResetUnlockedLevels();
            }
            else if (unlockAllLevels)
            {
                gameSavesLoader.UnlockAllLevels();
            }
            
            if (resetUnlockedBadges)
            {
                gameSavesLoader.ResetUnlockedBadges();
            }
            else if (unlockAllBadges)
            {
                gameSavesLoader.UnlockAllBadges();
            }
            
            SetLevelLocks();
            SetBadges();
            SetSettingsButtons();
        }
    }

    private void SetLevelLocks()
    {
        var levelUnlockers = FindObjectsOfType<LevelUnlocker>();
        foreach (var levelUnlocker in levelUnlockers)
        {
            var levelUnlocked = false;
            var levelName = levelUnlocker.levelName;
            if (gameSaveData.isUnlockedLevel.Keys.Contains(levelName))
            {
                levelUnlocked = gameSaveData.isUnlockedLevel[levelName];
                //bool isUnlocked = PlayerPrefs.GetInt(levelName + "Unlocked", 0) == 1;
                if (levelUnlocked)
                {
                    //Debug.Log($"Level {levelName} unlocked state: {PlayerPrefs.GetInt(levelName + "Unlocked", 0)}");
                    levelUnlocker.Unlock();
                }
            }

            
        }
    }
    
    private void SetSettingsButtons()
    {
        var soundEffectSettingButtons = FindObjectsOfType<SoundEffectSettingButton>(true).First();
        if (AudioManager.Instance.HasSoundEffects)
        {
            soundEffectSettingButtons.Enable();
        }
        else
        {
            soundEffectSettingButtons.Disable();
        }
        
        var backgroundMusicSettingButton = FindObjectsOfType<BackgroundMusicSettingButton>(true).First();
        if (AudioManager.Instance.HasBackgroundMusic)
        {
            backgroundMusicSettingButton.Enable();
        }
        else
        {
            backgroundMusicSettingButton.Disable();
        }
    }

    public void UnlockBadge(string badgeName, BadgeTier tier)
    {
        if (gameSaveData.unlockedBadgeTier.ContainsKey(badgeName) && tier > gameSaveData.unlockedBadgeTier[badgeName])
        {
            gameSaveData.unlockedBadgeTier[badgeName] = tier;
        }

        // check the unlock also unlocked the "mastering all levels" badge - impossible from level manager level
        bool hasAllLevelsMastered = true;
        foreach (var level in gameSaveData.levelOrder)
        {
            var hasMasteredLevel = gameSaveData.unlockedBadgeTier[level.Key] == BadgeTier.Gold;
            hasAllLevelsMastered &= hasMasteredLevel;
        }

        if (hasAllLevelsMastered)
        {
            gameSaveData.unlockedBadgeTier["allMastered"] = BadgeTier.Gold;
        }
        
        gameSavesLoader.SaveGameData();
    }
    
    private void SetBadges()
    {
        var badges = FindObjectsOfType<Badge>(true);
        foreach (var badge in badges)
        {
            var badgeName = badge.name;
            if (gameSaveData.unlockedBadgeTier.Keys.Contains(badgeName))
            {
                var unlockedTier = gameSaveData.unlockedBadgeTier[badgeName];
                badge.Unlock(unlockedTier);
            }
        }
    }

    public void ResetAllProgress()
    {
        // Clear level progress
        gameSavesLoader.ResetUnlockedLevels();
        gameSavesLoader.ResetUnlockedBadges();
        PlayerPrefs.DeleteAll(); // Clear all PlayerPrefs entries for thoroughness
        gameSavesLoader.SaveGameData(); // Save empty state
        
        Debug.Log("All progress and data have been reset.");
    }
}