using UnityEngine;
using System.Collections;
/**********************************************************************************/
// RegeneratedWeaponCtr
//
/**********************************************************************************/
[System.Serializable]
public class RegeneratedWeaponCtr : ClassicWeaponCtr
{
    public float TimeToRegenerateAmmo = 1.0f;
    protected float m_currentRegenTimer = 0.0f;

    /**********************************************************************************/
    // RegeneratedWeaponCtr конструктор
    //
    /**********************************************************************************/
    public RegeneratedWeaponCtr(int ownerID) :
        base(ownerID)
    {
        m_currentRegenTimer = TimeToRegenerateAmmo;
    }


    /**********************************************************************************/
    // UpdateWeaponState конструктор
    // расширяем базовый метод регенерацией снарядов
    /**********************************************************************************/
    public override void UpdateWeaponState(Vector2 position, Base.DIREC direction)
    {
        // регенерируем 1 зарад каждые TimeToRegenerateAmmo секунд
        m_currentRegenTimer -= Time.deltaTime;
        if(m_currentRegenTimer <= 0)
        {
            ChargeAmmo(1);
            m_currentRegenTimer = TimeToRegenerateAmmo;
        }

        base.UpdateWeaponState(position, direction);
    }
}
