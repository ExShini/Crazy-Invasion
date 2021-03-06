﻿////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// IRadar интерфейс
// интерфейс определяющий механику обнаружения противников юнитом
//
/**********************************************************************************/
public interface IRadar
{
    /**********************************************************************************/
    // эвент сообщающий об обнаружении противника и передающий данные о нём
    //
    /**********************************************************************************/
    event RadarDataEvent RadarUpdate;

    /**********************************************************************************/
    // эвент сообщающий об обнаружении цели для преследовании/сопровождения
    //
    /**********************************************************************************/
    event GOEvent TargetToMove;

    /**********************************************************************************/
    // основная процессинговая функция радара, здесь мы обнаружаем противников
    //
    /**********************************************************************************/
    void Update();

    /**********************************************************************************/
    // функция обновления позиции юнита
    //
    /**********************************************************************************/
    void PositionUpdate(Point position);

    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    void ResetComponent();
}
