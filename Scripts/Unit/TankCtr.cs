using UnityEngine;
using UnityEditor;
////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// TankCtr класс
// контроллер танка
// идёт к противнику, стреляет во всех на своём пути
//
/**********************************************************************************/

class TankCtr : Unit
{
    public WeaponController Weapon = new WeaponController();
    public int SearchingRadius = 3;

    void Start()
    {
        InitializeUnit();

        m_state = UNIT_STATE.ACTIVE;
        Weapon.SetUnitID(ID);
        Weapon.SetOwner((PLAYER)m_ownerID);

        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_drive = new BaseDrive(this);
        m_armor = new BaseArmor();
        LineRadar radar = new LineRadar(this);
        radar.SetRadarRadius(SearchingRadius);
        StalkerRadar stRadar = new StalkerRadar((PLAYER)m_ownerID);

        m_radars.Add(radar);
        m_radars.Add(stRadar);

        InitializeUnit();

        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);

        // настраиваем подключения компонент:
        // передача координат радару
        m_drive.PositionUpdate += radar.PositionUpdate;
        // настраиваем преследование
        stRadar.TargetToMove += m_drive.SetTargetToMove;
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

