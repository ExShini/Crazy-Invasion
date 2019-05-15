using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**********************************************************************************************/
// LocalizationManager класс
// ответственнен за загрузку локализации
//
/**********************************************************************************************/
public class LocalizationManager : MonoBehaviour
{

    public static LocalizationManager instance;

    private Dictionary<string, string> m_localizedText;
    private List<LocalizedText> m_localizedTexts = new List<LocalizedText>();
    private bool m_isReady = false;
    private string m_missingTextString = "Localized text not found";
    private string m_defaultLanguageFile = "Localization_Ru.json";

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    /**********************************************************************************************/
    // Start загружаем дефолтноый перевод
    //
    /**********************************************************************************************/
    void Start()
    {
        LoadLocalizedText(m_defaultLanguageFile);
    }

    /**********************************************************************************************/
    // LoadLocalizedText загружает локализацию из указанного файла
    //
    /**********************************************************************************************/
    public void LoadLocalizedText(string fileName)
    {
        m_isReady = false;

        m_localizedText = new Dictionary<string, string>();
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                m_localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

            Debug.Log("Data loaded, dictionary contains: " + m_localizedText.Count + " entries");

            // обновляем локализацию для всех элементов
            foreach (var localization in m_localizedTexts)
            {
                localization.UpdateTextLocalization();
            }
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }

        m_isReady = true;
    }

    /**********************************************************************************************/
    // сохраняем все элементы на текущей сцене для обновления их при переключении языка
    //
    /**********************************************************************************************/
    public void ResgistrLocalizedText(LocalizedText lText)
    {
        m_localizedTexts.Add(lText);
    }

    /**********************************************************************************************/
    // перед загрузкой нового уровня сбрасываем регистрации, чтобы не обращаться к уничтоженным объектам
    //
    /**********************************************************************************************/
    public void ResetRegistration()
    {
        m_localizedTexts.Clear();
    }

    /**********************************************************************************************/
    // GetLocalizedValue возвращает локализованный текст для текущего языка
    //
    /**********************************************************************************************/
    public string GetLocalizedValue(string key)
    {
        string result = m_missingTextString;
        if (m_localizedText.ContainsKey(key))
        {
            result = m_localizedText[key];
        }

        return result;

    }

    /**********************************************************************************************/
    // возвращает true если загрузка локализации была произведена
    //
    /**********************************************************************************************/
    public bool GetIsReady()
    {
        return m_isReady;
    }

}