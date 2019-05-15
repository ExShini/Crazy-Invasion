using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// BaseMonsterCtr класс
// базовый класс для всех простых зомби
//
/**********************************************************************************/
public class BaseMonsterCtr : Unit
{
    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    void Start()
    {
        InitializeUnit();
        m_state = UNIT_STATE.ACTIVE;

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_drive = new BaseDrive(this);
        m_armor = new BaseArmor();
        StalkerRadar radar = new StalkerRadar((PLAYER)Owner);
        m_radars.Add(radar);

        // настраиваем подключения компонент:
        // передача координат радару
        m_drive.PositionUpdate += radar.PositionUpdate;
        // передача данных о преследуемых целях
        radar.TargetToMove += m_drive.SetTargetToMove;
        // событие смерти юнита
        m_armor.UnitIsDown += this.OnDead;
        // устанавливаем оружие ближнего боя
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);

        // устанавливаем дефолтные значения
        SetDefaultParameter();
        SetEffectsCollection();
    }
}
