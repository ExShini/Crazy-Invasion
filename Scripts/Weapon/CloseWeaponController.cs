using UnityEngine;
using UnityEditor;

/**********************************************************************************/
// CloseWeaponController класс
// отвечает за обработку состояния оружия ближнего боя
/**********************************************************************************/

[System.Serializable]
public class CloseWeaponController : WeaponController
{
    public int Damage;
    public string HitSoundKey = "";

    public override void UpdateWeaponState(Vector2 position, Base.DIREC direction)
    {
        if (m_state == WEAPON_STATE.FIRE)
        {
            if (HitSoundKey != "")
            {
                GameAudioManager.Instance.PlaySound(HitSoundKey);
            }

            m_currentNumOfFiredBullets++;
            if (m_currentNumOfFiredBullets >= NumberOfBullet)
            {
                m_state = WEAPON_STATE.RECHARGE;
                m_currentRechargeTimer = FireRechargeTime;
            }
        }
        else if (m_state == WEAPON_STATE.RECHARGE)
        {
            m_currentRechargeTimer -= Time.deltaTime;
            if (m_currentRechargeTimer <= 0)
            {
                m_currentRechargeTimer = 0;
                m_state = WEAPON_STATE.READY;
                m_currentNumOfFiredBullets = 0;
            }
        }
    }
}