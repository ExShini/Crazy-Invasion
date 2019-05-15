using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// GlobalTimerController
// Глобальный таймер, дергает другие объекты для усоществления периодичных действий
//
/**********************************************************************************/
public class GlobalTimerController : MonoBehaviour
{

    float m_timeCounter = 0.0f;
    float m_timerLimit = 1.0f;

    void FixedUpdate()
    {
        m_timeCounter += Time.deltaTime;
        if(m_timeCounter >= m_timerLimit)
        {
            m_timeCounter -= m_timerLimit;

            // производим периодичные вызовы
            PathFinder.GetInstance().TrafficJamTimeDegradation();
        }
    }
}
