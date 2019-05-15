using UnityEngine;
using System.Collections;

/**********************************************************************************/
// WanderingShildedGunner класс
// контроллер бродячего стрелка с защитным полем
//
/**********************************************************************************/
public class WanderingShieldedLiberator : PopeCtr
{
    public float ShieldDuration = 3.0f;
    private LineRadarBuilding m_radar;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    void Start()
    {
        m_state = UNIT_STATE.ACTIVE;
        Weapon.SetUnitID(ID);
        Weapon.SetOwner((PLAYER)m_ownerID);

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_drive = new WanderingDrive(this);
        ShieldedArmor armor = new ShieldedArmor(gameObject);
        armor.SetShiledDuration(ShieldDuration);
        m_armor = armor;
        m_radar = new LineRadarBuilding(this);
        m_radar.SetRadarRadius(SearchingRadius);
        m_radars.Add(m_radar);

        InitializeUnit();

        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);

        // настраиваем подключения компонент:
        // передача координат радару
        m_drive.PositionUpdate += m_radar.PositionUpdate;
        // передача данных о целях для стрельбы
        m_radar.RadarUpdate += m_weaponMaster.UpdateMainRadarData;
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
