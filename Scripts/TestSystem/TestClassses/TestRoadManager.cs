using UnityEngine;
using System.Collections;

/**********************************************************************************/
// TestRoadManager
// тестовый класс для RoadManager
//
/**********************************************************************************/
public class TestRoadManager : RoadManager, TestInterface
{
    public string RunTest()
    {
        string report = string.Empty;
        bool testRes = false;

        // ResetRoadMap
        BaseTest.CheckTest(ref testRes, Test1_1(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test1_2(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test1_3(ref testRes), ref report);

        // CheckRoadRules
        BaseTest.CheckTest(ref testRes, Test2_1(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test2_2(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test2_3(ref testRes), ref report);

        // BuildRoadRules
        BaseTest.CheckTest(ref testRes, Test3_1(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test3_2(ref testRes), ref report);
        BaseTest.CheckTest(ref testRes, Test3_3(ref testRes), ref report);

        return report;
    }

    /**********************************************************************************/
    // имя тест съюта
    //
    /**********************************************************************************/
    public string TestSuiteName()
    {
        return "TestRoadManager";
    }

    /**********************************************************************************/
    // Test1_1
    // тест для ResetRoadMap функции
    //
    /**********************************************************************************/
    string Test1_1(ref bool result)
    {
        string Name = "ResetRoadMap 1";
        result = true;
        int xSize = 0;
        int ySize = 0;
        int SizeOfBlock = 5;

        // тест
        xSize = 3;
        ySize = 3;
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);
        // проверяем размерность блока
        result = result && (m_blockRoadRulesDescriptor.GetLength(0) == xSize);
        result = result && (m_blockRoadRulesDescriptor.GetLength(1) == ySize);
        result = result && SubTest1_1(xSize, ySize);

        return Name;
    }


    /**********************************************************************************/
    // Test1_2
    // тест для ResetRoadMap функции
    //
    /**********************************************************************************/
    string Test1_2(ref bool result)
    {
        string Name = "ResetRoadMap 2";
        result = true;
        int xSize = 0;
        int ySize = 0;
        int SizeOfBlock = 5;

        // тест
        xSize = 4;
        ySize = 6;
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);
        // проверяем размерность блока
        result = result && (m_blockRoadRulesDescriptor.GetLength(0) == xSize);
        result = result && (m_blockRoadRulesDescriptor.GetLength(1) == ySize);
        result = result && SubTest1_1(xSize, ySize);


        return Name;
    }


    /**********************************************************************************/
    // Test1_3
    // тест для ResetRoadMap функции
    //
    /**********************************************************************************/
    string Test1_3(ref bool result)
    {
        string Name = "ResetRoadMap 3";
        result = true;
        int xSize = 0;
        int ySize = 0;
        int SizeOfBlock = 5;

        // тест
        xSize = 6;
        ySize = 9;
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);
        // проверяем размерность блока
        result = result && (m_blockRoadRulesDescriptor.GetLength(0) == xSize);
        result = result && (m_blockRoadRulesDescriptor.GetLength(1) == ySize);
        result = result && SubTest1_1(xSize, ySize);


        return Name;
    }

    // сабтест проверяющий границы генерируемого поля
    bool SubTest1_1(int xSize, int ySize)
    {
        bool result = true;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                // проверяем границы по Х
                if (x == 0)
                {
                    ROAD_CONNECTION_STATUS status = m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.LEFT];
                    if (status != ROAD_CONNECTION_STATUS.BLOCKED)
                    {
                        result = false;
                    }
                }
                else if (x == xSize - 1)
                {
                    ROAD_CONNECTION_STATUS status = m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT];
                    if (status != ROAD_CONNECTION_STATUS.BLOCKED)
                    {
                        result = false;
                    }
                }


                // проверяем границы по У
                if (y == 0)
                {
                    ROAD_CONNECTION_STATUS status = m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.DOWN];
                    if (status != ROAD_CONNECTION_STATUS.BLOCKED)
                    {
                        result = false;
                    }
                }
                else if (y == ySize - 1)
                {
                    ROAD_CONNECTION_STATUS status = m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.UP];
                    if (status != ROAD_CONNECTION_STATUS.BLOCKED)
                    {
                        result = false;
                    }
                }
            }
        }


        return result;
    }


    /**********************************************************************************/
    // Test2_1
    // тест для CheckRoadRules функции
    //
    /**********************************************************************************/
    string Test2_1(ref bool result)
    {
        string Name = "CheckRoadRules 1";

        // тест
        int xSize = 3;
        int ySize = 3;
        int SizeOfBlock = 5;

        // подготавливаем структуры для теста
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);

        // устанавливаем тестовую модель
        int x = xSize / 2;
        for (int y = 0; y < ySize; y++)
        {
            m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.BLOCKED;
            m_blockRoadRulesDescriptor[x + 1, y].RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.BLOCKED;
        }

        // негативный тест
        bool checkingRes = this.CheckRoadRules(xSize, ySize);

        result = checkingRes == false;

        return Name;
    }

    /**********************************************************************************/
    // Test2_2
    // тест для CheckRoadRules функции
    //
    /**********************************************************************************/
    string Test2_2(ref bool result)
    {
        string Name = "CheckRoadRules 2";

        // тест
        int xSize = 6;
        int ySize = 8;
        int SizeOfBlock = 5;

        // подготавливаем структуры для теста
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);

        // устанавливаем тестовую модель
        int x = xSize / 2;
        for (int y = 0; y < ySize; y++)
        {
            m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.BLOCKED;
            m_blockRoadRulesDescriptor[x + 1, y].RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.BLOCKED;
        }

        // негативный тест
        bool checkingRes = this.CheckRoadRules(xSize, ySize);

        result = checkingRes == false;

        return Name;
    }


    /**********************************************************************************/
    // Test2_3
    // тест для CheckRoadRules функции
    //
    /**********************************************************************************/
    string Test2_3(ref bool result)
    {
        string Name = "CheckRoadRules 3";

        // тест
        int xSize = 9;
        int ySize = 8;
        int SizeOfBlock = 5;

        // подготавливаем структуры для теста
        this.ResetRoadMap(xSize, ySize, SizeOfBlock);

        // устанавливаем тестовую модель
        int x = xSize / 2;
        for (int y = 0; y < ySize; y++)
        {
            m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.BLOCKED;
            m_blockRoadRulesDescriptor[x + 1, y].RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.BLOCKED;
        }

        // позитивный тест
        bool checkingRes = this.CheckRoadRules(xSize, ySize);

        result = checkingRes == false;


        for (int y = 0; y < ySize; y++)
        {
            m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] = ROAD_CONNECTION_STATUS.POSSIBLE;
            m_blockRoadRulesDescriptor[x + 1, y].RoadConnections[(int)Base.DIREC.LEFT] = ROAD_CONNECTION_STATUS.POSSIBLE;
        }

        checkingRes = this.CheckRoadRules(xSize, ySize);
        result &= checkingRes == true;

        return Name;
    }


    /**********************************************************************************/
    // Test3_1
    // тест для BuildRoadRules функции
    //
    /**********************************************************************************/
    string Test3_1(ref bool result)
    {
        string Name = "BuildRoadRules 1";

        // тест
        int xSize = 11;
        int ySize = 11;
        m_blockRoadRulesDescriptor = new BlockDescriptorImitation[xSize, ySize];

        // подготавливаем структуры для теста
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

                this.m_blockRoadRulesDescriptor[x, y] = bd;
            }
        }

        this.BuildRoadRules(xSize, ySize, 0.0f);

        // проверяем, что ни одно правило в центральной части карты не было установлено
        bool isFine = true;
        for (int x = 1; x < xSize - 1; x++)
        {
            for (int y = 1; y < ySize - 1; y++)
            {
                if (this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.DOWN] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.LEFT] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.UP] == ROAD_CONNECTION_STATUS.NEEDED
                                        )
                {
                    isFine = false;
                }

            }
        }

        result = isFine;

        return Name;
    }



    /**********************************************************************************/
    // Test3_2
    // тест для BuildRoadRules функции
    //
    /**********************************************************************************/
    string Test3_2(ref bool result)
    {
        string Name = "BuildRoadRules 2";

        // тест
        int xSize = 11;
        int ySize = 11;
        m_blockRoadRulesDescriptor = new BlockDescriptorImitation[xSize, ySize];

        // подготавливаем структуры для теста
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

                this.m_blockRoadRulesDescriptor[x, y] = bd;
            }
        }

        this.BuildRoadRules(xSize, ySize, 1.0f);

        // проверяем, что все до одного правила в центральной части карты были установлены
        bool isFine = true;
        for (int x = 1; x < xSize - 1; x++)
        {
            for (int y = 1; y < ySize - 1; y++)
            {
                if (this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.DOWN] != ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.LEFT] != ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] != ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.UP] != ROAD_CONNECTION_STATUS.NEEDED
                                        )
                {
                    isFine = false;
                }

            }
        }

        result = isFine;

        return Name;
    }



    /**********************************************************************************/
    // Test3_3
    // тест для BuildRoadRules функции
    //
    /**********************************************************************************/
    string Test3_3(ref bool result)
    {
        string Name = "BuildRoadRules 3";

        // тест
        int xSize = 9;
        int ySize = 9;
        m_blockRoadRulesDescriptor = new BlockDescriptorImitation[xSize, ySize];

        // подготавливаем структуры для теста
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

                this.m_blockRoadRulesDescriptor[x, y] = bd;
            }
        }

        this.BuildRoadRules(xSize, ySize);

        // проверяем, что при стандартных настройках мы вносим какое-то кол-во рандома в карту
        // для успешного прохождения теста достаточно всего одного коннекшена
        bool isFine = false;
        for (int x = 1; x < xSize - 1; x++)
        {
            for (int y = 1; y < ySize - 1; y++)
            {
                if (this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.DOWN] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.LEFT] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.RIGHT] == ROAD_CONNECTION_STATUS.NEEDED ||
                    this.m_blockRoadRulesDescriptor[x, y].RoadConnections[(int)Base.DIREC.UP] == ROAD_CONNECTION_STATUS.NEEDED
                                        )
                {
                    isFine = true;
                }

            }
        }

        result = isFine;

        return Name;
    }
}
