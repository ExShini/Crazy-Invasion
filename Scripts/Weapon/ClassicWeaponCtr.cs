using UnityEngine;
using System.Collections;

/**********************************************************************************/
// ClassicWeaponCtr класс
// отвечает за обработку состояния оружия и его контроль
/**********************************************************************************/
[System.Serializable]
public class ClassicWeaponCtr : WeaponController
{

    protected int m_currentAmmo = 0;
    public int MagazineAmmo = 0;
    protected int m_currentMagazineAmmo = 0;
    public int ShellsInShot = 1;

    /**********************************************************************************/
    // WeaponController конструктор
    /**********************************************************************************/
    public ClassicWeaponCtr(int ownerID) :
        base(ownerID)
    {
        m_currentAmmo = NumberOfBullet;
        m_currentMagazineAmmo = MagazineAmmo;
    }


    /**********************************************************************************/
    // функция стрельбы
    //
    /**********************************************************************************/
    protected override void FireWeapon(Vector2 position, Base.DIREC direction)
    {
        for (int shell = 0; shell < ShellsInShot; shell++)
        {
            if (m_currentAmmo > 0 && m_currentMagazineAmmo > 0)
            {
                CreateBullet(position, direction);

                m_currentAmmo--;
                m_currentMagazineAmmo--;
            }
        }

        if (m_currentAmmo <= 0)
        {
            m_state = WEAPON_STATE.EMPTY;
            return;
        }

        if (m_currentMagazineAmmo == 0)
        {
            m_state = WEAPON_STATE.RECHARGE;
            m_currentRechargeTimer = FireRechargeTime;
        }
        else
        {
            m_state = WEAPON_STATE.READY;
        }
    }

    /**********************************************************************************/
    // Обновляем состояние оружия на UI
    //
    /**********************************************************************************/
    public override void UpdateCtrStatuses(PlayerController.WEAPON_SLOT slot)
    {
        // TODO: сделать что-нибудь с этой структурой вызовов
        if (m_state == WeaponController.WEAPON_STATE.FIRE)
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, 0, m_currentAmmo, NumberOfBullet);
        }
        else if (m_state == WeaponController.WEAPON_STATE.READY)
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, 100, m_currentAmmo, NumberOfBullet);
        }
        else
        {
            UIController.GetInstance().SetWeaponState(m_ownerID, slot, getChargeState(), m_currentAmmo, NumberOfBullet);
        }
    }

    /**********************************************************************************/
    // создаём поражающий элемент
    //
    /**********************************************************************************/
    protected virtual void CreateBullet(Vector2 position, Base.DIREC direction)
    {
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
        }
    }

    /**********************************************************************************/
    // функция дозарядки оружия (используется, когла подбирается боезапас)
    //
    /**********************************************************************************/
    public void ChargeAmmo(int ammo)
    {
        m_currentAmmo += ammo;
        m_currentMagazineAmmo = MagazineAmmo;
        if (m_currentAmmo > NumberOfBullet)
        {
            m_currentAmmo = NumberOfBullet;
        }

        if (m_currentAmmo > 0)
        {
            m_state = WEAPON_STATE.READY;
        }
        else
        {
            m_state = WEAPON_STATE.EMPTY;
        }
    }

    /**********************************************************************************/
    // функция перезарядки оружия
    // в данном контексте оно определяет частоту стрельбы
    //
    /**********************************************************************************/
    protected override void RechargeWeapon()
    {
        m_currentRechargeTimer -= Time.deltaTime;
        if (m_currentRechargeTimer <= 0)
        {
            m_state = WEAPON_STATE.READY;
            m_currentMagazineAmmo = MagazineAmmo;
            if (m_currentMagazineAmmo > m_currentAmmo)
            {
                m_currentMagazineAmmo = m_currentAmmo;
            }
        }
    }
}
