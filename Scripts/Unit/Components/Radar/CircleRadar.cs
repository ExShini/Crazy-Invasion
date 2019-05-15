////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// CircleRadar
// основной радар существ преследователей
// данный радар не использует игрока в качестве цели
//
/**********************************************************************************/

using System.Collections.Generic;
using UnityEngine;

class CircleRadar : StalkerRadar, IRadar
{

    public new event RadarDataEvent RadarUpdate;
    public new event GOEvent TargetToMove;

    Unit m_unitWithRadar;
    int m_searchingRadius;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public CircleRadar(Unit unitWithRadar)
        : base((PLAYER)unitWithRadar.Owner)
    {
    }

    /**********************************************************************************/
    // устанавливаем радиус обзора
    //
    /**********************************************************************************/
    public void SetRadius(int radius)
    {
        m_searchingRadius = radius;
    }

    /**********************************************************************************/
    // обновляем цель
    //
    /**********************************************************************************/
    protected override void UpdateTarget()
    {
        List<CIGameObject> units = GameObjectMapController.GetInstance().SearchEnemiesInRadius(m_currentPosition, m_searchingRadius, m_owner);
        if (units.Count == 0)
        {
            m_locedTarget = null;
        }
        else
        {
            // если есть цели - выбираем одну случайно
            CIGameObject targetCtr = units[Random.Range(0, units.Count)];
            m_locedTarget = targetCtr.gameObject;
        }

        // извещаем всех заинтересованных
        if(TargetToMove != null)
        {
            TargetToMove(m_locedTarget);
        }

        if(RadarUpdate != null)
        {
            RadarData data = new RadarData();
            data.DetectedEnemy.Add(m_locedTarget);
            RadarUpdate(data);
        }
    }
}

