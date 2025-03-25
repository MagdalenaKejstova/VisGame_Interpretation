using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Resources;
using Newtonsoft.Json;
using UnityEngine;

public class GameSavesLoader : MonoBehaviour
{
    public string gameSaveDataFilePath = "/gameSaves.json";
    
    public void LoadGameSaveData(GameTextData gameTextData)
    {
         var loadPath = Application.persistentDataPath + gameSaveDataFilePath;
        if (!File.Exists(loadPath))
        {
            Debug.Log("Creating new save data as none exists.");
            var newSaveData = new GameSaveData();
            PrefillGameSaveData(newSaveData, gameTextData);
            GameManager.Instance.gameSaveData = newSaveData;
            SaveGameData(); // Make sure it saves right after creation
        }
        else
        {
            var jsonData = File.ReadAllText(loadPath);
            var gameSaveData = JsonConvert.DeserializeObject<GameSaveData>(jsonData);
            if (gameSaveData == null)
            {
                Debug.Log("Corrupted save file or new install, prefilling save data.");
                gameSaveData = new GameSaveData();
                PrefillGameSaveData(gameSaveData, gameTextData);
            }
            GameManager.Instance.gameSaveData = gameSaveData;
            Debug.Log($"Loaded game save data: {jsonData}"); // Log the actual JSON to see what's loaded
        }
    }

    private void PrefillGameSaveData(GameSaveData gameSaveData, GameTextData gameTextData)
    {
        int index = 0;
        foreach (var levelName in gameTextData.levelTexts.Keys)
        {
            var isUnlocked = index == 0;
            gameSaveData.isUnlockedLevel.Add(levelName, isUnlocked);
            gameSaveData.unlockedBadgeTier.Add(levelName, BadgeTier.Locked);
            index++;
            gameSaveData.levelOrder.Add(levelName, index);
        }
        gameSaveData.unlockedBadgeTier.Add("allMastered", BadgeTier.Locked);
    }

    public void ResetUnlockedLevels()
    {
        var levelOrder = GameManager.Instance.gameSaveData.levelOrder;
        foreach (var levelOrderInfo in levelOrder)
        {
            var levelName = levelOrderInfo.Key;
            var levelIndex = levelOrderInfo.Value;
            GameManager.Instance.gameSaveData.isUnlockedLevel[levelName] = levelIndex == 1;
        }
        SaveGameData();
    }

    public void ResetUnlockedBadges()
    {
        var badgeData = GameManager.Instance.gameSaveData.unlockedBadgeTier;
        foreach (var badgeName in badgeData.Keys.ToList())
        {
            badgeData[badgeName] = BadgeTier.Locked;
        }
        SaveGameData();
    }
    
    public void UnlockAllLevels()
    {
        var levelData = GameManager.Instance.gameSaveData.isUnlockedLevel;
        foreach (var levelName in levelData.Keys.ToList())
        {
            levelData[levelName] = true;
        }
        SaveGameData();
    }
    
    public void UnlockAllBadges(BadgeTier tier = BadgeTier.Gold)
    {
        var badgeData = GameManager.Instance.gameSaveData.unlockedBadgeTier;
        foreach (var badgeName in badgeData.Keys.ToList())
        {
            badgeData[badgeName] = tier;
        }
        SaveGameData();
    }
    

    public void SaveGameData()
    {
        var currentSaveData = GameManager.Instance.gameSaveData;
        string jsonData = JsonConvert.SerializeObject(currentSaveData);

        File.WriteAllText(Application.persistentDataPath + gameSaveDataFilePath, jsonData);
    }
}