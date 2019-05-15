using UnityEngine;
using UnityEditor;

public class WeaponLibrary : MonoBehaviour
{
    static WeaponLibrary s_instance = null;


    public RegeneratedWeaponCtr TentaclesWeapon_Pl1;
    public RegeneratedWeaponCtr TentaclesWeapon_Pl2;
    public ClassicWeaponCtr BlusterWeapon;
    public ClassicWeaponCtr ShotGun;
    public ClassicWeaponCtr MocusGun_Pl1;
    public ClassicWeaponCtr MocusGun_Pl2;
    public ClassicWeaponCtr PlasmaGrenade;
    public ClassicWeaponCtr RGDGrenade;
    public ClassicWeaponCtr AcidGun;
    public ClassicWeaponCtr TurelBuilder_Pl1;
    public ClassicWeaponCtr TurelBuilder_Pl2;

    public WeaponImageSet TentaclesIcoSpriteSet;
    public WeaponImageSet BlusterIcoSpriteSet;
    public WeaponImageSet ShotGunIcoSpriteSet;
    public WeaponImageSet MocusIcoSpriteSet;
    public WeaponImageSet PlasmaGrenadeIcoSpriteSet;
    public WeaponImageSet RGDGrenadeIcoSpriteSet;
    public WeaponImageSet AcidGunSpriteSet;
    public WeaponImageSet TurelBuilderSpriteSet;

    /**********************************************************************************/
    //  защищаемся от повторного создания объекта
    //
    /**********************************************************************************/
    void Awake()
    {
        // защищаемся от повторного создания объекта
        if (s_instance == null)
        {
            s_instance = this;
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
        }

        // делаем GameManager неучтожимым при загрузке новой сцены
        DontDestroyOnLoad(gameObject);
    }


    /**********************************************************************************/
    // GetInstance
    //
    /**********************************************************************************/
    public static WeaponLibrary GetInstance()
    {
        if (s_instance == null)
        {
            Debug.LogError("WeaponLibrary instance is null!");
        }

        return s_instance;
    }

