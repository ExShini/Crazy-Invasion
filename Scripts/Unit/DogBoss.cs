﻿using UnityEngine;
using System.Collections;
////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// DogBoss класс
// контроллер боса собаки
//
/**********************************************************************************/
public class DogBoss : IshekoidCtr
{
    new protected void Start()
    {
        base.Start();
        m_ownerID = (int)PLAYER.NEUTRAL;
    }

    /**********************************************************************************/
    // обработчик смерти
    //
    /**********************************************************************************/
    protected override void OnDead(DamageData finalStrikeData)
    {
        TargetController.GetInstance().TargetIsDead(gameObject);
        base.OnDead(finalStrikeData);
    }
}
