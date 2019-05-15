using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// WeaponMaster
// класс управляющий вооружением юнита
//
/**********************************************************************************/
public class WeaponMaster
{
    // эвенты контроля движения (используются в момент стрельбы, когда юнит не должен двигаться)
    public event GameEvent PauseDrive;
    public event GameEvent ResumeDrive;

    Unit m_unitWithWeapon;
    Dictionary<WeaponSlot, WeaponController> m_weapons = new Dictionary<WeaponSlot, WeaponController>();
    bool m_isFiring = false;    // обозночает текущее состояние стрельбы в текущий момент

    // состояние мастера вооружений
    bool m_isActive;

    public enum WeaponSlot
    {
        CLOSE_WEAPON,
        MAIN_WEAPON,
        ADDITIONAL_WEAPON,
    }


    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public WeaponMaster(Unit unitWithWeapon)
    {
        m_unitWithWeapon = unitWithWeapon;
        m_isActive = true;
    }

    /**********************************************************************************/
    // ф-ция отключает WeaponMaster, можно использовать в тех случаях, когда объект
    // не имеет вооружения
    //
    /**********************************************************************************/
    public void DisableWM()
    {
        m_isActive = false;
    }

    /**********************************************************************************/
    // сеттер для вооружения
    //
    /**********************************************************************************/
    public void SetWeapon(WeaponSlot slot, WeaponController weapon)
    {
        m_weapons[slot] = weapon;
    }


    /**********************************************************************************/
    // основная процессинговая функция
    // обновляем состояния всего вооружения
    //
    /**********************************************************************************/
    public void Update()
    {
        if (!m_isActive)
            return;

        Base.DIREC directionOfMovement = m_unitWithWeapon.MoveDirection;
        Vector2 physicalUnitPosition = m_unitWithWeapon.GetGlobalPositionCenter_Unity();

        bool weaponFiring = false;
        foreach (var weapon in m_weapons)
        {
            WeaponController wCtr = weapon.Value;
            wCtr.UpdateWeaponState(physicalUnitPosition, directionOfMovement);

            // проверяем состояние оружия
            // в случае, если мы прекратили стрелять, мы должны вновь запустить движение
            weaponFiring |= wCtr.State == WeaponController.WEAPON_STATE.FIRE;
        }

        // стреляли, но прекратили
        if (weaponFiring == false && m_isFiring == true && ResumeDrive != null)
        {
            ResumeDrive();
        }
    }

    /**********************************************************************************/
    // функция обновления данных для основного вооружения
    //
    /**********************************************************************************/
    public void UpdateMainRadarData(RadarData data)
    {
        if (!m_isActive)
            return;

        if (!m_weapons.ContainsKey(WeaponSlot.MAIN_WEAPON))
        {
            Debug.LogError("We didn't set main weapon, but recive radar data for this weapon");
            return;
        }

        if (m_weapons[WeaponSlot.MAIN_WEAPON] == null)
        {
            Debug.LogError("We have no main weapon (null), but recive radar data for this weapon");
            return;
        }

        // если есть цели - открываем огонь
        if (data.DetectedEnemy != null)
        {
            if (data.DetectedEnemy.Count > 0)
            {
                WeaponController ctr = m_weapons[WeaponSlot.MAIN_WEAPON];
                UseWeapon(ctr);
            }
        }
    }

    /**********************************************************************************/
    // функция обновления данных для дополнительного вооружения
    //
    /**********************************************************************************/
    public void UpdateAdditionalRadarData(RadarData data)
    {
        if (!m_isActive)
            return;

        if (!m_weapons.ContainsKey(WeaponSlot.ADDITIONAL_WEAPON))
        {
            Debug.LogError("We didn't set additional weapon, but recive radar data for this weapon");
            return;
        }

        if (m_weapons[WeaponSlot.ADDITIONAL_WEAPON] == null)
        {
            Debug.LogError("We have no additional weapon (null), but recive radar data for this weapon");
            return;
        }

        // если есть цели - открываем огонь
        if (data.DetectedEnemy != null)
        {
            if (data.DetectedEnemy.Count > 0)
            {
                WeaponController ctr = m_weapons[WeaponSlot.ADDITIONAL_WEAPON];
                UseWeapon(ctr);
            }
        }
    }

