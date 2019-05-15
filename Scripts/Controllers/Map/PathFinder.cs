using UnityEngine;
using System.Collections.Generic;

/**********************************************************************************/
// PathFinder
// класс ответственный за поиск путей на игровой карте
//
/**********************************************************************************/
public class PathFinder
{

    private static PathFinder s_instance = null;

    private bool[,] m_freeSpaceGlobalMap = null;            // глобальная карта свободного пространства
    private BlockDescriptorImitation[,] m_connectionGlobalMap = null;            // глобальная карта свободного пространства (в формате BDI)
                                                                                 // данная структура задумывалась как инструмент создания специальных точек входа на карту
                                                                                 // с этих точек должна была быть возможность сойти, но при этом на неё нельзя было вернуться -
                                                                                 // эффект односторонней проходимости
                                                                                 // В текущий момент эта особенность не используется

    private bool[,] m_spaceIsAchievable = null;         // данный массив необходим для маркировки клеток по их достижимости для юнитов
                                                        // m_freeSpaceGlobalMap обозначает физичисую пустоту пространства, однако в это время из-за
                                                        // структуры карты может быть недостижима

    private BlockDescriptorImitation[,] m_blockConnectionData;      // массив содержащий информацию о соединениях между блоками
    private Base.DIREC[,] m_singMap;        // массив системы указателей путей
    private Point[] m_exitBlockPoints;      // массив хранящий стандартные точки выхода из блоков по сторонам света (кеш-таблица для оптимизации)
    private Point[] m_transferBlockPoints;  // массив хранящий стандартные точки перехода из блока по сторонам света (кеш-таблица для оптимизации)
    private int[,] m_trafficData;           // массив хранящий данные о загруженности путей
    private int m_trafficJamCost = 60;      // настройка определяющая "стоимость" пробки
    private int m_trafficIncrCost = 1;      // настройка определяющая "стоимость" планируемого прохода по точке
    private int m_trafficTimeDiсrCost = 10; // настройка определяющая убывание "стоимости" пробки в точке (раз в 1 сек)
    private int m_trafficDiсrCost = 3;     // настройка определяющая убывание "стоимости" пробки в точке (после прохождения юнита)
    private int m_trafficJamLimit = 30;     // настройка определяющая нижнюю границу "стоимости" прохода по точке, после которой она будет считаться пробкой
    private LinkedList<Point> m_traffickJamPointsPing;
    private LinkedList<Point> m_traffickJamPointsPong;


    private int m_sizeOfBlock = 5;
    private int m_xSizeOfMap = 0;   // в блоках
    private int m_ySizeOfMap = 0;   // в блоках

    /**********************************************************************************/
    // PathFinder конструктор
    //
    /**********************************************************************************/
    private PathFinder()
    {
        m_exitBlockPoints = new Point[(int)Base.DIREC.NUM_OF_DIRECTIONS];
        m_transferBlockPoints = new Point[(int)Base.DIREC.NUM_OF_DIRECTIONS];
    }

