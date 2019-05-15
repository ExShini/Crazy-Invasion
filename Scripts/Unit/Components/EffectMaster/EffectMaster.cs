using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// EffectMaster
// класс определяющий воздействия различных эффектов на юнита
//
/**********************************************************************************/
public class EffectMaster
{
    // список действующих на объект эффектов
    protected Dictionary<UnitEffect.EFFECT_TYPE, UnitEffect> m_effects = new Dictionary<UnitEffect.EFFECT_TYPE, UnitEffect>();

    // состояние мастера эффектов
    bool m_isActive;

    /**********************************************************************************/
    // EffectMaster конструктор
    //
    /**********************************************************************************/
    public EffectMaster()
    {
        m_isActive = true;
    }

    /**********************************************************************************/
    // ф-ция отключает EffectMaster, можно использовать в тех случаях, когда объект
    // не предполагает использование эффектов
    //
    /**********************************************************************************/
    public void DisableEM()
    {
        m_isActive = false;
    }

    /**********************************************************************************/
    // основная процессинговая функция
    //
    /**********************************************************************************/
    public void Update()
    {
        if (!m_isActive)
            return;

        // обрабатываем все наложенные эффекты
        foreach (var effect in m_effects)
        {
            effect.Value.Process();
        }
    }

    /**********************************************************************************/
    // функция добавления новых эффектов
    //
    /**********************************************************************************/
    public void SetEffect(UnitEffect.EFFECT_TYPE effectSlot, UnitEffect effect)
    {
        m_effects[effectSlot] = effect;
    }

    /**********************************************************************************/
    // функция применения эффектов
    //
    /**********************************************************************************/
    public void ApplyEffect(EffectDescriptor effect)
    {
        if (!m_isActive)
            return;

        UnitEffect.EFFECT_TYPE eType = effect.Type;
        bool weHaveEffect = m_effects.ContainsKey(eType);
        bool effectWasApplied = false;

        if (weHaveEffect)
        {
            if (m_effects[eType] != null)
            {
                m_effects[eType].EffectProducer = effect.EffectProducer;
                m_effects[eType].Activate(effect.Value);
                effectWasApplied = true;
            }
        }

        if (effectWasApplied == false && effect.Responsibility == EffectDescriptor.EffectResponsibility.REQUIRED)
        {
            Debug.LogError("We cann't apply effect: " + eType + " !");
        }
    }

    /**********************************************************************************/
    // функция отключения эффектов
    //
    /**********************************************************************************/
    public void DeactivateEffects()
    {
        foreach (var EffectPair in m_effects)
        {
            EffectPair.Value.Deactivate();
        }
    }


    /**********************************************************************************/
    // функция сброса состояний
    //
    /**********************************************************************************/
    public void ResetComponent()
    {
    }
}

