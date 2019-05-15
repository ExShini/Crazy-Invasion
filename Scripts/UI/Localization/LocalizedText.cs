using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************************************************/
// класс ответственный за использование локализованного на специфичный язык текста
//
/**********************************************************************************************/
public class LocalizedText : MonoBehaviour
{
    public string Key = "";

    // Use this for initialization
    void Start()
    {
        UpdateTextLocalization();
        LocalizationManager.instance.ResgistrLocalizedText(this);
    }

    public void SetNewKey(string newKey)
    {
        Key = newKey;
        UpdateTextLocalization();
    }

    public void UpdateTextLocalization()
    {
        if(Key == "" || Key == null)
        {
            return;
        }

        Text text = GetComponent<Text>();
        text.text = LocalizationManager.instance.GetLocalizedValue(Key);
    }

}