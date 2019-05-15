using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// LaserCtr класс
// контроллер игрового лазер-объекта
/**********************************************************************************/
public class LaserCtr : MonoBehaviour
{


    private LineRenderer m_lineRenderer;
    public Transform LaserHitPoint;
    public float MaxDist = 0.96f;
    public float Offset = 0.15f;

    protected Base.DIREC m_laserDirection = Base.DIREC.DOWN;
    protected Transform m_ownerObject = null;

    /**********************************************************************************/
    // Инициализация
    //
    /**********************************************************************************/
    void Start ()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_lineRenderer.enabled = false;
        m_lineRenderer.useWorldSpace = true;
    }

    /**********************************************************************************/
    // выключаем лазер
    /**********************************************************************************/
    public void DisableLaser()
    {
        m_lineRenderer.enabled = false;
        LaserHitPoint.gameObject.SetActive(false);
    }

    /**********************************************************************************/
    // обновляем позицию лазера
    /**********************************************************************************/
    private void Update()
    {
        UpdateCoordinates();
    }

    /**********************************************************************************/
    // устанавливаем объект носитель лазера
    /**********************************************************************************/
    public void SetOwnerObject(GameObject owner)
    {
        m_ownerObject = owner.transform;
    }

    /**********************************************************************************/
    // обновляем позицию лазера
    /**********************************************************************************/
    protected void UpdateCoordinates()
    {
        if(m_ownerObject == null)
        {
            return;
        }

        Vector2 LaserEmmiterPosition = new Vector2(m_ownerObject.position.x, m_ownerObject.position.y);
        Point dirPoint = new Point(0, 0);
        dirPoint.ShiftPoint(m_laserDirection);
        Vector3 directionVector = dirPoint.GetUnityPoint() / Base.SIZE_OF_CELL;

        // выбираем направление
        switch (m_laserDirection)
        {
            case Base.DIREC.DOWN:
                LaserEmmiterPosition.y -= Offset;
                break;
            case Base.DIREC.UP:
                LaserEmmiterPosition.y += Offset;
                break;
            case Base.DIREC.LEFT:
                LaserEmmiterPosition.x -= Offset;
                break;
            case Base.DIREC.RIGHT:
                LaserEmmiterPosition.x += Offset;
                break;
        }

        // устанавливаем первую точку
        transform.position = LaserEmmiterPosition;
        m_lineRenderer.SetPosition(0, transform.position);

        // определяем вторую точку
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionVector);
        LaserHitPoint.position = hit.point;

        float dis = (transform.position - LaserHitPoint.position).magnitude;
        if (dis > MaxDist)
        {
            LaserHitPoint.position = transform.position + directionVector * MaxDist;
        }

        m_lineRenderer.SetPosition(1, LaserHitPoint.position);
    }

    /**********************************************************************************/
    // запускаем анимацию стрельбы и настраиваем игровые объекты
    /**********************************************************************************/
    public void Fire(Vector2 position, Base.DIREC direction)
    {
        m_laserDirection = direction;
        UpdateCoordinates();
        m_lineRenderer.enabled = true;
        LaserHitPoint.gameObject.SetActive(true);
    }
}
