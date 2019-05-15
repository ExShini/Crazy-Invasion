using UnityEngine;
using System.Collections;
/**********************************************************************************/
// GMovingObject
// контроллер дроп-пода
//
/**********************************************************************************/
public class DropPodCtr : CIGameObject
{
    enum DR_STATE
    {
        ACTIVE,
        DROPPED,
        FINISHED,
        NO_STATE
    }

    public float DropTime = 4.0f;
    public string BurstSoundKey = "dropPod_explosion";
    public ParticleSystem BurstSystem;
    protected float m_currentDropTime = 0.0f;
    protected Point m_dropPosition;
    Base.GO_TYPE m_dropType = Base.GO_TYPE.NONE_TYPE;
    DR_STATE m_dropState = DR_STATE.NO_STATE;

    protected Animator m_animator;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    public void Start()
    {
        m_animator = GetComponent<Animator>();
        InitializeGO();
    }

    /**********************************************************************************/
    // падаем в позицию
    //
    /**********************************************************************************/
    public void DropInPosition(Point dropPosition)
    {
        m_dropPosition = dropPosition;
        m_currentDropTime = DropTime;

        Vector2 startPosition = dropPosition.GetUnityPoint();
        startPosition.y += (int)(speed * DropTime) + Base.HALF_OF_CELL;
        startPosition.x += Base.HALF_OF_CELL;

        transform.position = startPosition;
        m_dropState = DR_STATE.ACTIVE;
    }

    /**********************************************************************************/
    // устанавливаем тип сбрасываемого дропа
    //
    /**********************************************************************************/
    public void SetDropType(Base.GO_TYPE dropType)
    {
        m_dropType = dropType;
    }

    /**********************************************************************************/
    // функция не получения урона :)
    //
    /**********************************************************************************/
    public override void ApplyDamage(DamageData damage)
    {
        // не получаем никакого урона    
    }

    /**********************************************************************************/
    // сбрасываем настройки к дефолтным
    //
    /**********************************************************************************/
    public override void ResetGObject()
    {
        m_currentDropTime = DropTime;
        gameObject.SetActive(true);
    }


    /**********************************************************************************/
    // процессинг дроп-пода
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        // приостанавливаем всё на время паузы
        if(GameManager.GamePaused)
        {
            if (m_rb2d.velocity.sqrMagnitude != 0.0f)
            {
                m_rb2d.velocity = new Vector2(0, 0);
            }
            return;
        }


        // падаем вниз
        if(m_dropState == DR_STATE.ACTIVE)
        {
            m_currentDropTime -= Time.deltaTime;
            if(m_currentDropTime <= 0.0f)
            {
                m_dropState = DR_STATE.DROPPED;
                m_animator.SetBool("Burn", true);

                // включаем звуковой эффект и анимацию частиц
                if (BurstSoundKey != "")
                {
                    GameAudioManager.Instance.PlaySound(BurstSoundKey);
                }
                BurstSystem.Play();

                // трясём камеру
                CameraControllerDuelMode.ShakeCamera();
            }
            else
            {
                MoveGObject(Base.DIREC.DOWN);
            }
        }
        // проверка на окончание анимации
        // после неё объект возвращается в пулл объектов фабрики
        // и создаем объект дропа
        else if (m_dropState == DR_STATE.DROPPED)
        {
            if (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.EndOfAnimation"))
            {
                m_animator.SetBool("Burn", false);
                gameObject.SetActive(false);
                m_dropState = DR_STATE.FINISHED;

                GameObject go = ObjectFactory.GetInstance().CreateGObject(transform.position, Base.DIREC.DOWN, m_dropType, false);  // создаём дроп

                // устанавливаем производстенную базу в null, в случае с дропом этот механизм не используется
                BonusCtr bctr = go.GetComponent<BonusCtr>();
                if(bctr != null)
                {
                    bctr.ProductionBase = null;
                }

                ObjectFactory.GetInstance().ReturnObjectToCash(gameObject, GOType);       // возвращаем объект дроп-пода в кеш фабрики объектов
                
            }
        }
    }

    /**********************************************************************************/
    // эффекты
    //
    /**********************************************************************************/
    public override void ApplyEffect(EffectDescriptor effect)
    {
        // эффекты не применяются
    }
}
