﻿using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// IDrive интерфейс
// интерфейс определяющий механику передвижения юнита и определяющий его позицию во внутреигровых координатах
//
/**********************************************************************************/
public interface IDrive
{
    /**********************************************************************************/
    // эвент сообщающий о изменении координаты юнита в пространстве
    //
    /**********************************************************************************/
    event PositionUpdateEvent PositionUpdate;

    /**********************************************************************************/
    // основная процессинговая функция движетеля, здесь мы перемещаем юнита
    //
    /**********************************************************************************/
    void Update();

    /**********************************************************************************/
    // функция остановки движения, применяется если необходимо остановиться, к примеру
    // во время стрельбы
    //
    /**********************************************************************************/
    void StopMoving();


    /**********************************************************************************/
    // функция возвращающая возможность двигаться 
    //
    /**********************************************************************************/
    void StartMoving();


    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    void ResetComponent();


    /**********************************************************************************/
    // функция устанавливает цель для приследования
    //
    /**********************************************************************************/
    void SetTargetToMove(GameObject target);
}
