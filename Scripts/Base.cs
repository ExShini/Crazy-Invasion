using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/**********************************************************************************/
// Файл свалка для общих enum и глобальных переменных
//
/**********************************************************************************/

public enum PLAYER : int
{
    PL1 = 1,
    PL2 = 2,
    NEUTRAL = -1,
    ANGRY = -98,
    NO_PLAYER = -99     // этот инднтификатор можно использовать при поиске всех объектов (как виртуального, несуществующего игрока)
}

public enum ROAD_CONNECTION_STATUS : int
{
    NEEDED = 0,     // соединение дороги нужно
    POSSIBLE = 1,
    BLOCKED = 2     // соединение дороги не возможно(заблокировано/не к чему присоеденять)
}

public class DamageData
{
    public enum RESPONSE
    {
        EXPECTED,
        NOT_EXPECTED
    }

    public enum DAMAGE_TYPE
    {
        PHYSICAL,
        ACID
    }

    public int Damage = 0;
    public DAMAGE_TYPE DamageType;
    public CIGameObject Damager = null;
    public RESPONSE ExpectResponce = RESPONSE.EXPECTED;

    public DamageData()
    {
    }

    public DamageData(int Damage, DAMAGE_TYPE type, CIGameObject Damager, RESPONSE ExpectResponce)
    {
        this.Damage = Damage;
        this.DamageType = type;
        this.Damager = Damager;
        this.ExpectResponce = ExpectResponce;
    }
}

public class CaptureData
{
    public int OwnweID;
    public int CapturePower;
}

public class RadarData
{
    public List<Base.DIREC> EnemyDirection = new List<Base.DIREC>();
    public List<GameObject> DetectedEnemy = new List<GameObject>();
}

// ДЕЛЕГАТЫ:
// делегаты использующиеся для системы игровых событий
public delegate void GameEvent();
public delegate void GOEvent(GameObject gObject);
public delegate void DestructionEvent(DamageData finalStrikeData);
public delegate void RadarDataEvent(RadarData data);
public delegate void PositionUpdateEvent(Point position);


/*************************************/
public class Base
{
    // размер ячейки в системе координат Unity
    public static float SIZE_OF_CELL = 0.32f;
    public static float HALF_OF_CELL = 0.16f;

    public enum DIREC : int
    {
        DOWN = 0,
        RIGHT = 1,
        UP = 2,
        LEFT = 3,
        NUM_OF_DIRECTIONS = 4,
        NO_DIRECTION = -1
    };


    /**********************************************************************************/
    // конвертируем строку к GO_TYPE
    //
    /**********************************************************************************/
    public static GO_TYPE StringToGOType(string strType)
    {
        return (Base.GO_TYPE)Enum.Parse(typeof(Base.GO_TYPE), strType);
    }

    /**********************************************************************************/
    // конвертируем строку к BLOCK_TYPE
    //
    /**********************************************************************************/
    public static BLOCK_TYPE StringToBlockType(string strType)
    {
        return (Base.BLOCK_TYPE)Enum.Parse(typeof(Base.BLOCK_TYPE), strType);
    }

    /**********************************************************************************/
    // возвращает имя игрока
    //
    /**********************************************************************************/
    public static string GetPlayerName(PLAYER id)
    {
        switch(id)
        {
            case PLAYER.NEUTRAL:
                return "Mugiks";
            case PLAYER.NO_PLAYER:
                return "No player!";
            case PLAYER.PL1:
                return "Xserg III";
            case PLAYER.PL2:
                return "Gizmo";
        }

        return "GetPlayerName: Wrong input!";
    }

    /**********************************************************************************/
    // возвращает случайное направление
    //
    /**********************************************************************************/
    public static DIREC GetRandomDirection()
    {
        return (DIREC)Random.Range(0, (int)Base.DIREC.NUM_OF_DIRECTIONS);
    }

    /**********************************************************************************/
    // инвертирует направление на противоположное
    //
    /**********************************************************************************/
    public static DIREC InvertDirection(DIREC direction)
    {
        switch(direction)
        {
            case DIREC.DOWN:
                return DIREC.UP;
            case DIREC.UP:
                return DIREC.DOWN;
            case DIREC.LEFT:
                return DIREC.RIGHT;
            case DIREC.RIGHT:
                return DIREC.LEFT;
            default:
                return DIREC.NO_DIRECTION;
        }
    }

