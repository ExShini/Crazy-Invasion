/**********************************************************************************/
// WallController класс
// Базовый класс для всех разрушимых стен в игре
//
/**********************************************************************************/
class WallController : GeneratedEnvironmentCtr
{
    public bool IsFreeAfterDestruction = true;

    /**********************************************************************************/
    // инициализация, расширенная
    //
    /**********************************************************************************/
    public override void InitializeUnit()
    {
        base.InitializeUnit();

        // так как объект создаётся не стандартным путём (производства в здании), а как элемент окружения,
        // то некому установить им m_unitID, выдаём его им принудительно
        m_unitID = ObjectFactory.GetInstance().GetUnitID();

        // регистрируем элемент окружения в контроллере
        GameObjectMapController.GetInstance().RegistEnvironment(this);
    }

    /**********************************************************************************/
    // устанавливаем компоненты для объекта
    // в данном случае объект получает урон и может быть разрушен
    //
    /**********************************************************************************/
    protected override void BuildComponents()
    {
        // создаем компоненты
        m_effectMaster = new EffectMaster();
        m_effectMaster.DisableEM();
        m_weaponMaster = new WeaponMaster(this);
        m_weaponMaster.DisableWM();
        m_drive = new EmptyDrive();
        m_armor = new BaseArmor();
        EmptyRadar radar = new EmptyRadar();
        m_radars.Add(radar);

        // настраиваем разрушаемость
        m_armor.UnitIsDown += OnDead;

        SetDefaultParameter();
    }

    /**********************************************************************************/
    // обработчик разрушения стены
    //
    /**********************************************************************************/
    protected override void OnDead(DamageData finalStrikeData)
    {
        base.OnDead(finalStrikeData);

        // если объект освобождает проход - отмечаем клетку в PathFinder как свободную
        if (IsFreeAfterDestruction)
        {
            Point myPosition = GetGlobalPosition();
            PathFinder.GetInstance().SetCellAsFree(myPosition.x, myPosition.y);

            m_collider.enabled = false;
        }
    }

    /**********************************************************************************/
    // обработчик отключения юнита, расширенный
    // в случае, если мы не предполагаем освобождение пространства после разрушения объекта
    // мы не можем реиспользовать его для повторного размещения, а значит и не можем отключить и вернуть в фабрику объектов
    //
    /**********************************************************************************/
    protected override void OnFailing()
    {
        if(IsFreeAfterDestruction)
        {
            base.OnFailing();
        }
    }
}
