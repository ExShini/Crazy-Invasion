using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**********************************************************************************/
// BlockDescriptor
// описывает параметры блока
//
/**********************************************************************************/
public class BlockDescriptor : MonoBehaviour
{

    public List<GameObject> Objects = new List<GameObject>();
    public List<GameObject> Buildings = new List<GameObject>();
    public List<GameObject> Enviroment = new List<GameObject>();
    public bool[,] FreeSpaceMap = null;     // карта свободного пространства, используется при генерации новых элементов в блоке
    public bool[,] FreeWaysMap = null;      // карта свободных клеток пути, используется для построения путей

    protected int m_freeSpaceCounter = 0;

    public int xSize = 1;   // x размер блока относительно стандартного размера (мультипликатор)
    public int ySize = 1;   // y размер блока относительно стандартного размера (мультипликатор)


    // именованное состояние соединений с соседними блоками
    // используется только при работе в Unity инспекторе
    public ROAD_CONNECTION_STATUS DOWN;
    public ROAD_CONNECTION_STATUS RIGHT;
    public ROAD_CONNECTION_STATUS UP;
    public ROAD_CONNECTION_STATUS LEFT;

    [HideInInspector]
    public ROAD_CONNECTION_STATUS[] RoadConnections = null;




    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public BlockDescriptor()
    {
        RoadConnections = new ROAD_CONNECTION_STATUS[(int)Base.DIREC.NUM_OF_DIRECTIONS];
    }


    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    void Start()
    {
        RoadConnections = new ROAD_CONNECTION_STATUS[(int)Base.DIREC.NUM_OF_DIRECTIONS];
        RoadConnections[(int)Base.DIREC.DOWN] = DOWN;
        RoadConnections[(int)Base.DIREC.RIGHT] = RIGHT;
        RoadConnections[(int)Base.DIREC.UP] = UP;
        RoadConnections[(int)Base.DIREC.LEFT] = LEFT;
    }

