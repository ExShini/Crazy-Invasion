using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**********************************************************************************/
// RegeneratedLauncherWeaponCtr
// регенерируемое, самонаводящееся
//
/**********************************************************************************/
[System.Serializable]
public class RegeneratedLauncherWeaponCtr : RegeneratedWeaponCtr
{

    /**********************************************************************************/
    // RegeneratedLauncherWeaponCtr конструктор
    //
    /**********************************************************************************/
    public RegeneratedLauncherWeaponCtr(int ownerID) :
        base(ownerID)
    {
        m_currentRegenTimer = TimeToRegenerateAmmo;
    }

    /**********************************************************************************/
    // создаём поражающий элемент
    //
    /**********************************************************************************/
    protected override void CreateBullet(Vector2 position, Base.DIREC direction)
    {
        // определяем направления запуска снаряда 
        Point targetPosition = m_target.GetComponent<CIGameObject>().GetGlobalPosition();
        Point unitPosition = new Point((int)(position.x / Base.SIZE_OF_CELL), (int)(position.y / Base.SIZE_OF_CELL));
        LinkedList<Point> pathToTarget = PathFinder.GetInstance().GetWay(unitPosition, targetPosition);

        // если объект находится не в той же клетке, что и цель, используем вторую точку пути, так как она находится в направлении движения
        if (pathToTarget.Count > 1)
        {
            pathToTarget.RemoveLast();
            Point launcherPoint = pathToTarget.Last.Value;
            direction = (launcherPoint - unitPosition).ToDirection();
        }
        

        GameObject bulletObj = ObjectFactory.GetInstance().CreateGObject(position, direction, BulletType);
        Bullet bulletCtr = bulletObj.GetComponent<Bullet>();

        if (bulletCtr != null)
        {
            bulletCtr.Owner = m_ownerID;
            bulletCtr.OwnerUnitID = m_unitID;
        }
        else
        {
            // некоторые типы оружия используют других существ в качестве снаряда
            // в этом случае используем GMovingObject
            CIGameObject gmo = bulletObj.GetComponent<CIGameObject>();
            gmo.Owner = m_ownerID;

            ScarabCtr scarabCtr = bulletObj.GetComponent<ScarabCtr>();
            if (scarabCtr != null)
            {
                scarabCtr.SetTarget(m_target);
            }
        }
    }
}
