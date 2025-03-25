using UnityEngine;

public class LocaleInitializer : MonoBehaviour
{
    public LocaleSelector LocaleSelector;

    private void Start()
    {
        LocaleSelector.ChangeLocale(DataManager.Instance.LocaleID);
    }
}