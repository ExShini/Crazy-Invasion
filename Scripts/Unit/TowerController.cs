/**********************************************************************************/
// TowerController класс
// класс контролирующий сторожевые вышки
/**********************************************************************************/
using UnityEngine;

class TowerController : WallController
{
    public WeaponController Weapon;
    public int SearchRadius = 3;
    public float SearchRate = 1.0f;

    /**********************************************************************************/
    // инициализация, расширенная
    // так как создание нейтральных вышек отличается от производства других юнитов, нам необходимо
    // указывать принадлежность самостоятельно
    //
    /**********************************************************************************/
    public override void InitializeUnit()
    {
        m_ownerID = (int)PLAYER.NEUTRAL;
        base.InitializeUnit();

        // башни часто имеет 2ой этаж, анимацию которого так же нужно переключать в зависимости от ситуаций
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        for (int ind = 0; ind < allChildren.Length; ind++)
        {
            GameObject component = allChildren[ind].gameObject;
            // получаем аниматор 2 этажа для контроля
            if (component.tag == "II_Floor")
            {
                m_animators.Add(component.GetComponent<Animator>());
            }
        }
    }

    /**********************************************************************************/
    // устанавливаем компоненты для башни
    //
    /**********************************************************************************/
    protected override void BuildComponents()
    {
        // создаем и настраиваем компоненты
        m_effectMaster = new EffectMaster();
        m_weaponMaster = new WeaponMaster(this);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.CLOSE_WEAPON, CloseWeapon);
        m_weaponMaster.SetWeapon(WeaponMaster.WeaponSlot.MAIN_WEAPON, Weapon);
        Weapon.SetOwner((PLAYER)m_ownerID);
        Weapon.SetUnitID(m_unitID);

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

