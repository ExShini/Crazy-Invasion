using UnityEngine;

/**********************************************************************************************/
// контроллер кнопки вбора языка
// загружает соответствующий язык
//
/**********************************************************************************************/
public class LngBtnController : MonoBehaviour
{

    public void LoadLocalization(string fileName)
    {
        LocalizationManager.instance.LoadLocalizedText(fileName);
    }

}
