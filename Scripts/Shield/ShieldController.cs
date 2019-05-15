using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************************/
// класс ответсвенный за контроль анимации щита и его состояния
//
/**********************************************************************************************/
public class ShieldController : MonoBehaviour
{

    public enum SHIELD_STATE
    {
        ACTIVE,
        EMERGENCY_ACTIVE,
        DISABLED
    };


    private Animator m_animator;
    private SHIELD_STATE m_shieldState = SHIELD_STATE.DISABLED;
    private float m_shiledDuration = 0.0f;
    private float m_currentShieldTimer = 0.0f;

    private int m_shieldPower = 0;
    private int m_maxPower = 3;

    // ********
    // СВОЙСТВА:

    // текущее состояние щита
    public SHIELD_STATE State
    {
        get { return m_shieldState; }
    }

    // длительность работы щита
    public float ShieldDuration
    {
        get { return m_shiledDuration; }
        set { m_shiledDuration = value; }
    }

    // текущая сила основного щита
    public int ShieldPower
    {
        get { return m_shieldPower; }
        set { m_shieldPower = value; }
    }

    // максимальная сила основного щита
    public int ShieldMaxPower
    {
        get { return m_maxPower; }
        set { m_maxPower = value; }
    }

    // ********
    // МЕТОДЫ:


    /**********************************************************************************************/
    // инициализация (Unity)
    //
    /**********************************************************************************************/
    void Start()
    {
        m_animator = GetComponent<Animator>();

    }

    /**********************************************************************************************/
    // функция процессинга
    //
    /**********************************************************************************************/
    private void FixedUpdate()
    {
        if(GameManager.GamePaused)
        {
            // останавливаем все процессы во время паузы
            return;
        }

        if (m_shieldState == SHIELD_STATE.EMERGENCY_ACTIVE)
        {
            m_currentShieldTimer -= Time.deltaTime;
            if (m_currentShieldTimer <= 0)
            {
                DisableShield();
            }
        }

    }

    /**********************************************************************************************/
    // увеличиваем мощность щита
    //
    /**********************************************************************************************/
    public void IncreasePower(int powerToAdd)
    {
        m_shieldPower += powerToAdd;

        // проверяем на достижения лимита мощности
        if(m_shieldPower > m_maxPower)
        {
            m_shieldPower = m_maxPower;
        }

        // активируем щит
        if(m_shieldPower > 0)
        {
            if (m_shieldState == SHIELD_STATE.DISABLED)
            {
                m_shieldState = SHIELD_STATE.ACTIVE;
            }
            m_animator.SetBool("mainShieldIsActive", true);
        }
    }

    /**********************************************************************************************/
    // пробуем поглотить урон щитом
    // функция возвращает кол-во урона, которе щит не смог поглотить
    //
    /**********************************************************************************************/
    public int TakeDamage(int damage)
    {
        int diff = m_shieldPower - damage;
        if (diff > 0)
        {
            // если есть запас прочности щита - включаем анимацию повреждения щита
            m_shieldPower = diff;
            m_animator.SetTrigger("mainShieldIsDamaged");

            return 0;   // возвращаем 0, так как весь урон будет поглощён щитом
        }
        else
        {
            // если запаса прочности щита нет - выключаем его
            m_shieldPower = 0;
            m_animator.SetBool("mainShieldIsActive", false);
            m_shieldState = SHIELD_STATE.DISABLED;
            return -diff;
        }
    }

    /**********************************************************************************************/
    // активируем аварийный щит и устанавливаем таймер щита
    //
    /**********************************************************************************************/
    public void ActivateEmergencyShield()
    {
        m_animator.SetBool("shieldIsActive", true);
        m_shieldState = SHIELD_STATE.EMERGENCY_ACTIVE;
        m_currentShieldTimer = m_shiledDuration;
    }

    /**********************************************************************************************/
    // выключаем аварийный щит
    //
    /**********************************************************************************************/
    public void DisableShield()
    {
        m_animator.SetBool("shieldIsActive", false);

        if(m_shieldPower > 0)
        {
            m_shieldState = SHIELD_STATE.ACTIVE;
        }
        else
        {
            m_shieldState = SHIELD_STATE.DISABLED;
        }

        m_currentShieldTimer = 0.0f;
    }
}
