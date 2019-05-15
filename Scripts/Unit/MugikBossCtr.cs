using UnityEngine;
using UnityEditor;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
/**********************************************************************************/
/**********************************************************************************/
// MugikCtr класс
// контроллер лучшего среди деревенских мужиков
// бродит, стреляет по врагам, здоровый как бык и почти не пьёт
// если бы не характер - был бы первым женихом
//
/**********************************************************************************/
public class MugikBossCtr: MugikCtr
{
    void Start()
    {
        InitializeUnit();
        /*
        m_state = UNIT_STATE.ACTIVE;
        Weapon.SetUnitID(ID);
        Weapon.SetOwner(PLAYER.NEUTRAL);
        Alg_WanderingHunter alg = new Alg_WanderingHunter(this);
        alg.SetWeaponController(Weapon);
        alg.SearchingRadius = SearchingRadius;
        alg.SpeedMultiplier = SpeedMultiplier;
        alg.SearchRate = 0.7f;
        m_algorithm = alg;
        m_ownerID = (int)PLAYER.NEUTRAL;
        */
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