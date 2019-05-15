using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// BlusterBulletCtr класс
//
/**********************************************************************************/
public class BlusterBulletCtr : Bullet
{
    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    new public void Start()
    {
        base.Start();
        GOType = Base.GO_TYPE.BLUSTER;
    }
}
