using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// LineRadarBuilding
// основной радар существ, работающих с постройками
//
/**********************************************************************************/
class LineRadarBuilding : LineRadar, IRadar
{
    public new event RadarDataEvent RadarUpdate;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public LineRadarBuilding(Unit unitWithRadar)
        : base(unitWithRadar)
    {
    }

    /**********************************************************************************/
    // производим поиск цели
    // в случае обнаружения, функция сгенерирует
    //
    /**********************************************************************************/
    protected override void UpdateTarget()
    {
        // если ещё не начали никуда двигаться, стрелять тоже не будем
        // защита от дурака
        Base.DIREC direction = m_unitWithRadar.MoveDirection;
        if (direction == Base.DIREC.NO_DIRECTION)
        {
            m_targetCheckTimer += m_targetCheckTimerLimit;
            return;
        }

        // получаем список впомогательных точек для поиска
        List<Point> pointsToCheck = m_cachePoints[(int)direction];
        foreach (Point specPoint in pointsToCheck)
        {
            Point realPointToCheck = specPoint + m_currentPosition;
            List<BuildingController> buildingsInPoint = GameObjectMapController.GetInstance().SearchEnemiesBuildingInRadius(realPointToCheck, 0, m_owner);

            if (buildingsInPoint.Count > 0)
            {
                BuildingController targetCtr = buildingsInPoint[Random.Range(0, buildingsInPoint.Count)];
                if (RadarUpdate != null)
                {
                    RadarData data = new RadarData();
                    data.EnemyDirection.Add(direction);
                    data.DetectedEnemy.Add(targetCtr.gameObject);
                    RadarUpdate(data);
                }
            }

            m_targetCheckTimer += m_targetCheckTimerLimit;
        }
    }
}

