using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// BaseDrive
// базовый класс определяющий алгоритмы перемещения юнита
//
/**********************************************************************************/
public class BaseDrive : IDrive
{
    public event PositionUpdateEvent PositionUpdate;

    protected Unit m_unitToDrive;

    protected GameObject m_target = null;
    protected bool m_driveIsPaused = false;

    // компоненты использующиеся при построении путей
    protected float m_pathUpdatingTimeCounter = 0.0f;
    protected float m_rateOfPathUpdating = 4.0f;
    protected LinkedList<Point> m_path;

    // блок данных необходимый для детектирования ситуации "пробки"
    private Vector2 m_priviusTurnPosition = new Vector2(-1f, -1f);
    private int m_trafficJamCounter = 0;
    private float m_originalRadius = 0.16f;
    private bool m_sizeWasChanged = false;

    private enum TRAFFIC_JAM_STATE : int
    {
        NO_JAM = 0,
        TRAFFIC_JAM = 5,
        CRITICAL_JAM = 10,
        SUPER_JAM = 15,
        SUPER_CRITICAL_JAM = 30
    }

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public BaseDrive(Unit unitToDrive)
    {
        m_unitToDrive = unitToDrive;
    }

    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    public virtual void ResetComponent()
    {
        m_target = null;
        m_driveIsPaused = false;
    }

    /**********************************************************************************/
    // функция устанавливает цель для приследования
    //
    /**********************************************************************************/
    public virtual void SetTargetToMove(GameObject target)
    {
        m_target = target;
    }

    /**********************************************************************************/
    // функция возвращающая возможность двигаться 
    //
    /**********************************************************************************/
    public void StartMoving()
    {
        m_driveIsPaused = false;
    }

    /**********************************************************************************/
    // функция остановки движения, применяется если необходимо остановиться, к примеру
    // во время стрельбы
    //
    /**********************************************************************************/
    public void StopMoving()
    {
        m_driveIsPaused = true;
    }

    /**********************************************************************************/
    // основная процессинговая функция движетеля, здесь мы перемещаем юнита
    //
    /**********************************************************************************/
    public virtual void Update()
    {
        if (!m_driveIsPaused)
        {
            MoveToGObject(m_target);
            UpdateGamePosition();
        }

        UpdateTimers();
    }

    /**********************************************************************************/
    // функция обновления игровой позиции юнита
    // если координата изменилась - сообщаем об этом всем заинтересованным
    //
    /**********************************************************************************/
    protected void UpdateGamePosition()
    {
        // обновляем координаты в UnitMapController-е
        bool positionWasUpdated = GameObjectMapController.GetInstance().UpdateUnitPosition(m_unitToDrive);

        // если координата изменилась - оповещаем всех заинтересованных
        if (positionWasUpdated && PositionUpdate != null)
        {
            Point position = m_unitToDrive.GetGlobalPosition();
            PositionUpdate(position);
        }
    }

    /**********************************************************************************/
    // функция обновления таймеров
    //
    /**********************************************************************************/
    protected virtual void UpdateTimers()
    {
        // таймер обновления пути
        m_pathUpdatingTimeCounter -= Time.deltaTime;
    }


    /**********************************************************************************/
    // основная функция движения к заданному объекту
    //
    /**********************************************************************************/
    protected virtual void MoveToGObject(GameObject target)
    {
        // проверяем цель, если целей больше нет - останавливаемся
        if (target == null)
        {
            return;
        }

        // получаем и проверяем контроллер цели
        CIGameObject gmoTarget = target.GetComponent<CIGameObject>();
        if (gmoTarget == null)
        {
            Debug.LogError("Wrong target! GMovingObject is NULL!");
            return;
        }

        // двигаемся к намеченной цели
        Point pointToAchive = gmoTarget.GetGlobalPosition();
        MoveToPoint(pointToAchive);
    }

    /**********************************************************************************/
    // основная функция движения к заданной точке
    //
    /**********************************************************************************/
    protected virtual void MoveToPoint(Point pointToAchive)
    {
        // обновляем путь до цели, если таймер истёк
        if (m_pathUpdatingTimeCounter <= 0.0f)
        {
            BuildPath(pointToAchive);
        }
        // в противном случае двигаемся по ранее намеченному маршруту
        else
        {
            // проверка
            if (m_path == null)
            {
                Debug.LogError("m_path is null");
                return;
            }

            // если нам некуда больше идти - ищём новый путь (сбрасываем таймер, новый путь будет построен на сл. цикле)
            if (m_path.Count == 0)
            {
                m_pathUpdatingTimeCounter = 0.0f;
                return;
            }

            Point poinToMove = m_path.Last.Value;
            bool pointAlreadyAchived = m_unitToDrive.MoveGObjectToPoint(poinToMove);

            // если точка уже достигнута - выбираем следующую и идём к ней
            if (pointAlreadyAchived)
            {
                // при этом не забываем уменьшить велечину загруженности для точки, которую планируем покинуть
                PathFinder.GetInstance().TrafficJamDegradation(poinToMove);
                m_path.RemoveLast();
                if (m_path.Count == 0)
                {
                    m_pathUpdatingTimeCounter = 0.0f;
                    return;
                }

                poinToMove = m_path.Last.Value;
                m_unitToDrive.MoveGObjectToPoint(poinToMove);
            }

            // проверяем, не попали ли мы в "транспортную пробку"
            ChecForTrafficJam();
        }
    }

