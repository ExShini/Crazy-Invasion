using UnityEngine;
using System.Collections;
/**********************************************************************************/
// MugikCtr класс
// контроллер попа
// бродит, освобождает здания
//
/**********************************************************************************/
public class PopeCtr : Unit
{
    public int SearchingRadius = 3;
    public WeaponController Weapon = new WeaponController((int)PLAYER.NEUTRAL);

    void Start()
    {
        m_state = UNIT_STATE.ACTIVE;
        Weapon.SetUnitID(ID);
        Weapon.SetOwner(PLAYER.NEUTRAL);

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_drive = new WanderingDrive(this);
        m_armor = new BaseArmor();
        LineRadarBuilding radar = new LineRadarBuilding(this);
        radar.SetRadarRadius(SearchingRadius);
        m_radars.Add(radar);

        InitializeUnit();

        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);

        // настраиваем подключения компонент:
        // передача координат радару
        m_drive.PositionUpdate += radar.PositionUpdate;
        // передача данных о целях для стрельбы
        radar.RadarUpdate += m_weaponMaster.UpdateMainRadarData;
        // контроль движения при стрельбе
        m_weaponMaster.PauseDrive += m_drive.StopMoving;
        m_weaponMaster.ResumeDrive += m_drive.StartMoving;
        // событие смерти юнита
        m_armor.UnitIsDown += this.OnDead;


        // устанавливаем дефолтные значения
        SetDefaultParameter();
        SetEffectsCollection();
    }
}
