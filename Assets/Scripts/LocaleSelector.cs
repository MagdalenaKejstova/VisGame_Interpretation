using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{
    private bool _active = false;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
    }

    public void ChangeLocale(int localeID)
    {
        if (_active)
            return;

        SetLocale(localeID);
    }

    void SetLocale(int localeID)
    {
        _active = true;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        DataManager.Instance.LocaleID = localeID;
        DataManager.Instance.SaveLocale();
        _active = false;
    }
}