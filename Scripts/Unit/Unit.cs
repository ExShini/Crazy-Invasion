using UnityEngine;
using System.Collections.Generic;

/**********************************************************************************/
// Unit класс
// базовый класс для всех юнитов
//
/**********************************************************************************/
public abstract class Unit : CIGameObject
{
    [System.Serializable]
    public class UnitDrop
    {
        public Base.GO_TYPE Drop;
        public int Weight;
    }

    public CloseWeaponController CloseWeapon;

    protected List<Animator> m_animators = new List<Animator>();
    public int health = 1;
    public int scoreCost = 1;
    public UnitDrop[] Drop;
    protected UNIT_STATE m_state = UNIT_STATE.NO_STATE;

    // компоненты
    protected IArmor m_armor;
    protected IDrive m_drive;
    protected WeaponMaster m_weaponMaster;
    protected EffectMaster m_effectMaster;
    protected List<IRadar> m_radars = new List<IRadar>();

    public enum UNIT_STATE
    {
        ACTIVE = 0,
        FAILING,
        DIED,
        NO_STATE
    }

    /**********************************************************************************************/
    // свойство возвращающее состояние
    //
    /**********************************************************************************************/
    public UNIT_STATE State
    {
        get { return m_state; }
    }

    /**********************************************************************************************/
    // инициализация
    //
    /**********************************************************************************************/
    public virtual void InitializeUnit()
    {
        if (!IsInitialized())
        {
            m_animators.Add(GetComponent<Animator>());
            InitializeGO();
        }
    }