    /**********************************************************************************/
    // обрабатываем параметры блока и заполняем карту свободных ячеек
    //
    /**********************************************************************************/
    public void UpdateFreeSpaceMap(int SizeOfBlock)
    {
        m_freeSpaceCounter = SizeOfBlock * SizeOfBlock;

        int sizeOfObjectMapX = SizeOfBlock * xSize;
        int sizeOfObjectMapY = SizeOfBlock * ySize;

        if (FreeSpaceMap == null)
        {
            FreeSpaceMap = new bool[sizeOfObjectMapX, sizeOfObjectMapY];

            // считаем, что мы имеем всё поле свободным
            for (int x = 0; x < sizeOfObjectMapX; x++)
            {
                for (int y = 0; y < sizeOfObjectMapY; y++)
                {
                    FreeSpaceMap[x, y] = true;
                }
            }
        }

        if (FreeWaysMap == null)
        {
            FreeWaysMap = new bool[sizeOfObjectMapX, sizeOfObjectMapY];

            // считаем, что мы имеем всё поле свободным
            for (int x = 0; x < sizeOfObjectMapX; x++)
            {
                for (int y = 0; y < sizeOfObjectMapY; y++)
                {
                    FreeWaysMap[x, y] = true;
                }
            }
        }


        // проверяем соединения блоков
        if (RoadConnections == null)
        {
            Debug.LogError("RoadConnections not initialized!");
            return;
        }

        // средняя нижняя точка
        if (RoadConnections[(int)Base.DIREC.DOWN] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            FreeSpaceMap[sizeOfObjectMapX / 2, 0] = false;
            m_freeSpaceCounter--;
        }

        // средняя верхняя точка
        if (RoadConnections[(int)Base.DIREC.UP] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            FreeSpaceMap[sizeOfObjectMapX / 2, sizeOfObjectMapY - 1] = false;
            m_freeSpaceCounter--;
        }

        // средняя левая точка
        if (RoadConnections[(int)Base.DIREC.LEFT] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            FreeSpaceMap[0, sizeOfObjectMapY / 2] = false;
            m_freeSpaceCounter--;
        }

        // средняя правая точка
        if (RoadConnections[(int)Base.DIREC.RIGHT] != ROAD_CONNECTION_STATUS.BLOCKED)
        {
            FreeSpaceMap[sizeOfObjectMapX - 1, sizeOfObjectMapY / 2] = false;
            m_freeSpaceCounter--;
        }

        // производим учёт зданий
        foreach (GameObject building in Buildings)
        {
            BuildingController bc = building.GetComponent<BuildingController>();

            if (bc == null)
            {
                Debug.LogError("BuildingController is null!");
                return;
            }

            Point buildingPosion = bc.GetLocalPosition();
            buildingPosion.x -= bc.XBuildingSize / 2;
            buildingPosion.y -= bc.YBuildingSize / 2;

            for (int xFreeMap = buildingPosion.x; xFreeMap < buildingPosion.x + bc.XBuildingSize; xFreeMap++)
            {
                for (int yFreeMap = buildingPosion.y; yFreeMap < buildingPosion.y + bc.YBuildingSize; yFreeMap++)
                {
                    FreeSpaceMap[xFreeMap, yFreeMap] = false;
                    FreeWaysMap[xFreeMap, yFreeMap] = false;
                    m_freeSpaceCounter--;
                }
            }

            Point roadPoint = new Point(buildingPosion) + bc.RoadPoint;
            FreeSpaceMap[roadPoint.x, roadPoint.y] = false;
            m_freeSpaceCounter--;
        }


        // производим учёт сгенерированного окружения
        foreach (GameObject envObj in Enviroment)
        {
            GeneratedEnvironmentCtr gec = envObj.GetComponent<GeneratedEnvironmentCtr>();

            if (gec == null)
            {
                Debug.LogError("BuildingController is null!");
                return;
            }

            Point gePosion = gec.POSITION;
            gePosion.x -= gec.SIZE / 2;
            gePosion.y -= gec.SIZE / 2;

            for (int xFreeMap = gePosion.x; xFreeMap < gePosion.x + gec.SIZE; xFreeMap++)
            {
                for (int yFreeMap = gePosion.y; yFreeMap < gePosion.y + gec.SIZE; yFreeMap++)
                {
                    FreeSpaceMap[xFreeMap, yFreeMap] = false;
                    FreeWaysMap[xFreeMap, yFreeMap] = gec.FREE_WALKING;
                    m_freeSpaceCounter--;
                }
            }
        }


    }


    /**********************************************************************************/
    // функция возвращает кол-во свободных ячеек в блоке
    // перед использованием необходимо вызвать UpdateFreeSpaceMap функцию для правильного рассчёта пространства
    //
    /**********************************************************************************/
    public int GetNumberOfFreeSpace()
    {
        return m_freeSpaceCounter;
    }
}




/**********************************************************************************/
// имитация для BlockDescriptor
// используется для построения зависимостей и путей между блоками
// вынужденная мера, ибо создание MonoBehaviour через new очень гадкое дело, всё ломается
//
/**********************************************************************************/
public class BlockDescriptorImitation
{
    public ROAD_CONNECTION_STATUS[] RoadConnections = null;

    /**********************************************************************************/
    // BlockDescriptorImitation конструктор
    //
    /**********************************************************************************/
    public BlockDescriptorImitation()
    {
        RoadConnections = new ROAD_CONNECTION_STATUS[(int)Base.DIREC.NUM_OF_DIRECTIONS];
    }

    /**********************************************************************************/
    // BlockDescriptorImitation копирующий конструктор
    //
    /**********************************************************************************/
    public BlockDescriptorImitation(BlockDescriptorImitation other)
    {
        RoadConnections = new ROAD_CONNECTION_STATUS[(int)Base.DIREC.NUM_OF_DIRECTIONS];

        for (int direction = (int)Base.DIREC.DOWN; direction < (int)Base.DIREC.NUM_OF_DIRECTIONS; direction++)
        {
            RoadConnections[direction] = other.RoadConnections[direction];
        }
    }
}