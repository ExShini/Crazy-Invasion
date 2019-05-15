using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// CIGameObject класс
// базовый класс для всех игровых объектов - игроки, юниты, снаряды
/**********************************************************************************/
public abstract class CIGameObject : MonoBehaviour
{
    public float speed = 0.0f;
    [HideInInspector]
    public float speedMultiplier = 1.0f;
    public int SizeOfObject = 1;
    protected Rigidbody2D m_rb2d = null;
    protected CircleCollider2D m_collider = null;
    protected Vector2 m_direction;

    public Base.GO_TYPE GOType;
    protected int m_ownerID = 0;
    protected int m_unitID = 0;
    protected BuildingController m_productionBase = null;

    protected Base.DIREC m_movementDirection = Base.DIREC.NO_DIRECTION;

    protected bool m_initializes = false;

    // возвращает владельца
    public virtual int Owner
    {
        get { return m_ownerID; }
        set { m_ownerID = value; }
    }

    // возвращает id
    public int ID
    {
        get { return m_unitID; }
        set { m_unitID = value; }
    }

    public Vector2 Position
    {
        get { return m_rb2d.position; }
    }

    public BuildingController ProductionBase
    {
        get { return m_productionBase; }
        set { m_productionBase = value; }
    }

    /**********************************************************************************************/
    // свойство возвращающее направление текущего движения
    //
    /**********************************************************************************************/
    public Base.DIREC MoveDirection
    {
        get { return m_movementDirection; }
    }

    /**********************************************************************************/
    // инициализация GMovingObject
    //
    /**********************************************************************************/
    public virtual void InitializeGO()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        m_rb2d = GetComponent<Rigidbody2D>();
        if(m_rb2d != null)
        {
            m_rb2d.freezeRotation = true;
        }

        m_collider = GetComponent<CircleCollider2D>();

