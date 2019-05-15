using UnityEngine;
using System.Collections;


/**********************************************************************************/
// эффект нанесения урона юниту кислотой
// раз в период генерирует еденицу урона своему носителю
//
/**********************************************************************************/
public class AcidEffect : TimeLimmitedEffect
{

    protected float m_acidDamageStep = 1.0f;
    protected int m_stepWasApplied = 0;
    protected int m_acidDamage = 1;

    public AcidEffect()
    {
        Type = EFFECT_TYPE.ACID;
    }

    /**********************************************************************************/
    // активируем эффект и взводим таймер отравления
    //
    /**********************************************************************************/
    public override void Activate(float Value = 0)
    {
        m_stepWasApplied = 0;
        SetTimeLimit(Value);
        base.Activate(Value);

        ApplyAcidDamage();
    }

    /**********************************************************************************/
    // раз в период генерирует еденицу урона своему носителю
    //
    /**********************************************************************************/
    protected override void TimeEffect()
    {
        int currentStep = (int)System.Math.Floor(m_timeLimitation - m_currentTimer);
        if(currentStep > m_stepWasApplied)
        {
            ApplyAcidDamage();
            m_stepWasApplied++;
        }
    }

    /**********************************************************************************/
    // наносим урон кислотой
    //
    /**********************************************************************************/
    protected void ApplyAcidDamage()
    {
        DamageData damageData = new DamageData(m_acidDamage, DamageData.DAMAGE_TYPE.ACID, EffectProducer, DamageData.RESPONSE.NOT_EXPECTED);
        m_objectUnderEffect.SendMessage("ApplyDamage", damageData, SendMessageOptions.DontRequireReceiver);
    }
}