    /**********************************************************************************************/
    // инициализация дефолтного набора эффектов
    //
    /**********************************************************************************************/
    protected virtual void SetEffectsCollection()
    {
        // добавляем всем юнитам эффект урона
        DamageEffect de = new DamageEffect();
        de.ObjectUnderEffect = gameObject;
        m_effectMaster.SetEffect(de.Type, de);

        // для отображения кислотного урона
        DamageEffect ade = new DamageEffect();
        ade.ObjectUnderEffect = gameObject;
        ade.SetAcidType();
        m_effectMaster.SetEffect(ade.Type, ade);

        // эффект нанесения урона кислотой
        AcidEffect ae = new AcidEffect();
        ae.ObjectUnderEffect = gameObject;
        m_effectMaster.SetEffect(ae.Type, ae);

        // эффект замораживания
        FreezeEffect fe = new FreezeEffect();
        fe.ObjectUnderEffect = gameObject;
        m_effectMaster.SetEffect(fe.Type, fe);


        // получаем саб компоненты
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        for (int ind = 0; ind < allChildren.Length; ind++)
        {
            GameObject component = allChildren[ind].gameObject;
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
                fe.EffectAnimator = freezeEffectAnimator;
            }
        }
    }



    /**********************************************************************************/
    // функция FixedUpdate
    // основной процессинг происходит здесь
    //
    /**********************************************************************************/
    protected virtual void FixedUpdate()
    {
        if (!IsInitialized())
        {
            return;
        }

        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        // если с монстром всё ок - исполняем алгоритм
        if (m_state == UNIT_STATE.ACTIVE)
        {
            m_drive.Update();
            m_effectMaster.Update();
            m_armor.Update();

            foreach (IRadar radar in m_radars)
            {
                radar.Update();
            }

            m_weaponMaster.Update();
        }
        // проверка на окончание анимации смерти
        // после неё объект возвращается в пулл объектов фабрики
        else if (m_state == UNIT_STATE.FAILING)
        {
            OnFailing();
        }
    }



    /**********************************************************************************/
    // проверка столновения
    //
    /**********************************************************************************/
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!IsInitialized())
        {
            return;
        }
        CheckMonsterAttack(coll);
    }

    /**********************************************************************************/
    // проверка столновения
    //
    /**********************************************************************************/
    void OnCollisionStay2D(Collision2D coll)
    {
        if (!IsInitialized())
        {
            return;
        }
        CheckMonsterAttack(coll);
    }


    /**********************************************************************************/
    // функция проверяет столкновение зомби с объектами
    // если это враг, то происходит атака
    // при этом сам монстр погибает
    //
    /**********************************************************************************/
    void CheckMonsterAttack(Collision2D coll)
    {
        GameObject collidedGO = coll.gameObject;
        CheckMonsterAttack(collidedGO);
    }

    /**********************************************************************************/
    // функция проверяет столкновение монстра с объектами
    // если это враг, то происходит атака
    // при этом сам монстр погибает
    //
    /**********************************************************************************/
    void CheckMonsterAttack(GameObject collidedGO)
    {
        if (m_state == UNIT_STATE.ACTIVE)
        {
            m_weaponMaster.UseCloseWeapon(collidedGO);
        }
    }


    /**********************************************************************************************/
    // функция установки дефолтных параметров юнита
    //
    /**********************************************************************************************/
    public virtual void SetDefaultParameter()
    {
        m_state = UNIT_STATE.ACTIVE;
        m_armor.SetHealth(health);
    }

    /**********************************************************************************************/
    // функция установки дефолтных параметров юнита
    //
    /**********************************************************************************************/
    override public void ResetGObject()
    {
        // если объект был использован впервые - необходимости сбрасывать настройки нет
        if(IsInitialized())
        {
            m_drive.ResetComponent();
            m_effectMaster.ResetComponent();
            m_armor.ResetComponent();

            foreach (IRadar radar in m_radars)
            {
                radar.ResetComponent();
            }

            m_weaponMaster.ResetComponent();

            SetDefaultParameter();
            gameObject.SetActive(true);
        }
    }

    /**********************************************************************************/
    // функция получения урона юнитом
    //
    /**********************************************************************************/
    override public void ApplyDamage(DamageData damage)
    {
        if (m_state == UNIT_STATE.ACTIVE)
        {
            // объект получает урон
            m_armor.TakeDamage(damage);

            // накладываем визуальный эффект
            EffectDescriptor dE = new EffectDescriptor();
            if (damage.DamageType == DamageData.DAMAGE_TYPE.ACID)
            {
                dE.Type = UnitEffect.EFFECT_TYPE.DAMAGE_ACID;
            }
            else
            {
                dE.Type = UnitEffect.EFFECT_TYPE.DAMAGE;
            }
            dE.Responsibility = EffectDescriptor.EffectResponsibility.REQUIRED;
            ApplyEffect(dE);

            // если есть возможность нанести ответный урон - делаем это
            if (damage.ExpectResponce == DamageData.RESPONSE.EXPECTED)
            {
                m_weaponMaster.UseCloseWeapon(damage.Damager, true);
            }
        }
    }

    /**********************************************************************************/
    // функция применения эффектов
    //
    /**********************************************************************************/
    public override void ApplyEffect(EffectDescriptor effect)
    {
        m_effectMaster.ApplyEffect(effect);
    }

    /**********************************************************************************/
    // данное переопределение передаёт контроллеру данные о направлении движении и сохраняет
    // информацию для дальнейшего использования
    //
    /**********************************************************************************/
    override public void MoveGObject(Base.DIREC direction)
    {
        SetMovementDirection(direction);
        base.MoveGObject(direction);
    }

    /**********************************************************************************/
    // устанавливаем направление движения
    //
    /**********************************************************************************/
    public void SetMovementDirection(Base.DIREC direction)
    {
        m_movementDirection = direction;
        foreach(var animator in m_animators)
        {
            animator.SetInteger("direction", (int)direction);
        }
    }


    /**********************************************************************************/
    // обработчик смерти
    // может переопределяться для специальной обработки смерти, к примеру боссами
    // данная функция должна быть подключена к m_armor компоненту
    //
    /**********************************************************************************/
    protected virtual void OnDead(DamageData finalStrikeData)
    {
        m_state = UNIT_STATE.FAILING;
        foreach (var animator in m_animators)
        {
            animator.SetTrigger("isFailing");
        }

        // сообщаем котроллеру о том, что нас можно не учитывать
        GameObjectMapController.GetInstance().ReleaseUnitFromMap(this);

        // сообщаем зданию производившему это существо о  том, что юнит погиб и оно может производить следующий объект
        if (m_productionBase != null)
        {
            m_productionBase.ReleaseProduction(m_unitID);
        }

        // проверяем дроп
        CheckUnitDrop();

        // увеличиваем счётчик очков игрока за убийство
        int killerID = finalStrikeData.Damager.Owner;
        if (killerID != Owner)
        {
            GameManager.GetInstance().IncreasePlayerScoreUnitLose((PLAYER)killerID, scoreCost);
        }
    }

    /**********************************************************************************/
    // функция проверяет наличие возможного дропа с юнита
    //
    /**********************************************************************************/
    private void CheckUnitDrop()
    {
        // проверяем дроп
        if (Drop != null)
        {
            if (Drop.Length > 0)
            {
                // получаем общий вес набора потенциального лута
                int weightOfSet = 0;
                for (int i = 0; i < Drop.Length; i++)
                {
                    weightOfSet += Drop[i].Weight;
                }

                // выбираем объект для дропа
                int random = Random.Range(0, weightOfSet + 1);
                Base.GO_TYPE typeOfDrop = Base.GO_TYPE.NONE_TYPE;
                weightOfSet = 0;
                for (int i = 0; i < Drop.Length; i++)
                {
                    weightOfSet += Drop[i].Weight;
                    typeOfDrop = Drop[i].Drop;

                    if (weightOfSet >= random)
                    {
                        break;
                    }
                }

                // создаём дроп
                if (typeOfDrop != Base.GO_TYPE.NONE_TYPE)
                {
                    Vector2 dropPosition = GetGlobalPosition().GetUnityPoint() + new Vector2(Base.HALF_OF_CELL, Base.HALF_OF_CELL);
                    GameObject production = ObjectFactory.GetInstance().CreateGObject(dropPosition, Base.DIREC.DOWN, typeOfDrop, false);
                }
            }
        }
    }

    /**********************************************************************************/
    // обработчик отключения юнита
    // дожидается окончания анимации смерти
    // снимает все наложенные на существо эффекты и везвращает в пулл ObjectFactory
    //
    /**********************************************************************************/
    protected virtual void OnFailing()
    {
        // предполагаем, что все аниматоры синхронизованы по тайменгам между собой
        // используем первый аниматор для индикации конца анимации
        Animator animator = m_animators[0];

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.EndOfAnimation"))
        {
            gameObject.SetActive(false);
            m_state = UNIT_STATE.DIED;

            // деактивируем все эффекты
            m_effectMaster.DeactivateEffects();

            // ресетим модификаток скорости
            speedMultiplier = 1.0f;
            ObjectFactory.GetInstance().ReturnObjectToCash(gameObject, GOType);       // возвращаем объект в кеш фабрики объектов
        }
    }
}


