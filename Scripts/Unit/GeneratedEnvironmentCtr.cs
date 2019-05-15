using UnityEngine;
using System.Collections;

/**********************************************************************************/
// GeneratedEnvironmentCtr класс
// контроллер генерируемого окружения
//
/**********************************************************************************/
public class GeneratedEnvironmentCtr : Unit
{
    // тип генерируемого объекта
    public enum ENV_TYPE : int
    {
        GARDEN = 0,
        CHURCH_ENV = 1,
        INDUSTRY_ENV = 2,
        FIELD_ENV = 3,
        MILITARY_ENV = 4,
        NONE = -1
    }

    public ENV_TYPE TYPE = ENV_TYPE.NONE;
    public bool FREE_WALKING = true;
    public int SIZE = 1;
    public Point POSITION = new Point(0, 0);

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    void Start()
    {
        InitializeUnit();
        m_state = UNIT_STATE.ACTIVE;

        BuildComponents();
    }

    /**********************************************************************************/
    // устанавливаем компоненты для объекта
    // в данном случае это компоненты пустышки, так как осмысленного поведения у
    // кустов и ящиков с мусором не предусмотрено
    //
    /**********************************************************************************/
    protected virtual void BuildComponents()
    {
        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_effectMaster.DisableEM();
        m_weaponMaster = new WeaponMaster(this);
        m_weaponMaster.DisableWM();
        m_drive = new EmptyDrive();
        m_armor = new EmptyArmor();
        EmptyRadar radar = new EmptyRadar();
        m_radars.Add(radar);
    }
}