    /**********************************************************************************/
    // используем указанное оружие
    //
    /**********************************************************************************/
    private void UseWeapon(WeaponController ctr)
    {
        ctr.Fire();

        // приостанавливаем движени на стрельбу
        if (!m_isFiring && ResumeDrive != null)
        {
            m_isFiring = true;
            PauseDrive();
        }

        // если оружие предполагает саморазрушение юнита/объекта после использования - наносим себе ультимативный урон
        if (ctr.SelfDamaged)
        {
            Vector2 physicalUnitPosition = m_unitWithWeapon.GetGlobalPositionCenter_Unity();
            Base.DIREC directionOfMovement = m_unitWithWeapon.MoveDirection;
            ctr.UpdateWeaponState(physicalUnitPosition, directionOfMovement);
            m_unitWithWeapon.ApplyDamage(new DamageData(100, DamageData.DAMAGE_TYPE.PHYSICAL, m_unitWithWeapon, DamageData.RESPONSE.NOT_EXPECTED));
        }
    }

    /**********************************************************************************/
    // функция применения оружия ближнего боя
    // используется в случае нападения при столкновении с игровым объектом
    // отличие от другой UseCloseWeapon ф-ции заключается в том, что мы изначально не знаем на кого/что наткнулись
    // и можем ли вообще применить оружие к этому объекту
    //
    /**********************************************************************************/
    public void UseCloseWeapon(GameObject target)
    {
        if (!m_isActive)
            return;

        string otherObjTag = target.tag;
        if (otherObjTag == "Player" || otherObjTag == "Unit")
        {
            bool unitStateCheck = true;
            // проверяем, живая ли цель, мёртвых атоковать не будем
            if (otherObjTag == "Unit")
            {
                Unit collidedUnit = target.GetComponent<Unit>();
                if (collidedUnit == null)
                {
                    Debug.LogError("UseCloseWeapon: unit have no 'unit' controller!");
                    return;
                }

                unitStateCheck = collidedUnit.State == Unit.UNIT_STATE.ACTIVE;
            }

            CIGameObject objectUnderAttackCtr = target.GetComponent<CIGameObject>();
            int collidedGOOwner = objectUnderAttackCtr.Owner;

            // если объект жив и принадлежит врагу, атакуем его
            if (unitStateCheck && m_unitWithWeapon.Owner != collidedGOOwner)
            {
                // наносим урон врагу
                UseCloseWeapon(objectUnderAttackCtr, true);
            }
        }
    }


    /**********************************************************************************/
    // функция применения оружия ближнего боя
    //
    /**********************************************************************************/
    public void UseCloseWeapon(CIGameObject target, bool response)
    {
        if (!m_isActive)
            return;

        // если монстр не умеет атаковать в ближнем бою - скипаем процесс
        // проверяем боеготовность
        CloseWeaponController closeWeapon = m_weapons[WeaponSlot.CLOSE_WEAPON] as CloseWeaponController;
        if (closeWeapon == null)
        {
            return;
        }

        if (closeWeapon.Damage == 0 || closeWeapon.State == WeaponController.WEAPON_STATE.RECHARGE)
        {
            return;
        }

        DamageData.RESPONSE expectation = DamageData.RESPONSE.EXPECTED;
        if (!response)
        {
            expectation = DamageData.RESPONSE.NOT_EXPECTED;
        }

        closeWeapon.Fire();
        target.ApplyDamage(new DamageData(closeWeapon.Damage, DamageData.DAMAGE_TYPE.PHYSICAL, m_unitWithWeapon, expectation));

        // если оружие предполагает саморазрушение юнита/объекта после использования - наносим себе ультимативный урон
        if (closeWeapon.SelfDamaged)
        {
            m_unitWithWeapon.ApplyDamage(new DamageData(100, DamageData.DAMAGE_TYPE.PHYSICAL, m_unitWithWeapon, DamageData.RESPONSE.NOT_EXPECTED));
        }
    }

    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    public void ResetComponent()
    {
        m_isFiring = false;
    }
}

