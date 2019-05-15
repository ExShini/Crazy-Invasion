using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/**********************************************************************************/
// MapGenerator класс
// генератор карты
//
/**********************************************************************************/
public class MapGenerator : MonoBehaviour
{
    // контейнер содержащий ссылку на префаб здания и соответсвующий ему вес
    [System.Serializable]
    public class BlockWeight
    {
        public Base.BLOCK_TYPE BlockType = Base.BLOCK_TYPE.NO_TYPE;
        public int Weight = 0;
    }

    [System.Serializable]
    public class BuildingWeight
    {
        public Base.GO_TYPE BuildingType = Base.GO_TYPE.NONE_TYPE;
        public int Weight = 0;
    }

    public class MapGeneratorSettings
    {
        public int MapXSize = 0;
        public int MapYSize = 0;
        public List<BlockWeight> AvalibleBlocks = new List<BlockWeight>();
    }

    public float SizeOfCell = Base.SIZE_OF_CELL;
    public int SizeOfBlocks = 5;    // размер блока
    public int MapSizeX = 10;       // размер карты в ячейках
    public int MapSizeY = 10;       // размер карты в ячейках
    public int BorderSize = 2;

    public GameObject Player1Object = null;
    public GameObject Player2Object = null;

    public GameObject GroundCollector = null;
    public GameObject[] GroundCollection = null;
    public GameObject[] BordersCollection = null;

    public GameObject BuildingCollector = null;
    public GameObject EmptyBlock = null;
    public GameObject[] BaseBlockCollection = null;

    public GameObject[] RoadsCollection = null;
    public GameObject[] GeneratedEnvCollection = null;

    List<BlockWeight> m_availableBlocks = new List<BlockWeight>();
    protected BlockDescriptor[,] m_blocksMap = new BlockDescriptor[50, 50];
    protected int m_maxBlockGenerationWeight = 0;
    protected int m_level = 0;
    protected static MapGenerator s_instance = null;

    /**********************************************************************************/
    // MapGenerator конструктор
    //
    /**********************************************************************************/
    protected MapGenerator() :
        base()
    {
        s_instance = this;
    }


