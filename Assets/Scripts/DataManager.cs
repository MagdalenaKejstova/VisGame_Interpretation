using System;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public int LocaleID;

    private string _SavefilePath = "/savefile.json";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocale();
    }

    [Serializable]
    private class SaveData
    {
        public int LocaleID;
    }

    public void SaveLocale()
    {
        SaveData data = new SaveData();
        data.LocaleID = LocaleID;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + _SavefilePath, json);
    }

    public void LoadLocale()
    {
        string path = Application.persistentDataPath + _SavefilePath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            LocaleID = data.LocaleID;
        }
    }
}