    /**********************************************************************************/
    // PathFinder GetInstance
    //
    /**********************************************************************************/
    public static PathFinder GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new PathFinder();
        }

        return s_instance;
    }


    /**********************************************************************************/
    // функция инициализации
    //
    /**********************************************************************************/
    public void InitializePathFinder(int xSizeInBlocks, int ySizeInBlocks, int blockSize)
    {
        m_xSizeOfMap = xSizeInBlocks;
        m_ySizeOfMap = ySizeInBlocks;

        m_singMap = new Base.DIREC[xSizeInBlocks * ySizeInBlocks, xSizeInBlocks * ySizeInBlocks];
        m_blockConnectionData = new BlockDescriptorImitation[xSizeInBlocks, ySizeInBlocks];
        m_freeSpaceGlobalMap = new bool[xSizeInBlocks * blockSize, ySizeInBlocks * blockSize];
        m_spaceIsAchievable = new bool[xSizeInBlocks * blockSize, ySizeInBlocks * blockSize];
        m_connectionGlobalMap = new BlockDescriptorImitation[xSizeInBlocks * blockSize, ySizeInBlocks * blockSize];
        m_sizeOfBlock = blockSize;

        // задаём координаты точек выхода из блоков
        m_exitBlockPoints[(int)Base.DIREC.DOWN] = new Point(blockSize / 2, 0);
        m_exitBlockPoints[(int)Base.DIREC.UP] = new Point(blockSize / 2, blockSize - 1);
        m_exitBlockPoints[(int)Base.DIREC.LEFT] = new Point(0, blockSize / 2);
        m_exitBlockPoints[(int)Base.DIREC.RIGHT] = new Point(blockSize - 1, blockSize / 2);

        m_transferBlockPoints[(int)Base.DIREC.DOWN] = new Point(blockSize / 2, -1);
        m_transferBlockPoints[(int)Base.DIREC.UP] = new Point(blockSize / 2, blockSize);
        m_transferBlockPoints[(int)Base.DIREC.LEFT] = new Point(-1, blockSize / 2);
        m_transferBlockPoints[(int)Base.DIREC.RIGHT] = new Point(blockSize, blockSize / 2);

        m_trafficData = new int[xSizeInBlocks * blockSize, ySizeInBlocks * blockSize];
        m_traffickJamPointsPing = new LinkedList<Point>();
        m_traffickJamPointsPong = new LinkedList<Point>();
    }

    /**********************************************************************************/
    // функция оповещающая PathFinder о пробке в конкретной точке
    // это приведёт к увеличению стоимости прохода через данную точку
    //
    /**********************************************************************************/
    public void TrafficJamNotification(Point trafficJamPoint)
    {
        // если точка ещё не была отмечена как точка с высоким трафиком - делаем это
        if (m_trafficData[trafficJamPoint.x, trafficJamPoint.y] <= m_trafficJamLimit)
        {
            m_traffickJamPointsPing.AddLast(trafficJamPoint);
        }
        m_trafficData[trafficJamPoint.x, trafficJamPoint.y] += m_trafficJamCost;
    }

    /**********************************************************************************/
    // функция производящая уменьшение значения трафика после выхода юнита из этой клетки
    // используется как противопоставление увеличиванию стоимости пути при его прокладывании
    //
    /**********************************************************************************/
    public void TrafficJamDegradation(Point pointToLeave)
    {
        int trafficDataCost = m_trafficData[pointToLeave.x, pointToLeave.y];
        trafficDataCost -= m_trafficDiсrCost;
        if (trafficDataCost < 0)
        {
            trafficDataCost = 0;
        }
        m_trafficData[pointToLeave.x, pointToLeave.y] = trafficDataCost;
    }

    /**********************************************************************************/
    // функция производящая уменьшение значения трафика в проблемных точках раз в 1 сек
    // смысл в том, что однажды создав серьёзную пробку, монстры перестанут туда стремиться
    // однако спустя какое-то время (по разрешению "пробки") территория снова должна стать пригодной для передвидения
    //
    /**********************************************************************************/
    public void TrafficJamTimeDegradation()
    {
        while (m_traffickJamPointsPing.Count > 0)
        {
            // берём точки из одной "стопки", проверяем, и если всё ещё подходят - перекладываем в другую
            Point trafficJamPoint = m_traffickJamPointsPing.Last.Value;
            m_traffickJamPointsPing.RemoveLast();

            int trafficJamValue = m_trafficData[trafficJamPoint.x, trafficJamPoint.y];
            trafficJamValue -= m_trafficTimeDiсrCost;

            if (trafficJamValue > 0)
            {
                // сохраняем в pong списке
                m_traffickJamPointsPong.AddLast(trafficJamPoint);
            }
            else
            {
                // отрицательным оно не должно быть, ставим 0
                trafficJamValue = 0;
            }

            // обновляем значение трафика в точке
            m_trafficData[trafficJamPoint.x, trafficJamPoint.y] = trafficJamValue;
        }

        // меняем местами Ping и Pong
        LinkedList<Point> swap = m_traffickJamPointsPing;
        m_traffickJamPointsPing = m_traffickJamPointsPong;
        m_traffickJamPointsPong = swap;

    }

    /**********************************************************************************/
    // функция проверяющая точку на адекватность с размерами карты и проверяющая возможность 
    // пройти по ней
    //
    /**********************************************************************************/
    public bool ValidatePathCell(int x, int y)
    {
        if( !CheckCoordinateLimits(x, y))
        {
            return false;
        }

        // проверяем на доступность клетки для хотьбы
        return m_spaceIsAchievable[x, y];
    }


    /**********************************************************************************/
    // функция проверяющая точку на адекватность с размерами карты
    //
    /**********************************************************************************/
    protected bool CheckCoordinateLimits(Point pointToCheck)
    {
        return CheckCoordinateLimits(pointToCheck.x, pointToCheck.y);
    }

    /**********************************************************************************/
    // функция проверяющая точку на адекватность с размерами карты
    //
    /**********************************************************************************/
    protected bool CheckCoordinateLimits(int x, int y)
    {

        // проверяем на размер карты
        if (x < 0 || y < 0 || x >= (m_xSizeOfMap * m_sizeOfBlock) || y >= (m_ySizeOfMap * m_sizeOfBlock))
        {
            return false;
        }

        return true;
    }

    /**********************************************************************************/
    // функция проверяющая точку на адекватность с размерами карты и проверяющая возможность 
    // пройти по ней
    //
    /**********************************************************************************/
    public bool ValidatePathCell(Point pathPoint)
    {
        return ValidatePathCell(pathPoint.x, pathPoint.y);
    }

    /**********************************************************************************/
    // функция передающая данные о свободном пространстве в блоке
    //
    /**********************************************************************************/
    public void ApplyBlockMapData(int xBlockCor, int yBlockCor, BlockDescriptor bd)
    {
        int xFreeSpaceMapData = xBlockCor * m_sizeOfBlock;
        int yFreeSpaceMapData = yBlockCor * m_sizeOfBlock;

        if (bd == null || bd.FreeSpaceMap == null)
        {
            Debug.LogError("BlockDescriptor not initialized!");
        }

        // копируем данные о свободном пространстве внутри блока
        for (int x = 0; x < m_sizeOfBlock; x++)
        {
            for (int y = 0; y < m_sizeOfBlock; y++)
            {
                m_freeSpaceGlobalMap[x + xFreeSpaceMapData, y + yFreeSpaceMapData] = bd.FreeWaysMap[x, y];
            }
        }
    }


    /**********************************************************************************/
    // функция передающая информацию о соединениях блоков друг с другом
    //
    /**********************************************************************************/
    public void ApplyBlockConnectionData(BlockDescriptorImitation[,] blockConnectionData)
    {
        for (int x = 0; x < m_xSizeOfMap; x++)
        {
            for (int y = 0; y < m_ySizeOfMap; y++)
            {
                m_blockConnectionData[x, y] = new BlockDescriptorImitation(blockConnectionData[x, y]);
            }
        }
    }

    /**********************************************************************************/
    // строим пути перемещений между блоками 
    // адаптируем карту свободного пространства для построения путей
    //
    // NOTE: данный алгоритм по идеи можно сильно оптимизировать за счёт маркирования путей так же и для промежуточных точек
    //       возможно в будующем стоит об этом позаботиться, особенно если речь пойдёт о больших картах
    /**********************************************************************************/
    public void BuildPathsMap()
    {
        // строим пути перемещений между блоками 
        // выбираем точку "из"
        for (int xPointToMark = 0; xPointToMark < m_xSizeOfMap; xPointToMark++)
        {
            for (int yPointToMark = 0; yPointToMark < m_ySizeOfMap; yPointToMark++)
            {
                Point fromPoint = new Point(xPointToMark, yPointToMark);

                // выбираем точку "куда"
                for (int xPointToConnect = 0; xPointToConnect < m_xSizeOfMap; xPointToConnect++)
                {
                    for (int yPointToConnect = 0; yPointToConnect < m_ySizeOfMap; yPointToConnect++)
                    {
                        Point pointToConnect = new Point(xPointToConnect, yPointToConnect);

                        if (!fromPoint.IsSamePoint(pointToConnect))
                        {
                            // просчитываем путь
                            BuildPath(fromPoint, pointToConnect);
                        }
                    }
                }
            }
        }

        // адаптируем карту свободного пространства для построения путей
        // стартовой точкой для обновления выбираем одну из точек перехода нулевого блока
        // проходимся по списку соединений и выбираем первый
        Point startUpdatePont = new Point();
        bool pointIsChoosed = false;
        for (int direct = (int)Base.DIREC.DOWN; direct < (int)Base.DIREC.NUM_OF_DIRECTIONS && !pointIsChoosed; direct++)
        {
            if (m_blockConnectionData[0, 0].RoadConnections[direct] != ROAD_CONNECTION_STATUS.BLOCKED)
            {
                startUpdatePont = m_exitBlockPoints[direct];
                pointIsChoosed = true;
            }
        }

        if (!pointIsChoosed)
        {
            Debug.LogError("We cann't choose the exit point for map updating");
        }

        // сначала ресетим все соединения, потом проходимся по всей карте волновым методом и проводим необходимую калькуляцию
        SetMapConnectionData();
        UpdateCellConnectionMap(startUpdatePont);
    }

    /**********************************************************************************/
    // проходимся по всей карте и расставляем статусы соединений для всех точек 
    //
    /**********************************************************************************/
    private void SetMapConnectionData()
    {
        for (int xPointToAdapt = 0; xPointToAdapt < m_xSizeOfMap * m_sizeOfBlock; xPointToAdapt++)
        {
            for (int yPointToAdapt = 0; yPointToAdapt < m_ySizeOfMap * m_sizeOfBlock; yPointToAdapt++)
            {
                bool cellIsFree = m_spaceIsAchievable[xPointToAdapt, yPointToAdapt];
                MarkCellWayStatus(xPointToAdapt, yPointToAdapt, cellIsFree);

            }
        }
    }

    /**********************************************************************************/
    // функция проходит по всей карте, смотрит на свободное пространство и обновляет карту переходов из клетки в клетку
    //
    /**********************************************************************************/
    private void UpdateCellConnectionMap(Point pointToStartWave)
    {
        LinkedList<Point> pointToProcess = new LinkedList<Point>();

        // проверяем первую точку, она должна быть свободной
        if (m_freeSpaceGlobalMap[pointToStartWave.x, pointToStartWave.y])
        {
            if(!m_spaceIsAchievable[pointToStartWave.x, pointToStartWave.y])
            {
                m_spaceIsAchievable[pointToStartWave.x, pointToStartWave.y] = true;
            }
        }
        else
        {
            Debug.LogError("UpdateCellConnectionMap: we tryed to use wrong point to start: " + pointToStartWave.ToString());
            return;
        }

        // если точка подходящая - добавляем её в начало списка и начинаем процессить
        pointToProcess.AddFirst(pointToStartWave);
        while (pointToProcess.Count > 0)
        {
            Point curreentPoint = pointToProcess.First.Value;
            pointToProcess.RemoveFirst();

            // верхняя точка
            Point toCheck = new Point(curreentPoint);
            toCheck.ShiftPoint(Base.DIREC.UP);
            if (CheckCoordinateLimits(toCheck))
            {
                // если точка свободна (физически) и ещё не была добавлена в карту перемещений - процессим её
                if(m_freeSpaceGlobalMap[toCheck.x, toCheck.y] && !m_spaceIsAchievable[toCheck.x, toCheck.y])
                {
                    m_spaceIsAchievable[toCheck.x, toCheck.y] = true;
                    pointToProcess.AddLast(toCheck);
                }
            }


            // нижняя точка
            toCheck = new Point(curreentPoint);
            toCheck.ShiftPoint(Base.DIREC.DOWN);
            if (CheckCoordinateLimits(toCheck))
            {
                // если точка свободна (физически) и ещё не была добавлена в карту перемещений - процессим её
                if (m_freeSpaceGlobalMap[toCheck.x, toCheck.y] && !m_spaceIsAchievable[toCheck.x, toCheck.y])
                {
                    m_spaceIsAchievable[toCheck.x, toCheck.y] = true;
                    pointToProcess.AddLast(toCheck);
                }
            }

            // левая точка
            toCheck = new Point(curreentPoint);
            toCheck.ShiftPoint(Base.DIREC.LEFT);
            if (CheckCoordinateLimits(toCheck))
            {
                // если точка свободна (физически) и ещё не была добавлена в карту перемещений - процессим её
                if (m_freeSpaceGlobalMap[toCheck.x, toCheck.y] && !m_spaceIsAchievable[toCheck.x, toCheck.y])
                {
                    m_spaceIsAchievable[toCheck.x, toCheck.y] = true;
                    pointToProcess.AddLast(toCheck);
                }
            }

            // правая точка
            toCheck = new Point(curreentPoint);
            toCheck.ShiftPoint(Base.DIREC.RIGHT);
            if (CheckCoordinateLimits(toCheck))
            {
                // если точка свободна (физически) и ещё не была добавлена в карту перемещений - процессим её
                if (m_freeSpaceGlobalMap[toCheck.x, toCheck.y] && !m_spaceIsAchievable[toCheck.x, toCheck.y])
                {
                    m_spaceIsAchievable[toCheck.x, toCheck.y] = true;
                    pointToProcess.AddLast(toCheck);
                }
            }

            // обновляем статус для текущей клетки
            MarkCellWayStatus(curreentPoint.x, curreentPoint.y, m_spaceIsAchievable[curreentPoint.x, curreentPoint.y]);
        }
    }

    /**********************************************************************************/
    // помечаем, что теперь по клетке можно пройти
    //
    /**********************************************************************************/
    public void SetCellAsFree(int xPoint, int yPoint)
    {
        m_freeSpaceGlobalMap[xPoint, yPoint] = true;
        UpdateCellConnectionMap(new Point(xPoint, yPoint));
    }

    /**********************************************************************************/
    // маркируем клетку по статусу проходимости
    //
    /**********************************************************************************/
    private void MarkCellWayStatus(int xPoint, int yPoint, bool cellIsFree)
    {
        // маркеруем соседние проходы в соответсвии со статусом текущей клетки
        ROAD_CONNECTION_STATUS status = ROAD_CONNECTION_STATUS.POSSIBLE;
        if (cellIsFree == false)
        {
            status = ROAD_CONNECTION_STATUS.BLOCKED;
        }

        // верхняя точка
        BlockDescriptorImitation bdi = GetGlobalConnectionCell(xPoint, yPoint + 1);
        if (bdi != null)
        {
            bdi.RoadConnections[(int)Base.DIREC.DOWN] = status;
        }

        // нижняя точка
        bdi = GetGlobalConnectionCell(xPoint, yPoint - 1);
        if (bdi != null)
        {
            bdi.RoadConnections[(int)Base.DIREC.UP] = status;
        }

        // левая точка
        bdi = GetGlobalConnectionCell(xPoint - 1, yPoint);
        if (bdi != null)
        {
            bdi.RoadConnections[(int)Base.DIREC.RIGHT] = status;
        }

        // правая точка
        bdi = GetGlobalConnectionCell(xPoint + 1, yPoint);
        if (bdi != null)
        {
            bdi.RoadConnections[(int)Base.DIREC.LEFT] = status;
        }
    }

    /**********************************************************************************/
    // возвращаем информацию о возможных переходах между кдетками
    // 
    /**********************************************************************************/
    BlockDescriptorImitation GetGlobalConnectionCell(int x, int y)
    {
        // проверяем координаты
        if (x >= m_connectionGlobalMap.GetLength(0) || x < 0)
        {
            return null;
        }

        if (y >= m_connectionGlobalMap.GetLength(1) || y < 0)
        {
            return null;
        }

        // если BDI ещё не использовался(создавался) - создаём
        if (m_connectionGlobalMap[x, y] == null)
        {
            m_connectionGlobalMap[x, y] = new BlockDescriptorImitation();
        }

        BlockDescriptorImitation bdi = m_connectionGlobalMap[x, y];
        return bdi;
    }

    /**********************************************************************************/
    // ГЛАВНАЯ ФУНКЦИЯ ПОИСКА ПУТИ ИЗ ОДНОЙ ТОЧКИ ПРОСТРАНСТВА В ДРУГУЮ
    // оценивает позиции объектов и выбирает оптимальные точки для достижения цели
    //
    // ВНИМАНИЕ: для оптимизации при построении пути между удалёнными точками функция будет возвращать путь
    // до промежуточной точки
    //
    /**********************************************************************************/
    public LinkedList<Point> GetWay(Point from, Point to)
    {
        // определяем в одном или в разных блоках находятся точки
        Point blockFromPosition = new Point(from.x / m_sizeOfBlock, from.y / m_sizeOfBlock);
        Point blockToPosition = new Point(to.x / m_sizeOfBlock, to.y / m_sizeOfBlock);
        Point distanceVector = blockToPosition - blockFromPosition;


        // если from и to находятся в рамках соседних блоков - строим прямой маршрут
        if (Mathf.Abs(distanceVector.x) < 2 && Mathf.Abs(distanceVector.y) < 2)
        {
            return BuildSpecificWay(from, to);
        }
        // если в разных - смотрим в каком направлении необходимо осуществить движение и идём в сторону выхода
        else
        {
            MapGenerator mg = MapGenerator.GetInstance();
            if (!mg.CheckCoordinates(from) || !mg.CheckCoordinates(to))
            {
                Debug.LogError("Wrong coordinates");
            }

            // выбираем переходный блок для построения пути, но обрезаем длину пути ограничиваясь только
            // соседними блоками
            // укорачивание по Х
            if (distanceVector.x > 1)
            {
                distanceVector.x = 1;
            }
            else if (distanceVector.x < -1)
            {
                distanceVector.x = -1;
            }

            // укорачивание по Y
            if (distanceVector.y > 1)
            {
                distanceVector.y = 1;
            }
            else if (distanceVector.y < -1)
            {
                distanceVector.y = -1;
            }

            // определяемся с новой точкой выхода
            Point exitBlockShift = blockFromPosition + distanceVector;

            // определяем точку выхода из блока в блок, расположенный ближе к цели
            int fromKey = GetSingKey(exitBlockShift.x, exitBlockShift.y);
            int toKey = GetSingKey(blockToPosition.x, blockToPosition.y);


            if (fromKey < 0 || fromKey >= m_singMap.GetLength(0) || toKey < 0 || toKey >= m_singMap.GetLength(1))
            {
                Debug.LogError("Wrong coordinates");
            }

            Base.DIREC directionToMove = m_singMap[fromKey, toKey];
            Point exitPoint = new Point(m_exitBlockPoints[(int)directionToMove]);
            Point transferPoint = new Point(m_transferBlockPoints[(int)directionToMove]);

            exitPoint.x += exitBlockShift.x * m_sizeOfBlock;
            exitPoint.y += exitBlockShift.y * m_sizeOfBlock;

            transferPoint.x += exitBlockShift.x * m_sizeOfBlock;
            transferPoint.y += exitBlockShift.y * m_sizeOfBlock;

            // строим путь до точки выхода
            LinkedList<Point> way = BuildSpecificWay(from, exitPoint);
            // добавляем трансферную точку (точка входа в блок, в который будет происходить переход) и возвращаем путь
            way.AddFirst(transferPoint);
            return way;
        }

    }


    /**********************************************************************************/
    // функция строящая путь между точками и возвращающая коллекцию точек для прохождения
    //
    /**********************************************************************************/
    LinkedList<Point> BuildSpecificWay(Point from, Point to)
    {
        LinkedList<Point> wayPoints = new LinkedList<Point>();

        // если точка старта совпадает с точкой конца пути - возвращаем пустую коллекцию
        if (from.IsSamePoint(to))
        {
            return wayPoints;
        }

        // подготавливаем блюпринт пути
        // TODO: здоровый, подумать, можно ли оптимизировать расход памяти и времени
        WayNode[,] bluprintOfRoad = new WayNode[m_xSizeOfMap * m_sizeOfBlock, m_ySizeOfMap * m_sizeOfBlock];
        for (int x = 0; x < m_xSizeOfMap * m_sizeOfBlock; x++)
        {
            for (int y = 0; y < m_ySizeOfMap * m_sizeOfBlock; y++)
            {
                bluprintOfRoad[x, y] = new WayNode();
            }
        }

        PrepareBluprintOfPath(bluprintOfRoad, from, to, m_connectionGlobalMap, m_sizeOfBlock * 3);

        // проходим по результирующему пути задом наперёд и формируем список точек пути
        Point toProcess = new Point(to);
        int maxSteps = (from - to).GetSimpleLength() * 3;   // устанавливаем ограничитель, но не меньше 20 шаговы
        if(maxSteps < 20)
        {
            maxSteps = 20;
        }

        do
        {
            WayNode nodeToProcess = bluprintOfRoad[toProcess.x, toProcess.y];
            wayPoints.AddLast(new Point(toProcess));
            // так же увеличиваем загруженность трафика на данном пути
            m_trafficData[toProcess.x, toProcess.y] += m_trafficIncrCost;
            toProcess.ShiftPoint(nodeToProcess.previusRoadDirection);
            maxSteps--;

        } while (!toProcess.IsSamePoint(from) && maxSteps > 0);

        if (maxSteps == 0)
        {
            if (!toProcess.IsSamePoint(from))
            {
                Logger.CreatePathFinderErrorReport(bluprintOfRoad, from, to, m_freeSpaceGlobalMap);
                Debug.LogError("ERROR!!! Path constraction was failed!!! from: " + from.ToString() + " to: " + to.ToString());
            }
        }

        // TODO: дополнительное выравнивание по точке начала пути способствует значительному возростанию точности и аккуратности прохождения маршрута
        // юнит, не цепляется за посторонние статичные объекты
        // с другой стороны, это иногда вызывает странное поведение, когда юнит делает короткое движение "туда-обратно" в точку начала маршрута (выравнивается)
        // а потом разворачивается на 180 градусов и продолжает движение к следующей точке маршрута (возможно решается по средствам регулировки точности при выполнении манёвров)

        // удаляем последнюю точку, так как она совподает с координатой from
        // wayPoints.RemoveLast();

        return wayPoints;
    }



    /**********************************************************************************/
    // строим путь из from в to
    // !!!!!!! используется ТОЛЬКО при построении системы указателей !!!!!!!
    //
    /**********************************************************************************/
    private void BuildPath(Point from, Point to)
    {
        if (from.IsSamePoint(to))
        {
            Debug.LogError("We cann't build path from 1 same point");
            return;
        }

        WayNode[,] bluprintOfRoad = new WayNode[m_xSizeOfMap, m_ySizeOfMap];
        for (int x = 0; x < m_xSizeOfMap; x++)
        {
            for (int y = 0; y < m_ySizeOfMap; y++)
            {
                bluprintOfRoad[x, y] = new WayNode();
            }
        }

        PrepareBluprintOfPath(bluprintOfRoad, from, to, m_blockConnectionData, m_sizeOfBlock * 4);

        // проходим по результирующему пути задом наперёд и запоминаем направление движения 
        // для системы указателей важно только направление первого шага в пути
        Base.DIREC nextDirection = Base.DIREC.NO_DIRECTION;
        Point toProcess = new Point(to);

        do
        {
            WayNode nodeToProcess = bluprintOfRoad[toProcess.x, toProcess.y];
            toProcess.ShiftPoint(nodeToProcess.previusRoadDirection);
            nextDirection = Base.InvertDirection(nodeToProcess.previusRoadDirection);

        } while (!toProcess.IsSamePoint(from));

        // по окончанию обработки nextDirection содержит направление первого шага по достижению точки "to" из "from"
        // его и сохраняем в таблице

        int keyToMark = GetSingKey(from.x, from.y);     // ключик блока "from"
        int keyToConnect = GetSingKey(to.x, to.y);      // ключик блока "to"

        m_singMap[keyToMark, keyToConnect] = nextDirection;
    }


    /**********************************************************************************/
    // возвращает ключик блока по его координатам
    //
    /**********************************************************************************/
    int GetSingKey(int x, int y)
    {
        return x + y * m_xSizeOfMap;
    }



    /**********************************************************************************/
    // строим путь из точки from в точку to
    // результат записываем в bluprintOfPath
    //
    // @ bluprintOfPath - чертёж дороги (пустой, если не хотим накладывать какие-то ограничения и модификации поведения при поиске пути)
    // @ from - точка откуда строят путь
    // @ to - точка куда строят путь
    // @ map - карта с ограничением движений
    // @ maxCost - максимальная кол-во итераций при расчёте пути
    //
    /**********************************************************************************/
    void PrepareBluprintOfPath(WayNode[,] bluprintOfPath, Point from, Point to, BlockDescriptorImitation[,] map, int maxCost)
    {
        LinkedList<Point> currentPontsToProcess = new LinkedList<Point>();
        LinkedList<Point> nextIterationPontsToProcess = new LinkedList<Point>();

        // устанавливаем стартовые параметры алгоритма - начальную точку и её стоимость пути
        bluprintOfPath[from.x, from.y].wayCost = 0;
        currentPontsToProcess.AddFirst(from);
        int possibleClosestDist = (Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y)) * WayNode.GROUND_COST;



        int nodeClosestDist = possibleClosestDist;
        bool buildingComplite = false;
        int maxSteps = maxCost;

        while (!buildingComplite && maxSteps > 0)
        {
            maxSteps--;
            while (currentPontsToProcess.Count > 0)
            {
                // забираем точку для просчёта и работаем с ней
                Point currentPoint = currentPontsToProcess.First.Value;
                currentPontsToProcess.RemoveFirst();


                int currentWayCost = bluprintOfPath[currentPoint.x, currentPoint.y].wayCost;

                // просчитываем верхную точку, если соединение возможно
                if (map[currentPoint.x, currentPoint.y].RoadConnections[(int)Base.DIREC.UP] != ROAD_CONNECTION_STATUS.BLOCKED)
                    CalculatePoint(bluprintOfPath, currentPoint, to, Base.DIREC.DOWN, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем нижнюю точку, если соединение возможно
                if (map[currentPoint.x, currentPoint.y].RoadConnections[(int)Base.DIREC.DOWN] != ROAD_CONNECTION_STATUS.BLOCKED)
                    CalculatePoint(bluprintOfPath, currentPoint, to, Base.DIREC.UP, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем левую точку, если соединение возможно
                if (map[currentPoint.x, currentPoint.y].RoadConnections[(int)Base.DIREC.LEFT] != ROAD_CONNECTION_STATUS.BLOCKED)
                    CalculatePoint(bluprintOfPath, currentPoint, to, Base.DIREC.RIGHT, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);

                // просчитываем правую точку, если соединение возможно
                if (map[currentPoint.x, currentPoint.y].RoadConnections[(int)Base.DIREC.RIGHT] != ROAD_CONNECTION_STATUS.BLOCKED)
                    CalculatePoint(bluprintOfPath, currentPoint, to, Base.DIREC.LEFT, currentWayCost, ref nodeClosestDist, currentPontsToProcess, nextIterationPontsToProcess);
            }


            // принимаем решение о дальнейших итерациях

            if (to.x < 0 || to.x > bluprintOfPath.GetLength(0) ||
                to.y < 0 || to.y > bluprintOfPath.GetLength(1))
            {
                Debug.LogError("Wrong position");
            }

            WayNode WayNodeFinal = bluprintOfPath[to.x, to.y];
            if (WayNodeFinal.wayCost == possibleClosestDist)
            {
                buildingComplite = true;
            }
            else
            {
                // если мы нашли путь (любой оптимальности) и при этом исчерпали возможности для построения маршрута - завершаем проектирование маршрута
                // так же завершаем поиск пути в случае исчерпания ходов построения пути (только при условии, что у нас есть хоть какой-то путь)
                if (WayNodeFinal.wayCost != WayNode.UNREACHABLE)
                {
                    if (maxSteps == 0 || nextIterationPontsToProcess.Count == 0)
                    {
                        buildingComplite = true;
                    }

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
        }


        if (!buildingComplite)
        {
            Debug.LogError("We cann't buld the path!!! From: " + from + ". To: " + to);
            Logger.CreatePathFinderErrorReport(bluprintOfPath, from, to, m_freeSpaceGlobalMap);
        }
    }



    /**********************************************************************************/
    // просчитываем конкретную точку построения пути
    // проверяем её стоимость и на оснеовании проверки вносим коррективы в карту маршрутов
    //
    /**********************************************************************************/
    void CalculatePoint(WayNode[,] bluprintOfRoadMap, Point currentPoint, Point to, Base.DIREC prevPointDir, int currentWayCost, ref int nodeClosestDist,
        LinkedList<Point> currentPontsToProcess, LinkedList<Point> nextIterationPontsToProcess)
    {
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
        if (newX < 0 || newX >= bluprintOfRoadMap.GetLength(0) || newY < 0 || newY >= bluprintOfRoadMap.GetLength(1))
        {
            return;
        }

        WayNode wayNodeToCheck = bluprintOfRoadMap[newX, newY];

        // формирование цены прохода происходит из стоимости предыдущих клеток + стоимость прохода по клетке + загруженность пути
        int possibleWayCost = currentWayCost + wayNodeToCheck.cellCost + m_trafficData[newX, newY];
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



}