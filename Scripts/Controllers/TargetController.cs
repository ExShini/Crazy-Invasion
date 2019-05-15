using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Random = UnityEngine.Random;

/**********************************************************************************/
// TargetController класс
// отвечает за определение основных целей юнитов
//
/**********************************************************************************/
public class TargetController : MonoBehaviour
{
    public enum TC_STATE
    {
        INITIALIZATION,
        READY,
    }

    protected static TargetController s_instance = null;
    protected TC_STATE m_state = TC_STATE.INITIALIZATION;
    protected string m_defaultSettingsDataFile = "CompanyTargetData.json";
    protected BossCompanyData m_bossData;
    protected Dictionary<string, int> m_bossWeights = new Dictionary<string, int>();


    protected int m_levelDifficulties = 0;
    protected int m_currentDifficulties = 0;
    protected LinkedList<string> m_bossesToDeploy = new LinkedList<string>();
    protected Dictionary<int, GameObject> m_npcTarget = new Dictionary<int, GameObject>();


    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    public static TargetController GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("CompanyManager instance is null!");
        }

        return s_instance;
    }

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
        m_state = TC_STATE.INITIALIZATION;

        string filePath = Path.Combine(Application.streamingAssetsPath, m_defaultSettingsDataFile);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            m_bossData = JsonUtility.FromJson<BossCompanyData>(dataAsJson);

            // сохраняем информацию о боссах по типу босса
            foreach(BossSettings setting in m_bossData.bosses)
            {
                m_bossWeights[setting.BossType] = setting.Weight;
            }

            m_state = TC_STATE.READY;
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
        return m_state == TC_STATE.READY;
    }

    /**********************************************************************************/
    //  добавляем на карту новых боссов и обозначаем их целями для юнитов
    //
    /**********************************************************************************/
    public void SetNewCompanyMission(int MissionDifficulties, List<string> Bosses)
    {
        m_currentDifficulties = 0;
        m_levelDifficulties = MissionDifficulties;

        // сохраняем всех боссов
        foreach (string bossType in Bosses)
        {
            m_bossesToDeploy.AddLast(bossType);
        }

        // обновляем список целей
        UpdateNPCTargets();
    }

    /**********************************************************************************/
    //  обновляем список NPC целей
    //  если необходимо добавляем новых боссоов
    //
    /**********************************************************************************/
    void UpdateNPCTargets()
    {
        // пробуем добавить новых боссов на уровень
        // если это возможно, будем добавлять боссов до тех пор пока не израсходуем лимит по сложности уровня
        bool tryToAddNewBoss = true;
        while(tryToAddNewBoss)
        {
            if (m_currentDifficulties < m_levelDifficulties && m_bossesToDeploy.Count > 0)
            {
                string nextBossToDeploy = m_bossesToDeploy.First.Value;

                if (!m_bossWeights.ContainsKey(nextBossToDeploy))
                {
                    Debug.LogError("TargetController:UpdateNPCTargets: we have no data for " + nextBossToDeploy + " boss!");
                    return;
                }

                // проверяем, можно ли добавить нового босса
                int nextBossWeight = m_bossWeights[nextBossToDeploy];
                if (m_currentDifficulties + nextBossWeight <= m_levelDifficulties)
                {
                    AddNewBoss(nextBossToDeploy);
                    m_currentDifficulties += nextBossWeight;
                    m_bossesToDeploy.RemoveFirst();
                }
                else
                {
                    // останавливаем добавление боссов
                    tryToAddNewBoss = false;
                }
            }
            else
            {
                // останавливаем добавление боссов
                tryToAddNewBoss = false;
            }
        }
    }

    /**********************************************************************************/
    //  добавляем нового босса на карту
    //
    /**********************************************************************************/
    void AddNewBoss(string bossType)
    {
        // преобразовываем строку к типу юнита
        Base.GO_TYPE bossGOType = (Base.GO_TYPE)Enum.Parse(typeof(Base.GO_TYPE), bossType);

        // создаем экземпляр юнита 
        GameObject bossObject = ObjectFactory.GetInstance().CreateGObject(new Vector2(0, 0), Base.DIREC.DOWN, bossGOType);

        // выбираем рандомную позицию для юнита
        MapGenerator mg = MapGenerator.GetInstance();
        int xMapSizeInBlocks = mg.MapSizeX / mg.SizeOfBlocks;
        int yMapSizeInBlocks = mg.MapSizeY / mg.SizeOfBlocks;

        int x = Random.Range(0, xMapSizeInBlocks);
        int y = Random.Range(0, yMapSizeInBlocks);

        // помещаем юнита в блок
        mg.PlaceObjectInBlock(x, y, bossObject);

        // сохраняем объект как цель уровня
        CIGameObject gmo = bossObject.GetComponent<CIGameObject>();
        m_npcTarget[gmo.ID] = bossObject;
    }

    /**********************************************************************************/
    //  функция добавляет новые экста цели для уничтожения
    //  может быть использована для регистрации боссов, что прибывают с волнами или специальные стационарыне объекты
    //
    /**********************************************************************************/
    public void RegistrAsExtraNPCTarget(GameObject targetToRegistr)
    {
        // сохраняем объект как цель уровня
        CIGameObject gmo = targetToRegistr.GetComponent<CIGameObject>();
        m_npcTarget[gmo.ID] = targetToRegistr;
    }

    /**********************************************************************************/
    //  функция дерегестрирует цель
    //  производится проверка выигрыша игрока
    //
    /**********************************************************************************/
    public void TargetIsDead(GameObject target)
    {
        if(GameManager.GetInstance().GameMode == GameManager.GAME_MODE.SINGLE)
        {
            CIGameObject gmo = target.GetComponent<CIGameObject>();

            if(gmo.GOType == Base.GO_TYPE.PLAYER)
            {
                CompanyManager.GetInstance().OnGoalsFailed();
            }
            else
            {
                m_npcTarget.Remove(gmo.ID);

                // обновляем текущую сложность
                int bossWeight = m_bossWeights[gmo.GOType.ToString()];
                m_currentDifficulties -= bossWeight;

                UpdateNPCTargets();

                if (m_npcTarget.Count == 0)
                {
                    CompanyManager.GetInstance().OnGoalsAchieved();
                }
            }
        }
        else
        {
            PlayerController pc = target.GetComponent<PlayerController>();
            DuelManager.GetInstance().OnGoalAchived(pc.playerId);
        }
 
    }

    /**********************************************************************************/
    // Функция возвращает главного противника для указанного игрока (противник данному игроку)
    //
    /**********************************************************************************/
    public GameObject GetTarget(PLAYER unitOwnerId, Point unitPosition)
    {
        GameManager gm = GameManager.GetInstance();

        // если играется дуэль - возвращаем в качестве цели "противного" инопланетянина
        if (gm.GameMode == GameManager.GAME_MODE.DUEL)
        {
            if (unitOwnerId == PLAYER.PL1)
            {
                return gm.GetPlayersObject(PLAYER.PL2);
            }
            else
            {
                return gm.GetPlayersObject(PLAYER.PL1);
            }
        }

        // если играется компания - пробуем определить ближайшего противника
        else if (gm.GameMode == GameManager.GAME_MODE.SINGLE)
        {
            GameObject closestTarget = null;
            int closestDist = 9999;

            // перебираем все актуальные цели и ищем ближайшую к нам
            foreach(var targetPair in m_npcTarget)
            {
                GameObject potantialTarget = targetPair.Value;
                CIGameObject gmo = potantialTarget.GetComponent<CIGameObject>();

                Point tPosition = gmo.GetGlobalPosition();
                Point dist = unitPosition - tPosition;
                int potDist = dist.GetSimpleLength();

                // запоминаем новую ближайшую цель
                if (potDist < closestDist)
                {
                    closestTarget = potantialTarget;
                    closestDist = potDist;
                }
            }

            return closestTarget;
        }

        Debug.LogError("Wrong GameMode!");
        return null;
    }
}
