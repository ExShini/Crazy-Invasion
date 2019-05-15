using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**********************************************************************************/
// RoadNode
// описывает параметры ноды
// используется для построения путей
//
/**********************************************************************************/
public class RoadNode: MonoBehaviour
{
    protected int m_x = 0;
    protected int m_y = 0;
    protected float m_size = 0.0f;
    public List<Base.DIREC> Directions = new List<Base.DIREC>();

    /**********************************************************************************/
    // RoadNode если нода подключена к игровому объекту, то она самомтоятельно может рассчитать свои координаты
    //
    /**********************************************************************************/
    private void Start()
    {
    }

    // СВОЙСТВА

    public int XCor
    {
        get { return m_x; }
        set { m_x = value; }
    }

    public int YCor
    {
        get { return m_y; }
        set { m_y = value; }
    }

    public float XRealCor
    {
        get { return (float)m_x * m_size; }
    }

    public float YRealCor
    {
        get { return (float)m_y * m_size; }
    }
}
