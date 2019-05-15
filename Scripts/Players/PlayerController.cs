using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// PlayerController класс
// контроллер игрока
/**********************************************************************************/
public class PlayerController : CIGameObject
{
    protected enum FIRE_STATE : int
    {
        REDAY = 0,
        FIRE,
        RECHARGE
    };

    public enum WEAPON_SLOT
    {
        MAIN = 0,
        CAPTURE = 1,
        EXLOSION = 2,
        SPECIAL = 3,
        ALL = 4
    }

    private PlayerInputCtr m_inputCtr = null;  // input controller
    private ShieldController m_shieldCtr = null;

    // набор активного вооружения
    protected ClassicWeaponCtr m_mainWeaponCtr = null;
    protected ClassicWeaponCtr m_captureWeaponCtr = null;
    protected ClassicWeaponCtr m_explosionWeaponCtr = null;
    protected ClassicWeaponCtr m_specialWeaponCtr = null;

    // коллекции потенциально доступного вооружения
    List<ClassicWeaponCtr> m_mainWeaponCollection = new List<ClassicWeaponCtr>();
    List<ClassicWeaponCtr> m_captureWeaponCollection = new List<ClassicWeaponCtr>();
    List<ClassicWeaponCtr> m_explosionWeaponCollection = new List<ClassicWeaponCtr>();
    List<ClassicWeaponCtr> m_specialWeaponCollection = new List<ClassicWeaponCtr>();

    // списки идентификаторов вооружения
    List<WEAPON> m_mainWeaponIds = new List<WEAPON>();
    List<WEAPON> m_captureWeaponIds = new List<WEAPON>();
    List<WEAPON> m_explosionWeaponIds = new List<WEAPON>();
    List<WEAPON> m_specialWeaponIds = new List<WEAPON>();

    private Animator m_animator;

    //параметры настройки игроков
    public PLAYER playerId = PLAYER.NO_PLAYER;
    public int Health = 10;
    public int ShieldMaxPoints = 3;
    public float ShieldDuration = 3.0f;

    protected Dictionary<UnitEffect.EFFECT_TYPE, UnitEffect> m_effects = new Dictionary<UnitEffect.EFFECT_TYPE, UnitEffect>();
    protected bool m_isActive = true;

    /**********************************************************************************/
    // функция инициализации игрока
    // собираем контроллер, ищем все необходимые элементы
    //
    /**********************************************************************************/
    void Start()
    {
        m_inputCtr = new PlayerInputCtr();
        m_animator = GetComponent<Animator>();

        // регистрируемся как игрок
        GameManager.GetInstance().RegisterPlayer(this);
        // настраиваем input контроллер
        m_inputCtr.DetermineAxisNameViaPlayerId((int)playerId);

        // настраиваем эффекты
        SetEffects();

        // получаем внутренние компоненты
        GetInnerComponents();

        // настраиваем оружие
        SetWeaponToSlots();

        // сообщаем UI о нашем запасе здоровья
        UIController.GetInstance().SetPlayerHealth((int)playerId, Health, ShieldMaxPoints);

        this.Owner = (int)playerId;
        m_unitID = -(int)playerId;

        InitializeGO();

        // производим первое обращение к оружейным слотам для синхронизации их с UI
        UpdateWeapon((int)Base.DIREC.DOWN);
    }

    /**********************************************************************************/
    // работаем с внутренними компонентами объекта игрока
    // получаем ссылку на ShieldController и настраиваем FreezeEffect
    //
    /**********************************************************************************/
    private void GetInnerComponents()
    {
        // получаем саб компоненты
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        for (int ind = 0; ind < allChildren.Length; ind++)
        {
            GameObject component = allChildren[ind].gameObject;
            if (component.tag == "ShieldGen")
            {
                m_shieldCtr = component.GetComponent<ShieldController>();

                if (m_shieldCtr == null)
                {
                    Debug.Log("ERROR! Shield ctr is NULL!!!");
                    return;
                }

                // устанавливаем продолжительность работы щита
                m_shieldCtr.ShieldDuration = this.ShieldDuration;
                m_shieldCtr.ShieldMaxPower = this.ShieldMaxPoints;
            }

            // настраваем FreezeEffect
            if (component.tag == "FreezeEffect")
            {
                Animator freezeEffectAnimator = component.GetComponent<Animator>();
                if (freezeEffectAnimator == null)
                {
                    Debug.Log("ERROR! freezeEffect Animator is NULL!!!");
                    return;
                }

                // вручаем ему аниматор
                FreezeEffect fe = m_effects[UnitEffect.EFFECT_TYPE.ACID] as FreezeEffect;
                fe.EffectAnimator = freezeEffectAnimator;
            }
        }
    }

