using UnityEngine;
using System.Collections;


/**********************************************************************************/
// визуальный эффект нанесения урона юниту
// временно окрашивает текстуру в красный цвет
//
/**********************************************************************************/
public class DamageEffect : TimeLimmitedEffect
{

    public Color StartColor;
    public Color EndColor;
    protected Color m_colorDiff;
    protected SpriteRenderer m_spriteRenderer = null;

    public DamageEffect()
    {
        // устанавливаем временное ограничение для эффекта урона
        SetTimeLimit(0.5f);

        StartColor = Color.red;
        EndColor = Color.white;

        m_colorDiff = EndColor - StartColor;
        Type = EFFECT_TYPE.DAMAGE;
    }

    /**********************************************************************************/
    // устанавливаем DamageEffect для работы с кислотым уроном эффектом
    //
    /**********************************************************************************/
    public void SetAcidType()
    {
        StartColor = Color.green;
        m_colorDiff = EndColor - StartColor;
        Type = EFFECT_TYPE.DAMAGE_ACID;
    }

    /**********************************************************************************/
    // get - set
    // получем SpriteRenderer объекта
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
            m_spriteRenderer = value.GetComponent<SpriteRenderer>();

            // запоминаем оригинальную окраску юнита, к которой будем возвращаться
            EndColor = m_spriteRenderer.color;
            if (m_spriteRenderer == null)
            {
                Debug.LogError("m_spriteRenderer is null");
            }
        }
    }

    /**********************************************************************************/
    // функция отключения эффекта
    //
    /**********************************************************************************/
    public override void Deactivate()
    {
        m_spriteRenderer.color = EndColor;
        base.Deactivate();
    }


    /**********************************************************************************/
    // функция временного эффекта
    // окрашиваем юнита
    //
    /**********************************************************************************/
    protected override void TimeEffect()
    {
        float timePart = (m_timeLimitation - m_currentTimer) / m_timeLimitation;
        m_spriteRenderer.color = StartColor + timePart * m_colorDiff;
    }
}