    /**********************************************************************************/
    // GetWeaponById - возвращает копию контроллера по указанному ID
    //
    /**********************************************************************************/
    public ClassicWeaponCtr GetWeaponById(WEAPON weaponId, PLAYER ownerID)
    {
        ClassicWeaponCtr ctr = null;

        if(weaponId == WEAPON.TENTAKLES && ownerID == PLAYER.PL1)
        {
            RegeneratedWeaponCtr tentakles = new RegeneratedWeaponCtr((int)ownerID);
            tentakles.BulletType = TentaclesWeapon_Pl1.BulletType;
            tentakles.FireRechargeTime = TentaclesWeapon_Pl1.FireRechargeTime;
            tentakles.MagazineAmmo = TentaclesWeapon_Pl1.MagazineAmmo;
            tentakles.NumberOfBullet = TentaclesWeapon_Pl1.NumberOfBullet;
            tentakles.TimeToRegenerateAmmo = TentaclesWeapon_Pl1.TimeToRegenerateAmmo;
            ctr = tentakles;
        }
        else if (weaponId == WEAPON.TENTAKLES && ownerID == PLAYER.PL2)
        {
            RegeneratedWeaponCtr tentakles = new RegeneratedWeaponCtr((int)ownerID);
            tentakles.BulletType = TentaclesWeapon_Pl2.BulletType;
            tentakles.FireRechargeTime = TentaclesWeapon_Pl2.FireRechargeTime;
            tentakles.MagazineAmmo = TentaclesWeapon_Pl2.MagazineAmmo;
            tentakles.NumberOfBullet = TentaclesWeapon_Pl2.NumberOfBullet;
            tentakles.TimeToRegenerateAmmo = TentaclesWeapon_Pl2.TimeToRegenerateAmmo;
            ctr = tentakles;
        }
        else if(weaponId == WEAPON.BLUSTER)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = BlusterWeapon.BulletType;
            wCtr.FireRechargeTime = BlusterWeapon.FireRechargeTime;
            wCtr.MagazineAmmo = BlusterWeapon.MagazineAmmo;
            wCtr.NumberOfBullet = BlusterWeapon.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.ACID_GUN)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = AcidGun.BulletType;
            wCtr.FireRechargeTime = AcidGun.FireRechargeTime;
            wCtr.MagazineAmmo = AcidGun.MagazineAmmo;
            wCtr.NumberOfBullet = AcidGun.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.SHOTGUN)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = ShotGun.BulletType;
            wCtr.FireRechargeTime = ShotGun.FireRechargeTime;
            wCtr.MagazineAmmo = ShotGun.MagazineAmmo;
            wCtr.NumberOfBullet = ShotGun.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.MOCUS && ownerID == PLAYER.PL1)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = MocusGun_Pl1.BulletType;
            wCtr.FireRechargeTime = MocusGun_Pl1.FireRechargeTime;
            wCtr.MagazineAmmo = MocusGun_Pl1.MagazineAmmo;
            wCtr.NumberOfBullet = MocusGun_Pl1.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.MOCUS && ownerID == PLAYER.PL2)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = MocusGun_Pl2.BulletType;
            wCtr.FireRechargeTime = MocusGun_Pl2.FireRechargeTime;
            wCtr.MagazineAmmo = MocusGun_Pl2.MagazineAmmo;
            wCtr.NumberOfBullet = MocusGun_Pl2.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.PLASMA_GRENADE)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = PlasmaGrenade.BulletType;
            wCtr.FireRechargeTime = PlasmaGrenade.FireRechargeTime;
            wCtr.MagazineAmmo = PlasmaGrenade.MagazineAmmo;
            wCtr.NumberOfBullet = PlasmaGrenade.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.RGD_GRENADE)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = RGDGrenade.BulletType;
            wCtr.FireRechargeTime = RGDGrenade.FireRechargeTime;
            wCtr.MagazineAmmo = RGDGrenade.MagazineAmmo;
            wCtr.NumberOfBullet = RGDGrenade.NumberOfBullet;
            ctr = wCtr;
        }
        else if(weaponId == WEAPON.TUREL_BUILDER && ownerID == PLAYER.PL1)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = TurelBuilder_Pl1.BulletType;
            wCtr.FireRechargeTime = TurelBuilder_Pl1.FireRechargeTime;
            wCtr.MagazineAmmo = TurelBuilder_Pl1.MagazineAmmo;
            wCtr.NumberOfBullet = TurelBuilder_Pl1.NumberOfBullet;
            ctr = wCtr;
        }
        else if (weaponId == WEAPON.TUREL_BUILDER && ownerID == PLAYER.PL2)
        {
            ClassicWeaponCtr wCtr = new ClassicWeaponCtr((int)ownerID);
            wCtr.BulletType = TurelBuilder_Pl2.BulletType;
            wCtr.FireRechargeTime = TurelBuilder_Pl2.FireRechargeTime;
            wCtr.MagazineAmmo = TurelBuilder_Pl2.MagazineAmmo;
            wCtr.NumberOfBullet = TurelBuilder_Pl2.NumberOfBullet;
            ctr = wCtr;
        }

        return ctr;
    }

    /**********************************************************************************/
    // GetWeaponIcons - возвращает набор иконок зарядок оружия по указанному ID
    //
    /**********************************************************************************/
    public WeaponImageSet GetWeaponIcons(WEAPON weaponId)
    {
        switch(weaponId)
        {
            case WEAPON.BLUSTER:
                return BlusterIcoSpriteSet;
            case WEAPON.ACID_GUN:
                return AcidGunSpriteSet;
            case WEAPON.SHOTGUN:
                return ShotGunIcoSpriteSet;
            case WEAPON.MOCUS:
                return MocusIcoSpriteSet;
            case WEAPON.TENTAKLES:
                return TentaclesIcoSpriteSet;
            case WEAPON.PLASMA_GRENADE:
                return PlasmaGrenadeIcoSpriteSet;
            case WEAPON.RGD_GRENADE:
                return RGDGrenadeIcoSpriteSet;
            case WEAPON.TUREL_BUILDER:
                return TurelBuilderSpriteSet;
            default:
                Debug.LogError("We have no ico set for this weapon: " + weaponId.ToString());
                return null;
        }
    }
}