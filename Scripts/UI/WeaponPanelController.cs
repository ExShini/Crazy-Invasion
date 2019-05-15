using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************************************************/
// WeaponPanelController класс
// контролирует все объекты в панели оружия конкретного игрока
//
/**********************************************************************************************/
public class WeaponPanelController : MonoBehaviour
{
    public Image[] weaponImageArr;
    public Color ColorChecme;
    protected List<WeaponImgController> m_weaponImgCtrArr = new List<WeaponImgController>();


    /**********************************************************************************************/
    // инициализация
    // собираем контроллер
    //
    /**********************************************************************************************/
    void Awake()
    {
        foreach (Image weaponImage in weaponImageArr)
        {
            if (weaponImage == null)
            {
                Debug.Log("ERROR! weaponImage is null");
                return;
            }

            WeaponImgController m_weaponImgCtr = weaponImage.GetComponent<WeaponImgController>();

            if (m_weaponImgCtr == null)
            {
                Debug.Log("ERROR! m_weaponImgCtr is null");
                return;
            }

            m_weaponImgCtrArr.Add(m_weaponImgCtr);
        }
    }

    /**********************************************************************************************/
    // устанавливаем конкретное оружие в конкретный слот
    //
    /**********************************************************************************************/
    public void SetWeaponInSlot(PlayerController.WEAPON_SLOT slot, WEAPON weaponType)
    {
        if ((int)slot >= m_weaponImgCtrArr.Count)
        {
            Debug.LogError("ERROR! wrong weaponId: " + slot);
            return;
        }

        WeaponImageSet iconsSet = WeaponLibrary.GetInstance().GetWeaponIcons(weaponType);

        Debug.Log("SetWeaponInSlot");
        if(iconsSet.sprite100 == null)
        {
            Debug.LogError("iconsSet is empty!");
        }

        m_weaponImgCtrArr[(int)slot].SpriteSet = iconsSet;

        // настраиваем цветовую схему
        Color colorForUse = Color.white;
        if(iconsSet.UseColoring)
        {
            colorForUse = ColorChecme;
        }

        m_weaponImgCtrArr[(int)slot].SetColorScheme(colorForUse);
    }

    /**********************************************************************************************/
    // функция устанавливает состояние для конкретного оружия
    //
    /**********************************************************************************************/
    public void SetWeaponState(PlayerController.WEAPON_SLOT weaponSlot, int weaponProgress, int CurrentAmmo, int MaxAmmo)
    {
        if ((int)weaponSlot >= m_weaponImgCtrArr.Count)
        {
            Debug.Log("ERROR! wrong weaponId");
            return;
        }

        m_weaponImgCtrArr[(int)weaponSlot].SetWeaponChargeState(weaponProgress, CurrentAmmo, MaxAmmo);
    }
}
