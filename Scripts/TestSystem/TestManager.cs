﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// TestManager класс
// менеджер юнит тестов
// вспомогательный класс для тестирования игровой системы
//
/**********************************************************************************/
public class TestManager : MonoBehaviour {

    string m_report = "";

    /**********************************************************************************/
    // запускаем все тесты на старте
    //
    /**********************************************************************************/
    void Start () {
        RunTests();
    }


    /**********************************************************************************/
    // основная тестовая функция
    // запускаем все тесты
    //
    /**********************************************************************************/
    void RunTests()
    {
        Debug.Log("******************************************************************************************************\n *****  TEST SYSTEM START *****");


        TestRoadManager testRoadManager = new TestRoadManager();
        RunTest(testRoadManager);

        //TestMapGenerarot testMapGenerator = new TestMapGenerarot();
        //RunTest(testMapGenerator);

        Debug.Log("REPORT:\n" + m_report);
        Debug.Log("\n*****  TEST SYSTEM FINISH *****\n******************************************************************************************************");
    }


    /**********************************************************************************/
    // запускаем конкретный тест
    //
    /**********************************************************************************/
    void RunTest(TestInterface test)
    {
        m_report += "Test " + test.TestSuiteName() + " started";
        m_report += test.RunTest();
        m_report += "\n ***** \n";
    }
}
