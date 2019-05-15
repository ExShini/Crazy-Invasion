using UnityEngine;
using UnityEditor;


/**********************************************************************************/
// CompanyDescriptor класс
// данный контейнер служит для хранения информации о структуре одиночной компании
//
//
/**********************************************************************************/

[System.Serializable]
public class CompanyDescriptor
{
    public MissionDescriptor[] missions;
}


[System.Serializable]
public class CompanyDescriptorEditor
{
    public MissionDescriptorEditor[] missions;
}

/**********************************************************************************/
// MissionDescriptor класс
// данный контейнер описывает условия отдельной ключевой миссии
//
/**********************************************************************************/
[System.Serializable]
public class MissionDescriptor
{
    public int RequiredStoryLineProgress;
    public int MissionDifficulties;
    public string[] MissionBosses;
    public int MapXSize;
    public int MapYSize;
    public AvailableBuilding[] Buildings;
    public DropDescriptor DropDescriptor;
}

[System.Serializable]
public class MissionDescriptorEditor
{
    public int RequiredStoryLineProgress;
    public int MissionDifficulties;
    public Base.GO_TYPE[] MissionBosses;
    public int MapXSize;
    public int MapYSize;
    public AvailableBuildingEditor[] Buildings;
    public DropDescriptorEditor DropDescriptor;
}

/**********************************************************************************/
// BuildingType класс
// данный контейнер хранит в себе пару здание-вес
// используется для настройки генерации карт
//
/**********************************************************************************/
[System.Serializable]
public class AvailableBuilding
{
    public string BuildingType;
    public int Weight;
}

[System.Serializable]
public class AvailableBuildingEditor
{
    public Base.BLOCK_TYPE BuildingType;
    public int Weight;
}

/**********************************************************************************/
// BossCompanyData & BossSettings
// Классы описывающий настройки для генерации боссов
//
/**********************************************************************************/
[System.Serializable]
public class BossCompanyData
{
    public BossSettings[] bosses;
}

[System.Serializable]
public class BossSettings
{
    public string BossType;
    public int Weight;
}