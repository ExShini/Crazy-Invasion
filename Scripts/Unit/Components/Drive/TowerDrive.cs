using UnityEngine;

/**********************************************************************************/
// TowerDrive класс
// движетель - используется с объектами у которых нет механики перемещения, но которым необходимо
// иметь представление о своей позиции
// требует, чтобы у объекта имелось Rigidbody2D
//
/**********************************************************************************/
class TowerDrive : EmptyDrive, IDrive
{
    public new event PositionUpdateEvent PositionUpdate;

    protected bool m_firstIteration = true;
    protected Unit m_unitToStay;
    protected float m_switchDirectionTimerLimit = 1.0f;
    protected float m_currentSwitchDirectionTimer = 0.0f;
    protected bool m_switchinIsActive = true;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public TowerDrive(Unit unitToStay)
    {
        m_unitToStay = unitToStay;
    }

    /**********************************************************************************/
    // активирует функционал башни, приводящий к вращению
    //
    /**********************************************************************************/
    public void ActivateSwitching()
    {
        m_switchinIsActive = true;
    }

    /**********************************************************************************/
    // деактивирует функционал башни, приводящий к вращению
    //
    /**********************************************************************************/
    public void DisableSwitching()
    {
        m_switchinIsActive = true;
    }

    /**********************************************************************************/
    // сеттер устанавливающий частоту разворота корпуса/направления осмотра башни
    //
    /**********************************************************************************/
    public void SetSwitchDirectionTimer(float timerLimit)
    {
        m_switchDirectionTimerLimit = timerLimit;
    }

    /**********************************************************************************/
    // при ресете сбрасываем флаг первой итерации в true, так как это может понадобиться
    // в случае реиспользовании объекта
    //
    /**********************************************************************************/
    public override void ResetComponent()
    {
        m_currentSwitchDirectionTimer = 0.0f;
        m_firstIteration = true;
    }

    /**********************************************************************************/
    // процессинговая функция
    // сообщаем нашему объекту где мы находимся, но только на первой итерации
    //
    /**********************************************************************************/
    public override void Update()
    {
        if (m_firstIteration == true)
        {
            if (PositionUpdate != null)
            {
                Point position = m_unitToStay.GetGlobalPosition();
                PositionUpdate(position);
                GameObjectMapController.GetInstance().UpdateUnitPosition(m_unitToStay);
                m_firstIteration = false;
            }
        }

        if(m_switchinIsActive)
        {
            SwitchDirection();
        }

        UpdateTimers();
    }
    
    /**********************************************************************************/
    // вращаем "турель"
    //
    /**********************************************************************************/
    public void SwitchDirection()
    {
        if(m_currentSwitchDirectionTimer <= 0.0f)
        {
            Base.DIREC direction = Base.GetRandomDirection();
            m_unitToStay.SetMovementDirection(direction);
            m_currentSwitchDirectionTimer += m_switchDirectionTimerLimit;
        }
    }

    /**********************************************************************************/
    // обновляем таймера
    //
    /**********************************************************************************/
    protected virtual void UpdateTimers()
    {
        m_currentSwitchDirectionTimer -= Time.deltaTime;
    }
}
