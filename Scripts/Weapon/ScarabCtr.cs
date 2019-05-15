using UnityEngine;
using System.Collections;

/**********************************************************************************/
// Контроллер скарабея
//
/**********************************************************************************/
public class ScarabCtr : BaseMonsterCtr
{
    public float LifeLimit = 3.0f;
    protected float m_defaulLifeTime = 3.0f;
    protected GameObject m_target = null;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    public void Start()
    {
        m_defaulLifeTime = LifeLimit;
        //base.Start();

        /*
        if(m_target != null)
        {
            Alg_BaseStalker ald = (Alg_BaseStalker)m_algorithm;
            ald.Target = m_target;
        }
        */
    }

    /**********************************************************************************/
    // устанавливаем дефолтные параметры
    //
    /**********************************************************************************/
    public override void SetDefaultParameter()
    {
        LifeLimit = m_defaulLifeTime;
        base.SetDefaultParameter();
    }

    /**********************************************************************************/
    // проверяем время жизни
    //
    /**********************************************************************************/
    protected override void FixedUpdate()
    {
        if (!IsInitialized())
        {
            return;
        }

        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        if (m_state == UNIT_STATE.ACTIVE)
        {
            LifeLimit -= Time.deltaTime;
            if(LifeLimit <= 0)
            {
                // погибаем от "старости"
                ApplyDamage(new DamageData(health, DamageData.DAMAGE_TYPE.PHYSICAL, this, DamageData.RESPONSE.NOT_EXPECTED));
            }
        }

        base.FixedUpdate();
    }

    /**********************************************************************************/
    // функция позволяет установить цель для приследования
    //
    /**********************************************************************************/
    public void SetTarget(GameObject target)
    {
        m_target = target;
        /*
        if (IsInitialized())
        {
            Alg_BaseStalker ald = (Alg_BaseStalker)m_algorithm;
            ald.Target = target;
        }
        */
    }
}
