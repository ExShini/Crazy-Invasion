using UnityEngine;
using System.Collections;


public class EffectDescriptor
{
    public enum EffectResponsibility
    {
        TRY_TO_APPLY,
        REQUIRED
    }

    public UnitEffect.EFFECT_TYPE Type;
    public EffectResponsibility Responsibility;
    public float Value = 0.0f;
    public CIGameObject EffectProducer = null;

    public EffectDescriptor()
    {
    }

    public EffectDescriptor(UnitEffect.EFFECT_TYPE type, int value, CIGameObject producer, EffectResponsibility responsibility)
    {
        Type = type;
        Responsibility = responsibility;
        Value = value;
        EffectProducer = producer;
    }
}

/**********************************************************************************/
// UnitEffect
// базовый класс для всех эффектов
//
/**********************************************************************************/
public class UnitEffect
{
    public enum EFFECT_TYPE
    {
        DAMAGE = 0,
        DAMAGE_ACID = 1,
        ACID = 2,
        FREEZING = 3,
        NO_TYPE = -1
    }

    public EFFECT_TYPE Type;
    public CIGameObject EffectProducer = null;
    protected GameObject m_objectUnderEffect;

    protected bool m_isActive = false;

    /**********************************************************************************/
    // get - set
    //
    /**********************************************************************************/
    public virtual GameObject ObjectUnderEffect
    {
        get
        {
            return m_objectUnderEffect;
        }

        set
        {
            m_objectUnderEffect = value;
        }
    }



    /**********************************************************************************/
    // функция процессинга эффекта
    // если эффект активен, производим рассчёты
    //
    /**********************************************************************************/
    public void Process()
    {
        if (m_isActive)
        {
            if (m_objectUnderEffect == null)
            {
                Debug.LogError("ObjectUnderEffect is null");
                return;
            }

            // если объект неактивен - сбрасываем эффект
            if(!ObjectUnderEffect.activeInHierarchy)
            {
                return;
            }

            ProcessEffect();
        }
    }

    /**********************************************************************************/
    // внутренняя функция процессинга эффекта
    //
    /**********************************************************************************/
    virtual protected void ProcessEffect()
    {
        Debug.LogError("ProcessEffect function should be overrided!");
    }

    /**********************************************************************************/
    // функция активации эффекта
    //
    /**********************************************************************************/
    public virtual void Activate(float Value = 0.0f)
    {
        m_isActive = true;
    }

    /**********************************************************************************/
    // функция выключения эффекта
    //
    /**********************************************************************************/
    public virtual void Deactivate()
    {
        m_isActive = false;
    }
}



/**********************************************************************************/
// TimeLimmitedEffect
// базовый класс для всех временных эффектов
//
/**********************************************************************************/
public abstract class TimeLimmitedEffect : UnitEffect
{

    protected float m_timeLimitation = 0.0f;
    protected float m_currentTimer = 0.0f;


    /**********************************************************************************/
    // устанавливаем таймер
    //
    /**********************************************************************************/
    public void SetTimeLimit(float EffectTimeLimit)
    {
        m_timeLimitation = EffectTimeLimit;
    }


    /**********************************************************************************/
    // функция активации эффекта
    //
    /**********************************************************************************/
    public override void Activate(float Value = 0)
    {
        m_currentTimer = m_timeLimitation;
        base.Activate(Value);
    }

    /**********************************************************************************/
    // функция отключения эффекта
    //
    /**********************************************************************************/
    public override void Deactivate()
    {
        m_currentTimer = 0.0f;
        base.Deactivate();
    }

    /**********************************************************************************/
    // внутренняя функция процессинга эффекта
    //
    /**********************************************************************************/
    protected override void ProcessEffect()
    {
        m_currentTimer -= Time.deltaTime;
        if (m_currentTimer > 0.0f)
        {
            TimeEffect();
        }
        else
        {
            Deactivate();
        }
    }

    /**********************************************************************************/
    // функция временного эффекта
    //
    /**********************************************************************************/
    abstract protected void TimeEffect()
;
}