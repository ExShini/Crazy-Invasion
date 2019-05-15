using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/**********************************************************************************/
//  библиотека хранящая настройки для генерации блоков:
//  - Типы блоков
// Для каждого блока хранится:
//  - количество зданий
//  - Тип зданий и их веса
//  - Типы возможных окружений
//  - Количество элементов окружения
//
/**********************************************************************************/
public class BlockLibrary: MonoBehaviour
{
    
    static BlockLibrary s_instance = null;
    public BlockSettingsPair[] BlockSettings;
    Dictionary<Base.BLOCK_TYPE, BlockSettings> m_hash = new Dictionary<Base.BLOCK_TYPE, BlockSettings>();

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
    public static BlockLibrary GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("BlockLibrary instance is null!");
        }

        return s_instance;
    }

    /**********************************************************************************/
    // возвращаем настройки для указанного типа блока
    //
    /**********************************************************************************/
    public BlockSettings GetBlockSettings(Base.BLOCK_TYPE type)
    {
        if(BlockSettings.Length == 0)
        {
            Debug.LogError("BlockSettings is empty!");
            return null;
        }

        // хешируем таблицу, если ещё этого не сделали
        if(m_hash.Count == 0)
        {
            for(int i = 0; i < BlockSettings.Length; i++)
            {
                m_hash[BlockSettings[i].Type] = BlockSettings[i].Settings;
            }
        }

        return m_hash[type];
    }
}