    /**********************************************************************************/
    // MapGenerator GetInstance
    //
    /**********************************************************************************/
    static public MapGenerator GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("!!! MapGenerator instance is NULL !!!");
        }
        return s_instance;
    }

    /**********************************************************************************/
    // производим начальные проверки и предустановки
    //
    /**********************************************************************************/
    void Start()
    {
        // проверяем настройки генератора
        if (MapSizeX > m_blocksMap.GetLength(0))
        {
            Debug.LogError("Wrong MapGenerator's setting! MapSizeX > m_objectsMap.GetLength(0)");
            return;
        }

        if (MapSizeY > m_blocksMap.GetLength(1))
        {
            Debug.LogError("Wrong MapGenerator's setting! MapSizeY > m_objectsMap.GetLength(1)");
            return;
        }

        // устанавливаем коллекцию дорог для менеджера дорог)
        RoadManager.GetInstance().SetRoadCollection(new List<GameObject>(RoadsCollection));
    }

    /**********************************************************************************/
    // функция запускающая генерацию карты
    //
    /**********************************************************************************/
    public void GenerateMap(MapGeneratorSettings settings)
    {
        MapSizeX = settings.MapXSize;
        MapSizeY = settings.MapYSize;

        m_availableBlocks = settings.AvalibleBlocks;


        // подготавливаемся к работе
        InitializeGenertorСash();

        // генерируем карту
        GenerateMap(MapSizeX, MapSizeY);

        // Инициализируем юнит-карта контроллер
        GameObjectMapController.GetInstance().InitController(MapSizeX, MapSizeY);
    }

    /**********************************************************************************/
    // функция подготавливающая генератор к работе
    //
    /**********************************************************************************/
    void InitializeGenertorСash()
    {
        // определяем суммарный вес всех зданий участвующих в генерации
        for (int i = 0; i < m_availableBlocks.Count; i++)
        {
            m_maxBlockGenerationWeight += m_availableBlocks[i].Weight;
        }
    }

    /**********************************************************************************/
    // функция устанавливает уровень для генерации карты
    //
    /**********************************************************************************/
    public void SetLevel(int level)
    {
        m_level = level;
    }

    /**********************************************************************************/
    // основная функция ответсвенная за генерацию карты
    //
    /**********************************************************************************/
    public void GenerateMap(int xSize, int ySize)
    {
        // проверяем размер карты
        if (xSize % SizeOfBlocks != 0 || ySize % SizeOfBlocks != 0)
        {
            Debug.LogError("GenerateMap: wrong size of map! X: " + xSize + " Y: " + ySize + " but SizeOfBlocks: " + SizeOfBlocks);
            return;
        }

        // сбросываем все настройки перед новой итерацией генерации карты
        ResetMap();

        // инициализируем внутреннюю структуру PathFinder-а
        PathFinder.GetInstance().InitializePathFinder(xSize / SizeOfBlocks, ySize / SizeOfBlocks, SizeOfBlocks);

        // генерируем поверхность
        GenerateGround(xSize, ySize);

        // генерируем блоки со зданиями и объектами
        GenerateBlocks(xSize, ySize);

        // передаём данные о соединениях блоков в PathFinder
        RoadManager.GetInstance().TransferConnectionDataToPathFinder();

        // строим таблицу "дорожных указателей" для последующей оптимизации поиска пути
        PathFinder.GetInstance().BuildPathsMap();

        // размещаем объекты игроков
        PlacePlayersObjectsInMap();
    }

    /**********************************************************************************/
    // функция выбирает подходящее для игроков место 
    // NOTE!!! эта функция использует результат работы PathFinder-а, а значит должна вызываться только после
    // окончания его работы
    //
    /**********************************************************************************/
    void PlacePlayersObjectsInMap()
    {
        // для начала определяемся с игровым модом
        // от этого будет зависить место расположения игроков

        GameManager.GAME_MODE mode = GameManager.GetInstance().GameMode;
        int xMapSizeInBlocks = MapSizeX / SizeOfBlocks;
        int yMapSizeInBlocks = MapSizeY / SizeOfBlocks;

        // если у нас дуэль - выбираем два блока
        if (mode == GameManager.GAME_MODE.DUEL)
        {
            // выбираем первый блок
            int x = Random.Range(0, xMapSizeInBlocks);
            int y = Random.Range(0, yMapSizeInBlocks);

            Point blockFor1Player = new Point(x, y);
            Point blockFor2Player = new Point(blockFor1Player);

            // подбираем вторую точку
            while (blockFor2Player.IsSamePoint(blockFor1Player))
            {
                blockFor2Player.x = Random.Range(0, xMapSizeInBlocks);
                blockFor2Player.y = Random.Range(0, yMapSizeInBlocks);
            }

            // после этого помещаем их в конкретный блок
            PlaceObjectInBlock(blockFor1Player.x, blockFor1Player.y, Player1Object);
            PlaceObjectInBlock(blockFor2Player.x, blockFor2Player.y, Player2Object);
        }
        // если одиночная игра - ставим оба объекта в одно место
        // так как один из них будет отключён - проблем с этим не должно возникнуть
        else if (mode == GameManager.GAME_MODE.SINGLE)
        {
            // выбираем блок в центре
            int x = xMapSizeInBlocks / 2 - 1;
            int y = xMapSizeInBlocks / 2 - 1;

            // помещаем обоих игроков в один блок
            PlaceObjectInBlock(x, y, Player1Object);
            PlaceObjectInBlock(x, y, Player2Object);
        }
        else
        {
            Debug.LogError("Wrong game mode!");
        }
    }

    /**********************************************************************************/
    // функция помещает объект в заданный блок в случайную свободную позицию
    //
    /**********************************************************************************/
    public void PlaceObjectInBlock(int xBlock, int yBlock, GameObject gObject)
    {
        // выбираем рандомную позицию внутри блока и проверяем её на доступность
        Point positionToPlace = new Point(-1, -1);

        while (!PathFinder.GetInstance().ValidatePathCell(positionToPlace))
        {
            positionToPlace.x = Random.Range(0, SizeOfBlocks) + xBlock * SizeOfBlocks;
            positionToPlace.y = Random.Range(0, SizeOfBlocks) + yBlock * SizeOfBlocks;
        }

        // помещаем объект в выбранную позицию
        gObject.transform.SetPositionAndRotation(new Vector3((float)positionToPlace.x * SizeOfCell + Base.HALF_OF_CELL,
                                                                (float)positionToPlace.y * SizeOfCell + Base.HALF_OF_CELL), new Quaternion());
    }

    /**********************************************************************************/
    // функция проверяет адекватность координат
    //
    /**********************************************************************************/
    public bool CheckCoordinates(int x, int y)
    {
        if (x < 0 || x >= MapSizeX || y < 0 || y >= MapSizeY)
        {
            return false;
        }

        return true;
    }

    /**********************************************************************************/
    // функция проверяет адекватность координат
    //
    /**********************************************************************************/
    public bool CheckCoordinates(Point point)
    {
        return CheckCoordinates(point.x, point.y);
    }

    /**********************************************************************************/
    // функция ответсвенная за генерацию блоков
    //
    /**********************************************************************************/
    void GenerateBlocks(int xSize, int ySize)
    {
        // проверяем соответствие размера карты и блоков, они должны быть кратными
        if (MapSizeX % SizeOfBlocks != 0 || MapSizeY % SizeOfBlocks != 0)
        {
            Debug.LogError("MapGenerator::GenerateBlocks size of map or size of blocks is wrong!");
            return;
        }

        // шанс использования предопределённого блока
        // вынести в настройки (?)
        // ! НА ТЕКУЩИЙ МОМЕНТ ФУНКЦИОНАЛ ОТКЛЮЧЁН !
        // требуется коллекция предопределённых блоков для использования
        float chanseOfSpecialBloks = -0.1f;


        int xMapSizeInBlocks = MapSizeX / SizeOfBlocks;
        int yMapSizeInBlocks = MapSizeY / SizeOfBlocks;

        // проходимся по всей карте и заполняем её блоками
        for (int x = 0; x < xMapSizeInBlocks; x++)
        {
            for (int y = 0; y < yMapSizeInBlocks; y++)
            {
                // определяем, используем ли мы специальный блок или генерируемый
                float chanse = Random.Range(0.0f, 1.0f);
                bool isGenerated = false;
                GameObject toInstantiate = null;

                // выбираем блок для "постройки"
                if (chanse <= chanseOfSpecialBloks)
                {
                    int attempts = 5;
                    bool succsecc = false;

                    while (attempts > 0 && !succsecc)
                    {
                        toInstantiate = BaseBlockCollection[Random.Range(0, BaseBlockCollection.Length)];
                        succsecc = RoadManager.GetInstance().IsBlockOk(toInstantiate, x, y);
                        attempts--;
                    }

                    // если не получилось выбрать блок - переключаемся на генерацию
                    if (!succsecc)
                    {
                        toInstantiate = EmptyBlock;
                        isGenerated = true;
                    }
                }
                // генерируем блок, для этого используется пустая болванка
                else
                {
                    toInstantiate = EmptyBlock;
                    isGenerated = true;
                }


                // проверяем возможность установки
                // если нет - уходим на следующую итерацию
                BlockDescriptor descriptor = toInstantiate.GetComponent<BlockDescriptor>();

                if (!IsPositionFree(x, y, descriptor))
                {
                    continue;
                }

                // создаём новый экземпляр блока
                GameObject instance = Instantiate(toInstantiate, new Vector3((float)x * SizeOfCell * SizeOfBlocks, (float)y * SizeOfCell * SizeOfBlocks, 0.0f), Quaternion.identity) as GameObject;

                // устанавливаем в родителя
                instance.transform.SetParent(BuildingCollector.transform);

                // выполняем окончательную установку и настройку блока
                bool res = PlaceBlockAtPosition(instance, x, y);
                if (res == false)
                {
                    Debug.LogError("Somothing is wrong with block place!");
                }

                BlockDescriptor instDescriptor = instance.GetComponent<BlockDescriptor>();

                if (isGenerated)
                {
                    // генерируем внутярнку блока
                    // объекты, дороги
                    GenerateBlockContent(instance, instDescriptor, x, y);
                }
                else
                {
                    instDescriptor.UpdateFreeSpaceMap(SizeOfBlocks);
                    // обновляем правила дорог
                    RoadManager.GetInstance().UpdateRulesForBlock(x, y, instDescriptor.RoadConnections);
                }

                PathFinder.GetInstance().ApplyBlockMapData(x, y, instDescriptor);
            }
        }
    }


    /**********************************************************************************/
    // функция генерирует внутренности блока
    //
    // порядок генерации объектов:
    // - Здания
    // - Дороги
    // - Генерируемое окружение
    /**********************************************************************************/
    void GenerateBlockContent(GameObject block, BlockDescriptor descriptor, int x, int y)
    {
        // определяемся с ограничениями на соединение дорог
        RoadManager.GetInstance().SetRoadRulesToBlock(descriptor, x, y);


        // обновляем карту занятых и свободных клеток
        descriptor.UpdateFreeSpaceMap(SizeOfBlocks);


        // определяем тип блока для генерации
        Base.BLOCK_TYPE blockToGenerate = GetBlockTypeToGenerate();

        // олучаем настройки для данного блока из библиотеки блоков
        BlockSettings settingsForGeneration = BlockLibrary.GetInstance().GetBlockSettings(blockToGenerate);

        // устанавливаем здания
        // для это выбираем его(их) из имеющейся коллекции
        List<GameObject> toInstantiateCollection = GetBuildingToInstance(settingsForGeneration);

        foreach (GameObject toInstantiate in toInstantiateCollection)
        {
            // получаем контроллер у объекта шаблона, он нам потребуется для определения размеров здания
            BuildingController bcOfPattern = toInstantiate.GetComponent<BuildingController>();

            // пробуем установить здание в блок
            bool success = false;
            int attempts = 10;
            while (!success && attempts > 0)
            {
                int xBuildingSize = bcOfPattern.XBuildingSize;
                int yBuildingSize = bcOfPattern.YBuildingSize;


                int xCorToPlace = Random.Range(0, (descriptor.xSize * SizeOfBlocks) - xBuildingSize + 1);
                int yCorToPlace = Random.Range(0, (descriptor.ySize * SizeOfBlocks) - yBuildingSize + 1);

                bool isPossible = true;

                // проверяем тушку здания
                for (int xCorToCheck = xCorToPlace; xCorToCheck < xCorToPlace + xBuildingSize; xCorToCheck++)
                {
                    for (int yCorToCheck = yCorToPlace; yCorToCheck < yCorToPlace + yBuildingSize; yCorToCheck++)
                    {
                        if (descriptor.FreeSpaceMap[xCorToCheck, yCorToCheck] != true)
                        {
                            isPossible = false;
                        }
                    }
                }

                // проверяем точку выхода из здания
                if (isPossible)
                {
                    Point roadPoint = new Point(xCorToPlace, yCorToPlace) + bcOfPattern.RoadPoint;
                    if (roadPoint.x < 0 || roadPoint.y < 0 || roadPoint.x >= SizeOfBlocks || roadPoint.y >= SizeOfBlocks)
                    {
                        isPossible = false;
                    }
                    else
                    {
                        if (descriptor.FreeSpaceMap[roadPoint.x, roadPoint.y] != true)
                        {
                            isPossible = false;
                        }
                    }
                }

                // если получилось подобрать координаты, устанавливаем новое здание в позицию и выходим из цикла
                if (isPossible)
                {
                    // правильные координаты будут выставлены несколькими шагами дальше через localPosition
                    GameObject instance = Instantiate(toInstantiate, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;

                    // устанавливаем в родителя
                    instance.transform.SetParent(block.transform);
                    instance.transform.localPosition = new Vector3((float)(xCorToPlace + xBuildingSize / 2) * SizeOfCell,
                                                                    (float)(yCorToPlace + yBuildingSize / 2) * SizeOfCell, 0.0f);

                    // сохраняем ссылку на все сгенерированные здания
                    descriptor.Buildings.Add(instance);

                    // обновляем карту свободных клеток в блоке
                    descriptor.UpdateFreeSpaceMap(SizeOfBlocks);

                    success = true;
                }

                // уменьшаем оставшееся кол-во попыток
                attempts--;
            }
        }


        // достраиваем дороги
        RoadManager.GetInstance().BuildRoadInBlock(descriptor, x, y, SizeOfBlocks);

        // генерируем прочие декоротивные и не только объекты
        GenerateBlockEnviroment(block, descriptor, settingsForGeneration, SizeOfBlocks);

    }

    /**********************************************************************************/
    // функция выбиарет тип блока для размещения на карте
    //
    /**********************************************************************************/
    virtual protected Base.BLOCK_TYPE GetBlockTypeToGenerate()
    {
        int randomWeight = Random.Range(1, m_maxBlockGenerationWeight + 1);
        int currentBlockWeight = 0;
        Base.BLOCK_TYPE blockToInstantiate = Base.BLOCK_TYPE.NO_TYPE;

        for (int i = 0; i < m_availableBlocks.Count; i++)
        {
            blockToInstantiate = m_availableBlocks[i].BlockType;
            currentBlockWeight += m_availableBlocks[i].Weight;

            // если достигли или перешагнули значение случайного веса - выходим из цикла и используем последнее выбранное здание 
            if (currentBlockWeight >= randomWeight)
            {
                break;
            }
        }

        return blockToInstantiate;
    }


    /**********************************************************************************/
    // функция выбиарет здание(ия) для генерации на карте.
    //
    /**********************************************************************************/
    virtual protected List<GameObject> GetBuildingToInstance(BlockSettings settings)
    {
        /*****************/
        // определяем кол-во зданий, которые необходимо будет выбрать для генерации
        int numOfBuildings = 0;
        int maxGenWeight = 0;
        foreach (var numVariant in settings.NumOfBuildings)
        {
            maxGenWeight += numVariant.y;
        }

        int randomWeight = Random.Range(1, maxGenWeight + 1);
        int currentGenWeight = 0;

        for (int i = 0; i < settings.NumOfBuildings.Length; i++)
        {
            numOfBuildings = settings.NumOfBuildings[i].x;      // количество зданий для генерации
            currentGenWeight += settings.NumOfBuildings[i].y;   // веса для данного варианта кол-ва зданий

            // если достигли или перешагнули значение случайного веса - выходим из цикла и используем последнее выбранное здание 
            if (currentGenWeight >= randomWeight)
            {
                break;
            }
        }

        /*****************/
        // выбираем здания для генерации
        List<GameObject> toInstantiate = new List<GameObject>();

        // подсчитываем сумму всех весов в списке потенциальных зданий
        maxGenWeight = 0;
        foreach (var building in settings.Buildings)
        {
            maxGenWeight += building.Weight;
        }

        // собираем коллекцию префабов для дальнейшей генерации
        ObjectLibrary objLib = ObjectLibrary.GetInstance();
        for (; numOfBuildings > 0; numOfBuildings--)
        {
            randomWeight = Random.Range(1, maxGenWeight + 1);
            int currentBuildingWeight = 0;
            Base.GO_TYPE buildingType = Base.GO_TYPE.NONE_TYPE;


            for (int i = 0; i < settings.Buildings.Length; i++)
            {
                buildingType = settings.Buildings[i].BuildingType;
                currentBuildingWeight += settings.Buildings[i].Weight;

                // если достигли или перешагнули значение случайного веса - выходим из цикла и используем последнее выбранное здание 
                if (currentBuildingWeight >= randomWeight)
                {
                    break;
                }
            }

            GameObject prefab = objLib.GetPrefab(buildingType);
            toInstantiate.Add(prefab);
        }

        return toInstantiate;
    }

    /**********************************************************************************/
    // функция генерирует наполнитель для блока
    // вызывается после генерации зданий и дорог, когда ограничители по пространству уже определены
    //
    /**********************************************************************************/
    void GenerateBlockEnviroment(GameObject block, BlockDescriptor descriptor, BlockSettings settings, int SizeOfBlocks)
    {
        int numberOfGeneratedObject = settings.NumOfEnvElements;
        List<GameObject> applicableObjects = new List<GameObject>();

        foreach (GeneratedEnvironmentCtr.ENV_TYPE objType in settings.EnvTypes)
        {
            // формирование списка применимых для данного блока элементов
            foreach (GameObject generatedEnvObj in GeneratedEnvCollection)
            {
                // получаем контроллер и проверяем его тип
                GeneratedEnvironmentCtr ctr = generatedEnvObj.GetComponent<GeneratedEnvironmentCtr>();
                if (ctr == null)
                {
                    Debug.LogError("Wrong GeneratedEnvironmentCtr! is Null!");
                    return;
                }

                if (ctr.TYPE == objType)
                {
                    applicableObjects.Add(generatedEnvObj);
                }
            }
        }

        if (applicableObjects.Count == 0)
        {
            Debug.LogWarning("We have no enviroment items for: " + settings.EnvTypes.ToString() + ";  types");
            return;
        }

        // выбираем рандомные элементы из получившегося набора и используем их для заполнения блока
        // обновляем карту пространства
        for (int itemN = 0; itemN < numberOfGeneratedObject; itemN++)
        {
            GameObject envItemToInstance = applicableObjects[Random.Range(0, applicableObjects.Count)];

            bool generatingSuccess = false;
            bool weHaveFreeSpace = true;
            while (!generatingSuccess && weHaveFreeSpace)
            {

                // состовляем коллекцию свободных точек
                List<Point> freeSpacePoints = new List<Point>();
                for (int x = 0; x < SizeOfBlocks; x++)
                {
                    for (int y = 0; y < SizeOfBlocks; y++)
                    {
                        if (descriptor.FreeSpaceMap[x, y] == true)
                        {
                            freeSpacePoints.Add(new Point(x, y));
                        }
                    }
                }

                // проверяем наличие свободного пространства
                if(freeSpacePoints.Count == 0)
                {
                    weHaveFreeSpace = false;
                    continue;
                }

                // выбираем случайную точку из свободных и устанавливаем туда элемент окружения
                Point randomFreePosition = freeSpacePoints[Random.Range(0, freeSpacePoints.Count)];
                int xPos = randomFreePosition.x;
                int yPos = randomFreePosition.y;


                // правильные координаты будут выставлены несколькими шагами дальше через localPosition
                GameObject instance = Instantiate(envItemToInstance, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;

                GeneratedEnvironmentCtr gec = instance.GetComponent<GeneratedEnvironmentCtr>();

                // устанавливаем в родителя ( TODO: без учёта размера генерируемого элемента)
                instance.transform.SetParent(block.transform);
                instance.transform.localPosition = new Vector3(((float)(xPos) + ((float)gec.SIZE) / 2) * SizeOfCell,
                                                                ((float)(yPos) + ((float)gec.SIZE) / 2) * SizeOfCell, 0.0f);

                // сохраняем локальные координаты 
                gec.POSITION = new Point(xPos, yPos);

                // сохраняем ссылку на все сгенерированные элементы окружения
                descriptor.Enviroment.Add(instance);

                // обновляем карту свободных клеток в блоке
                descriptor.UpdateFreeSpaceMap(SizeOfBlocks);

                generatingSuccess = true;
            }

        }
    }


    /**********************************************************************************/
    // функция проверяет позицию для установки блока
    // возвращает true если можно установить
    //
    /**********************************************************************************/
    bool IsPositionFree(int x, int y, BlockDescriptor descriptor)
    {
        if (descriptor == null)
        {
            Debug.LogError("BlockDescriptor is NULL!");
            return false;
        }
        return IsPositionFree(x, y, descriptor.xSize, descriptor.ySize);
    }

    /**********************************************************************************/
    // функция проверяет позицию для установки блока
    // возвращает true если можно установить
    //
    /**********************************************************************************/
    bool IsPositionFree(int x, int y, int xSize, int ySize)
    {
        // проверяем размерности
        if (x + xSize * SizeOfBlocks >= m_blocksMap.GetLength(0) ||
            x + xSize * SizeOfBlocks >= MapSizeX)
        {
            return false;
        }

        if (y + ySize * SizeOfBlocks >= m_blocksMap.GetLength(1) ||
            y + ySize * SizeOfBlocks >= MapSizeY)
        {
            return false;
        }

        // проверяем на предмет коллизий с другими объектами
        for (int xAdditional = 0; xAdditional < xSize; xAdditional++)
        {
            for (int yAdditional = 0; yAdditional < ySize; yAdditional++)
            {
                if (m_blocksMap[x + xAdditional, y + yAdditional] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /**********************************************************************************/
    // функция очищает все массивы генератора
    //
    /**********************************************************************************/
    void ResetMap()
    {
        int iSize = m_blocksMap.GetLength(0);
        int zSize = m_blocksMap.GetLength(1);

        for (int i = 0; i < iSize; i++)
        {
            for (int z = 0; z < zSize; z++)
            {
                m_blocksMap[i, z] = null;
            }
        }

        // сбрасываем настройки дорог
        RoadManager.GetInstance().ResetRoadMap(MapSizeX / SizeOfBlocks, MapSizeY / SizeOfBlocks, SizeOfBlocks);
    }

    /**********************************************************************************/
    // функция ответсвенная за установку блока в определённую позицию
    // если установка возможна, возвращает true
    //
    /**********************************************************************************/
    bool PlaceBlockAtPosition(GameObject block, int x, int y)
    {
        BlockDescriptor descriptor = block.GetComponent<BlockDescriptor>();

        if (descriptor == null)
        {
            Debug.LogError("BlockDescriptor is NULL!!!");
            return false;
        }

        int xSize = descriptor.xSize;
        int ySize = descriptor.ySize;

        if (IsPositionFree(x, y, xSize, ySize))
        {
            for (int xAdditional = 0; xAdditional < xSize; xAdditional++)
            {
                for (int yAdditional = 0; yAdditional < ySize; yAdditional++)
                {
                    m_blocksMap[x + xAdditional, y + yAdditional] = descriptor;
                }
            }
            // удалось разместить - возвращаем true
            return true;
        }

        // если не удалось разместить возвращаем false
        return false;
    }

    /**********************************************************************************/
    // функция ответсвенная за генерацию земли
    //
    /**********************************************************************************/
    void GenerateGround(int xSize, int ySize)
    {

        if (GroundCollector == null)
        {
            Debug.LogError("!!! GroundCollectionObject is NULL");
            return;
        }

        for (int x = -BorderSize; x < xSize + BorderSize; x++)
        {
            for (int y = -BorderSize; y < ySize + BorderSize; y++)
            {

                // выбираем тайл земли для укладки
                GameObject toInstantiate = GroundCollection[Random.Range(0, GroundCollection.Length)];

                // создаём тайл земли
                GameObject instance = Instantiate(toInstantiate, new Vector3((float)x * SizeOfCell + SizeOfCell / 2, (float)y * SizeOfCell + SizeOfCell / 2, 0f), Quaternion.identity) as GameObject;

                // устанавливаем в родителя
                instance.transform.SetParent(GroundCollector.transform);


                // добавляем лес как ограничитель
                if (x < 0 || x >= xSize || y < 0 || y >= ySize)
                {
                    // выбираем тайл леса
                    GameObject borderToInstantiate = BordersCollection[Random.Range(0, BordersCollection.Length)];

                    // создаём тайл леса
                    GameObject borderInstance = Instantiate(borderToInstantiate, new Vector3((float)x * SizeOfCell + SizeOfCell / 2, (float)y * SizeOfCell + SizeOfCell / 2, 0f), Quaternion.identity) as GameObject;

                    // устанавливаем в родителя
                    borderInstance.transform.SetParent(GroundCollector.transform);
                }
            }
        }
    }

}
