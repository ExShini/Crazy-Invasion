using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// ObjectFactory
// фабрика объектов
// отвечает за создание и реисаользование игровых объектов
//
/**********************************************************************************/
public class ObjectFactory : MonoBehaviour
{

    private static ObjectFactory s_instance = null;
    public float offset = 0.5f;

    private int m_unitIDCounter = 100;
    private Dictionary<Base.GO_TYPE, LinkedList<GameObject>> m_objectColection;

    /**********************************************************************************/
    // ObjectFactory инициализация
    //
    /**********************************************************************************/
    private void Awake()
    {
        SetInstance(this);
        m_objectColection = new Dictionary<Base.GO_TYPE, LinkedList<GameObject>>();
    }

    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    static public ObjectFactory GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new ObjectFactory();
        }

        return s_instance;
    }


    // TODO: грязноватая структура, разобраться как организовать порядок инициализации лучше
    /**********************************************************************************/
    // SetInstance
    //
    /**********************************************************************************/
    static void SetInstance(ObjectFactory instance)
    {
        s_instance = instance;
    }


    /**********************************************************************************/
    // функция создания игровых объектов
    // используется, когда надо добавить что-то на игровую сцену
    /**********************************************************************************/
    public GameObject CreateGObject(Vector2 from, Base.DIREC direction, Base.GO_TYPE objectType, bool useOffset = true)
    {
        GameObject instance = null;

        // пробуем получить объект из кеша
        instance = GetGObjectFromCash(from, direction, objectType, useOffset);

        // если в кеше ничего нет, создаём новые объект
        if (instance == null)
        {
            instance = GetNewGObjectInstance(from, direction, objectType, useOffset);
        }

        if (instance == null)
        {
            Debug.Log("CreateGObject:: MAIN ERROR!");
        }

        return instance;
    }

    /**********************************************************************************/
    // функция создания игровых объектов
    // пытаемся найти объект в кеше
    /**********************************************************************************/
    protected GameObject GetGObjectFromCash(Vector2 from, Base.DIREC direction, Base.GO_TYPE objectType, bool useOffset)
    {
        GameObject instance = null;
        Vector2 objectDirection = new Vector2(0f, 0f);
        Vector2 newPosition = new Vector2(0f, 0f);

        // проверяем кеш объектов
        // если он у нас уже имеется - реиспользуем
        if (m_objectColection.ContainsKey(objectType))
        {
            LinkedList<GameObject> objectPull = m_objectColection[objectType];

            if (objectPull.Count > 0)
            {
                instance = objectPull.First.Value;
                objectPull.RemoveFirst();

                float OffsetSize = offset;
                if (!useOffset)
                {
                    OffsetSize = 0;
                }

                switch (direction)
                {
                    case Base.DIREC.DOWN:
                        newPosition = new Vector2(from.x, from.y - OffsetSize);
                        objectDirection.y = -1f;
                        break;
                    case Base.DIREC.UP:
                        newPosition = new Vector2(from.x, from.y + OffsetSize);
                        objectDirection.y = 1f;
                        break;
                    case Base.DIREC.LEFT:
                        newPosition = new Vector2(from.x - OffsetSize, from.y);
                        objectDirection.x = -1f;
                        break;
                    case Base.DIREC.RIGHT:
                        newPosition = new Vector2(from.x + OffsetSize, from.y);
                        objectDirection.x = 1f;
                        break;
                }
            }

        }
        else
        {
            // если это первое создание объекта - создаем так же для него LinkedList для последующего хранения
            m_objectColection[objectType] = new LinkedList<GameObject>();
        }

        // если нашёлся объект - устанавливаем ему направление
        if (instance != null)
        {
            Transform objTransform = instance.GetComponent<Transform>();
            objTransform.position = new Vector3(newPosition.x, newPosition.y, 0);

            if (instance.tag != "Bonus")
            {
                CIGameObject ctr = instance.GetComponent<CIGameObject>();
                ctr.SetDirection(objectDirection);

                instance.GetComponent<CIGameObject>().ResetGObject();
            }
            else
            {
                BonusCtr ctr = instance.GetComponent<BonusCtr>();
                ctr.ResetBonus();
            }
        }

        return instance;
    }

    /**********************************************************************************/
    // функция создания игровых объектов
    // создаём новый объект
    /**********************************************************************************/
    protected GameObject GetNewGObjectInstance(Vector2 from, Base.DIREC direction, Base.GO_TYPE objectType, bool useOffset)
    {
        GameObject instance = null;
        GameObject toInstantiate = ObjectLibrary.GetInstance().GetPrefab(objectType);

        if (toInstantiate == null)
        {
            Debug.LogError("ObjectFactory::GetNewGObjectInstance: toInstantiate is null!!!");
            return null;
        }

        float OffsetSize = offset;
        if (!useOffset)
        {
            OffsetSize = 0;
        }

        Vector2 objectDirection = new Vector2(0f, 0f);

        switch (direction)
        {
            case Base.DIREC.DOWN:
                instance = Instantiate(toInstantiate, new Vector3(from.x, from.y - OffsetSize, 0f), Quaternion.identity) as GameObject;
                objectDirection.y = -1f;
                break;
            case Base.DIREC.UP:
                instance = Instantiate(toInstantiate, new Vector3(from.x, from.y + OffsetSize, 0f), Quaternion.identity) as GameObject;
                objectDirection.y = 1f;
                break;
            case Base.DIREC.LEFT:
                instance = Instantiate(toInstantiate, new Vector3(from.x - OffsetSize, from.y, 0f), Quaternion.identity) as GameObject;
                objectDirection.x = -1f;
                break;
            case Base.DIREC.RIGHT:
                instance = Instantiate(toInstantiate, new Vector3(from.x + OffsetSize, from.y, 0f), Quaternion.identity) as GameObject;
                objectDirection.x = 1f;
                break;
        }

        // бонусы у нас недвижимые объекты и контроллер у них соответсвенно свой
        if (instance.tag != "Bonus")
        {
            CIGameObject ctr = instance.GetComponent<CIGameObject>();
            ctr.ID = GetUnitID();
            ctr.SetDirection(objectDirection);
        }

        return instance;
    }


    /**********************************************************************************/
    // получаем новый ID для юнита
    //
    /**********************************************************************************/
    public int GetUnitID()
    {
        int id = m_unitIDCounter;
        m_unitIDCounter++;
        return id;
    }

    /**********************************************************************************/
    // функция возврата игровых объектов в кеш
    //
    /**********************************************************************************/
    public void ReturnObjectToCash(GameObject objectInstance, Base.GO_TYPE objectType)
    {
        if (!m_objectColection.ContainsKey(objectType))
        {
            m_objectColection[objectType] = new LinkedList<GameObject>();
        }

        LinkedList<GameObject> cash = m_objectColection[objectType];
        cash.AddLast(objectInstance);
    }



}
