using UnityEngine;
using System.Collections;

public class CameraControllerSingleMode : CameraControllerDuelMode
{
    new protected void Start()
    {
        base.Start();
    }

    /**********************************************************************************************/
    // получаем контроллеры игроков
    // возвращаем true в случае успеха
    //
    /**********************************************************************************************/
    protected override bool GetPlayersControllers()
    {
        GameManager gManager = GameManager.GetInstance();
        m_pl1Ctr = gManager.GetPlayer();
        m_pl2Ctr = gManager.GetPlayer();

        if (m_pl1Ctr == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
