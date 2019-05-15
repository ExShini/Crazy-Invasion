using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectLibrary : MonoBehaviour
{
    static ObjectLibrary s_instance = null;

    public GameObject[] Units;
    public GameObject[] NeutralUnits;
    public GameObject[] Shots;
    public GameObject[] Buildings;
    public GameObject[] Bonuses;
    
    protected Dictionary<Base.GO_TYPE, GameObject> m_prefabsCollection = new Dictionary<Base.GO_TYPE, GameObject>();

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

        // делаем GameManager неучтожимым при загрузке новой сцены
        DontDestroyOnLoad(gameObject);
    }

    /**********************************************************************************/
    //  создаём таблицу объектов
    //
    /**********************************************************************************/
    void Start()
    {
        // обрабатываем все GMovingObject по одному
        // юниты игроков
        CIGameObject gmo = null;
        for (int i = 0; i < Units.Length; i ++)
        {
            gmo = Units[i].GetComponent<CIGameObject>();
            m_prefabsCollection[gmo.GOType] = gmo.gameObject;
        }

        // нейтральные юниты
        for (int i = 0; i < NeutralUnits.Length; i++)
        {
            gmo = NeutralUnits[i].GetComponent<CIGameObject>();
            m_prefabsCollection[gmo.GOType] = gmo.gameObject;
        }

        // снаряды
        for (int i = 0; i < Shots.Length; i++)
        {
            gmo = Shots[i].GetComponent<CIGameObject>();
            m_prefabsCollection[gmo.GOType] = gmo.gameObject;
        }


        // здания
        BuildingController bCtr = null;
        for (int i = 0; i < Buildings.Length; i++)
        {
            bCtr = Buildings[i].GetComponent<BuildingController>();
            m_prefabsCollection[bCtr.BuildingType] = bCtr.gameObject;
        }


        // обрабатываем все бонусы
        BonusCtr bonus = null;
        for (int i = 0; i < Bonuses.Length; i++)
        {
            bonus = Bonuses[i].GetComponent<BonusCtr>();
            m_prefabsCollection[(Base.GO_TYPE)bonus.BonusType] = bonus.gameObject;
        }
    }

    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    public static ObjectLibrary GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("CompanyManager instance is null!");
        }

        return s_instance;
    }

    /**********************************************************************************/
    //  функция возвращает префаб по типу
    //
    /**********************************************************************************/
    public GameObject GetPrefab(Base.GO_TYPE type)
    {
        if(!m_prefabsCollection.ContainsKey(type))
        {
            Debug.LogError("We have no prefabs for " + type.ToString() + " !!!");
            return null;
        }
        return m_prefabsCollection[type];
    }
}