    /**********************************************************************************/
    // инвертирует направление на противоположное
    //
    /**********************************************************************************/
    public static DIREC InvertDirection(int direction)
    {
        switch (direction)
        {
            case (int)DIREC.DOWN:
                return DIREC.UP;
            case (int)DIREC.UP:
                return DIREC.DOWN;
            case (int)DIREC.LEFT:
                return DIREC.RIGHT;
            case (int)DIREC.RIGHT:
                return DIREC.LEFT;
            default:
                return DIREC.NO_DIRECTION;
        }
    }


    /**********************************************************************************/
    // ID для всех игровых юнитов
    //
    /**********************************************************************************/
    public enum GO_TYPE : int
    {
        PLAYER = 0,

        // СНАРЯДЫ:
        BLUSTER = 1,
        BRAIN_SLUG = 2,
        SHOT = 3,
        BRAIN_MUCUS_PL1 = 4,
        BRAIN_MUCUS_PL2 = 5,
        HOLY_WATER = 6,
        SCARAB_PL1 = 7,
        SCARAB_PL2 = 8,
        TENTACLE_PL1 = 9,
        PLASMA_GRENADE = 10,
        TENTACLE_PL2 = 11,
        RGD_GRENADE = 12,
        ACID_BOLT = 13,
        PLASMA_SHOT = 14,
        AK_SHOT = 15,
        PLASMA_BOLT = 16,
        BUILD_CUPSULE_PL1 = 17,
        BUILD_CUPSULE_PL2 = 18,


        FIRST_CLOSE_WEAPON = 50,
        CANINE_CLOSE_WEAPON = FIRST_CLOSE_WEAPON,
        LAST_CLOSE_WEAPON = 80,

        // ЮНИТЫ:
        ZOMBIE_PL1 = 100,
        ZOMBIE_PL2 = 101,

        ISHEKOID_PL1 = 102,
        ISHEKOID_PL2 = 103,

        ENERGY_BOMBER_PL1 = 104,
        ENERGY_BOMBER_PL2 = 105,

        BOROVIC_PL1 = 106,
        CIRCULOID_PL2 = 107,

        TANK_PL1 = 108,
        TANK_PL2 = 109,

        TUREL_PL1 = 110,
        TUREL_PL2 = 111,

        // НЕЙТРАЛЬНЫЕ ЮНИТЫ:
        MUGIK = 200,
        DOG = 201,
        BEAR = 202,
        POPE = 203,
        SOLDER = 204,

        // БОСС ЮНИТЫ
        MUGIK_BOSS = 300,
        DOG_BOSS = 301,


        // ЗДАНИЯ
        HOME = 500,
        KENNEL = 501,
        ENERGY_STATION = 502,
        CHURCH = 503,
        BARRACKS = 504,


        // СТЕНЫ / ГЕНЕРИРУЕМЫЕ ОБЪЕКТЫ
        WALL = 700,
        GARBAGE_COLLECTOR = 701,
        GUARD_TOWER = 702,

        // БОНУСЫ:
        SHEILD_BONUS = 800,
        MOCUS_BONUS = 801,
        BL_ENERGY_BONUS = 802,
        PLASMA_GRENADE_BONUS = 803,
        RGD_GRENADE_BONUS = 804,
        SHOTGUN_BONUS = 805,
        ACID_GUN_BONUS = 806,
        TURELBUILDER_BONUS = 807,


        DROP_POD = 900,

        // БЕЗ ТИПА:
        NONE_TYPE = -1
    }

    /**********************************************************************************/
    // ID для генерируемых блоков карты
    //
    /**********************************************************************************/
    public enum BLOCK_TYPE
    {
        HOME_BLOCK = 1,
        KENNEL_BLOCK = 2,
        ENERGY_STATION_BLOCK = 3,
        CHURCH_BLOCK = 4,
        FIELD_BLOCK = 5,
        BARRACKS_BLOCK = 6,

        NO_TYPE = -1
    }
}
