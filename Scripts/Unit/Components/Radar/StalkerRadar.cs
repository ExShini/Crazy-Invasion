using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// StalkerRadar
// базовый радар, в качестве цели выбирается основная цель партии - один из игроков(противник) в случае пвп, или одна из целей миссии в случае пве режима
//
/**********************************************************************************/

class StalkerRadar : IRadar
{
    public event RadarDataEvent RadarUpdate;
    public event GOEvent TargetToMove;

    // таймера
    protected float m_targetCheckTimer = 0.0f;
    protected float m_targetCheckTimerLimit = 4.0f;

    protected Point m_currentPosition;
    protected GameObject m_locedTarget = null;
    protected PLAYER m_owner;


    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public StalkerRadar(PLAYER owner)
    {
        m_owner = owner;
    }

    /**********************************************************************************/
    // определяем частоту поиска цели
    //
    /**********************************************************************************/
    public void SetTargetCheckRate(float seconds)
    {
        m_targetCheckTimerLimit = seconds;
    }

    /**********************************************************************************/
    // сеттер
    //
    /**********************************************************************************/
    public void SetOwner(PLAYER owner)
    {
        m_owner = owner;
    }

    /**********************************************************************************/
    // основная процессинговая функция радара, здесь мы обнаружаем противников
    //
    /**********************************************************************************/
    public virtual void Update()
    {
        // проверяем состояние нашей цели
        CheckForTargetState();
        // обновляем таймера
        UpdateTimers();
    }

    /**********************************************************************************/
    // обновляем таймера
    //
    /**********************************************************************************/
    protected virtual void UpdateTimers()
    {
        m_targetCheckTimer -= Time.deltaTime;
    }

    /**********************************************************************************/
    // проверяем состояние цели
    // если цель оказывается мёртвой - выбираем новую
    //
    /**********************************************************************************/
    public virtual void CheckForTargetState()
    {
        if (m_targetCheckTimer <= 0)
        {
            if (m_locedTarget == null)
            {
                UpdateTarget();
                return;
            }

            if (m_locedTarget.tag != "Player")
            {
                Unit controller = m_locedTarget.GetComponent<Unit>();
                if (controller == null)
                {
                    Debug.LogError("Target controller is null!");
                }

                if (controller.State != Unit.UNIT_STATE.ACTIVE)
                {
                    UpdateTarget();
                }
            }

            m_targetCheckTimer += m_targetCheckTimerLimit;
        }
    }


    /**********************************************************************************/
    // обновляем цель
    //
    /**********************************************************************************/
    protected virtual void UpdateTarget()
    {
        m_locedTarget = TargetController.GetInstance().GetTarget(m_owner, m_currentPosition);
        TargetToMove(m_locedTarget);
    }

    /**********************************************************************************/
    // обновляем позицию для сканирования
    //
    /**********************************************************************************/
    public void PositionUpdate(Point position)
    {
        m_currentPosition = position;
    }

    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    public void ResetComponent()
    {
        m_locedTarget = null;
        m_currentPosition = new Point();
        m_targetCheckTimer = 0.0f;
    }
}

