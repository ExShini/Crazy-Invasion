using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WEAPON
{
    NPC_WEAPON = -1,
    BLUSTER = 0,
    MOCUS = 1,
    TENTAKLES = 2,
    PLASMA_GRENADE = 3,
    RGD_GRENADE = 4,
    SHOTGUN = 5,
    ACID_GUN = 6,
    TUREL_BUILDER = 7
}

/**********************************************************************************/
// WeaponController класс
// отвечает за обработку состояния оружия и его контроль
/**********************************************************************************/
[System.Serializable]
public class WeaponController
{
    public enum WEAPON_STATE
    {
        READY,
        FIRE,
        RECHARGE,
        EMPTY
    }

    public int NumberOfBullet = 0;
    protected int m_currentNumOfFiredBullets = 0;
    protected int m_ownerID = 0;
    protected int m_unitID = 0;
    public float FireRechargeTime = 2.5f;
    protected float m_currentRechargeTimer = 0.0f;
    protected WEAPON_STATE m_state = WEAPON_STATE.READY;
    public Base.GO_TYPE BulletType = Base.GO_TYPE.NONE_TYPE;
    public bool SelfDamaged = false;

    protected GameObject m_target = null;


    // ********
    // СВОЙСТВА:

    public WEAPON_STATE State
    {
        get { return m_state; }
    }

    // ********
    // МЕТОДЫ:

    /**********************************************************************************/
    // WeaponController конструктор
    /**********************************************************************************/
    public WeaponController(int ownerID)
    {
        m_ownerID = ownerID;
    }

    /**********************************************************************************/
    // WeaponController конструктор
    /**********************************************************************************/
    public WeaponController()
    {
        m_ownerID = (int)PLAYER.NO_PLAYER;
    }

    /**********************************************************************************/
    // устанавливаем сторону владельца
    /**********************************************************************************/
    public void SetOwner(PLAYER id)
    {
        m_ownerID = (int)id;
    }

    /**********************************************************************************/
    // функция устанавливает ID юнита стрелка, он будет передаваться при создании снарядов
    //
    /**********************************************************************************/
    public void SetUnitID(int unitID)
    {
        m_unitID = unitID;
    }

    /**********************************************************************************/
    // функция обновляющая состояние оружия на GUI
    //
    /**********************************************************************************/
    public virtual void UpdateCtrStatuses(PlayerController.WEAPON_SLOT slot)
    {
        // TODO: сделать что-нибудь с этой структурой вызовов
        if (m_state == WeaponController.WEAPON_STATE.FIRE)
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, 0, NumberOfBullet - m_currentNumOfFiredBullets, NumberOfBullet);
        }
        else if (m_state == WeaponController.WEAPON_STATE.READY)
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, 100, NumberOfBullet, NumberOfBullet);
        }
        else
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, getChargeState(), 0, NumberOfBullet);
        }
    }

    /**********************************************************************************/
    // функция возвращает текущий прогресс перезарядки
    //
    /**********************************************************************************/
    public virtual int getChargeState()
    {
        int progress = 0;
        progress = (int)(((FireRechargeTime - m_currentRechargeTimer) / FireRechargeTime) * 100.0f);
        return progress;
    }

    /**********************************************************************************/
    // функция включения стрельбы
    // если готовы - начинаем палить
    //
    /**********************************************************************************/
    public void Fire()
    {
        if (m_state == WEAPON_STATE.READY)
        {
            m_state = WEAPON_STATE.FIRE;
        }
        else
        {
            StopFire();
        }
    }

    /**********************************************************************************/
    // стреляем по конкретной цели (если оружие поддерживает подобный функционал(определяется конкретной реализацией WeaponController-а))
    //
    /**********************************************************************************/
    public void Fire(GameObject target)
    {
        m_target = target;
        Fire();
    }

    /**********************************************************************************/
    // функция выключения стрельбы
    // в данном контексте пуста, но используются в наследниках как индикация остановки стрельбы
    //
    /**********************************************************************************/
    public virtual void StopFire()
    {
        // do nothing
    }

    /**********************************************************************************/
    // функция обработки оружия
    // если начали стрелять - стреляем пока не выпустим всю очередь
    // если перезаряжаемся - считаем время до окончания перезарядки
    //
    /**********************************************************************************/
    public virtual void UpdateWeaponState(Vector2 position, Base.DIREC direction)
    {
        if (m_state == WEAPON_STATE.FIRE)
        {
            FireWeapon(position, direction);
        }
        else if (m_state == WEAPON_STATE.RECHARGE)
        {
            RechargeWeapon();
        }
    }

    /**********************************************************************************/
    // функция стрельбы
    //
    /**********************************************************************************/
    protected virtual void FireWeapon(Vector2 position, Base.DIREC direction)
    {
        GameObject bulletObj = ObjectFactory.GetInstance().CreateGObject(position, direction, BulletType);
        Bullet bulletCtr = bulletObj.GetComponent<Bullet>();
        if(bulletCtr != null)
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
        }


        m_currentNumOfFiredBullets++;
        if (m_currentNumOfFiredBullets >= NumberOfBullet)
        {
            m_state = WEAPON_STATE.RECHARGE;
            m_currentRechargeTimer = FireRechargeTime;
        }
    }

    /**********************************************************************************/
    // функция перезарядки оружия
    //
    /**********************************************************************************/
    protected virtual void RechargeWeapon()
    {
        m_currentRechargeTimer -= Time.deltaTime;
        if (m_currentRechargeTimer <= 0)
        {
            m_currentRechargeTimer = 0;
            m_state = WEAPON_STATE.READY;
            m_currentNumOfFiredBullets = 0;
        }
    }
}
