using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**********************************************************************************/
// GameObjectMapController класс ответственный за отслеживание положения юнитов и зданий на игровом поле
//
/**********************************************************************************/
public class GameObjectMapController
{
    private static GameObjectMapController s_instance = null;

    private int m_xMapSize = 0;
    private int m_yMapSize = 0;

    private Dictionary<int, Dictionary<int, CIGameObject>> m_UnitToMap;     // хранит массивы со списками объектов по координате (ключ позиции / Dictionary< id юнита / контроллер юнита >)
    private Dictionary<int, Point> m_UnitToPoint;           // хранит информацию где был юнит и в какой части буферов его искать

    private Dictionary<int, int> m_BuildingsToMap;      // хранит массивы со списками объектов по координате (ключ позиции / идентификатор здания)
    private Dictionary<int, BuildingController> m_BuildingIdToController;   // хранит связь ключ здания (внутренний) - контроллер здания
    private int m_nextBuildingKey = 1;


    private Dictionary<int, int> m_EnvironmentToMap;      // хранит массивы со списками объектов по координате (ключ позиции / идентификатор объекта окружения)
    private Dictionary<int, GeneratedEnvironmentCtr> m_EnvironmentIdToController;   // хранит связь ключ здания (внутренний) - контроллер объекта окружения
    private int m_nextEnvironmentKey = 1;


    private int m_maxPossibleSearchingRadius = 10;
    List<List<Point>> m_pointsToSearch;




    /**********************************************************************************/
    // UnitMapController конструктор
    //
    /**********************************************************************************/
    GameObjectMapController()
    {
        ResetControllerData();
    }