    /**********************************************************************************/
    // обновляем/строим путь из точки где стоит наш юнит в целевую
    //
    /**********************************************************************************/
    private void BuildPath(Point pointToAchive)
    {
        // строим путь до цели
        Point unitPoint = m_unitToDrive.GetGlobalPosition();
        m_path = PathFinder.GetInstance().GetWay(unitPoint, pointToAchive);

        m_pathUpdatingTimeCounter = m_rateOfPathUpdating;
    }

    /**********************************************************************************/
    // функция проверяющая на состояние транспортной пробки
    // и пробующая разрешить это состояние
    //
    /**********************************************************************************/
    protected virtual void ChecForTrafficJam()
    {
        // проверяем, не попали ли мы в "транспортную пробку"
        Vector2 currentPosition = m_unitToDrive.GetGlobalPosition_Unity();
        // делаем предположение, что объект должен проходить хотя бы 80% от ожидаемой скорости
        float expectedPositionDiff = m_unitToDrive.speed * Time.deltaTime * 0.8f;
        float positionDiff = (currentPosition - m_priviusTurnPosition).magnitude;

        if (positionDiff < expectedPositionDiff)
        {
            m_trafficJamCounter++;
            if (m_trafficJamCounter >= (int)TRAFFIC_JAM_STATE.TRAFFIC_JAM)
            {
                // ********************* //
                // ! NOTE: порядок проверок в обратном порядке (оптимизация?) - от большого к малому

                // если пробка серьёзная - извещаем об этом Path Finder
                // но перед этим обновляем путь (через сброс m_pathUpdatingTimeCounter) и сбрасываем m_trafficJamCounter во избежание излишнего зацикливания
                if (m_trafficJamCounter >= (int)TRAFFIC_JAM_STATE.SUPER_CRITICAL_JAM)
                {
                    m_pathUpdatingTimeCounter = 0;
                    m_trafficJamCounter -= 10;  // уменьшаем счётчик, чтобы не спамить в PathFinder
                    PathFinder.GetInstance().TrafficJamNotification( m_unitToDrive.GetGlobalPosition());
                }

                // если всё совсем плохо - играемся с физическим размером объекта
                else if (m_trafficJamCounter >= (int)TRAFFIC_JAM_STATE.SUPER_JAM)
                {
                    m_sizeWasChanged = true;
                    float currentRadius = m_unitToDrive.GetPhysicalRadius();
                    if (currentRadius > m_originalRadius / 3.0f)
                    {
                        m_unitToDrive.SetPhysicalRadius(currentRadius - 0.1f);
                        Debug.Log("Unit Size change to:" + (currentRadius - 0.1f));
                    }
                }

                // проверяем как долго мы стоим в пробке
                // если пробка затянулась - пробуем отойти в сторону
                else if (m_trafficJamCounter >= (int)TRAFFIC_JAM_STATE.CRITICAL_JAM)
                {

                    // если попали в пробку, то пробуем отойти в случайном направлении
                    // сбрасываем старый путь m_path, но запоминаем точку, которую пытались достигнуть
                    Point lastpPointToGo = m_path.Last.Value;
                    m_path.Clear();

                    // в качестве точки назначения будем использовать одну из соседних точек
                    // выбираем случайное направление, за исключением того, в котором пытались двигать до этого
                    // для этого получаем точку в которой мы находимся сейчас и модифицируем её
                    // ps: так же проверяем клетку на доступность в целом для перемещений


                    // но для этого необходимо проверить на доступность все остальные клетки вокруг
                    // если альтернатив нет - продолжим попытки двигаться дальше
                    List<Point> avalibleDirection = new List<Point>();

                    Point currentPoint = m_unitToDrive.GetGlobalPosition();
                    Point pointToGo;

                    List<Base.DIREC> allDirections = new List<Base.DIREC>()
                    { Base.DIREC.UP, Base.DIREC.DOWN, Base.DIREC.LEFT, Base.DIREC.RIGHT };
                    
                    // проверяем все точки одну за одной
                    foreach (var direction in allDirections)
                    {
                        pointToGo = new Point(currentPoint);
                        pointToGo.ShiftPoint(direction);
                        if (!pointToGo.IsSamePoint(lastpPointToGo) && PathFinder.GetInstance().ValidatePathCell(pointToGo))
                        {
                            avalibleDirection.Add(pointToGo);
                        }
                    }
                    
                    // выбираем одно из возможных направлений, или если таких нет - прежнее
                    if (avalibleDirection.Count > 0)
                    {
                        pointToGo = avalibleDirection[Random.Range(0, avalibleDirection.Count)];
                    }
                    else
                    {
                        pointToGo = lastpPointToGo;
                    }

                    // добавляем точку как единственный элемент пути
                    m_path.AddLast(pointToGo);
                }
            }
        }
        else if (m_trafficJamCounter > 0)
        {
            // если происходит движение юнита - постепенно снижаем степень пробки
            m_trafficJamCounter--;
        }
        else if (m_sizeWasChanged)
        {
            // если вышли полностью из состояния пробки - возвращаем прежний радиус
            m_unitToDrive.SetPhysicalRadius(m_originalRadius);
        }

        m_priviusTurnPosition = currentPosition;
    }
}

