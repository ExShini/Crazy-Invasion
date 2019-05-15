using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// GrenadeRadar
// специальный радар предназначенный для использования в паре с гранатным (или схожим с ним) вооружением
// производит сканирование в точке удалённой от юнита на m_scaningRadius клеток по напралению движения и 
//
/**********************************************************************************/
class GrenadeRadar : LineRadar, IRadar
{
    public new event RadarDataEvent RadarUpdate;

    private bool m_radiusIsSet = false;
    private bool m_areaIsSet = false;

    protected int m_scaningAreaRadius;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public GrenadeRadar(Unit unitWithRadar) :
        base(unitWithRadar)
    {
    }

    /**********************************************************************************/
    // устанавливаем радиус сканирования
    //
    /**********************************************************************************/
    public override void SetRadarRadius(int radius)
    {
        base.SetRadarRadius(radius);
        m_radiusIsSet = true;
        BuildCachTable();
    }

    /**********************************************************************************/
    // устанавливаем зону сканирования
    //
    /**********************************************************************************/
    public virtual void SetScaningArea(int areaRadius)
    {
        m_scaningAreaRadius = areaRadius;
        m_areaIsSet = true;
    }

    /**********************************************************************************/
    // определяем точки для сканирования
    //
    /**********************************************************************************/
    protected virtual void BuildCachTable()
    {
        // таблица содержит в себе преподготовленный список точек (со смещениями) для поиска
        m_cachePoints[(int)Base.DIREC.DOWN].Add(new Point(0, -m_scaningRadius));
        m_cachePoints[(int)Base.DIREC.UP].Add(new Point(0, m_scaningRadius));
        m_cachePoints[(int)Base.DIREC.LEFT].Add(new Point(-m_scaningRadius, 0));
        m_cachePoints[(int)Base.DIREC.RIGHT].Add(new Point(m_scaningRadius, 0));
    }

    /**********************************************************************************/
    // производим поиск цели
    // в случае обнаружения произвойдёт RadarUpdate эвент
    //
    /**********************************************************************************/
    protected override void UpdateTarget()
    {
        if (m_targetCheckTimer <= 0)
        {
            // если ещё не начали никуда двигаться, стрелять тоже не будем
            // защита от дурака
            Base.DIREC direction = m_unitWithRadar.MoveDirection;
            if (direction == Base.DIREC.NO_DIRECTION)
            {
                m_targetCheckTimer += m_targetCheckTimerLimit;
                return;
            }

            // получаем список впомогательных точек для поиска врагов
            // и проводим поиск целей для стрельбы
            List<Point> pointsToCheck = m_cachePoints[(int)direction];
            foreach (Point specPoint in pointsToCheck)
            {
                Point realPointToCheck = specPoint + m_currentPosition;
                List<CIGameObject> unitsInPoint = GameObjectMapController.GetInstance().SearchEnemiesInRadius(realPointToCheck, m_scaningAreaRadius, m_owner);

                if (unitsInPoint.Count > 0)
                {
                    CIGameObject targetCtr = unitsInPoint[Random.Range(0, unitsInPoint.Count)];
                    if (RadarUpdate != null)
                    {
                        RadarData data = new RadarData();
                        data.EnemyDirection.Add(direction);
                        data.DetectedEnemy.Add(targetCtr.gameObject);
                        RadarUpdate(data);
                    }
                }
                else
                {
                    // проверяем игроков
                    foreach (CIGameObject plObject in m_players)
                    {
                        // считаем расстояние от точки метания снаряда до игроков
                        // если расстояние меньше m_scaningAreaRadius - вызываем RadarUpdate эвент
                        Point playerPosition = plObject.GetGlobalPosition();
                        Point positionDiff = playerPosition - realPointToCheck;

                        if(positionDiff.GetSimpleLength() < m_scaningAreaRadius)
                        {
                            RadarData data = new RadarData();
                            data.EnemyDirection.Add(direction);
                            data.DetectedEnemy.Add(plObject.gameObject);
                            RadarUpdate(data);
                        }
                    }
                }
            }

            m_targetCheckTimer += m_targetCheckTimerLimit;
        }
    }
}
