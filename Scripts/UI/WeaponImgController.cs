using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WeaponImageSet
{
    public Sprite sprite100 = null;
    public Sprite sprite75 = null;
    public Sprite sprite50 = null;
    public Sprite sprite25 = null;
    public Sprite sprite0 = null;

    public bool UseColoring = false;
}

/**********************************************************************************************/
// WeaponImgController класс
// класс ответственный за иконку оружия
// служит индикатором для кол-ва снарядов в оружии или индикатором перезарядки
//
/**********************************************************************************************/
public class WeaponImgController : MonoBehaviour
{
    public WeaponImageSet SpriteSet;
    public Text WeaponChargeText;

    protected Image m_imageIm = null;
    public Color ColorChecme = Color.white;
    protected bool m_isInited = false;


    /**********************************************************************************************/
    // инициализация
    //
    /**********************************************************************************************/
    void Awake()
    {
        m_imageIm = GetComponent<Image>();

        // считаем что по дефолту оружие заряжено
        SetWeaponChargeState(100, 0, 0);

        m_isInited = true;
        m_imageIm.color = ColorChecme;
    }

    /**********************************************************************************************/
    // устанавливаем цвет для покраски спрайтов
    //
    /**********************************************************************************************/
    public void SetColorScheme(Color colorForColoring)
    {
        ColorChecme = colorForColoring;
        if (m_isInited)
        {
            m_imageIm.color = ColorChecme;
        }
    }

    /**********************************************************************************************/
    // функция устанавливающая прогресс для оружия (перезарядка и кол-во патронов)
    //
    /**********************************************************************************************/
    public void SetWeaponChargeState(int progress, int CurrentAmmo, int AmmoMax)
    {
        if (progress == 100)
        {
            m_imageIm.sprite = SpriteSet.sprite100;
        }
        else if (progress >= 75)
        {
            m_imageIm.sprite = SpriteSet.sprite75;
        }
        else if (progress >= 50)
        {
            m_imageIm.sprite = SpriteSet.sprite50;
        }
        else if (progress >= 25)
        {
            m_imageIm.sprite = SpriteSet.sprite25;
        }
        else
        {
            m_imageIm.sprite = SpriteSet.sprite0;
        }

        m_imageIm.color = ColorChecme;
        WeaponChargeText.text = CurrentAmmo + "/" + AmmoMax;
    }
}
