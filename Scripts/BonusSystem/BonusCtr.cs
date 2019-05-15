using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// BonusCtr
// базовый контроллер для всех бонусов
//
/**********************************************************************************/
public class BonusCtr : MonoBehaviour {

    public enum BONUS_TYPE
    {
        SHIELD = (int)Base.GO_TYPE.SHEILD_BONUS,
        MOCUS = (int)Base.GO_TYPE.MOCUS_BONUS,
        BLUSTER_ENERGY = (int)Base.GO_TYPE.BL_ENERGY_BONUS,
        PLASMA_GRENADE = (int)Base.GO_TYPE.PLASMA_GRENADE_BONUS,
        RGD_GRENADE = (int)Base.GO_TYPE.RGD_GRENADE_BONUS,
        SHOTGUN = (int)Base.GO_TYPE.SHOTGUN_BONUS,
        ACID_GUN = (int)Base.GO_TYPE.ACID_GUN_BONUS,
        TUREL_BUILDER = (int)Base.GO_TYPE.TURELBUILDER_BONUS,

        NO_BONUS_TYPE = -1
    }

    public enum BONUS_SOURCE
    {
        EARTH,
        ALIEN
    }

    protected enum BONUS_STATE
    {
        READY = 1,
        TAKEN = 2,

        NO_STATE
    }

    public BONUS_TYPE BonusType = BONUS_TYPE.NO_BONUS_TYPE;
    public BONUS_SOURCE BonusSource = BONUS_SOURCE.ALIEN;
    protected BONUS_STATE m_state = BONUS_STATE.READY;

    private Animator m_animator;
    private Rigidbody2D m_rb2d;

    protected Point m_position;
    protected List<PlayerController> m_playerControllers = new List<PlayerController>();

    // таймера для проверки подбора
    protected float m_chekingRate = 0.25f;
    protected float m_chekingRateCounter = 0.0f;

    protected BuildingController m_productionBase = null;

    public BuildingController ProductionBase
    {
        get { return m_productionBase; }
        set { m_productionBase = value; }
    }

    // Use this for initialization
    void Start ()
    {
        m_rb2d = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();

        if(m_rb2d == null || m_animator == null)
        {
            Debug.LogError("Components not set!");
            return;
        }

        // получем контроллеры игроков, они нам потребуются для определения подбора
        GameManager gmanager = GameManager.GetInstance();
        if(gmanager.GameMode == GameManager.GAME_MODE.SINGLE)
        {
            m_playerControllers.Add(gmanager.GetPlayer());
        }
        else if(gmanager.GameMode == GameManager.GAME_MODE.DUEL)
        {
            m_playerControllers.Add(gmanager.GetPlayers(PLAYER.PL1));
            m_playerControllers.Add(gmanager.GetPlayers(PLAYER.PL2));
        }

        m_state = BONUS_STATE.READY;
    }


    /**********************************************************************************/
    // здесь мы в первую очередь проверяем подбор бонуса игроком
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        // если бонус подобрали, завершаем анимацию и отключаем объект
        if (m_state == BONUS_STATE.TAKEN)
        {
            CheckFinalization();
        }
        else if(m_state == BONUS_STATE.READY)
        {
            CheckForPicking();
        }
        else
        {
            Debug.LogError("Wrong state!");
        }

    }

    /**********************************************************************************/
    // проверяем подбор объекта
    //
    /**********************************************************************************/
    protected virtual void CheckForPicking()
    {
        m_chekingRateCounter -= Time.deltaTime;
        if (m_chekingRateCounter <= 0.0f)
        {
            m_chekingRateCounter += m_chekingRate;

            // производим проверку подбора
            foreach (PlayerController plCtr in m_playerControllers)
            {
                m_position = GetLocalPosition();
                Point playerPosition = plCtr.GetGlobalPosition();
                if (playerPosition.IsSamePoint(m_position))
                {
                    plCtr.ApplyBonus(BonusType);
                    m_state = BONUS_STATE.TAKEN;

                    // если бонус был сброшен с орбиты
                    if(BonusSource == BONUS_SOURCE.ALIEN)
                    {
                        // оповещаем DropManager о том, что бонус был подобран
                        DropManager.GetInstance().BonusWasTaked((Base.GO_TYPE)BonusType);
                    }

                    // включаем анимацию взятия бонуса и выходим
                    m_animator.SetBool("Burn", true);

                    if(m_productionBase != null)
                    {
                        m_productionBase.ReleaseProduction(0);
                    }
                    
                    return;
                }
            }
        }
    }

    /**********************************************************************************/
    // проверяем окончание анимации и возвращаем объект в фабрику для реиспользования
    //
    /**********************************************************************************/
    protected virtual void CheckFinalization()
    {
            if (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.EndOfAnimation"))
            {
                m_animator.SetBool("Burn", false);
                gameObject.SetActive(false);

                // подобное хитрое приведение работает по той причине, что мы присваиваем одинаковые значения для enum бонусов и go_type
                ObjectFactory.GetInstance().ReturnObjectToCash(gameObject, (Base.GO_TYPE)BonusType);
            }
    }

    /**********************************************************************************/
    // обновляем статус объекта
    //
    /**********************************************************************************/
    public void ResetBonus()
    {
        m_position = GetLocalPosition();
        m_state = BONUS_STATE.READY;
        gameObject.SetActive(true);
    }

    /**********************************************************************************/
    // возвращаем локальную позицию объекта
    //
    /**********************************************************************************/
    public Point GetLocalPosition()
    {
        Point position = new Point(Mathf.FloorToInt(gameObject.transform.localPosition.x / Base.SIZE_OF_CELL), Mathf.FloorToInt(gameObject.transform.localPosition.y / Base.SIZE_OF_CELL));
        return position;
    }
}