    /**********************************************************************************/
    // сбрасываем данные с предыдущих партий
    //
    /**********************************************************************************/
    public void ResetControllerData()
    {
        m_UnitToMap = new Dictionary<int, Dictionary<int, CIGameObject>>();
        m_UnitToPoint = new Dictionary<int, Point>();

        m_BuildingsToMap = new Dictionary<int, int>();
        m_BuildingIdToController = new Dictionary<int, BuildingController>();
        m_nextBuildingKey = 1;

        m_EnvironmentToMap = new Dictionary<int, int>();
        m_EnvironmentIdToController = new Dictionary<int, GeneratedEnvironmentCtr>();
        m_nextEnvironmentKey = 1;
    }

    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    static public GameObjectMapController GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new GameObjectMapController();
        }

        return s_instance;
    }

    /**********************************************************************************/
    // InitController
    // инициализируем контроллер и подготавливам хэш
    //
    /**********************************************************************************/
    public void InitController(int xMapSize, int yMapSize)
    {
        // сначала обнуляем данные
        ResetControllerData();

        m_xMapSize = xMapSize;
        m_yMapSize = yMapSize;

        // создаем хэш для быстрого определения точек поиска
        m_pointsToSearch = new List<List<Point>>();
        for (int i = 0; i <= m_maxPossibleSearchingRadius; i++)
        {
            m_pointsToSearch.Add(new List<Point>());
        }

        // подготавливаем набор точек для быстрого определения координат поиска
        // как результат получим таблицу в которой присутствуют наборы точек для поска отсортированные по их дистанции от точки поиска
        for (int xSimulated = -m_maxPossibleSearchingRadius; xSimulated <= m_maxPossibleSearchingRadius; xSimulated++)
        {
            for (int ySimulated = -m_maxPossibleSearchingRadius; ySimulated <= m_maxPossibleSearchingRadius; ySimulated++)
            {
                int radius = Mathf.Abs(ySimulated) + Mathf.Abs(xSimulated);
                if (radius <= m_maxPossibleSearchingRadius)
                {
                    Point pointToHesh = new Point(xSimulated, ySimulated);
                    m_pointsToSearch[radius].Add(pointToHesh);
                }
            }
        }
    }

    /**********************************************************************************/
    // функция устанавливает здание в карту позиций
    //
    /**********************************************************************************/
    public void RegistBuilding(BuildingController ctr)
    {
        int registrationId = m_nextBuildingKey;
        m_BuildingIdToController[registrationId] = ctr;
        Point buildingPosition = ctr.GetPositionOfLeftDownСorner();


        for (int xPos = buildingPosition.x; xPos < buildingPosition.x + ctr.XBuildingSize; xPos++)
        {
            for (int yPos = buildingPosition.y; yPos < buildingPosition.y + ctr.YBuildingSize; yPos++)
            {
                int positionKey = GetPositionKey(xPos, yPos);
                m_BuildingsToMap[positionKey] = registrationId;
            }
        }

        m_nextBuildingKey++;
    }


    /**********************************************************************************/
    // функция устанавливает разрушаемый объект окружения в карту позиций
    // объекты зарегестрированные таким образом не являются для кого либо "классическим"
    // противником, однако могут попасть под "лихую руку" и быть уничтоженными
    //
    /**********************************************************************************/
    public void RegistEnvironment(GeneratedEnvironmentCtr ctr)
    {
        int registrationId = m_nextEnvironmentKey;
        m_EnvironmentIdToController[registrationId] = ctr;
        Point objPosition = ctr.GetGlobalPosition() - new Point(ctr.SIZE / 2, ctr.SIZE / 2);


        for (int xPos = objPosition.x; xPos < objPosition.x + ctr.SIZE; xPos++)
        {
            for (int yPos = objPosition.y; yPos < objPosition.y + ctr.SIZE; yPos++)
            {
                int positionKey = GetPositionKey(xPos, yPos);
                m_EnvironmentToMap[positionKey] = registrationId;
            }
        }

        m_nextEnvironmentKey++;
    }

    /**********************************************************************************/
    // функция обновляет таблицы позиций юнитов
    // возвращает true, если координата была обновлена
    //
    /**********************************************************************************/
    public bool UpdateUnitPosition(CIGameObject unitCtr)
    {
        Point position = unitCtr.GetGlobalPosition();
        int unitID = unitCtr.ID;
        Dictionary<int, CIGameObject> listOfUnits;

        // проверяем - работали ли мы уже с этим юнитом
        // если нет - добавляем всё необходимое и выходим
        if (!m_UnitToPoint.ContainsKey(unitID))
        {
            m_UnitToPoint[unitID] = position;
            listOfUnits = GetListOfUnitInPosition(position);
            listOfUnits[unitID] = unitCtr;

            return true;
        }

        // проверяем, "изменилась ли координата?"
        // если нет - выходим
        Point oldPosition = m_UnitToPoint[unitID];
        if (oldPosition.IsSamePoint(position))
        {
            return false;
        }

        // в противном случае перемещаем объект из старой позиции в новую
        // обновляем и перезаписываем позицию объекта

        // удаляем старые записи
        listOfUnits = GetListOfUnitInPosition(oldPosition);
        listOfUnits.Remove(unitID);

        // добавляем новые
        listOfUnits = GetListOfUnitInPosition(position);
        listOfUnits[unitID] = unitCtr;
        m_UnitToPoint[unitID] = position;

        return true;

    }

    /**********************************************************************************/
    // функция убирающая юнита с карты, к примеру в случае смерти
    //
    /**********************************************************************************/
    public void ReleaseUnitFromMap(CIGameObject unitCtr)
    {
        Point position = unitCtr.GetGlobalPosition();
        int unitID = unitCtr.ID;
        Dictionary<int, CIGameObject> listOfUnits;

        // проверяем - работали ли мы уже с этим юнитом
        // если нет - выходим, ничего больше делать не надо, так как мы его и не учитывали
        if (!m_UnitToPoint.ContainsKey(unitID))
        {
            Debug.LogWarning("UnitMapController unregistred unit was released at " + position);
            return;
        }

        // убираем все данные о юните
        listOfUnits = GetListOfUnitInPosition(m_UnitToPoint[unitID]);
        if (!listOfUnits.ContainsKey(unitID))
        {
            Debug.LogWarning("UnitMapController! something wrong with inner structures! for unit at " + position);
            return;
        }

        listOfUnits.Remove(unitID);
        m_UnitToPoint.Remove(unitID);

    }

    /**********************************************************************************/
    // функция возвращает список юнитов в данной позиции
    //
    /**********************************************************************************/
    public Dictionary<int, CIGameObject> GetListOfUnitInPosition(Point position)
    {
        int positionKey = GetPositionKey(position.x, position.y);

        // если мы ещё не регистрировали объекты в этой клетке - создаем все необходимые элементы инфраструктуры
        if (!m_UnitToMap.ContainsKey(positionKey))
        {
            m_UnitToMap[positionKey] = new Dictionary<int, CIGameObject>();
        }

        return m_UnitToMap[positionKey];
    }

    /**********************************************************************************/
    // функция возвращает ключ позиции
    //
    /**********************************************************************************/
    int GetPositionKey(int x, int y)
    {
        return x + y * m_xMapSize;
    }


    /**********************************************************************************/
    // функция производит поиск противников в заданном радиусе
    //
    /**********************************************************************************/
    public List<CIGameObject> SearchEnemiesInRadius(Point position, int radius, PLAYER ownerId, bool closest = true)
    {
        List<CIGameObject> listOfUnits = new List<CIGameObject>();

        // начинаем поиск объектов от самого ближайшего
        for (int searchingRadius = 0; searchingRadius <= radius; searchingRadius++)
        {
            List<Point> pointsToCheck = m_pointsToSearch[searchingRadius];
            foreach (Point point in pointsToCheck)
            {
                Point pointToCheck = point + position;

                // проверяем точку на валидность
                if (pointToCheck.x < 0 || pointToCheck.x >= m_xMapSize || pointToCheck.y < 0 || pointToCheck.y > m_yMapSize)
                {
                    continue;   // если точка за границей карты - уходим на сл. итерацию
                }
                Dictionary<int, CIGameObject> listOfUnit = GetListOfUnitInPosition(pointToCheck);

                foreach (var unit in listOfUnit)
                {
                    // если нашли юнита другого игрока, добавляем его к списку
                    CIGameObject unitCtr = unit.Value;
                    if (unitCtr.Owner != (int)ownerId)
                    {
                        listOfUnits.Add(unitCtr);
                    }
                }
            }

            // если мы ищем ближайшего противника, в случае нахождения таковых, сразу же возвращаем список найденых
            // в противном случае продолжаем поиск до максимального радиуса
            if (closest && listOfUnits.Count > 0)
            {
                return listOfUnits;
            }
        }

        return listOfUnits;
    }


    /**********************************************************************************/
    // функция производит поиск зданий в заданном раудиуса
    //
    /**********************************************************************************/
    public List<BuildingController> SearchEnemiesBuildingInRadius(Point position, int radius, PLAYER ownerId, bool closest = true)
    {
        List<BuildingController> buildings = new List<BuildingController>();
        List<int> buildingKeys = new List<int>();

        // начинаем поиск объектов от самого ближайшего
        for (int searchingRadius = 0; searchingRadius <= radius; searchingRadius++)
        {
            List<Point> pointsToCheck = m_pointsToSearch[searchingRadius];
            foreach (Point point in pointsToCheck)
            {
                Point pointToCheck = point + position;

                // проверяем точку на валидность
                if (pointToCheck.x < 0 || pointToCheck.x >= m_xMapSize || pointToCheck.y < 0 || pointToCheck.y > m_yMapSize)
                {
                    continue;   // если точка за границей карты - уходим на сл. итерацию
                }
                int positionKey = GetPositionKey(pointToCheck.x, pointToCheck.y);
                if (m_BuildingsToMap.ContainsKey(positionKey))
                {
                    int buildingKey = m_BuildingsToMap[positionKey];
                    if (!buildingKeys.Contains(buildingKey))
                    {
                        buildingKeys.Add(buildingKey);
                        BuildingController ctr = m_BuildingIdToController[buildingKey];

                        if (ctr.GetOwnerId() != ownerId)
                        {
                            buildings.Add(ctr);
                        }
                    }
                }
            }

            // если мы ищем ближайшего противника, в случае нахождения таковых, сразу же возвращаем список найденых
            // в противном случае продолжаем поиск до максимального радиуса
            if (closest && buildings.Count > 0)
            {
                return buildings;
            }
        }

        return buildings;
    }


    /**********************************************************************************/
    // функция производит поиск зданий в заданном раудиуса
    //
    /**********************************************************************************/
    public List<GeneratedEnvironmentCtr> SearchEnvironmentInRadius(Point position, int radius, bool closest = true)
    {
        List<GeneratedEnvironmentCtr> envObject = new List<GeneratedEnvironmentCtr>();
        List<int> objKeys = new List<int>();

        // начинаем поиск объектов от самого ближайшего
        for (int searchingRadius = 0; searchingRadius <= radius; searchingRadius++)
        {
            List<Point> pointsToCheck = m_pointsToSearch[searchingRadius];
            foreach (Point point in pointsToCheck)
            {
                Point pointToCheck = point + position;

                // проверяем точку на валидность
                if (pointToCheck.x < 0 || pointToCheck.x >= m_xMapSize || pointToCheck.y < 0 || pointToCheck.y > m_yMapSize)
                {
                    continue;   // если точка за границей карты - уходим на сл. итерацию
                }
                int positionKey = GetPositionKey(pointToCheck.x, pointToCheck.y);
                if (m_EnvironmentToMap.ContainsKey(positionKey))
                {
                    int envKey = m_EnvironmentToMap[positionKey];
                    if (!objKeys.Contains(envKey))
                    {
                        objKeys.Add(envKey);
                        GeneratedEnvironmentCtr ctr = m_EnvironmentIdToController[envKey];
                        envObject.Add(ctr);
                    }
                }
            }

            // если мы ищем ближайшие объекты, в случае нахождения таковых, сразу же возвращаем список найденых
            // в противном случае продолжаем поиск до максимального радиуса
            if (closest && envObject.Count > 0)
            {
                return envObject;
            }
        }

        return envObject;
    }
}