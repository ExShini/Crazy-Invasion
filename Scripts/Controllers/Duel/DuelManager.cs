using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

/**********************************************************************************/
//  Менеджер дуэли
//
/**********************************************************************************/
public class DuelManager : MonoBehaviour
{

    protected enum DM_STATE
    {
        INITIALIZATION,
        READY,
        IN_GAME,
        WAIT_FOR_ROUND_START,
        WAIT_FOR_ROUND_END
    }

    public int RoundsToWin = 3;     //  количество побед в раундах необходимое для победы в дуэли
    protected int m_pl1WinnNum = 0;
    protected int m_pl2WinnNum = 0;

    public int PauseAfterRound = 3;
    public int PauseBeforeRound = 3;
    protected float m_currectTimer = 0.0f;

    protected static DuelManager s_instance = null;
    protected string m_defaultSettingsDataFile = "DuelMissionData.json";
    protected DM_STATE m_state = DM_STATE.INITIALIZATION;
    protected CompanyDescriptor m_companyMissionData;
    protected int m_levelNumber = 0;
    protected bool m_duelIsFinished = false;


    /**********************************************************************************/
    //  защищаемся от повторного создания объекта
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
    public static DuelManager GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("CompanyManager instance is null!");
        }

        return s_instance;
    }

    /**********************************************************************************************/
    // Start
    //
    /**********************************************************************************************/
    private void Start()
    {
        LoadSettingsFromFile();
    }


    /**********************************************************************************************/
    // Загружаем все данные из файлов настройки (m_defaultSettingsDataFile)
    //
    /**********************************************************************************************/
    void LoadSettingsFromFile()
    {
        m_state = DM_STATE.INITIALIZATION;

        string filePath = Path.Combine(Application.streamingAssetsPath, m_defaultSettingsDataFile);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            m_companyMissionData = JsonUtility.FromJson<CompanyDescriptor>(dataAsJson);

            m_state = DM_STATE.READY;
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
    }


    /**********************************************************************************************/
    // Запускаем новую дуэль
    //
    /**********************************************************************************************/
    public void SetNewDuel()
    {
        m_levelNumber = 0;
        m_pl1WinnNum = 0;
        m_pl2WinnNum = 0;
        m_duelIsFinished = false;
    }


    /**********************************************************************************************/
    // возвращает true если загрузка данных была произведена
    //
    /**********************************************************************************************/
    public bool GetIsReady()
    {
        return m_state == DM_STATE.READY;
    }


    /**********************************************************************************/
    // функция извещающая CompanyManager о загрузке новой карты в компании
    //
    /**********************************************************************************/
    public void OnSceneLoaded()
    {
        MissionDescriptor missionSettings = m_companyMissionData.missions[m_levelNumber];

        MapGenerator.MapGeneratorSettings set = new MapGenerator.MapGeneratorSettings();
        set.MapXSize = missionSettings.MapXSize;
        set.MapYSize = missionSettings.MapYSize;

        // разбираем все доступные для данной миссии здания и устанавливаем их в сет
        for (int i = 0; i < missionSettings.Buildings.Length; i++)
        {
            AvailableBuilding building = missionSettings.Buildings[i];
            Base.BLOCK_TYPE blockType = Base.StringToBlockType(building.BuildingType);

            MapGenerator.BlockWeight bw = new MapGenerator.BlockWeight();
            bw.BlockType = blockType;
            bw.Weight = building.Weight;

            set.AvalibleBlocks.Add(bw);
        }

        // генерируем карту для уровня в соответствии с настройками
        MapGenerator.GetInstance().GenerateMap(set);

        // устанавливаем параметры дропа
        List<DropItemDescriptor> dropList = new List<DropItemDescriptor>(missionSettings.DropDescriptor.DropItems);
        DropManager.GetInstance().SetLevel(dropList, missionSettings.DropDescriptor.MaxNumOfDroppedItem);

        /*
        DropManager.GetInstance().TimeBetweenDropMin = 8;
        DropManager.GetInstance().TimeBetweenDropMin = 16;
        */

        // приостанавливаем игру до момента начала раунда
        GameManager.SetPauseState(true, true);

        // ожидаем начала раунда
        m_state = DM_STATE.WAIT_FOR_ROUND_START;
        m_currectTimer = PauseBeforeRound;

        // обновляем счётчик побед
        UIController.GetInstance().SetGameScore(m_pl1WinnNum, m_pl2WinnNum);
    }


    /**********************************************************************************/
    // в FixedUpdate мы проверяем состояние игры
    //
    /**********************************************************************************/
    public void OnGoalAchived(PLAYER losePlayerID)
    {
        PLAYER winner = PLAYER.NO_PLAYER;
        if (losePlayerID == PLAYER.PL1)
        {
            winner = PLAYER.PL2;
            m_pl2WinnNum++;
        }
        else if (losePlayerID == PLAYER.PL2)
        {
            winner = PLAYER.PL1;
            m_pl1WinnNum++;
        }

        // обновляем UI и пишем имя победителя на экране
        string playerName = Base.GetPlayerName(winner);
        if (m_pl1WinnNum >= RoundsToWin || m_pl2WinnNum >= RoundsToWin)
        {
            UIController.GetInstance().DeclareWinner(playerName);
            m_duelIsFinished = true;
        }
        else
        {
            UIController.GetInstance().DeclareRoundWinner(playerName);
        }

        // обновляем счётчик побед
        UIController.GetInstance().SetGameScore(m_pl1WinnNum, m_pl2WinnNum);

        // приостанавливаем игру до следующего раунда / выхода из дуэли
        GameManager.SetPauseState(true, true);

        // ожидаем окончания рауда
        m_state = DM_STATE.WAIT_FOR_ROUND_END;
        m_currectTimer = PauseAfterRound;
    }

    /**********************************************************************************/
    // в FixedUpdate мы проверяем состояние игры
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        // дожидаемся начала раунда
        if (m_state == DM_STATE.WAIT_FOR_ROUND_START)
        {
            m_currectTimer -= Time.deltaTime;
            UIController.GetInstance().SetTimerMessage((int)Mathf.Floor(m_currectTimer + 0.5f));

            if (m_currectTimer <= 0.0)
            {
                GameManager.SetPauseState(false, true);
                m_state = DM_STATE.IN_GAME;

                UIController.GetInstance().HideMessagePanel();
            }
        }

        // выдерживаем паузу перед началом следующего раунда
        else if (m_state == DM_STATE.WAIT_FOR_ROUND_END)
        {
            m_currectTimer -= Time.deltaTime;
            if (m_currectTimer <= 0.0)
            {
                m_state = DM_STATE.READY;
                // если дуэль закончилась - выходим
                if (m_duelIsFinished)
                {
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                }
                else
                {
                    m_levelNumber++;
                    SceneManager.LoadScene("DuelLevel", LoadSceneMode.Single);
                }
            }
        }
    }
}
