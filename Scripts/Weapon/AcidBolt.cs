using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// AcidBolt класс
//
/**********************************************************************************/
public class AcidBolt: Bullet
{
    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    new public void Start()
    {
        base.Start();
        GOType = Base.GO_TYPE.ACID_BOLT;
    }

    /**********************************************************************************/
    // применяем эффеккт кислоты
    //
    /**********************************************************************************/
    protected override void ApplyBilletEffect(Collision2D coll)
    {
        EffectDescriptor effectDescr = new EffectDescriptor( UnitEffect.EFFECT_TYPE.ACID, damage, this, EffectDescriptor.EffectResponsibility.TRY_TO_APPLY);
        coll.gameObject.SendMessage("ApplyEffect", effectDescr, SendMessageOptions.DontRequireReceiver);
    }

}

