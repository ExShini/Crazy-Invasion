using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/**********************************************************************************************/
// StartupManager класс
// проверяет готовность всех компонентов перед загрузкой игрового меню
//
/**********************************************************************************************/
public class StartupManager : MonoBehaviour
{

    private IEnumerator Start()
    {
        // менеджер локализации
        while (!LocalizationManager.instance.GetIsReady())
        {
            yield return null;
        }

        // менеджер диалогов
        while (!CompanyDialogManager.GetInstance().GetIsReady())
        {
            yield return null;
        }

        // менеджер компании
        while (!CompanyManager.GetInstance().GetIsReady())
        {
            yield return null;
        }

        // менеджер дуэли
        while (!DuelManager.GetInstance().GetIsReady())
        {
            yield return null;
        }

        // менеджер целей
        while (!TargetController.GetInstance().GetIsReady())
        {
            yield return null;
        }

        // после загрузки всех компонент - включаем меню
        SceneManager.LoadScene("MainMenu");
    }
}