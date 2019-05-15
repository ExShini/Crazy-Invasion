using UnityEngine;
using System.Collections;

/**********************************************************************************/
// SceneLoadDetector
// простейший класс сообщающий всем заинтересованным, что сцена загрузилась
//
/**********************************************************************************/
public class SceneLoadDetector : MonoBehaviour
{
    void Start()
    {
        if(GameManager.GetInstance().GameMode == GameManager.GAME_MODE.SINGLE)
        {
            CompanyManager.GetInstance().OnSceneLoaded();
        }
        else if(GameManager.GetInstance().GameMode == GameManager.GAME_MODE.DUEL)
        {
            DuelManager.GetInstance().OnSceneLoaded();
        }
    }

}
