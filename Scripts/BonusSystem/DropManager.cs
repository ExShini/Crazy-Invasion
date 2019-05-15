using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/**********************************************************************************/
// DropManager управляет тем как часто будут падать капсулы с 
//
/**********************************************************************************/
public class DropManager : MonoBehaviour
{

    enum DM_STATE
    {
        INIT,
        READY,
        WAIT_FOR_DEPLAY,
        STOP
    }


    public float TimeBetweenDropMin = 10.0f;
    public float TimeBetweenDropMax = 30.0f;
    protected float m_currentTimerToNextDrop = 0.0f;

    protected static DropManager s_instance = null;
    protected List<DropItemDescriptorEditor> m_dropDescriptor;
    protected Dictionary<Base.GO_TYPE, int> m_deploedDrops;

    float m_levelTimer = 0.0f;
    int m_currentNumberOfDroppedBonus;
    int m_maxNumberOfDroppedBonus;  // количество бонусов так же влияет на частоту дропа, чем больше бонусов предполагается на уровне, тем чаще они будут падать

    DM_STATE m_state = DM_STATE.READY;

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
    public static DropManager GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("DropManager instance is null!");
        }

        return s_instance;
    }

    /**********************************************************************************/
    //  устанавливаем параметры миссионного дропа
    //
    /**********************************************************************************/
    public void SetLevel(List<DropItemDescriptor> dropSettings, int maxNumOfDropInSceen)
    {
        if (dropSettings.Count == 0)
        {
            Debug.LogError("Drop settings is empty!");
            return;
        }

        m_levelTimer = 0.0f;
        m_deploedDrops = new Dictionary<Base.GO_TYPE, int>();
        m_dropDescriptor = new List<DropItemDescriptorEditor>();

        // сохраняем настройки по количеству дропа
        foreach (var drop in dropSettings)
        {
            DropItemDescriptorEditor itemDrop = new DropItemDescriptorEditor(drop);
            m_dropDescriptor.Add(itemDrop);

            // устанавливаем одновременно допустимое на карте количесво конкретного дропа
            if(itemDrop.MaxNumOfDrop != (int)DropItemDescriptorEditor.DROP_MARKER.UNLIMITED)
            {
                m_deploedDrops[itemDrop.DropType] = itemDrop.MaxNumOfDrop;
            }
            else
            {
                m_deploedDrops[itemDrop.DropType] = maxNumOfDropInSceen;
            }

        }

        m_maxNumberOfDroppedBonus = maxNumOfDropInSceen;
        m_currentNumberOfDroppedBonus = 0;

        m_state = DM_STATE.WAIT_FOR_DEPLAY;
        m_currentTimerToNextDrop = Random.Range(TimeBetweenDropMin, TimeBetweenDropMax);
    }

    /**********************************************************************************/
    //  проверяем состояние дропа
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        if(GameManager.GamePaused)
        {
            return;
        }

        if (m_state == DM_STATE.WAIT_FOR_DEPLAY)
        {
            m_currentTimerToNextDrop -= Time.deltaTime;
            if (m_currentTimerToNextDrop <= 0.0f)
            {
                DeployRandomBonus();
                if (m_currentNumberOfDroppedBonus >= m_maxNumberOfDroppedBonus)
                {
                    m_state = DM_STATE.READY;
                }
                else
                {
                    m_currentTimerToNextDrop = Random.Range(TimeBetweenDropMin, TimeBetweenDropMax);
                }
            }
        }

        m_levelTimer += Time.deltaTime;
    }

    /**********************************************************************************/
    //  дропаем рандомный бонус
    //
    /**********************************************************************************/
    protected void DeployRandomBonus()
    {
        if (m_dropDescriptor.Count == 0)
        {
            Debug.LogError("m_dropDescriptor is empty!");
            return;
        }

        Base.GO_TYPE bonusType = ChoseDropType();

        // определяем место дропа
        // выбираем рандомную позицию и проверяем её на доступность
        int xMapSize = MapGenerator.GetInstance().MapSizeX;
        int yMapSize = MapGenerator.GetInstance().MapSizeY;

        Point positionToDrope = new Point(-1, -1);
        while (!PathFinder.GetInstance().ValidatePathCell(positionToDrope))
        {
            positionToDrope.x = Random.Range(0, xMapSize);
            positionToDrope.y = Random.Range(0, yMapSize);
        }

        // создаём дроп под
        Vector2 positionOutsideTheMap = new Vector2(-100, -100);
        GameObject dropPod = ObjectFactory.GetInstance().CreateGObject(positionOutsideTheMap, Base.DIREC.DOWN, Base.GO_TYPE.DROP_POD, false);
        DropPodCtr podeCtr = dropPod.GetComponent<DropPodCtr>();

        if(podeCtr == null)
        {
            Debug.LogError("podeCtr is null!");
            return;
        }

        // загружаем под полезной нагрузкой и отправляем
        podeCtr.SetDropType(bonusType);
        podeCtr.DropInPosition(positionToDrope);

        m_currentNumberOfDroppedBonus++;
    }


    /**********************************************************************************/
    //  выбираем тип бонуса бонус
    //
    /**********************************************************************************/
    private Base.GO_TYPE ChoseDropType()
    {
        int weightOfDropSet = 0;

        // формируем список допустимого дропа
        List<DropItemDescriptorEditor> avalibleDrop = new List<DropItemDescriptorEditor>();
        foreach (var possibleDrop in m_dropDescriptor)
        {
            Base.GO_TYPE type = possibleDrop.DropType;

            // отсекаем тех, с кем мы работать не можем
            // по количеству
            if (m_deploedDrops[type] <= 0)
            {
                continue;
            }

            // по времени
            if (possibleDrop.TimeForDropStart > m_levelTimer)
            {
                continue;
            }

            // подсчитываем суммарынй вес всего дропа
            weightOfDropSet += possibleDrop.DropWeight;
            avalibleDrop.Add(possibleDrop);
        }

        // проверочка
        if (avalibleDrop.Count == 0)
        {
            Debug.LogError("avalibleDrop is empty!");
            return Base.GO_TYPE.NONE_TYPE;
        }

        // определяем бонус для дропа
        int randomDropIndex = Random.Range(1, weightOfDropSet + 1);

        Base.GO_TYPE bonusType = Base.GO_TYPE.NONE_TYPE;
        int growingWeight = 0;
        int itemIndex = 0;


        do
        {
            DropItemDescriptorEditor potantialDrop = avalibleDrop[itemIndex];
            bonusType = potantialDrop.DropType;
            growingWeight += potantialDrop.DropWeight;

            itemIndex++;

        }
        while (growingWeight < randomDropIndex && itemIndex < avalibleDrop.Count);

        return bonusType;
    }

    /**********************************************************************************/
    //  извещаем менеджер, что бонус был подобран
    //
    /**********************************************************************************/
    public void BonusWasTaked(Base.GO_TYPE bonusType)
    {
        // в отношении бонусов прибывших не с орбиты - ничего не предпринимаем - они не в юрисдикции DropManager
        if (!m_deploedDrops.ContainsKey(bonusType))
        {
            Debug.Log("Bonus ignored: " + bonusType.ToString());
            return;
        }

        m_currentNumberOfDroppedBonus--;

        // увеличиваем счётчик одновременно присутствующих бонусов
        if (m_deploedDrops[bonusType] != (int)DropItemDescriptorEditor.DROP_MARKER.UNLIMITED)
        {
            m_deploedDrops[bonusType]++;
        }

        // корректируем общий счётчик бонусов
        if (m_currentNumberOfDroppedBonus < m_maxNumberOfDroppedBonus)
        {
            m_state = DM_STATE.WAIT_FOR_DEPLAY;
            m_currentTimerToNextDrop = Random.Range(TimeBetweenDropMin, TimeBetweenDropMax);
        }
    }
}