    /**********************************************************************************/
    // настраиваем эффекты
    //
    /**********************************************************************************/
    private void SetEffects()
    {
        // эффект замораживания
        FreezeEffect fe = new FreezeEffect();
        fe.ObjectUnderEffect = gameObject;
        m_effects[fe.Type] = fe;
        m_effects[UnitEffect.EFFECT_TYPE.ACID] = fe;
    }


    /**********************************************************************************/
    // функция применения эффектов
    //
    /**********************************************************************************/
    public override void ApplyEffect(EffectDescriptor effect)
    {
        UnitEffect.EFFECT_TYPE eType = effect.Type;
        bool weHaveEffect = m_effects.ContainsKey(eType);

        if (weHaveEffect)
        {
            m_effects[eType].EffectProducer = effect.EffectProducer;
            m_effects[eType].Activate(effect.Value);
        }
        else if (effect.Responsibility == EffectDescriptor.EffectResponsibility.REQUIRED)
        {
            Debug.LogError("We cann't apply effect: " + eType + " !");
        }
    }

    /**********************************************************************************/
    // устанавливаем оружие в соответсвующие им слоты
    //
    /**********************************************************************************/
    private void SetWeaponToSlots()
    {
        // настраиваем оружие
        // тентакли ^_^
        ClassicWeaponCtr ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.TENTAKLES, playerId);
        m_mainWeaponCollection.Add(ctr);
        m_captureWeaponCollection.Add(ctr);
        m_explosionWeaponCollection.Add(ctr);
        m_specialWeaponCollection.Add(ctr);

        m_mainWeaponIds.Add(WEAPON.TENTAKLES);
        m_captureWeaponIds.Add(WEAPON.TENTAKLES);
        m_explosionWeaponIds.Add(WEAPON.TENTAKLES);
        m_specialWeaponIds.Add(WEAPON.TENTAKLES);


