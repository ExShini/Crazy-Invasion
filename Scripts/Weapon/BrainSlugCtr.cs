using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**********************************************************************************/
// BrainSlugCtr класс
// слизень мозговой
//
/**********************************************************************************/
public class BrainSlugCtr : BulletRotated
{
    public int CapturePower = 10;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    new public void Start()
    {
        base.Start();
        m_originalSpeed = speed;
    }

    /**********************************************************************************/
    // функция проверки столкновения
    // если цель здание - происходит захват
    //
    /**********************************************************************************/
    override protected void CheckBulletBurn(Collision2D coll)
    {
        if (m_state == BULLET_STATE.FLY)
        {
            string otherObjTag = coll.gameObject.tag;
            if (otherObjTag == "Player" || otherObjTag == "wall" || otherObjTag == "Building")
            {

                if(otherObjTag == "Player")
                {
                    if(coll.gameObject.GetComponent<PlayerController>().playerId == (PLAYER)m_ownerID)
                    {
                        return;
                    }
                }

                m_state = BULLET_STATE.BURN;
                // выключаем коллайдер для сработавшего сняаряда
                speed = 0.0f;
                m_rb2d.velocity = new Vector2(0, 0);
                m_collider.enabled = false;
                m_animator.SetBool("Burn", true);
                PlayBurstSoundEffect();

                if (otherObjTag == "Building")
                {
                    CaptureData cdata = new CaptureData();
                    cdata.OwnweID = m_ownerID;
                    cdata.CapturePower = CapturePower;
                    coll.gameObject.SendMessage("Capture", cdata, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
