using UnityEngine;
using UnityEditor;
////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// SolderCtr класс
// контроллер солдата
// бродит, стреляет по врагам, метает гранаты
//
/**********************************************************************************/

class SolderCtr: Unit
{
    public WeaponController Weapon = new WeaponController();
    public int SearchingRadius = 3;

    public WeaponController Grenade = new WeaponController((int)PLAYER.NEUTRAL);

    void Start()
    {
        m_state = UNIT_STATE.ACTIVE;
        Weapon.SetUnitID(ID);
        Weapon.SetOwner(PLAYER.NEUTRAL);
        Grenade.SetUnitID(ID);
        Grenade.SetOwner(PLAYER.NEUTRAL);

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_drive = new WanderingDrive(this);
        m_armor = new BaseArmor();

        // основной стрелковый радар
        LineRadar radar = new LineRadar(this);
        radar.SetRadarRadius(SearchingRadius);
        m_radars.Add(radar);

        // дополнительный радар для гранат
        GrenadeRadar gRadar = new GrenadeRadar(this);
        // считаем дистанцию полёта гранаты, это необходимо для настройки гранатного радара
        GameObject grenade = ObjectLibrary.GetInstance().GetPrefab(Grenade.BulletType);
        GrenadeCtr gCtr = grenade.GetComponent<GrenadeCtr>();
        int range = (int)(gCtr.FlyDistance / Base.SIZE_OF_CELL);
        gRadar.SetRadarRadius(range);
        gRadar.SetScaningArea(gCtr.ExplosionRadius);
        m_radars.Add(gRadar);

        InitializeUnit();

        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.ADDITIONAL_WEAPON, Grenade);

        // настраиваем подключения компонент:
        // передача координат радару
        m_drive.PositionUpdate += radar.PositionUpdate;
        m_drive.PositionUpdate += gRadar.PositionUpdate;
        // передача данных о целях для стрельбы
        radar.RadarUpdate += m_weaponMaster.UpdateMainRadarData;
        // передача данных о целях для метания гранат
        gRadar.RadarUpdate += m_weaponMaster.UpdateAdditionalRadarData;
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