        m_initializes = true;
    }

    /**********************************************************************************/
    // возвращает статус инициализации
    //
    /**********************************************************************************/
    public bool IsInitialized()
    {
        return m_initializes;
    }

    /**********************************************************************************/
    // возвращает физический радиус объекта
    //
    /**********************************************************************************/
    public float GetPhysicalRadius()
    {
        return m_collider.radius;
    }

    /**********************************************************************************/
    // устанавливает физический радиус объекта
    //
    /**********************************************************************************/
    public void SetPhysicalRadius(float radius)
    {
        m_collider.radius = radius;
    }

    /**********************************************************************************/
    // перемещает объект в указанном направлении
    // movement определяет вектор скорости объекта
    //
    /**********************************************************************************/
    virtual public void MoveGObject(Vector2 movement)
    {
        Vector3 newPosition = m_rb2d.position + movement * speed * speedMultiplier * Time.deltaTime;
        m_rb2d.MovePosition(newPosition);
    }

    /**********************************************************************************/
    // перемещает объект в указанном направлении
    // movement определяет вектор скорости объекта
    //
    /**********************************************************************************/
    virtual public void MoveGObjectToPosition(Vector2 newPosition, bool useSizeObjectCorrection = true)
    {
        if(useSizeObjectCorrection)
        {
            newPosition.x += SizeOfObject * Base.HALF_OF_CELL;
            newPosition.y += SizeOfObject * Base.HALF_OF_CELL;
        }

        m_rb2d.MovePosition(newPosition);
    }

    /**********************************************************************************/
    // перемещает объект в указанном направлении
    //
    /**********************************************************************************/
    virtual public void MoveGObject(Base.DIREC direction)
    {
        Vector2 movement = new Vector2();
        switch (direction)
        {
            case Base.DIREC.DOWN:
                movement.y = -1f;
                break;
            case Base.DIREC.UP:
                movement.y = 1f;
                break;
            case Base.DIREC.LEFT:
                movement.x = -1f;
                break;
            case Base.DIREC.RIGHT:
                movement.x = 1f;
                break;
        }

        Vector3 newPosition = m_rb2d.position + movement * speed * speedMultiplier * Time.deltaTime;
        m_rb2d.MovePosition(newPosition);
    }

    /**********************************************************************************/
    // перемещает объект к указанной точке
    // возвращаем true если уже находимся рядом с этой точкой и движение не требуется
    //
    /**********************************************************************************/
    virtual public bool MoveGObjectToPoint(Point movePoint)
    {
        Vector2 unitPoint = GetGlobalPosition_Unity();
        Vector2 pointToMove = movePoint.GetUnityPoint();

        // проверяем - дошли мы до следующей точки
        // считаем разницу координат
        float xCorDiff = pointToMove.x - unitPoint.x;
        float yCorDiff = pointToMove.y - unitPoint.y;

        float diffCorr = Mathf.Abs(xCorDiff) + Mathf.Abs(yCorDiff);

        // рассчитываем точность движения
        // для скоростных юнитов значение будет увеличено
        float accurasity = 0.01f;
        float expectedSpead = speed * speedMultiplier * Time.deltaTime;
        if(expectedSpead > accurasity)
        {
            accurasity += expectedSpead;
        }

        if (diffCorr <= accurasity)
        {
            // если добрались, окончательно выравниваем объект по точке
            // и возвращаем true как сигнал об окончании движения
            MoveGObjectToPosition(pointToMove);
            return true;
        }

        Base.DIREC direction = Base.DIREC.NO_DIRECTION;
        // выбираем направление для сближения
        if (Mathf.Abs(xCorDiff) > Mathf.Abs(yCorDiff))
        {
            if (xCorDiff > 0)
                direction = Base.DIREC.RIGHT;
            else
                direction = Base.DIREC.LEFT;
        }
        else
        {
            if (yCorDiff > 0)
                direction = Base.DIREC.UP;
            else
                direction = Base.DIREC.DOWN;
        }

        MoveGObject(direction);

        return false;
    }

    /**********************************************************************************/
    // устанавливает направление движения
    //
    /**********************************************************************************/
    virtual public void SetDirection(Vector2 direction)
    {
        m_direction = direction;
    }

    /**********************************************************************************/
    // возвращаем глобальные координыты объекта 
    //
    /**********************************************************************************/
    virtual public Point GetGlobalPosition()
    {
        Point positon = new Point(Mathf.FloorToInt(m_rb2d.position.x / Base.SIZE_OF_CELL),
                                    Mathf.FloorToInt(m_rb2d.position.y / Base.SIZE_OF_CELL));
        return positon;
    }

    /**********************************************************************************/
    // возвращаем глобальные координыты объекта в системе координат Unity
    //
    /**********************************************************************************/
    public Vector2 GetGlobalPosition_Unity()
    {
        // рассчитываем координату левого нижнего угла объекта как координата центра - половина ширины/высоты объекта
        return new Vector2( m_rb2d.position.x - (float)SizeOfObject * Base.HALF_OF_CELL, m_rb2d.position.y - (float)SizeOfObject * Base.HALF_OF_CELL);
    }

    /**********************************************************************************/
    // возвращаем глобальные координыты ЦЕНТРА объекта в системе координат Unity
    //
    /**********************************************************************************/
    public Vector2 GetGlobalPositionCenter_Unity()
    {
        // рассчитываем координату левого нижнего угла объекта как координата центра - половина ширины/высоты объекта
        return new Vector2(m_rb2d.position.x, m_rb2d.position.y);
    }

    /**********************************************************************************/
    // функция сброса объекта к дефолтным параметрам
    // используется для всех реиспользуемых объектов!
    //
    /**********************************************************************************/
    abstract public void ResetGObject();


    /**********************************************************************************/
    // функция получения урона объектом
    //
    /**********************************************************************************/
    abstract public void ApplyDamage(DamageData damage);



    /**********************************************************************************/
    // функция применения эффектов
    //
    /**********************************************************************************/
    abstract public void ApplyEffect(EffectDescriptor effect);
}
