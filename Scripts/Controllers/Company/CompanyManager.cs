using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;

/**********************************************************************************/
// CompanyManager класс
// менеджер игровой партии компании
// отвечает за проведение компани
//
/**********************************************************************************/
public class CompanyManager : MonoBehaviour
{

    protected enum CM_STATE
    {
        INITIALIZATION,
        READY,
        WAIT_FOR_DIALOG_CLOSE
    }

    protected enum MISSION_STATE
    {
        FAILED,
        COMPLITED,
        RUNNED,
        NO_STATE
    }

    protected static CompanyManager s_instance = null;
    protected string m_defaultSettingsDataFile = "CompanyMissionData.json";
    protected CM_STATE m_state = CM_STATE.INITIALIZATION;
    protected MISSION_STATE m_missionState = MISSION_STATE.NO_STATE;
    protected CompanyDescriptor m_companyMissionData;

    protected int m_levelNumber = 0;

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
    public static CompanyManager GetInstance()
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
    // Запускаем новую компанию
    //
    /**********************************************************************************************/
    public void SetNewCompany()
    {
        m_levelNumber = 0;
    }

    /**********************************************************************************************/
    // Загружаем все данные из файлов настройки (m_defaultSettingsDataFile)
    //
    /**********************************************************************************************/
    void LoadSettingsFromFile()
    {
        m_state = CM_STATE.INITIALIZATION;

        string filePath = Path.Combine(Application.streamingAssetsPath, m_defaultSettingsDataFile);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            m_companyMissionData = JsonUtility.FromJson<CompanyDescriptor>(dataAsJson);

            m_state = CM_STATE.READY;
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
    }

    /**********************************************************************************************/
    // возвращает true если загрузка данных была произведена
    //
    /**********************************************************************************************/
    public bool GetIsReady()
    {
        return m_state == CM_STATE.READY;
    }


    /**********************************************************************************/
    // функция извещающая CompanyManager о загрузке новой карты в компании
    //
    /**********************************************************************************/
    public void OnSceneLoaded()
    {
        // переключаем состояние миссии в "запущено"
        m_missionState = MISSION_STATE.RUNNED;

        // при загрузке новой сцены запускаем генерацию карты
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

        // добавляем новые цели для уровня
        TargetController.GetInstance().SetNewCompanyMission(missionSettings.MissionDifficulties, new List<string>(missionSettings.MissionBosses));

        CompanyDialogManager.GetInstance().OnMissionLoaded(m_levelNumber);


        List<DropItemDescriptor> dropList = new List<DropItemDescriptor>(missionSettings.DropDescriptor.DropItems);
        DropManager.GetInstance().SetLevel(dropList, missionSettings.DropDescriptor.MaxNumOfDroppedItem);
    }

    /**********************************************************************************/
    // функция извещающая CompanyManager о том, что все цели игры были уничтожены
    //
    /**********************************************************************************/
    public void OnGoalsAchieved()
    {
        CompanyDialogManager.GetInstance().OnMissionComplite();
        m_missionState = MISSION_STATE.COMPLITED;
        m_state = CM_STATE.WAIT_FOR_DIALOG_CLOSE;

    }

    /**********************************************************************************/
    // функция извещающая CompanyManager о том, что игрок проиграл парию
    //
    /**********************************************************************************/
    public void OnGoalsFailed()
    {
        CompanyDialogManager.GetInstance().OnMissionFailed();
        m_missionState = MISSION_STATE.FAILED;
        m_state = CM_STATE.WAIT_FOR_DIALOG_CLOSE;

    }

    /**********************************************************************************/
    // в FixedUpdate мы проверяем некоторые состояния зависящие от отклика игрока
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        // в конце миссии и дожидаемся окончания диалогов - и производим запуск следующей(перезапуск текущей) миссии
        if(m_state == CM_STATE.WAIT_FOR_DIALOG_CLOSE)
        {
            if(CompanyDialogManager.GetInstance().GetIsReady())
            {
                if (m_missionState == MISSION_STATE.COMPLITED)
                {
                    // переходим на следующий уровень, если это возможно
                    m_levelNumber++;
                    if (m_levelNumber < m_companyMissionData.missions.Length)
                    {
                        SceneManager.LoadScene("SingleGame", LoadSceneMode.Single);
                    }
                    else
                    {
                        // если достигли конца компании - выходим в главное меню
                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                    }
                }
                else if (m_missionState == MISSION_STATE.FAILED)
                {
                    // перезапускаем текущий уровень
                    SceneManager.LoadScene("SingleGame", LoadSceneMode.Single);
                }
                else
                {
                    Debug.LogError("Wrong mission state :" + m_missionState.ToString() + " !");
                }

                m_state = CM_STATE.READY;
            }
        }
    }
}