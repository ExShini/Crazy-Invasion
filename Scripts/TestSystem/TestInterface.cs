using UnityEngine;
using System.Collections;

/**********************************************************************************/
// TestInterface интерфейс всех тестовых классов
//
/**********************************************************************************/
public interface TestInterface
{
    string RunTest();
    string TestSuiteName();
}

public class BaseTest
{
    public static void CheckTest(ref bool testResult, string TestName, ref string report)
    {
        if(testResult)
        {
            report += "\n" + "Test: " + TestName + " is OK";
        }
        else
        {
            report += "\n" + "Test: " + TestName + " is FAILED!";
            Debug.LogError("Test: " + TestName + " is FAILED!");
        }
        
    }
}