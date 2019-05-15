using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/**********************************************************************************/
// WayNode
// вспомогательный контейнер, использующийся при генерации дорог
//
/**********************************************************************************/
public class WayNode
{
    public static int UNREACHABLE = 999999;
    public static int ROAD_COST = 1;
    public static int GROUND_COST = 4;

    public Base.DIREC previusRoadDirection = Base.DIREC.NO_DIRECTION;
    public List<Base.DIREC> approvedDirections = new List<Base.DIREC>();
    public int wayCost = UNREACHABLE;
    public int cellCost = GROUND_COST;


}

/**********************************************************************************/
// Point
// класс описывающий точку в пространстве
//
/**********************************************************************************/
[System.Serializable]
public class Point
{
    public int x = 0;
    public int y = 0;

    public Point ()
    {

    }

    public Point(int xCor, int yCor)
    {
        this.x = xCor;
        this.y = yCor;
    }

    public Point(Point other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public void Copy(Point other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public static Point operator +(Point first, Point second)
    {
        Point result = new Point(first.x + second.x, first.y + second.y);
        return result;
    }

    public static Point operator -(Point first, Point second)
    {
        Point result = new Point(first.x - second.x, first.y - second.y);
        return result;
    }


    public override string ToString()
    {
        return "Point x: " + x + " y:" + y;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public void ShiftPoint(Base.DIREC direction)
    {
        switch (direction)
        {
            case Base.DIREC.DOWN:
                this.y--;
                break;
            case Base.DIREC.UP:
                this.y++;
                break;
            case Base.DIREC.LEFT:
                this.x--;
                break;
            case Base.DIREC.RIGHT:
                this.x++;
                break;
            case Base.DIREC.NO_DIRECTION:
                break;
            default:
                Debug.LogError("");
                break;
        }
    }

    public bool IsSamePoint(Point other)
    {
        if (other == null)
            return false;

        if ((this.x == other.x) && (this.y == other.y))
            return true;
        else
            return false;
    }

    public Vector2 GetUnityPoint()
    {
        return new Vector2((float)this.x * Base.SIZE_OF_CELL, (float)this.y * Base.SIZE_OF_CELL);
    }

    // возвращает упрощённую дистанцию, получаемую путём сложения модулей Х и У
    public int GetSimpleLength()
    {
        return Mathf.Abs(x) + Mathf.Abs(y);
    }

    // преобразует в преобладающее направление
    public Base.DIREC ToDirection()
    {
        if(Mathf.Abs(x) > Mathf.Abs(y))
        {
            if(x > 0)
            {
                return Base.DIREC.RIGHT;
            }
            else
            {
                return Base.DIREC.LEFT;
            }
        }
        else
        {
            if (y > 0)
            {
                return Base.DIREC.UP;
            }
            else
            {
                return Base.DIREC.DOWN;
            }
        }
    }
}

/**********************************************************************************/
// RoadManager
// класс занимающийся построением путей и их обслуживанием
//
/**********************************************************************************/
public class RoadManager
{
    protected static RoadManager s_instance = null;
    protected Dictionary<int, GameObject> m_roadDictionary = null;
    protected bool[,] m_roadObjectMap;       // коллекция флагов обозначающая, есть ли по указанным координатам дорога

    protected BlockDescriptorImitation[,] m_blockRoadRulesDescriptor;


    /**********************************************************************************/
    // RoadManager конструктор
    //
    /**********************************************************************************/
    public RoadManager()
    {
    }

    /**********************************************************************************/
    // сеттор для коллекция элементов дороги
    //
    /**********************************************************************************/
    public void SetRoadCollection(List<GameObject> collection)
    {
        if (m_roadDictionary == null)
        {
            m_roadDictionary = new Dictionary<int, GameObject>();
        }

        foreach (GameObject road in collection)
        {
            int key = GetRoadKey(road);
            m_roadDictionary[key] = road;
        }
    }


    /**********************************************************************************/
    // предоставляем данные для соединений в PathFinder
    // эта функция должна быть использована после окончания генерации всех соединений
    //
    /**********************************************************************************/
    public void TransferConnectionDataToPathFinder()
    {
        PathFinder.GetInstance().ApplyBlockConnectionData(m_blockRoadRulesDescriptor);
    }

    /**********************************************************************************/
    // возвращает ключ для объекта дороги
    //
    /**********************************************************************************/
    protected int GetRoadKey(GameObject roadObject)
    {
        RoadNode node = roadObject.GetComponent<RoadNode>();

        if (node == null)
        {
            Debug.LogError("Road node is NULL!");
            return -1;
        }

        return GetRoadKeyFromDirection(node.Directions);
    }

    /**********************************************************************************/
    // возвращает ключ для объекта дороги по списку соединяемых направлений
    //
    /**********************************************************************************/
    protected int GetRoadKeyFromDirection(List<Base.DIREC> directions)
    {
        int key = 0;

        foreach (Base.DIREC direction in directions)
        {
            switch (direction)
            {
                case Base.DIREC.DOWN:
                    key += 1;
                    break;
                case Base.DIREC.UP:
                    key += 10;
                    break;
                case Base.DIREC.LEFT:
                    key += 100;
                    break;
                case Base.DIREC.RIGHT:
                    key += 1000;
                    break;
                default:
                    Debug.LogError("Wrong value of direction: " + direction.ToString());
                    break;
            }
        }

        return key;
    }

    /**********************************************************************************/
    // RoadManager GetInstance
    //
    /**********************************************************************************/
    public static RoadManager GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new RoadManager();
        }

        return s_instance;
    }

    /**********************************************************************************/
    // ресетим карту дорог и карту зависимостей
    // устанавливаем новые рандомные зависимости между блоками
    //
    /**********************************************************************************/
    public void ResetRoadMap(int xSize, int ySize, int SizeOfBlock)
    {
        // проверки параметров
        if (xSize <= 0 || ySize <= 0)
        {
            Debug.LogError("ResetRoadMap: Wrong size of map! " + xSize.ToString() + ":" + ySize.ToString());
            return;
        }

        // сбрасываем старые правила соединений
        m_blockRoadRulesDescriptor = new BlockDescriptorImitation[xSize, ySize];
        bool mapIsFine = false;

        while (!mapIsFine)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    BlockDescriptorImitation bd = new BlockDescriptorImitation();

                    // для начала устанавливаем, что все соединения возможны
                    bd.RoadConnections[(int)Base.DIREC.DOWN] = ROAD_CONNECTION_STATUS.POSSIBLE;
                    bd.RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.POSSIBLE;
                    bd.RoadConnections[(int)Base.DIREC.UP] = ROAD_CONNECTION_STATUS.POSSIBLE;
                    bd.RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.POSSIBLE;

                    // выставляем ограничители от краёв карты
                    // блоки у краёв карты не должны соединяться с лесом
                    if (x == 0)
                    {
                        bd.RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.BLOCKED;
                    }
                    if (y == 0)
                    {
                        bd.RoadConnections[(int)Base.DIREC.DOWN] = ROAD_CONNECTION_STATUS.BLOCKED;
                    }

                    if (x == xSize - 1)
                    {
                        bd.RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.BLOCKED;
                    }
                    if (y == ySize - 1)
                    {
                        bd.RoadConnections[(int)Base.DIREC.UP] = ROAD_CONNECTION_STATUS.BLOCKED;
                    }

                    m_blockRoadRulesDescriptor[x, y] = bd;
                }
            }

