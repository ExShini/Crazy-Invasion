using UnityEngine;
using System.Collections;

/**********************************************************************************/
// визуальный эффект нанесения урона юниту
// временно окрашивает текстуру в красный цвет
//
/**********************************************************************************/
public class FreezeEffect : TimeLimmitedEffect
{
    public float FreezePower = 0.8f;
    protected float m_freezePowerApplyed = 0.0f;

    CIGameObject m_controlledCIO = null;
    Animator m_effectAnimator = null;

    public Animator EffectAnimator
    {
        get
        {
            return m_effectAnimator;
        }

        set
        {
            m_effectAnimator = value;
        }
    }

    public FreezeEffect()
    {
        // устанавливаем временное ограничение для заморозки
        SetTimeLimit(5.0f);
        Type = EFFECT_TYPE.FREEZING;
    }

    /**********************************************************************************/
    // св-во ObjectUnderEffect
    //
    /**********************************************************************************/
    public override GameObject ObjectUnderEffect
    {
        get
        {
            return base.ObjectUnderEffect;
        }

        set
        {
            base.ObjectUnderEffect = value;
            m_controlledCIO = value.GetComponent<CIGameObject>();
        }
    }

    /**********************************************************************************/
    // функция отключения эффекта
    //
    /**********************************************************************************/
    public override void Deactivate()
    {
        // возвращаем значение мультипликатора скорости и сбрасываем счётчик эффекта
        if (m_isActive)
        {
            m_controlledCIO.speedMultiplier += m_freezePowerApplyed;
            m_freezePowerApplyed = 0.0f;
        }

        // выключаем анимацию
        m_effectAnimator.SetBool("EffectIsActive", false);
        base.Deactivate();
    }

    /**********************************************************************************/
    // функция включения эффекта
    //
    /**********************************************************************************/
    public override void Activate(float Value = 0)
    {
        // включаем анимацию
        m_effectAnimator.SetBool("EffectIsActive", true);
        base.Activate(Value);
    }

    /**********************************************************************************/
    // функция временного эффекта
    // замедляем и окрашиваем юнита
    //
    /**********************************************************************************/
    protected override void TimeEffect()
    {
        // рассчитываем, какой уровень замедления должен быть применён к объекту
        float timePart = 1.0f - (m_timeLimitation - m_currentTimer) / m_timeLimitation;
        float currentFreezePowerLevel = FreezePower * timePart;

        // подсчитываем, насколько нужно поправить мультипликатор, чтобы соответсвовать необходимому уровню
        float freezePowerToApply = currentFreezePowerLevel - m_freezePowerApplyed;

        // проводим корректировку
        m_controlledCIO.speedMultiplier -= freezePowerToApply;
        m_freezePowerApplyed = currentFreezePowerLevel;

        Debug.Log("Freezed speedMultiplier: " + m_controlledCIO.speedMultiplier);
    }
}
