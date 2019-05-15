using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentacleCtr : BulletRotated
{
    public float FlyTime = 2.0f;
    protected float m_currentFlyTime = 0.0f;

    protected List<float> m_speedHash = new List<float>();

    protected float m_timeStep;
    protected float m_numOfSpeedHeshes = 10.0f;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    new public void Start()
    {
        base.Start();

        // рассчитываем полётную скорость
        // формула скорости  =  -a*t^2 + V
        m_originalSpeed = speed;
        float aK = m_originalSpeed / (FlyTime * FlyTime);
        m_timeStep = FlyTime / m_numOfSpeedHeshes;

        for (int i = 0; i < m_numOfSpeedHeshes; i++)
        {
            float SpeedValue = -aK * Mathf.Pow(m_timeStep * (float)i, 2) + m_originalSpeed;
            m_speedHash.Add(SpeedValue);
        }
    }


    /**********************************************************************************/
    // процессинг
    //
    /**********************************************************************************/
    protected override void FixedUpdate()
    {
        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        // манипулируем скоростью
        SpeedManipualation();

        // вызываем родительсикий метод
        base.FixedUpdate();
    }

    /**********************************************************************************/
    // функция контроля скорости
    // снаряд в процессе полёта замедляется
    //
    /**********************************************************************************/
    private void SpeedManipualation()
    {
        if (m_state != BULLET_STATE.BURN)
        {
            m_currentFlyTime += Time.deltaTime;
            int currentStep = (int)Mathf.Floor(m_currentFlyTime / m_timeStep);
            if (currentStep >= m_numOfSpeedHeshes)
            {
                m_state = BULLET_STATE.BURN;
                speed = 0.0f;
                m_animator.SetBool("Burn", true);
                return;
            }
            speed = m_speedHash[currentStep];
        }

    }


    /**********************************************************************************/
    // сброс настроек на дефолтные
    //
    /**********************************************************************************/
    public override void ResetGObject()
    {
        base.ResetGObject();
        m_currentFlyTime = 0.0f;
    }
}
