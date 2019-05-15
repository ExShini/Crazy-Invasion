/**********************************************************************************/
// контроллер строительной капсулы
//
/**********************************************************************************/
using UnityEngine;

class BuildingCapsuleCtr : Bullet
{
    public Base.GO_TYPE ObjectToBuild = Base.GO_TYPE.NONE_TYPE;
    public float StartDelay = 0.5f;
    public float MaxFlyTime = 5.0f;
    protected float m_currentTime = 0.0f;
    protected float m_checkingRate = 0.2f;
    protected float m_checkingTimer = 0.0f;

    /**********************************************************************************/
    // процессинг
    // добавляем проверку возможности строительства
    //
    /**********************************************************************************/
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (m_state == BULLET_STATE.FLY)
        {
            CheckForBuilderState();
            UpdateTimers();
        }
    }

    /**********************************************************************************/
    // проверяем на возможность строительства
    //
    /**********************************************************************************/
    protected void CheckForBuilderState()
    {
        if(m_currentTime >= StartDelay)
        {
            // если исчерпали время полёта - отключаемся 
            if (m_currentTime >= MaxFlyTime)
            {
                BurnBullet();
                return;
            }

            // проверяем информацию о данной точке
            // точка подходит для строительства в том случае если она свободна для перемещения, и
            // в этой позиции отсутвуют дороги
            // таким образом мы гарантируем отсутствие блокировок путей и возможность их построения юнитами
            Point position = GetGlobalPosition();
            bool couldWeBuildHere = true;
            couldWeBuildHere &= !RoadManager.GetInstance().IsRoadHere(position);
            couldWeBuildHere &= PathFinder.GetInstance().ValidatePathCell(position);

            if(couldWeBuildHere)
            {
                BuildObject();
            }
        }
        
    }

    /**********************************************************************************/
    // строим объект
    //
    /**********************************************************************************/
    protected void BuildObject()
    {
        Vector2 positionToBuild = GetGlobalPositionCenter_Unity();
        GameObject production = ObjectFactory.GetInstance().CreateGObject(positionToBuild, Base.DIREC.DOWN, ObjectToBuild, false);

        // бонусы - недвижимые объекты, потому CIObject не имеют
        if (production.tag != "Bonus")
        {
            CIGameObject gmo = production.GetComponent<CIGameObject>();
            gmo.Owner = Owner;
        }

        // отключаемся после строительства
        BurnBullet();
    }

    /**********************************************************************************/
    // обновляем таймера
    //
    /**********************************************************************************/
    protected void UpdateTimers()
    {
        m_checkingTimer += Time.deltaTime;
        m_currentTime += Time.deltaTime;
    }

    /**********************************************************************************/
    // функция проверки столкновения
    // строительная капсула игнорирует данные события
    //
    /**********************************************************************************/
    protected override void CheckBulletBurn(Collision2D coll)
    {
        // do nothing
    }


    /**********************************************************************************/
    // функция сброски параметров к дефолтным
    //
    /**********************************************************************************/
    public override void ResetGObject()
    {
        m_checkingTimer = 0.0f;
        m_currentTime = 0.0f;
        base.ResetGObject();
    }

}

