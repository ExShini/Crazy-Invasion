using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// Bullet класс
// базовый класс для всех снарядов
//
/**********************************************************************************/
public class Bullet : CIGameObject
{
    public enum BULLET_STATE : int
    {
        FLY,
        BURN,
    };

    protected BULLET_STATE m_state;
    protected Animator m_animator;
    protected float m_originalSpeed;

    public int damage = 1;
    public string ShotSoundKey = "";
    public string BurstSoundKey = "";
    public float ShakePower = 0f;

    [HideInInspector]
    public int OwnerUnitID = -1;


    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    public void Start()
    {
        m_animator = GetComponent<Animator>();
        m_originalSpeed = speed;
        InitializeGO();

        PlayShotSoundEffect();
    }

    /**********************************************************************************/
    // функция сброски параметров к дефолтным
    //
    /**********************************************************************************/
    override public void ResetGObject()
    {
        m_state = BULLET_STATE.FLY;
        gameObject.SetActive(true);
        speed = m_originalSpeed;
        m_collider.enabled = true;

        PlayShotSoundEffect();
    }

    /**********************************************************************************/
    // воспроизводим звук выстрела
    //
    /**********************************************************************************/
    protected void PlayShotSoundEffect()
    {
        if (ShotSoundKey != "")
        {
            GameAudioManager.Instance.PlaySound(ShotSoundKey);
        }
    }

    /**********************************************************************************/
    // воспроизводим звук столкновения/взрыва
    //
    /**********************************************************************************/
    protected void PlayBurstSoundEffect()
    {
        if (BurstSoundKey != "")
        {
            GameAudioManager.Instance.PlaySound(BurstSoundKey);
        }
    }

    /**********************************************************************************/
    // процессинг
    //
    /**********************************************************************************/
    protected virtual void FixedUpdate()
    {
        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            if(m_rb2d.velocity.sqrMagnitude != 0.0f)
            {
                m_rb2d.velocity = new Vector2(0, 0);
            }
            return;
        }

        if (m_state == BULLET_STATE.BURN)
        {
            if (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.EndOfAnimation"))
            {
                m_animator.SetBool("Burn", false);
                gameObject.SetActive(false);
                ObjectFactory.GetInstance().ReturnObjectToCash(gameObject, GOType);
            }
        }
        else if (IsInitialized())
        {
            MoveGObject(base.m_direction);
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

        CheckBulletBurn(coll);

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

        CheckBulletBurn(coll);
    }

    /**********************************************************************************/
    // функция проверки столкновения
    // если цель игрок, юнит или здание/препятствие - происходит взрыв снаряда
    //
    /**********************************************************************************/
    virtual protected void CheckBulletBurn(Collision2D coll)
    {
        if (m_state == BULLET_STATE.FLY)
        {
            string otherObjTag = coll.gameObject.tag;
            if (otherObjTag == "Player" || otherObjTag == "wall" || otherObjTag == "Building" || otherObjTag == "Unit")
            {

                bool unitStateCheck = true;
                // проверяем, живая ли цель, мёртвых атоковать не будем
                if (otherObjTag == "Unit")
                {
                    Unit collidedUnit = coll.gameObject.GetComponent<Unit>();
                    if (collidedUnit == null)
                    {
                        Debug.LogError("CheckMonsterAttack: unit have no 'unit' controller!");
                        return;
                    }

                    // проверяем состояние юнита + добавляет защиту от самострела (когда снаряд сталкивается с самим стрелком)
                    unitStateCheck = collidedUnit.State == Unit.UNIT_STATE.ACTIVE && OwnerUnitID != collidedUnit.ID;
                }
                else if (otherObjTag == "Player")
                {
                    // отключаем нанесение урона самому себе
                    CIGameObject collidedUnit = coll.gameObject.GetComponent<CIGameObject>();
                    if (Owner == collidedUnit.Owner)
                    {
                        unitStateCheck = false;
                    }
                }

                // если цель пригодна для анигиляции - наносим урон ^_^
                if (unitStateCheck)
                {
                    ApplyBilletEffect(coll);
                    BurnBullet();

                    // отключаем коллайдер и зануляем скорость
                    m_collider.enabled = false;
                    speed = 0.0f;
                    m_rb2d.velocity = new Vector2(0, 0);
                }
            }
        }
    }

    /**********************************************************************************/
    // переключаем нашу пулю в состояние "взрыва"
    //
    /**********************************************************************************/
    protected void BurnBullet()
    {
        CameraControllerDuelMode.ShakeCamera(ShakePower);   // сотрясаем камеру, если нужно
        m_state = BULLET_STATE.BURN;
        m_animator.SetBool("Burn", true);
        PlayBurstSoundEffect();
    }

    /**********************************************************************************/
    // функция применения эффекта снаряда
    // это может быть как урон (справедливо для большинства пуль), так и специальные эффекты, такие как замедление/яд и другие (реализуется в конкретных наследниках)
    //
    /**********************************************************************************/
    virtual protected void ApplyBilletEffect(Collision2D coll)
    {
        DamageData damageData = new DamageData(damage, DamageData.DAMAGE_TYPE.PHYSICAL, this, DamageData.RESPONSE.NOT_EXPECTED);
        coll.gameObject.SendMessage("ApplyDamage", damageData, SendMessageOptions.DontRequireReceiver);
    }

    /**********************************************************************************/
    // функция получения урона юнитом
    // NOTE: пуля не получает никаих повреждений, на то она и пуля :)
    //
    /**********************************************************************************/
    override public void ApplyDamage(DamageData damage)
    {
    }

    /**********************************************************************************/
    // функция применения эффектов
    // эффекты не применяются
    //
    /**********************************************************************************/
    public override void ApplyEffect(EffectDescriptor effect)
    {
    }
}

