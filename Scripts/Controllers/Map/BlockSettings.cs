using UnityEngine;
using UnityEditor;

/**********************************************************************************/
//  Класс описывающий настройки для генерации карты
//  - количество зданий (и распределение вероятносей по количеству)
//  - Тип зданий и их веса
//  - Типы возможных окружений
//  - Количество элементов окружения
//
/**********************************************************************************/
[System.Serializable]
public class BlockSettings
{
    public Vector2Int[] NumOfBuildings;     // X - количесво зданий, У - вероятность генерации такого количества
    public MapGenerator.BuildingWeight[] Buildings;
    public int NumOfEnvElements;
    public GeneratedEnvironmentCtr.ENV_TYPE[] EnvTypes;
}




/**********************************************************************************/
//  контейнер BLOCK_TYPE - BlockSettings
//
/**********************************************************************************/
[System.Serializable]
public class BlockSettingsPair
{
    public Base.BLOCK_TYPE Type;
    public BlockSettings Settings;
}