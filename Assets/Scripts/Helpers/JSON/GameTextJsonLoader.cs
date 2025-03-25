using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

public class GameTextsLoader : MonoBehaviour
{
    public Material red;
    public Material blue;
    public Material orange;
    public Material yellow;
    public Material green;
    public Material defaultMaterial;

    //public string gameTextDataFilePath = "game_texts";
    public string gameTextDataResourcePath = "game_texts";

    void Awake()
    {
        SetUpMaterialDefaults();
    }

    private void SetUpMaterialDefaults()
    {
        if (red == null)
        {
            red = new Material(Shader.Find("StandardCullOff"));
            red.color = Color.red;
        }

        if (blue == null)
        {
            blue = new Material(Shader.Find("StandardCullOff"));
            blue.color = Color.blue;
        }

        if (orange == null)
        {
            blue = new Material(Shader.Find("StandardCullOff"));
            blue.color = Color.yellow;
        }

        if (yellow == null)
        {
            blue = new Material(Shader.Find("StandardCullOff"));
            blue.color = Color.yellow;
        }

        if (green == null)
        {
            blue = new Material(Shader.Find("StandardCullOff"));
            blue.color = Color.green;
        }

        if (defaultMaterial == null)
        {
            defaultMaterial = new Material(Shader.Find("StandardCullOff"));
            defaultMaterial.color = Color.black;
        }
    }

    public void LoadGameTextData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(gameTextDataResourcePath);
        if (textAsset != null)
        {
            var gameTextData = JsonConvert.DeserializeObject<GameTextData>(textAsset.text);
            GameManager.Instance.gameTextData = gameTextData;
        }
        else
        {
            Debug.LogError("Game text data not found at: " + gameTextDataResourcePath);
        }
    }

    private string SubstituteColors(string jsonText)
    {
        string pattern = @"<color=(\w+?)>";

        string result = Regex.Replace(jsonText, pattern, match =>
        {
            string color = match.Groups[1].Value;
            switch (color)
            {
                case "red":
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(red.color) + ">";
                case "blue":
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(blue.color) + ">";
                case "yellow":
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(yellow.color) + ">";
                case "orange":
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(orange.color) + ">";
                case "green":
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(green.color) + ">";
                default:
                    return "<color=#" + ColorUtility.ToHtmlStringRGB(defaultMaterial.color) + ">";
            }
        });
        return result;
    }
}