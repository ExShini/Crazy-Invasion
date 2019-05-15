/**********************************************************************************/
// TurelController класс
// класс контролирующий построенные игроком турели
//
/**********************************************************************************/
class TurelController : Unit
{
    public WeaponController Weapon;
    public int SearchRadius = 3;
    public float SearchRate = 0.5f;

    /**********************************************************************************/
    // устанавливаем компоненты для турельки и инициализируем её
    //
    /**********************************************************************************/
    void Start()
    {
        // создаем и настраиваем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);
        Weapon.SetOwner((PLAYER)m_ownerID);
        Weapon.SetUnitID(m_unitID);

        InitializeUnit();

        m_drive = new TowerDrive(this);
        m_armor = new BaseArmor();
        LineRadar radar = new LineRadar(this);
        radar.SetRadarRadius(SearchRadius);
        radar.SetTargetCheckRate(SearchRate);
        m_radars.Add(radar);

        // настраиваем разрушаемость
        m_armor.UnitIsDown += OnDead;
        // передача местоположения юнита
        m_drive.PositionUpdate += radar.PositionUpdate;
        // передача данных о целях для стрельбы
        radar.RadarUpdate += m_weaponMaster.UpdateMainRadarData;

        SetDefaultParameter();
        SetEffectsCollection();
    }
}

