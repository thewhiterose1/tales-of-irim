using System;
using System.Collections.Generic;
using UnityEngine;

public class TranslationManager : MonoBehaviour
{
    public Language ChosenLanguage = Language.en;

    private Dictionary<string, string> TranslationDictionary = new Dictionary<string, string>();
    private TranslationList Translations;
    public TextAsset translationsJSON;

    public static TranslationManager Instance;

    public string GetTranslation(string key) => TranslationDictionary.GetValueOrDefault(key, key);


    public void Awake()
    {
        UpdateTranslations();
        Instance = this;
    }

    public void UpdateTranslations()
    {
        LoadJson();
        HighlightImportantText();
        ConvertToDictionary();
    }

    public void HighlightImportantText()
    {
        var keywords = new string[] 
        { "Imperial Dominion", "Irim", "Emperor", "Zygerios", "Tales of", "Etenor", 
            "Imperial Council", "Patrician Zorgos", "Scales", "Dragons", "Hedgemen",
            "High King Hyber", "Birdmen", "Captain Salazar", "Malediction Dunes",
            "Wicked Wallow", "Wyrdtrees", "Wyrdwood", "Forest Demon", "Jeramiah"};

        foreach (var keyword in keywords)
        {
            var events = Translations.Translations.FindAll(x => x.Value.Contains(keyword));

            foreach (var gameEvent in events)
            {
                gameEvent.Value = gameEvent.Value.Replace(keyword, $"<color=blue><b>{keyword}</b></color>");
            }
        };
    }

    public void LoadJson()
    {
        Translations = JsonUtility.FromJson<TranslationList>(translationsJSON.text);
    }

    public void ConvertToDictionary()
    {
        foreach (var translationPair in Translations.Translations)
        {
            TranslationDictionary.TryAdd(translationPair.Key, translationPair.Value);
        }
    }
}

[Serializable]
public class TranslationList
{
    public List<Translation> Translations;
}

[Serializable]
public class Translation
{
    public string Key;
    public string Value;
} 

public enum Language
{ 
    en
}