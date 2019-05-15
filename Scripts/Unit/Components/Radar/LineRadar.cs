using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// LineRadar
// основной радар стрелков - производит поиск целей по линии направления движения юнита
// при обнаружении оповещает все заинтересованные компоненты
// не лочит цели
//
/**********************************************************************************/
class LineRadar : StalkerRadar, IRadar
{
    public new event  RadarDataEvent RadarUpdate;

    protected Unit m_unitWithRadar;
    protected int m_scaningRadius = 0;

    protected List<Point>[] m_cachePoints = new List<Point>[(int)Base.DIREC.NUM_OF_DIRECTIONS];
    protected List<CIGameObject> m_players = new List<CIGameObject>();

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public LineRadar(Unit unitWithRadar)
        : base((PLAYER)unitWithRadar.Owner)
    {
        m_unitWithRadar = unitWithRadar;
        m_targetCheckTimerLimit = 1.0f;     // для стрелковых радаров частота поиска должна быть значительно выше, чем для StalkerRadar

        // подготавливаем вспомогательные компоненты
        for (int i = 0; i < (int)Base.DIREC.NUM_OF_DIRECTIONS; i++)
        {
            m_cachePoints[i] = new List<Point>();
        }

        GameManager.GAME_MODE mode = GameManager.GetInstance().GameMode;
        if (mode == GameManager.GAME_MODE.SINGLE)
        {
            m_players.Add(GameManager.GetInstance().GetPlayer());
        }
        else if (mode == GameManager.GAME_MODE.DUEL)
        {
            m_players.Add(GameManager.GetInstance().GetPlayers(PLAYER.PL1));
            m_players.Add(GameManager.GetInstance().GetPlayers(PLAYER.PL2));
        }
    }

    /**********************************************************************************/
    // устанавливаем радиус сканирования
    //
    /**********************************************************************************/
    public virtual void SetRadarRadius(int radius)
    {
        m_scaningRadius = radius;

        /**********************************************************************************/
        // при установки значения для радиуса поиска производим обновление кеш таблиц
        // таблица содержит в себе преподготовленный список точек (со смещениями) для поиска
        /**********************************************************************************/
        for (int range = 0; range < radius; range++)
        {
            m_cachePoints[(int)Base.DIREC.DOWN].Add(new Point(0, -range));
            m_cachePoints[(int)Base.DIREC.UP].Add(new Point(0, range));
            m_cachePoints[(int)Base.DIREC.LEFT].Add(new Point(-range, 0));
            m_cachePoints[(int)Base.DIREC.RIGHT].Add(new Point(range, 0));
        }
    }

    /**********************************************************************************/
    // основная процессинговая функция радара, здесь мы обнаружаем противников
    //
    /**********************************************************************************/
    public override void Update()
    {
        UpdateTarget();
        UpdateTimers();
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
                List<CIGameObject> unitsInPoint = GameObjectMapController.GetInstance().SearchEnemiesInRadius(realPointToCheck, 0, m_owner);

                if (unitsInPoint.Count > 0)
                {
                    CIGameObject targetCtr = unitsInPoint[Random.Range(0, unitsInPoint.Count)];
                    if(RadarUpdate != null)
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
                        if (plObject.Owner == (int)m_owner)
                            continue;

                        if (plObject.GetGlobalPosition().IsSamePoint(realPointToCheck))
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

