/**********************************************************************************************/
// LocalizationData класс
// LocalizationItem класс
// необходимы для десериализации текстов локализации
//
/**********************************************************************************************/

[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;
}

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}