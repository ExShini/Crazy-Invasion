using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/**********************************************************************************/
// CompanyDialogManager класс
// менеджер одиночной компании
// определяет работу диалогов
//
/**********************************************************************************/
public class CompanyDialogManager : MonoBehaviour
{

    #region Serializable DATA
    /**********************************************************************************/
    // набор классов представляющий информацию для сериализации
    //
    /**********************************************************************************/
    [System.Serializable]
    public class DialogDataCollection
    {
        public DialogData[] items;
    }

    [System.Serializable]
    public class DialogData
    {
        public string dialogKey;
        public DialogPair[] pairs;
    }

    [System.Serializable]
    public class DialogPair
    {
        public string IconType;
        public string textKey;
        public string NameKey;
    }

    #endregion

    protected enum CDM_STATE
    {
        DIALOG_PROCESS,
        INITIALIZATION,
        READY,
        NO_STATE
    }

    protected static CompanyDialogManager s_instance = null;
    protected CDM_STATE m_state = CDM_STATE.NO_STATE;
    protected Dictionary<string, DialogData> m_dialogDataCollection;
    protected int m_dialogPage = 0;

    protected PlayerInputCtr m_inputCtr = new PlayerInputCtr();
    protected float m_controllerBlockTimer = 0.0f;
    protected float m_controllerBlockTimerLimit = 1.0f;
    protected int m_levelNumber = 0;
    protected string m_dialogKey = "";

    protected string m_defaultSettingsDataFile = "CompanyDialogsData.json";

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
    public static CompanyDialogManager GetInstance()
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
        m_state = CDM_STATE.INITIALIZATION;

        m_dialogDataCollection = new Dictionary<string, DialogData>();
        string filePath = Path.Combine(Application.streamingAssetsPath, m_defaultSettingsDataFile);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            DialogDataCollection loadedData = JsonUtility.FromJson<DialogDataCollection>(dataAsJson);

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                m_dialogDataCollection.Add(loadedData.items[i].dialogKey, loadedData.items[i]);
            }

            Debug.Log("Company settings loaded, dictionary contains: " + m_dialogDataCollection.Count + " entries");
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }

        m_state = CDM_STATE.READY;
    }

    /**********************************************************************************************/
    // возвращает true если загрузка данных была произведена
    //
    /**********************************************************************************************/
    public bool GetIsReady()
    {
        return m_state == CDM_STATE.READY;
    }

    /**********************************************************************************/
    // функция сьрасывает настройки уровня к первому и начинает новую компанию
    //
    /**********************************************************************************/
    public void SetNewCompany(PLAYER playerID)
    {
        // настраиваем m_inputCtr на нужного игрока
        m_inputCtr.DetermineAxisNameViaPlayerId((int)playerID);
        m_levelNumber = 0;
    }

    /**********************************************************************************/
    // функция извещающая CompanyDialogManager о загрузке новой карты в компании
    //
    /**********************************************************************************/
    public void OnMissionLoaded(int levelNum)
    {
        // по завершению загрузки отображаем стартовый диалок для этого уровня
        // останавливаем игру на старте, чтобы показать все необходимые диалоги
        GameManager.SetPauseState(true, true);

        m_dialogPage = 0;
        m_state = CDM_STATE.DIALOG_PROCESS;
        m_levelNumber = levelNum;
        m_dialogKey = "_start";
        ShowNextDialogPage();

    }

    /**********************************************************************************/
    // функция извещающая CompanyDialogManager о успешном завершении карты в компании
    //
    /**********************************************************************************/
    public void OnMissionComplite()
    {
        GameManager.SetPauseState(true, true);

        m_dialogPage = 0;
        m_state = CDM_STATE.DIALOG_PROCESS;
        m_dialogKey = "_complite";
        ShowNextDialogPage();
    }

    /**********************************************************************************/
    // функция извещающая CompanyDialogManager о провале карты в компании
    //
    /**********************************************************************************/
    public void OnMissionFailed()
    {
        GameManager.SetPauseState(true, true);

        m_dialogPage = 0;
        m_state = CDM_STATE.DIALOG_PROCESS;
        m_dialogKey = "_failed";
        ShowNextDialogPage();
    }

    /**********************************************************************************/
    // функция устанавливает
    //
    /**********************************************************************************/
    protected void ShowNextDialogPage()
    {
        string levelKey = "mission_" + m_levelNumber + m_dialogKey;
        DialogData dialogs = m_dialogDataCollection[levelKey];

        if (dialogs.pairs.Length > m_dialogPage)
        {
            DialogPair currentDialogPage = dialogs.pairs[m_dialogPage];
            Base.GO_TYPE goIconType = (Base.GO_TYPE)Enum.Parse(typeof(Base.GO_TYPE), currentDialogPage.IconType);

            UIController.GetInstance().SetDialog(goIconType, currentDialogPage.textKey, currentDialogPage.NameKey);

            // взводим таймер блокировки
            m_controllerBlockTimer = m_controllerBlockTimerLimit;
            m_dialogPage++;
        }
        else
        {
            Debug.Log("Dialog complited");

            // скрываем диалог и переходим к игре
            m_state = CDM_STATE.READY;
            UIController.GetInstance().HideDialog();
            GameManager.SetPauseState(false, true);
        }

    }

    /**********************************************************************************/
    // процессинговая функция
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        if (m_state == CDM_STATE.DIALOG_PROCESS)
        {
            m_controllerBlockTimer -= Time.deltaTime;
            bool buttonIsPressed = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.ANY_KEY);

            if (m_controllerBlockTimer <= 0 && buttonIsPressed)
            {
                ShowNextDialogPage();
            }
        }
    }
}