            // определяем правила соединения блоков перед генерацией карты
            BuildRoadRules(xSize, ySize);

            // проверка доступности блоков и правка правил
            mapIsFine = CheckRoadRules(xSize, ySize);
        }

        // обновляем массив "дорожных" пометок
        m_roadObjectMap = new bool[xSize * SizeOfBlock, ySize * SizeOfBlock];
    }

    /**********************************************************************************/
    // возвращает истину, если в клетке существует дорога
    //
    /**********************************************************************************/
    public bool IsRoadHere(Point point)
    {
        return IsRoadHere(point.x, point.y);
    }

    /**********************************************************************************/
    // возвращает истину, если в клетке существует дорога
    //
    /**********************************************************************************/
    public bool IsRoadHere(int x, int y)
    {
        if(m_roadObjectMap == null)
        {
            Debug.LogError("m_roadObjectMap is null!");
            return false;
        }

        return m_roadObjectMap[x, y];
    }


    /**********************************************************************************/
    // проверка доступности блоков
    // если какие-то правила мешают доступности блоков - это необходимо исправить
    //
    /**********************************************************************************/
    protected bool CheckRoadRules(int xSize, int ySize)
    {
        // проверки параметров
        if (xSize <= 0 || ySize <= 0)
        {
            Debug.LogError("CheckRoadRules: Wrong size of map! " + xSize.ToString() + ":" + ySize.ToString());
            return false;
        }


        // начинаем проверку соединений с левого нижнего угла
        // волновой алгорим
        Point startPoint = new Point(0, 0);
        LinkedList<Point> pointsToProcess = new LinkedList<Point>();
        pointsToProcess.AddFirst(startPoint);

        // вспомогательные структуры для подсчёта кол-ва соединений
        int linkedBlocksCounter = 0;
        bool[,] connectedBlocks = new bool[xSize, ySize];

        for (int i = 0; i < xSize; i++)
        {
            for (int y = 0; y < ySize; y++)
            {
                connectedBlocks[i, y] = false;
            }
        }
        connectedBlocks[0, 0] = true;
        linkedBlocksCounter++;

        while (pointsToProcess.Count > 0)
        {
            Point pointToCheck = pointsToProcess.First.Value;
            pointsToProcess.RemoveFirst();

            BlockDescriptorImitation bd = m_blockRoadRulesDescriptor[pointToCheck.x, pointToCheck.y];

            if (bd.RoadConnections[(int)Base.DIREC.DOWN] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                Point newPointToCheck = new Point(pointToCheck.x, pointToCheck.y - 1);
                // если точка ещё не была достижима - увеличиваем счётчик и маркируем её как достижимую
                if (!connectedBlocks[newPointToCheck.x, newPointToCheck.y])
                {
                    connectedBlocks[newPointToCheck.x, newPointToCheck.y] = true;
                    linkedBlocksCounter++;
                    pointsToProcess.AddLast(newPointToCheck);
                }
            }

            if (bd.RoadConnections[(int)Base.DIREC.UP] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                Point newPointToCheck = new Point(pointToCheck.x, pointToCheck.y + 1);
                // если точка ещё не была достижима - увеличиваем счётчик и маркируем её как достижимую
                if (!connectedBlocks[newPointToCheck.x, newPointToCheck.y])
                {
                    connectedBlocks[newPointToCheck.x, newPointToCheck.y] = true;
                    linkedBlocksCounter++;
                    pointsToProcess.AddLast(newPointToCheck);
                }
            }

            if (bd.RoadConnections[(int)Base.DIREC.LEFT] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                Point newPointToCheck = new Point(pointToCheck.x - 1, pointToCheck.y);
                // если точка ещё не была достижима - увеличиваем счётчик и маркируем её как достижимую
                if (!connectedBlocks[newPointToCheck.x, newPointToCheck.y])
                {
                    connectedBlocks[newPointToCheck.x, newPointToCheck.y] = true;
                    linkedBlocksCounter++;
                    pointsToProcess.AddLast(newPointToCheck);
                }
            }

            if (bd.RoadConnections[(int)Base.DIREC.RIGHT] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                Point newPointToCheck = new Point(pointToCheck.x + 1, pointToCheck.y);
                // если точка ещё не была достижима - увеличиваем счётчик и маркируем её как достижимую
                if (!connectedBlocks[newPointToCheck.x, newPointToCheck.y])
                {
                    connectedBlocks[newPointToCheck.x, newPointToCheck.y] = true;
                    linkedBlocksCounter++;
                    pointsToProcess.AddLast(newPointToCheck);
                }
            }
        }


        if (linkedBlocksCounter != xSize * ySize)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    /**********************************************************************************/
    // определяем правила соединения блоков перед генерацией карты
    //
    /**********************************************************************************/
    protected void BuildRoadRules(int xSize, int ySize, float chanseOfConnection = 0.1f)
    {
        // проверки параметров
        if (xSize <= 0 || ySize <= 0 || chanseOfConnection < 0)
        {
            Debug.LogError("BuildRoadRules: Wrong arguments! " + "xSize " + xSize + " ySize " + ySize + "chanseOfConnection " + chanseOfConnection);
            return;
        }

        // генерируем новые правила соединений
        for (int i = 0; i < xSize; i++)
        {
            for (int z = 0; z < ySize; z++)
            {
                for (int direction = (int)Base.DIREC.DOWN; direction <= (int)Base.DIREC.LEFT; direction++)
                {
                    float currectConnectionChance = Random.Range(0.0f, 1.0f);
                    if (currectConnectionChance <= chanseOfConnection)
                    {
                        BlockDescriptorImitation bd = m_blockRoadRulesDescriptor[i, z];
                        if (bd.RoadConnections[direction] != ROAD_CONNECTION_STATUS.BLOCKED)
                        {
                            bd.RoadConnections[direction] = ROAD_CONNECTION_STATUS.NEEDED;
                        }

                        UpdateRulesForBlock(i, z, bd.RoadConnections);
                    }
                }
            }
        }
    }

    /**********************************************************************************/
    // записываем правила дорог в дескриптор блока
    // как правило это используется для генерируемых блоков
    //
    /**********************************************************************************/
    public void SetRoadRulesToBlock(BlockDescriptor descriptor, int x, int y)
    {
        // проверки параметров
        if (x < 0 || y < 0 || descriptor == null)
        {
            Debug.LogError("SetRoadRulesToBlock: Wrong arguments! " + "x " + x + " y " + y + "descriptor is null? " + (descriptor == null).ToString());
            return;
        }

        BlockDescriptorImitation bd = m_blockRoadRulesDescriptor[x, y];
        for (int direction = (int)Base.DIREC.DOWN; direction < (int)Base.DIREC.NUM_OF_DIRECTIONS; direction++)
        {
            descriptor.RoadConnections[direction] = bd.RoadConnections[direction];
        }
    }

    /**********************************************************************************/
    // строим дороги внутри блока
    //
    /**********************************************************************************/
    public void BuildRoadInBlock(BlockDescriptor descriptor, int x, int y, int blockSize)
    {
        // проверки параметров
        if (x < 0 || y < 0 || descriptor == null || blockSize < 0)
        {
            Debug.LogError("BuildRoadInBlock: Wrong arguments! " + "x " + x + " y " + y + " blockSize " + blockSize + "descriptor is null? " + (descriptor == null).ToString());
            return;
        }

        // подготавливаем коллекции для работы с дорогами и путями
        List<Point> pointsToConnect = new List<Point>();
        WayNode[,] roadMap = new WayNode[blockSize, blockSize];
        for (int i = 0; i < blockSize; i++)
        {
            for (int z = 0; z < blockSize; z++)
            {
                roadMap[i, z] = new WayNode();
            }
        }

        // добавляем точки дорог к зданиям
        for (int buildingID = 0; buildingID < descriptor.Buildings.Count; buildingID++)
        {
            GameObject building = descriptor.Buildings[buildingID];
            BuildingController ctr = building.GetComponent<BuildingController>();

            int xCorInRoadMap = ctr.RoadPoint.x + ctr.GetLocalPosition().x - ctr.XBuildingSize / 2;
            int yCorInRoadMap = ctr.RoadPoint.y + ctr.GetLocalPosition().y - ctr.YBuildingSize / 2;

            pointsToConnect.Add(new Point(xCorInRoadMap, yCorInRoadMap));
            roadMap[xCorInRoadMap, yCorInRoadMap].approvedDirections.Add(Base.DIREC.UP);
            roadMap[xCorInRoadMap, yCorInRoadMap].previusRoadDirection = Base.DIREC.UP;
            roadMap[xCorInRoadMap, yCorInRoadMap].cellCost = WayNode.ROAD_COST;
        }


        // добавляем все входы в блок, которые надо соеденить
        if (descriptor.RoadConnections[(int)Base.DIREC.DOWN] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            int xCorInRoadMap = blockSize / 2;
            int yCorInRoadMap = 0;
            pointsToConnect.Add(new Point(xCorInRoadMap, yCorInRoadMap));
            roadMap[xCorInRoadMap, yCorInRoadMap].approvedDirections.Add(Base.DIREC.DOWN);
            roadMap[xCorInRoadMap, yCorInRoadMap].previusRoadDirection = Base.DIREC.DOWN;
            roadMap[xCorInRoadMap, yCorInRoadMap].cellCost = WayNode.ROAD_COST;
        }

        if (descriptor.RoadConnections[(int)Base.DIREC.UP] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            int xCorInRoadMap = blockSize / 2;
            int yCorInRoadMap = blockSize - 1;
            pointsToConnect.Add(new Point(xCorInRoadMap, yCorInRoadMap));
            roadMap[xCorInRoadMap, yCorInRoadMap].approvedDirections.Add(Base.DIREC.UP);
            roadMap[xCorInRoadMap, yCorInRoadMap].previusRoadDirection = Base.DIREC.UP;
            roadMap[xCorInRoadMap, yCorInRoadMap].cellCost = WayNode.ROAD_COST;
        }

        if (descriptor.RoadConnections[(int)Base.DIREC.LEFT] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            int xCorInRoadMap = 0;
            int yCorInRoadMap = blockSize / 2;
            pointsToConnect.Add(new Point(xCorInRoadMap, yCorInRoadMap));
            roadMap[xCorInRoadMap, yCorInRoadMap].approvedDirections.Add(Base.DIREC.LEFT);
            roadMap[xCorInRoadMap, yCorInRoadMap].previusRoadDirection = Base.DIREC.LEFT;
            roadMap[xCorInRoadMap, yCorInRoadMap].cellCost = WayNode.ROAD_COST;
        }

        if (descriptor.RoadConnections[(int)Base.DIREC.RIGHT] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            int xCorInRoadMap = blockSize - 1;
            int yCorInRoadMap = blockSize / 2;
            pointsToConnect.Add(new Point(xCorInRoadMap, yCorInRoadMap));
            roadMap[xCorInRoadMap, yCorInRoadMap].approvedDirections.Add(Base.DIREC.RIGHT);
            roadMap[xCorInRoadMap, yCorInRoadMap].previusRoadDirection = Base.DIREC.RIGHT;
            roadMap[xCorInRoadMap, yCorInRoadMap].cellCost = WayNode.ROAD_COST;
        }


        // проходим по всем точкам соединения и строим чертёж дороги
        for (int i = 0; i < pointsToConnect.Count; i++)
        {
            Point from = pointsToConnect[i];

            for (int z = i + 1; z < pointsToConnect.Count; z++)
            {
                Point to = pointsToConnect[z];

                // после выбора точки строим дорогу "from - to"
                // подготавливаем чертёж
                PrepareBluprintOfRoads(descriptor, roadMap, from, to, blockSize);

                // обнуляем стоимости путей после каждой итерации построения
                for (int xBuprint = 0; xBuprint < blockSize; xBuprint++)
                {
                    for (int yBuprint = 0; yBuprint < blockSize; yBuprint++)
                    {
                        roadMap[xBuprint, yBuprint].wayCost = WayNode.UNREACHABLE;
                    }
                }
            }
        }


        // строим дорогу
        for (int xBuprint = 0; xBuprint < blockSize; xBuprint++)
        {
            for (int yBuprint = 0; yBuprint < blockSize; yBuprint++)
            {
                List<Base.DIREC> approvedDirection = roadMap[xBuprint, yBuprint].approvedDirections;
                if (approvedDirection.Count != 0)
                {
                    int roadKey = GetRoadKeyFromDirection(approvedDirection);

                    if (!m_roadDictionary.ContainsKey(roadKey))
                    {
                        Debug.LogError("Wrong road generation!");
                    }

                    GameObject roadToInstance = m_roadDictionary[roadKey];

                    // правильные координаты будут выставлены несколькими шагами дальше через localPosition
                    GameObject roadInstance = GameObject.Instantiate(roadToInstance, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;

                    // устанавливаем в родителя
                    roadInstance.transform.SetParent(descriptor.gameObject.transform);
                    roadInstance.transform.localPosition = new Vector3((float)(xBuprint) * Base.SIZE_OF_CELL + Base.HALF_OF_CELL,
                                                                    (float)(yBuprint) * Base.SIZE_OF_CELL + Base.HALF_OF_CELL, 0.0f);

                    // отмечаем данную ячейку как занятую для добавления новых объектов
                    descriptor.FreeSpaceMap[xBuprint, yBuprint] = false;

                    // сохраняем отметку о построенной дороге
                    m_roadObjectMap[xBuprint * x, yBuprint * y] = true;
                }
            }
        }

        // обновляем правила дорог
        UpdateRulesForBlock(x, y, descriptor.RoadConnections);
    }

    /**********************************************************************************/
    // обновляем правила дорог для блока
    // если этого не сделать, то возможны колизии в соседних блоках, особенно это касается предопределённых блоков
    //
    /**********************************************************************************/
    public void UpdateRulesForBlock(int x, int y, ROAD_CONNECTION_STATUS[] roadConnections)
    {

        // проверки параметров
        if (x < 0 || y < 0 || roadConnections == null || roadConnections.Length != (int)Base.DIREC.NUM_OF_DIRECTIONS)
        {
            Debug.LogError("UpdateRulesForBlock: Wrong arguments! " + "x " + x + " y " + y + "roadConnections is null? " + (roadConnections == null).ToString() + " roadConnections.Length " + roadConnections.Length);
            return;
        }

        // обновляем правила для текущего и соседних блоков
        for (int i = 0; i < (int)Base.DIREC.NUM_OF_DIRECTIONS; i++)
        {
            if (roadConnections[i] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                m_blockRoadRulesDescriptor[x, y].RoadConnections[i] = ROAD_CONNECTION_STATUS.NEEDED;
                roadConnections[i] = ROAD_CONNECTION_STATUS.NEEDED;
            }
            else
            {
                m_blockRoadRulesDescriptor[x, y].RoadConnections[i] = ROAD_CONNECTION_STATUS.BLOCKED;
            }
        }

        // проходим по окрестным блокам и обновляем условия
        // правила должны быть синхронизированы в соседних блоках
        for (int direction = (int)Base.DIREC.DOWN; direction < (int)Base.DIREC.NUM_OF_DIRECTIONS; direction++)
        {
            Point blockToUpdateCor = new Point(x, y);
            switch (direction)
            {
                case (int)Base.DIREC.DOWN:
                    blockToUpdateCor.y--;
                    break;
                case (int)Base.DIREC.UP:
                    blockToUpdateCor.y++;
                    break;
                case (int)Base.DIREC.LEFT:
                    blockToUpdateCor.x--;
                    break;
                case (int)Base.DIREC.RIGHT:
                    blockToUpdateCor.x++;
                    break;
                default:
                    Debug.LogError("PrepareBluprintOfRoads: wrong direction!");
                    break;
            }

            // проверяем координыты
            if (blockToUpdateCor.x < 0 || blockToUpdateCor.x >= m_blockRoadRulesDescriptor.GetLength(0) || blockToUpdateCor.y < 0 || blockToUpdateCor.y >= m_blockRoadRulesDescriptor.GetLength(1))
            {
                continue;
            }

            BlockDescriptorImitation blockToUpdates = m_blockRoadRulesDescriptor[blockToUpdateCor.x, blockToUpdateCor.y];
            Base.DIREC connectionToUpdate = Base.InvertDirection(direction);

            // синхронизируем
            blockToUpdates.RoadConnections[(int)connectionToUpdate] = m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)direction];
        }
    }

    /**********************************************************************************/
    // строим дорогу из точки from в точку to
    // результат записываем в bluprintOfRoadMap
    //
    /**********************************************************************************/
    void PrepareBluprintOfRoads(BlockDescriptor descriptor, WayNode[,] bluprintOfRoadMap, Point from, Point to, int sizeOfBlock)
    {

        // проверки параметров
        if (from == null || to == null || descriptor == null || bluprintOfRoadMap == null || bluprintOfRoadMap.Length == 0)
        {
            Debug.LogError("PrepareBluprintOfRoads: Wrong arguments! " + "descriptor is null? " + (descriptor == null).ToString()
                + " from is null? " + (from == null).ToString()
                + " to is null? " + (to == null).ToString()
                + " bluprintOfRoadMap is null? " + (bluprintOfRoadMap == null).ToString()
                + " bluprintOfRoadMap.Length " + bluprintOfRoadMap.Length);
            return;
        }

        LinkedList<Point> currentPontsToProcess = new LinkedList<Point>();
        LinkedList<Point> nextIterationPontsToProcess = new LinkedList<Point>();

        // устанавливаем стартовые параметры алгоритма - начальную точку и её стоимость пути
        bluprintOfRoadMap[from.x, from.y].wayCost = 0;
        currentPontsToProcess.AddFirst(from);
        int possibleClosestDist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);



        int nodeClosestDist = possibleClosestDist;
        bool buildingComplite = false;
        int maxSteps = sizeOfBlock * 4;

        while (!buildingComplite && maxSteps > 0)
        {
            while (currentPontsToProcess.Count > 0)
            {
                // забираем точку для просчёта и работаем с ней
                Point currentPoint = currentPontsToProcess.First.Value;
                currentPontsToProcess.RemoveFirst();


                int currentWayCost = bluprintOfRoadMap[currentPoint.x, currentPoint.y].wayCost;

                // просчитываем верхную точку
                CalculatePoint(descriptor, bluprintOfRoadMap, currentPoint, to, Base.DIREC.DOWN, sizeOfBlock, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем нижнюю точку
                CalculatePoint(descriptor, bluprintOfRoadMap, currentPoint, to, Base.DIREC.UP, sizeOfBlock, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем левую точку
                CalculatePoint(descriptor, bluprintOfRoadMap, currentPoint, to, Base.DIREC.RIGHT, sizeOfBlock, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем правую точку
                CalculatePoint(descriptor, bluprintOfRoadMap, currentPoint, to, Base.DIREC.LEFT, sizeOfBlock, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);
            }


            // принимаем решение о дальнейших итерациях

            WayNode WayNodeFinal = bluprintOfRoadMap[to.x, to.y];
            if (WayNodeFinal.wayCost == possibleClosestDist)
            {
                buildingComplite = true;
            }
            else
            {
                // если мы исчерпали возможности для построения маршрута - завершаем проектирование маршрута
                if (WayNodeFinal.wayCost != WayNode.UNREACHABLE && nextIterationPontsToProcess.Count == 0)
                {
                    buildingComplite = true;
                }
                // в противном случае продолжаем
                else
                {
                    // меняем местами коллекции
                    LinkedList<Point> supportPointer = currentPontsToProcess;
                    currentPontsToProcess = nextIterationPontsToProcess;
                    nextIterationPontsToProcess = supportPointer;
                }
            }

            maxSteps--;
        }


        if (!buildingComplite)
        {
            Debug.LogError("PrepareBluprintOfRoads: We cann't build way");
            Logger.CreatePathFinderErrorReport(bluprintOfRoadMap, from, to, descriptor.FreeSpaceMap);
        }
        else
        {
            // формируем направления дороги по окончанию работы алгоритма "А star!"
            Point pointToBuild = new Point(to);
            WayNode nodeForBuilding = null;
            WayNode finalBuildingNode = bluprintOfRoadMap[from.x, from.y];
            Base.DIREC nextDirection = Base.DIREC.NO_DIRECTION;

            maxSteps = sizeOfBlock * 4;
            do
            {
                // проверяем координыты
                if (pointToBuild.x < 0 || pointToBuild.x >= sizeOfBlock || pointToBuild.y < 0 || pointToBuild.y >= sizeOfBlock)
                {
                    continue;
                }

                nodeForBuilding = bluprintOfRoadMap[pointToBuild.x, pointToBuild.y];

                // добавляем направления, если надо
                Base.DIREC newDirection = nodeForBuilding.previusRoadDirection;
                if (!nodeForBuilding.approvedDirections.Contains(newDirection))
                {
                    nodeForBuilding.approvedDirections.Add(newDirection);
                }

                if (!nodeForBuilding.approvedDirections.Contains(nextDirection) && nextDirection != Base.DIREC.NO_DIRECTION)
                {
                    nodeForBuilding.approvedDirections.Add(nextDirection);
                }

                // устанавливаем цену дороги
                nodeForBuilding.cellCost = WayNode.ROAD_COST;

                switch (newDirection)
                {
                    case Base.DIREC.DOWN:
                        pointToBuild.y--;
                        break;
                    case Base.DIREC.UP:
                        pointToBuild.y++;
                        break;
                    case Base.DIREC.LEFT:
                        pointToBuild.x--;
                        break;
                    case Base.DIREC.RIGHT:
                        pointToBuild.x++;
                        break;
                    default:
                        Debug.LogError("PrepareBluprintOfRoads: wrong direction!");
                        break;
                }

                nextDirection = Base.InvertDirection(newDirection);
                maxSteps--;

            } while (nodeForBuilding != finalBuildingNode && maxSteps > 0);
        }
    }



    /**********************************************************************************/
    // просчитываем конкретную точку построения пути
    // проверяем её стоимость и на оснеовании проверки вносим коррективы в карту маршрутов
    //
    /**********************************************************************************/
    void CalculatePoint(BlockDescriptor descriptor, WayNode[,] bluprintOfRoadMap, Point currentPoint, Point to, Base.DIREC prevPointDir, int sizeOfBlock, int currentWayCost, ref int nodeClosestDist,
        LinkedList<Point> currentPontsToProcess, LinkedList<Point> nextIterationPontsToProcess)
    {
        // проверки параметров
        if (currentPoint == null || to == null || descriptor == null || bluprintOfRoadMap == null || bluprintOfRoadMap.Length == 0)
        {
            Debug.LogError("CalculatePoint: Wrong arguments! " + "descriptor is null? " + (descriptor == null).ToString()
                + " currentPoint is null? " + (currentPoint == null).ToString()
                + " prevPointDir " + prevPointDir.ToString()
                + " to is null? " + (to == null).ToString()
                + " bluprintOfRoadMap is null? " + (bluprintOfRoadMap == null).ToString()
                + " bluprintOfRoadMap.Length " + bluprintOfRoadMap.Length);
            return;
        }


        int newX = currentPoint.x;
        int newY = currentPoint.y;

        // выбираем координаты новой точки в зависимости от положения предыдущей
        switch (prevPointDir)
        {
            case Base.DIREC.DOWN:
                newY++;
                break;
            case Base.DIREC.UP:
                newY--;
                break;
            case Base.DIREC.LEFT:
                newX++;
                break;
            case Base.DIREC.RIGHT:
                newX--;
                break;
        }

        // если координата удовлетворяет ...
        if (newX < 0 || newX >= sizeOfBlock || newY < 0 || newY >= sizeOfBlock)
        {
            return;
        }

        // ... и место свободно для дорог - процессим
        if (descriptor.FreeWaysMap[newX, newY] == false)
        {
            return;
        }


        WayNode wayNodeToCheck = bluprintOfRoadMap[newX, newY];
        int possibleWayCost = currentWayCost + wayNodeToCheck.cellCost;
        Point pointToCheck = null;

        if (wayNodeToCheck.wayCost > possibleWayCost)
        {
            wayNodeToCheck.wayCost = possibleWayCost;
            wayNodeToCheck.previusRoadDirection = prevPointDir;
            pointToCheck = new Point(newX, newY);

        }
        else if (wayNodeToCheck.wayCost == possibleWayCost)
        {
            // в случае, если стоимости равны - в половине случаев переключаемся на новый путь для разнообразия
            if (Random.Range(0, 100) >= 50)
            {
                wayNodeToCheck.wayCost = possibleWayCost;
                wayNodeToCheck.previusRoadDirection = prevPointDir;
            }
        }

        // если мы нашли точку для просчёта - определяемся когда будем её просчитывать
        // если "квадратное" растояние меньше или равно самого близкого к целевой точке, то будем просчитывать её в первую очередь
        if (pointToCheck != null)
        {
            int dist = Mathf.Abs(pointToCheck.x - to.x) + Mathf.Abs(pointToCheck.y - to.y);

            if (dist == 0)
            {
                nodeClosestDist = 0;
                return;
            }
            else if (dist <= nodeClosestDist)
            {
                currentPontsToProcess.AddLast(pointToCheck);
                nodeClosestDist = dist;
            }
            else
            {
                nextIterationPontsToProcess.AddLast(pointToCheck);
            }
        }
    }


    /**********************************************************************************/
    // проверяем, подойдёт ли блок в эту позицию
    //
    /**********************************************************************************/
    public bool IsBlockOk(GameObject block, int x, int y)
    {

        // проверка параметров
        if(block == null || x <= 0 || y <= 0)
        {
            Debug.LogError("IsBlockOk: wrong argument " + "block is null? " + (block == null).ToString() + " x " + x + " y " + y);
            return false;
        }

        BlockDescriptor bd = block.GetComponent<BlockDescriptor>();
        if (bd == null)
        {
            Debug.LogError("bd descriptor is null!");
            return false;
        }

        BlockDescriptorImitation bdToCompaire = m_blockRoadRulesDescriptor[x, y];

        if (bdToCompaire == null)
        {
            Debug.LogError("bdToCompaire descriptor is null!");
            return false;
        }

        // проверяем все стороны на возможность подключения дорог
        for (int direction = (int)Base.DIREC.DOWN; direction < (int)Base.DIREC.NUM_OF_DIRECTIONS; direction++)
        {
            if ((bdToCompaire.RoadConnections[direction] == ROAD_CONNECTION_STATUS.BLOCKED || bd.RoadConnections[direction] == ROAD_CONNECTION_STATUS.BLOCKED) &&
                (bdToCompaire.RoadConnections[direction] == ROAD_CONNECTION_STATUS.NEEDED || bd.RoadConnections[direction] == ROAD_CONNECTION_STATUS.NEEDED))
            {
                return false;
            }
        }

        return true;
    }
}