        // кислотная пушка
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.ACID_GUN, playerId);
        ctr.ChargeAmmo(ctr.NumberOfBullet);
        m_mainWeaponCollection.Add(ctr);
        m_mainWeaponIds.Add(WEAPON.ACID_GUN);

        // дробовик
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.SHOTGUN, playerId);
        ctr.ChargeAmmo(0);
        m_mainWeaponCollection.Add(ctr);
        m_mainWeaponIds.Add(WEAPON.SHOTGUN);

        // бластер
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.BLUSTER, playerId);
        ctr.ChargeAmmo(0);
        m_mainWeaponCollection.Add(ctr);
        m_mainWeaponIds.Add(WEAPON.BLUSTER);

        // слизь
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.MOCUS, playerId);
        ctr.ChargeAmmo(3);
        m_captureWeaponCollection.Add(ctr);
        m_captureWeaponIds.Add(WEAPON.MOCUS);

        // гранаты РГД
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.RGD_GRENADE, playerId);
        ctr.ChargeAmmo(0);
        m_explosionWeaponCollection.Add(ctr);
        m_explosionWeaponIds.Add(WEAPON.RGD_GRENADE);

        // гранаты плазменные
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.PLASMA_GRENADE, playerId);
        ctr.ChargeAmmo(0);
        m_explosionWeaponCollection.Add(ctr);
        m_explosionWeaponIds.Add(WEAPON.PLASMA_GRENADE);

        // строитель турелей
        ctr = WeaponLibrary.GetInstance().GetWeaponById(WEAPON.TUREL_BUILDER, playerId);
        ctr.ChargeAmmo(0);
        m_specialWeaponCollection.Add(ctr);
        m_specialWeaponIds.Add(WEAPON.TUREL_BUILDER);

        // обновляем оружейные слоты
        UpdateWeaponSlot(WEAPON_SLOT.ALL);
    }

    /**********************************************************************************/
    // обновляем оружейную систему
    //
    /**********************************************************************************/
    protected void UpdateWeaponSlot(WEAPON_SLOT slot)
    {
        // перебираем коллекцию вооружения с конца к началу
        // первое (наиболее технологичное) оружие с боезапасом устанавливается в слот

        // определяемся со списком оружейных слотов на обновление
        List<WEAPON_SLOT> slotsToUpdate;
        if (slot == WEAPON_SLOT.ALL)
        {
            slotsToUpdate = new List<WEAPON_SLOT> { WEAPON_SLOT.MAIN, WEAPON_SLOT.CAPTURE, WEAPON_SLOT.EXLOSION, WEAPON_SLOT.SPECIAL };
        }
        else
        {
            slotsToUpdate = new List<WEAPON_SLOT> { slot };
        }

        // производим обновление
        foreach (var updatedSlot in slotsToUpdate)
        {
            // выбираем элементы, с которыми нам предстоит работать
            List<ClassicWeaponCtr> weaponCollerction;
            List<WEAPON> weaponIdsCollection;
            PlayerInputCtr.CTR_KEY KeyToBlock;

            switch (updatedSlot)
            {
                case WEAPON_SLOT.MAIN:
                    weaponCollerction = m_mainWeaponCollection;
                    weaponIdsCollection = m_mainWeaponIds;
                    KeyToBlock = PlayerInputCtr.CTR_KEY.FIRE_1;
                    break;
                case WEAPON_SLOT.CAPTURE:
                    weaponCollerction = m_captureWeaponCollection;
                    weaponIdsCollection = m_captureWeaponIds;
                    KeyToBlock = PlayerInputCtr.CTR_KEY.FIRE_2;
                    break;
                case WEAPON_SLOT.EXLOSION:
                    weaponCollerction = m_explosionWeaponCollection;
                    weaponIdsCollection = m_explosionWeaponIds;
                    KeyToBlock = PlayerInputCtr.CTR_KEY.FIRE_3;
                    break;
                case WEAPON_SLOT.SPECIAL:
                    weaponCollerction = m_specialWeaponCollection;
                    weaponIdsCollection = m_specialWeaponIds;
                    KeyToBlock = PlayerInputCtr.CTR_KEY.FIRE_4;
                    break;
                default:
                    Debug.LogError("Unexpected weapon slot: " + updatedSlot);
                    return;
            }

            // итерируем по всеми списку вооружения в поисках подходящего элемента
            for (int i = weaponCollerction.Count - 1; i >= 0; i--)
            {
                ClassicWeaponCtr ctr = weaponCollerction[i];
                if (ctr.State != WeaponController.WEAPON_STATE.EMPTY)
                {
                    switch (updatedSlot)
                    {
                        case WEAPON_SLOT.MAIN:
                            m_mainWeaponCtr = ctr;
                            break;
                        case WEAPON_SLOT.CAPTURE:
                            m_captureWeaponCtr = ctr;
                            break;
                        case WEAPON_SLOT.EXLOSION:
                            m_explosionWeaponCtr = ctr;
                            break;
                        case WEAPON_SLOT.SPECIAL:
                            m_specialWeaponCtr = ctr;
                            break;
                    }

                    UIController.GetInstance().SetWeaponInSlot((int)playerId, updatedSlot, weaponIdsCollection[i]);
                    m_inputCtr.BlockButton(KeyToBlock);
                    break;
                }
            }
        }
    }

    /**********************************************************************************/
    // функция получения урона
    //
    /**********************************************************************************/
    override public void ApplyDamage(DamageData damage)
    {
        if (m_isActive)
        {
            ShieldController.SHIELD_STATE shiledState = m_shieldCtr.State;

            // аварийный щит поглощает весь урон в течении короткого времени
            if (shiledState == ShieldController.SHIELD_STATE.EMERGENCY_ACTIVE)
            {
                // игнорируем урон
            }
            else
            {
                int damageToLose = damage.Damage;
                int shieldToLose = damage.Damage;

                // пробуем защитится щитом
                if (shiledState == ShieldController.SHIELD_STATE.ACTIVE)
                {
                    damageToLose = m_shieldCtr.TakeDamage(damageToLose);
                    shieldToLose -= damageToLose;   // разница - кол-во потерянных очков прочности щита

                    // обновляем UI
                    UIController.GetInstance().LosePlayerShield((int)playerId, shieldToLose);
                }

                // если удалось погасить урон щитом - выходим
                if (damageToLose == 0)
                {
                    return;
                }

                // объект получает урон
                Health -= damageToLose;

                // сообщаем UI о потере здоровья
                UIController.GetInstance().LosePlayerHealth((int)playerId, damageToLose);

                if (Health <= 0)
                {
                    m_animator.SetTrigger("isLose");
                    m_isActive = false;

                    TargetController.GetInstance().TargetIsDead(gameObject);
                }

                // включаем щит
                m_shieldCtr.ActivateEmergencyShield();
            }
        }
    }

    /**********************************************************************************/
    // функция применяет бонус/модификатор к игроку
    //
    /**********************************************************************************/
    public void ApplyBonus(BonusCtr.BONUS_TYPE bonusType)
    {
        switch (bonusType)
        {
            // заряжаем щит
            case BonusCtr.BONUS_TYPE.SHIELD:
                int oldNumOfShieldPoints = m_shieldCtr.ShieldPower;
                m_shieldCtr.IncreasePower(1);   // пробуем увеличить силу щита на 1
                int pointsDiff = m_shieldCtr.ShieldPower - oldNumOfShieldPoints;    // выясняем разницу в зарядке щита, иногда она может и не меняться (ибо есть максимум)
                UIController.GetInstance().ChargePlayerShield((int)playerId, pointsDiff);
                break;

            // заряжаем слизь
            case BonusCtr.BONUS_TYPE.MOCUS:
                ChargeWeapon(WEAPON_SLOT.CAPTURE, WEAPON.MOCUS, 3);
                break;

            // заряжаем кислотную пушку
            case BonusCtr.BONUS_TYPE.ACID_GUN:
                ChargeWeapon(WEAPON_SLOT.MAIN, WEAPON.ACID_GUN, 5);
                break;

            // заряжаем бластер
            case BonusCtr.BONUS_TYPE.BLUSTER_ENERGY:
                ChargeWeapon(WEAPON_SLOT.MAIN, WEAPON.BLUSTER, 50);
                break;

            // дробовик
            case BonusCtr.BONUS_TYPE.SHOTGUN:
                ChargeWeapon(WEAPON_SLOT.MAIN, WEAPON.SHOTGUN, 25);
                break;

            // добавляем гранату РГД
            case BonusCtr.BONUS_TYPE.RGD_GRENADE:
                ChargeWeapon(WEAPON_SLOT.EXLOSION, WEAPON.RGD_GRENADE, 2);
                break;

            // добавляем плазменную гранату
            case BonusCtr.BONUS_TYPE.PLASMA_GRENADE:
                ChargeWeapon(WEAPON_SLOT.EXLOSION, WEAPON.PLASMA_GRENADE, 1);
                break;

            // добавляем турельку
            case BonusCtr.BONUS_TYPE.TUREL_BUILDER:
                ChargeWeapon(WEAPON_SLOT.SPECIAL, WEAPON.TUREL_BUILDER, 1);
                break;

            default:
                Debug.LogError("Wrong bonus type!");
                break;
        }
    }

    /**********************************************************************************/
    // функция зарядки оружия
    //
    /**********************************************************************************/
    void ChargeWeapon(WEAPON_SLOT slot, WEAPON weaponID, int ammo)
    {
        List<WEAPON> weaponIds = null;
        List<ClassicWeaponCtr> weaponCtrs = null;

        switch (slot)
        {
            case WEAPON_SLOT.MAIN:
                weaponIds = m_mainWeaponIds;
                weaponCtrs = m_mainWeaponCollection;
                break;
            case WEAPON_SLOT.CAPTURE:
                weaponIds = m_captureWeaponIds;
                weaponCtrs = m_captureWeaponCollection;
                break;
            case WEAPON_SLOT.EXLOSION:
                weaponIds = m_explosionWeaponIds;
                weaponCtrs = m_explosionWeaponCollection;
                break;
            case WEAPON_SLOT.SPECIAL:
                weaponIds = m_specialWeaponIds;
                weaponCtrs = m_specialWeaponCollection;
                break;
        }

        // пробуем найти нужное оружие и устанавливаем его индекс
        int index = 0;
        bool wasFind = false;
        for (; index < weaponIds.Count; index++)
        {
            WEAPON id = weaponIds[index];
            if (id == weaponID)
            {
                wasFind = true;
                break;
            }
        }

        if (!wasFind)
        {
            Debug.LogError("We cant charge weapon: " + weaponID + " in slot: " + slot);
            return;
        }

        // заряжаем оружие и обновляем UI
        ClassicWeaponCtr wCtr = weaponCtrs[index];
        wCtr.ChargeAmmo(ammo);
        UpdateWeaponSlot(slot);
    }

    /**********************************************************************************/
    // функция процессинга
    //
    /**********************************************************************************/
    void FixedUpdate()
    {
        // проверяем нажатие кнопки паузы
        bool pouseBtnIsPressed = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.PAUSE);
        if (pouseBtnIsPressed)
        {
            // если истина - переключаемся
            GameManager.SwitchPauseMode();
            m_inputCtr.BlockButton(PlayerInputCtr.CTR_KEY.PAUSE);
        }

        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        if (IsInitialized() && m_isActive)
        {
            int direction;
            bool isMoved;
            Vector2 movement = m_inputCtr.GetDirection(out direction, out isMoved);

            MoveGObject(movement);

            m_animator.SetBool("isMoving", isMoved);
            m_animator.SetInteger("direction", direction);

            // обрабатываем остальной инпут

            // оружие
            UpdateWeapon(direction);

            // обрабатываем все наложенные эффекты
            foreach (var effect in m_effects)
            {
                effect.Value.Process();
            }
        }
    }

    /**********************************************************************************/
    // UpdateWeapon обновляет состояние вооружения игрока
    //
    /**********************************************************************************/
    private void UpdateWeapon(int direction)
    {
        // "обрабатывваем" основное оружие
        bool isFired = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.FIRE_1);
        if (isFired)
        {
            m_mainWeaponCtr.Fire();
        }
        else
        {
            m_mainWeaponCtr.StopFire();
        }

        m_mainWeaponCtr.UpdateWeaponState(m_rb2d.position, (Base.DIREC)direction);
        m_mainWeaponCtr.UpdateCtrStatuses(WEAPON_SLOT.MAIN);
        if (m_mainWeaponCtr.State == WeaponController.WEAPON_STATE.EMPTY)
        {
            UpdateWeaponSlot(WEAPON_SLOT.MAIN);
        }

        // оржие захвата
        bool isFired2 = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.FIRE_2);
        if (isFired2)
        {
            m_captureWeaponCtr.Fire();
        }
        else
        {
            m_captureWeaponCtr.StopFire();
        }

        m_captureWeaponCtr.UpdateWeaponState(m_rb2d.position, (Base.DIREC)direction);
        m_captureWeaponCtr.UpdateCtrStatuses(WEAPON_SLOT.CAPTURE);
        if (m_captureWeaponCtr.State == WeaponController.WEAPON_STATE.EMPTY)
        {
            UpdateWeaponSlot(WEAPON_SLOT.CAPTURE);
        }

        // взрывчатка
        bool isFired3 = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.FIRE_3);
        if (isFired3)
        {
            m_explosionWeaponCtr.Fire();
        }
        else
        {
            m_explosionWeaponCtr.StopFire();
        }

        m_explosionWeaponCtr.UpdateWeaponState(m_rb2d.position, (Base.DIREC)direction);
        m_explosionWeaponCtr.UpdateCtrStatuses(WEAPON_SLOT.EXLOSION);
        if (m_explosionWeaponCtr.State == WeaponController.WEAPON_STATE.EMPTY)
        {
            UpdateWeaponSlot(WEAPON_SLOT.EXLOSION);
        }

        // Специальное
        bool isFired4 = m_inputCtr.IsKeyPressed(PlayerInputCtr.CTR_KEY.FIRE_4);
        if (isFired4)
        {
            m_specialWeaponCtr.Fire();
        }
        else
        {
            m_specialWeaponCtr.StopFire();
        }

        m_specialWeaponCtr.UpdateWeaponState(m_rb2d.position, (Base.DIREC)direction);
        m_specialWeaponCtr.UpdateCtrStatuses(WEAPON_SLOT.SPECIAL);
        if (m_specialWeaponCtr.State == WeaponController.WEAPON_STATE.EMPTY)
        {
            UpdateWeaponSlot(WEAPON_SLOT.SPECIAL);
        }
    }


    /**********************************************************************************/
    // ResetGObject не используется для игрока
    // возможно в будующем это изменится
    //
    /**********************************************************************************/
    override public void ResetGObject()
    {
        // пустой, ресетить игрока не надо
    }
}
