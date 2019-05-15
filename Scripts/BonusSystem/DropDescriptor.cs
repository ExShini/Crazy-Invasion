using UnityEngine;
using System.Collections;

[System.Serializable]
public class DropDescriptor
{
    public int MaxNumOfDroppedItem;
    public DropItemDescriptor[] DropItems;
}

[System.Serializable]
public class DropDescriptorEditor
{
    public int MaxNumOfDroppedItem;
    public DropItemDescriptorEditor[] DropItems;
}

[System.Serializable]
public class DropItemDescriptor
{
    public string DropType;
    public int DropWeight;
    public int MaxNumOfDrop;           // установить в 0, если ограничений по кол-ву дропа не ограничено
    public int TimeForDropStart;       // время в секундах, которое должно пройти с начала партии прежде чем начнётся данный дроп
}

[System.Serializable]
public class DropItemDescriptorEditor
{

    public enum DROP_MARKER: int
    {
        UNLIMITED = 0
    }

    public Base.GO_TYPE DropType;
    public int DropWeight;
    public int MaxNumOfDrop;           // установить в 0, если ограничений по кол-ву дропа не ограничено
    public int TimeForDropStart;       // время в секундах, которое должно пройти с начала партии прежде чем начнётся данный дроп


    public DropItemDescriptorEditor()
    {
    }

    public DropItemDescriptorEditor(DropItemDescriptor desc)
    {
        this.DropType = Base.StringToGOType(desc.DropType);
        this.DropWeight = desc.DropWeight;
        this.MaxNumOfDrop = desc.MaxNumOfDrop;
        this.TimeForDropStart = desc.TimeForDropStart;
    }
}