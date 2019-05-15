using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// ShieldedArmor 
// броня с энергетическим щитом
//
/**********************************************************************************/
class ShieldedArmor : BaseArmor
{
    protected ShieldController m_shieldCtr = null;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public ShieldedArmor(GameObject unitWithArmor)
    {
        // получаем контроллер щита
        Transform[] allChildren = unitWithArmor.GetComponentsInChildren<Transform>();
        for (int ind = 0; ind < allChildren.Length; ind++)
        {
            GameObject component = allChildren[ind].gameObject;
            if (component.tag == "ShieldGen")
            {
                m_shieldCtr = component.GetComponent<ShieldController>();

                if (m_shieldCtr == null)
                {
                    Debug.Log("ERROR! Shield ctr is NULL!!!");
                }
            }
        }
    }

    /**********************************************************************************/
    // сеттер для продолжительности работы щита
    //
    /**********************************************************************************/
    public void SetShiledDuration(float duration)
    {
        m_shieldCtr.ShieldDuration = duration;
    }


    /**********************************************************************************/
    // функция нанесения урона юниту
    //
    /**********************************************************************************/
    public override void TakeDamage(DamageData damage)
    {
        ShieldController.SHIELD_STATE shiledState = m_shieldCtr.State;

        // аварийный щит поглощает весь урон в течении короткого времени
        if (shiledState == ShieldController.SHIELD_STATE.EMERGENCY_ACTIVE)
        {
            // игнорируем урон
            return;
        }
        else
        {
            int damageToLose = damage.Damage;
            int shieldToLose = damage.Damage;

            // пробуем защитится щитом
            if (shiledState == ShieldController.SHIELD_STATE.ACTIVE)
            {
                damageToLose = m_shieldCtr.TakeDamage(damageToLose);
                shieldToLose -= damageToLose;   // разница - кол-во потерянных очков прочности щита
            }

            // если удалось погасить урон щитом - выходим
            if (damageToLose == 0)
            {
                return;
            }

            damage.Damage = damageToLose;
            base.TakeDamage(damage);

            // включаем щит (если ещё живы)
            if (m_unitHP > 0)
            {
                m_shieldCtr.ActivateEmergencyShield();
            }
        }
    }
}

