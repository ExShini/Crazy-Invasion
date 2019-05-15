using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**********************************************************************************/
// GameManager класс
// менеджер игровой партии
// вспомогательный класс для хранения общих игровых данных
//
/**********************************************************************************/
public class GameManager : MonoBehaviour
{
    public enum GAME_MODE
    {
        SINGLE,
        DUEL,
        NO_MODE
    };

    private Dictionary<PLAYER, PlayerController> m_playerDict = new Dictionary<PLAYER, PlayerController>();
    private Dictionary<PLAYER, GameObject> m_playerGameObj = new Dictionary<PLAYER, GameObject>();
    private PlayerController m_playerCtr_singleMode = null;
    private GameObject m_playerGO_singleMode = null;
    private PLAYER m_singlePlayerID = PLAYER.NO_PLAYER;

    private static GameManager s_instance;
    public GAME_MODE GameMode = GAME_MODE.NO_MODE;
    private static bool s_gamePaused = false;
    protected static bool s_hardPause = false;


    private bool m_levelComplite = false;
    private int m_pl1Score = 0;
    private int m_pl2Score = 0;


    public static bool GamePaused
    {
        get
        {
            return s_gamePaused;
        }
    }

    /**********************************************************************************/
    // Игра начинается здесь
    // Awake позволяет выполнять необходимую подготовку перед началом всех действий (и всех Start-ов)
    //
    /**********************************************************************************/
    void Awake()
    {
        // защищаемся от повторного создания объекта
        if (s_instance == null)
        {
            s_instance = this;
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
        }

        // делаем GameManager неучтожимым при загрузке новой сцены (?)
        DontDestroyOnLoad(gameObject);
    }

    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    public static GameManager GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("GameManager ");
        }

        return s_instance;
    }

    /**********************************************************************************/
    // подготавливаем менеджер к одиночной игре
    //
    /**********************************************************************************/
    public void InitSingleGame(PLAYER playerID)
    {
        Debug.Log("InitSingleGame");
        GameMode = GAME_MODE.SINGLE;

        // очищаем контроллер
        m_playerDict.Clear();
        m_playerGameObj.Clear();
        m_playerCtr_singleMode = null;
        m_playerGO_singleMode = null;

        m_singlePlayerID = playerID;

        GameAudioManager.Instance.SwitchToGameMode();
        CompanyManager.GetInstance().SetNewCompany();
        CompanyDialogManager.GetInstance().SetNewCompany(m_singlePlayerID);
        SceneManager.LoadScene("SingleGame", LoadSceneMode.Single);
    }


    /**********************************************************************************/
    // подготавливаем менеджер к дуэльной игре
    //
    /**********************************************************************************/
    public void InitDuelGame()
    {
        Debug.Log("InitDuelGame");
        GameMode = GAME_MODE.DUEL;

        // очищаем контроллер
        m_playerDict.Clear();
        m_playerGameObj.Clear();
        m_playerCtr_singleMode = null;
        m_playerGO_singleMode = null;

        m_singlePlayerID = PLAYER.NO_PLAYER;

        DuelManager.GetInstance().SetNewDuel();
        GameAudioManager.Instance.SwitchToGameMode();
        SceneManager.LoadScene("DuelLevel", LoadSceneMode.Single);
    }

    /**********************************************************************************/
    // RegisterPlayer
    // функция регистрирующая игрока
    //
    /**********************************************************************************/
    public void RegisterPlayer(PlayerController plCtr)
    {
        if (plCtr == null)
        {
            Debug.LogError("GameManager:registerPlayer : plCtr is null!!!");
        }

        if (GameMode == GAME_MODE.SINGLE)
        {
            // запоминаем активного игрока
            if(plCtr.playerId == m_singlePlayerID)
            {
                m_playerCtr_singleMode = plCtr;
                m_playerGO_singleMode = plCtr.gameObject;
            }
            else
            {
                // деактивируем ненужного нам игрока игрока
                plCtr.gameObject.SetActive(false);
            }
        }
        else if (GameMode == GAME_MODE.DUEL)
        {
            m_playerDict[plCtr.playerId] = plCtr;
            m_playerGameObj[plCtr.playerId] = plCtr.gameObject;
        }
        else
        {
            Debug.LogError("RegisterPlayer:: Wrong game mode!!!");
        }

    }

    /**********************************************************************************/
    // возвращаем контроллер игрока по id
    //
    /**********************************************************************************/
    public PlayerController GetPlayers(PLAYER id)
    {
        if (GameMode == GAME_MODE.SINGLE)
        {
            Debug.LogWarning("We tryed to use GetPlayers function in SINGLE game mode");
        }

        if (m_playerDict.ContainsKey(id))
        {
            return m_playerDict[id];
        }

        return null;
    }

    /**********************************************************************************/
    // возвращаем контроллер игрока
    //
    /**********************************************************************************/
    public PlayerController GetPlayer()
    {
        if (GameMode == GAME_MODE.DUEL)
        {
            Debug.LogWarning("We tryed to use GetPlayer function in DUEL game mode");
        }

        return m_playerCtr_singleMode;
    }

    /**********************************************************************************/
    // возвращаем объект игрока по id
    //
    /**********************************************************************************/
    public GameObject GetPlayersObject(PLAYER id)
    {
        if (GameMode == GAME_MODE.SINGLE)
        {
            Debug.LogWarning("We tryed to use GetPlayers function in SINGLE game mode");
        }

        if (m_playerGameObj.ContainsKey(id))
        {
            return m_playerGameObj[id];
        }

        return null;
    }

    /**********************************************************************************/
    // возвращаем объект игрока
    //
    /**********************************************************************************/
    public GameObject GetPlayerObject()
    {
        if (GameMode == GAME_MODE.DUEL)
        {
            Debug.LogWarning("We tryed to use GetPlayer function in DUEL game mode");
        }

        return m_playerGO_singleMode;
    }

 

    /**********************************************************************************/
    // функция регулирования игровой паузы
    //
    /**********************************************************************************/
    public static void SetPauseState(bool pauseValue, bool hardSet)
    {
        // если игра уже поставлена на паузу
        if(s_gamePaused == true)
        {
            // проверяем уровень защиты паузы
            if(s_hardPause == true)
            {
                // если изменение применяется с hardSet - проводим его
                if (hardSet)
                {
                    s_gamePaused = pauseValue;
                }
            }
            else
            {
                // если нет защиты паузы, по устанавливаем новое значение
                s_gamePaused = pauseValue;
            }
        }
        else
        {
            s_gamePaused = pauseValue;
            s_hardPause = hardSet;
        }

        // обновляем баннер паузы
        if(!s_hardPause)
        {
            UIController.GetInstance().SetPause(s_gamePaused);
        }
    }

    /**********************************************************************************/
    // переключатель игровой паузы
    //
    /**********************************************************************************/
    public static void SwitchPauseMode()
    {
        // переключаем паузу в мягком режиме
        SetPauseState(!GamePaused, false);
    }


    /**********************************************************************************/
    // увеличиваем счётчик очков за убийство монстров
    //
    /**********************************************************************************/
    public void IncreasePlayerScoreUnitLose(PLAYER killerOwnerID, int scopeToAdd)
    {
        if (killerOwnerID == PLAYER.PL1)
        {
            m_pl1Score += scopeToAdd;
            UIController.GetInstance().SetScore(PLAYER.PL1, m_pl1Score);
        }
        else if (killerOwnerID == PLAYER.PL2)
        {
            m_pl2Score += scopeToAdd;
            UIController.GetInstance().SetScore(PLAYER.PL2, m_pl2Score);
        }
    }

    /**********************************************************************************/
    // увеличиваем счётчик очков за захват зданий
    //
    /**********************************************************************************/
    public void IncreasePlayerScopeBuildingCuptured(PLAYER newOwener, int scopeToAdd)
    {
        if (newOwener == PLAYER.PL1)
        {
            m_pl1Score += scopeToAdd;
            UIController.GetInstance().SetScore(newOwener, m_pl1Score);
        }
        else if (newOwener == PLAYER.PL2)
        {
            m_pl2Score += scopeToAdd;
            UIController.GetInstance().SetScore(newOwener, m_pl2Score);
        }
    }

    /**********************************************************************************/
    // увеличиваем счётчик очков за производство юнитов
    //
    /**********************************************************************************/
    public void IncreasePlayerScopeUnitProduction(PLAYER owener, int scopeToAdd)
    {
        if (owener == PLAYER.PL1)
        {
            m_pl1Score += scopeToAdd;
            UIController.GetInstance().SetScore(owener, m_pl1Score);
        }
        else if (owener == PLAYER.PL2)
        {
            m_pl2Score += scopeToAdd;
            UIController.GetInstance().SetScore(owener, m_pl2Score);
        }
    }
}
