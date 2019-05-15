using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public class CompanyDataEditor : EditorWindow
{
    public CompanyDescriptorEditor CompanyData = null;
    protected string m_lastOpenFile = "";
    Vector2 m_scrollPos;

    [MenuItem("Window/Company Data Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CompanyDataEditor)).Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);

        if (CompanyData != null)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("CompanyData");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            // сохранение в ранее открываемый файл
            if(m_lastOpenFile != "")
            {
                if (GUILayout.Button("Save data"))
                {
                    SaveGameData(m_lastOpenFile);
                }
            }

            // сохранение в специфичный файл
            if (GUILayout.Button("Save data AS"))
            {
                SaveGameData();
            }
        }

        if (GUILayout.Button("Load data"))
        {
            LoadGameData();
        }

        if (GUILayout.Button("Create new data"))
        {
            CreateNewData();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /**********************************************************************************/
    // LoadGameData
    // загружаем данные из файла и парсим их в удобоваримый формат
    //
    /**********************************************************************************/
    private void LoadGameData()
    {
        string filePath = EditorUtility.OpenFilePanel("Select company data file", Application.streamingAssetsPath, "json");

        if (!string.IsNullOrEmpty(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            CompanyDescriptor companyDescriptor = JsonUtility.FromJson<CompanyDescriptor>(dataAsJson);
            m_lastOpenFile = filePath;  // запоминаем путь к файлу, будет использован в функции сохранения

            CompanyData = new CompanyDescriptorEditor();
            CompanyData.missions = new MissionDescriptorEditor[companyDescriptor.missions.Length];

            // разбираем миссию за миссией
            for (int missInd = 0; missInd < companyDescriptor.missions.Length; missInd++)
            {
                // создаём MissionDescriptorEditor и заполняем его данными из MissionDescriptor
                MissionDescriptor mission = companyDescriptor.missions[missInd];
                MissionDescriptorEditor missionEditor = new MissionDescriptorEditor();
                CompanyData.missions[missInd] = missionEditor;

                missionEditor.MapXSize = mission.MapXSize;
                missionEditor.MapYSize = mission.MapYSize;
                missionEditor.MissionDifficulties = mission.MissionDifficulties;
                missionEditor.RequiredStoryLineProgress = mission.RequiredStoryLineProgress;

                // конвертируем здание
                missionEditor.Buildings = new AvailableBuildingEditor[mission.Buildings.Length];
                for (int buildInd = 0; buildInd < mission.Buildings.Length; buildInd++)
                {
                    AvailableBuildingEditor buildingEditor = new AvailableBuildingEditor();
                    missionEditor.Buildings[buildInd] = buildingEditor;

                    buildingEditor.BuildingType = Base.StringToBlockType(mission.Buildings[buildInd].BuildingType);
                    buildingEditor.Weight = mission.Buildings[buildInd].Weight;
                }

                // конвертируем список боссов
                missionEditor.MissionBosses = new Base.GO_TYPE[mission.MissionBosses.Length];
                for (int bossInd = 0; bossInd < mission.MissionBosses.Length; bossInd++)
                {
                    missionEditor.MissionBosses[bossInd] = Base.StringToGOType(mission.MissionBosses[bossInd]);
                }


                // конвертируем настройки дропа
                missionEditor.DropDescriptor = new DropDescriptorEditor();
                missionEditor.DropDescriptor.MaxNumOfDroppedItem = mission.DropDescriptor.MaxNumOfDroppedItem;
                missionEditor.DropDescriptor.DropItems = new DropItemDescriptorEditor[mission.DropDescriptor.DropItems.Length];
                for (int dropInd = 0; dropInd < mission.DropDescriptor.DropItems.Length; dropInd++)
                {
                    DropItemDescriptorEditor dropEditor = new DropItemDescriptorEditor();
                    missionEditor.DropDescriptor.DropItems[dropInd] = dropEditor;

                    dropEditor.DropType = Base.StringToGOType(mission.DropDescriptor.DropItems[dropInd].DropType);
                    dropEditor.DropWeight = mission.DropDescriptor.DropItems[dropInd].DropWeight;
                }

            }
        }
    }


    /**********************************************************************************/
    // SaveGameData
    // сохраняем данные в файл, преобразуя к текстовому виду
    //
    /**********************************************************************************/
    private void SaveGameData(string useFilePath = "")
    {
        string filePath = "";
        if (useFilePath == "")
        {
            filePath = EditorUtility.SaveFilePanel("Save company data file", Application.streamingAssetsPath, "", "json");
        }
        else
        {
            filePath = useFilePath;
        }
         
        
        if (!string.IsNullOrEmpty(filePath))
        {
            CompanyDescriptor companyDescriptorToWrite = new CompanyDescriptor();
            companyDescriptorToWrite.missions = new MissionDescriptor[CompanyData.missions.Length];

            // разбираем миссию за миссией
            for (int missInd = 0; missInd < CompanyData.missions.Length; missInd++)
            {
                // создаём MissionDescriptorEditor и заполняем его данными из MissionDescriptor
                MissionDescriptor missionToWrite = new MissionDescriptor();
                MissionDescriptorEditor mission = CompanyData.missions[missInd];
                companyDescriptorToWrite.missions[missInd] = missionToWrite;

                missionToWrite.MapXSize = mission.MapXSize;
                missionToWrite.MapYSize = mission.MapYSize;
                missionToWrite.MissionDifficulties = mission.MissionDifficulties;
                missionToWrite.RequiredStoryLineProgress = mission.RequiredStoryLineProgress;

                // конвертируем здание
                missionToWrite.Buildings = new AvailableBuilding[mission.Buildings.Length];
                for (int buildInd = 0; buildInd < mission.Buildings.Length; buildInd++)
                {
                    AvailableBuilding buildingToWrite = new AvailableBuilding();
                    missionToWrite.Buildings[buildInd] = buildingToWrite;

                    buildingToWrite.BuildingType = mission.Buildings[buildInd].BuildingType.ToString();
                    buildingToWrite.Weight = mission.Buildings[buildInd].Weight;
                }

                // конвертируем список боссов
                missionToWrite.MissionBosses = new string[mission.MissionBosses.Length];
                for (int bossInd = 0; bossInd < mission.MissionBosses.Length; bossInd++)
                {
                    missionToWrite.MissionBosses[bossInd] = mission.MissionBosses[bossInd].ToString();
                }

                // конвертируем настройки дропа
                missionToWrite.DropDescriptor = new DropDescriptor();
                missionToWrite.DropDescriptor.MaxNumOfDroppedItem = mission.DropDescriptor.MaxNumOfDroppedItem;
                missionToWrite.DropDescriptor.DropItems = new DropItemDescriptor[mission.DropDescriptor.DropItems.Length];

                for (int dropInd = 0; dropInd < mission.DropDescriptor.DropItems.Length; dropInd++)
                {
                    DropItemDescriptor dropToWrite = new DropItemDescriptor();
                    missionToWrite.DropDescriptor.DropItems[dropInd] = dropToWrite;

                    dropToWrite.DropType = mission.DropDescriptor.DropItems[dropInd].DropType.ToString();
                    dropToWrite.DropWeight = mission.DropDescriptor.DropItems[dropInd].DropWeight;
                }
            }

            string dataAsJson = JsonUtility.ToJson(companyDescriptorToWrite);
            File.WriteAllText(filePath, dataAsJson);
        }
    }


    /**********************************************************************************/
    // CreateNewData
    // создаём пустышку для работы
    //
    /**********************************************************************************/
    private void CreateNewData()
    {
        m_lastOpenFile = "";
        CompanyData = new CompanyDescriptorEditor();
    }
}