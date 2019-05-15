using UnityEngine;

/**********************************************************************************/
// TestMapGenerarot
// тестовый класс для MapGenerarot
//
/**********************************************************************************/
public class TestMapGenerarot : MapGenerator, TestInterface
{
    public string RunTest()
    {
        string report = string.Empty;
        //bool testRes = false;



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
    // TestGenerateBlockEnviroment_1
    // тест для GenerateBlockEnviroment функции
    //
    /**********************************************************************************/
    string TestGenerateBlockEnviroment_1(ref bool result)
    {
        string Name = "ResetRoadMap 1";


        return Name;
    }
}