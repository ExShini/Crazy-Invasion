using UnityEngine;

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
/**********************************************************************************/
// BulletRotated класс
// класс пули, которой необходим функционал поворачивания изображения
// сюда относятся все снаряды с несферической рисовкой (пули, ракеты, слизни и т.д.)
//
/**********************************************************************************/
public class BulletRotated : Bullet
{
    new public void Start()
    {
        base.Start();
        m_rb2d.transform.eulerAngles = new Vector3(0.0f, 0.0f, m_rotationToSet);
    }

    protected float m_rotationToSet = 0.0f;

    /**********************************************************************************/
    // функция установки направления движения пули
    // данная реализация вращает её в нужном направлении
    //
    /**********************************************************************************/
    override public void SetDirection(Vector2 direction)
    {
        base.SetDirection(direction);

        if (direction.x == 1)
        {
            m_rotationToSet = 270;
        }
        else if (direction.x == -1)
        {
            m_rotationToSet = 90;
        }
        else if (direction.y == -1)
        {
            m_rotationToSet = 180;
        }
        else if (direction.y == 1)
        {
            m_rotationToSet = 0;
        }

        if (IsInitialized())
        {
            m_rb2d.transform.eulerAngles = new Vector3(0.0f, 0.0f, m_rotationToSet);
        }
    }
}