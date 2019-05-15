using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// IshekoidCtr класс
// контроллер ищекоида/псов/медведей
// эти юниты отвечают поведению "хищник" - ищут цель, бродя по округе, а найдя - преследуют её
//
/**********************************************************************************/
public class IshekoidCtr : Unit
{
    public int SearchingRadius = 3;
    public float SpeedBoost = 1.0f;

    protected void Start()
    {
        InitializeUnit();

        m_state = UNIT_STATE.ACTIVE;

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);

        PredatorDrive drive = new PredatorDrive(this);
        drive.SetSpeedMultiplier(SpeedBoost);
        m_drive = drive;

        m_armor = new BaseArmor();
        CircleRadar radar = new CircleRadar(this);
        radar.SetRadius(SearchingRadius);
        m_radars.Add(radar);

        InitializeUnit();

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

