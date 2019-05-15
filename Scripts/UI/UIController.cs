using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************************************************/
// UIController класс
// контроллер ответственный за работу всего GUI
// все обращения происходят через него
//
/**********************************************************************************************/
public class UIController : MonoBehaviour
{

    protected static UIController s_instance = null;

    public Text ScopePl1Text = null;
    public Text ScopePl2Text = null;
    public Text WinText = null;
    public GameObject MessagePanel = null;
    public GameObject HealthPanelPl1 = null;
    public GameObject HealthPanelPl2 = null;
    public GameObject WeaponPanelPl1 = null;
    public GameObject WeaponPanelPl2 = null;
    public GameObject DialogPanel = null;
    public Text Pl1GameScoreText = null;
    public Text Pl2GameScoreText = null;

    /**********************************************************************************************/
    // UIController конструктор
    //
    /**********************************************************************************************/
    UIController()
    {
        s_instance = this;
    }

    /**********************************************************************************************/
    // GetInstance
    //
    /**********************************************************************************************/
    static public UIController GetInstance()
    {
        return s_instance;
    }

    /**********************************************************************************************/
    // SetPlayerHealth
    // функция устанавливает кол-во здоровья для конкретного игрока
    // используется в момент старта партии
    //
    /**********************************************************************************************/
    public void SetPlayerHealth(int playerId, int health, int shield)
    {
        GameObject healthPanel = getPlayerHealthPanelById(playerId);
        HealthPanelCtr ctr = healthPanel.GetComponent<HealthPanelCtr>();
        ctr.SetHearts(health);
        ctr.SetShieldPoints(shield);
    }

    /**********************************************************************************************/
    // функция "заряжает" иконку щита игрока
    //
    /**********************************************************************************************/
    public void ChargePlayerShield(int playerId, int shieldPoints)
    {
        GameObject healthPanel = getPlayerHealthPanelById(playerId);
        HealthPanelCtr ctr = healthPanel.GetComponent<HealthPanelCtr>();
        ctr.ChargeShieldIcon(shieldPoints);
    }

    /**********************************************************************************************/
    // функция отнимает жизни у указанного игрока
    //
    /**********************************************************************************************/
    public void LosePlayerHealth(int playerId, int healthToLose)
    {
        GameObject healthPanel = getPlayerHealthPanelById(playerId);
        HealthPanelCtr ctr = healthPanel.GetComponent<HealthPanelCtr>();
        ctr.LoseHeart(healthToLose);
    }


    /**********************************************************************************************/
    // функция отнимает очки щита у указанного игрока
    //
    /**********************************************************************************************/
    public void LosePlayerShield(int playerId, int shieldPointToLose)
    {
        GameObject healthPanel = getPlayerHealthPanelById(playerId);
        HealthPanelCtr ctr = healthPanel.GetComponent<HealthPanelCtr>();
        ctr.LoseShield(shieldPointToLose);
    }

    /**********************************************************************************************/
    // функция устанавливает конкретноое оружие в конкретный слот
    //
    /**********************************************************************************************/
    public void SetWeaponInSlot(int playerId, PlayerController.WEAPON_SLOT slot, WEAPON weaponType)
    {
        GameObject weaponPanel = getWeaponPanelById(playerId);
        WeaponPanelController ctr = weaponPanel.GetComponent<WeaponPanelController>();
        ctr.SetWeaponInSlot(slot, weaponType);
    }


    /**********************************************************************************************/
    // функция устанавливает состояние конкретного оружия для указанного игрока
    //
    /**********************************************************************************************/
    public void SetWeaponState(int playerId, PlayerController.WEAPON_SLOT slot, int progress, int CurrentAmmo, int MaxAmmo)
    {
        GameObject weaponPanel = getWeaponPanelById(playerId);
        WeaponPanelController ctr = weaponPanel.GetComponent<WeaponPanelController>();
        ctr.SetWeaponState(slot, progress, CurrentAmmo, MaxAmmo);
    }

    /**********************************************************************************************/
    // функция возвращает ссылку на HealthPanel по id игрока
    // protected функция, используется только в самом классе
    //
    /**********************************************************************************************/
    protected GameObject getPlayerHealthPanelById(int id)
    {
        switch (id)
        {
            case 1:
                return HealthPanelPl1;
            case 2:
                return HealthPanelPl2;
        }

        Debug.Log("ERROR! getPlayerHealthPanelById wrong player ID!");
        return null;
    }

    /**********************************************************************************************/
    // функция возвращает ссылку на WeaponPanel по id игрока
    // protected функция, используется только в самом классе
    //
    /**********************************************************************************************/
    protected GameObject getWeaponPanelById(int id)
    {
        switch (id)
        {
            case 1:
                return WeaponPanelPl1;
            case 2:
                return WeaponPanelPl2;
        }

        Debug.Log("ERROR! getWeaponPanelById wrong player ID!");
        return null;
    }


    /**********************************************************************************************/
    // Объявляем победителя матча/компании
    //
    /**********************************************************************************************/
    public void DeclareWinner(string winnerName)
    {
        WinText.text = winnerName.ToUpper() + " - ULTIMATE WINNER!";
        MessagePanel.SetActive(true);
    }

    /**********************************************************************************************/
    // Объявляем победителя раунда
    //
    /**********************************************************************************************/
    public void DeclareRoundWinner(string winnerName)
    {
        WinText.text = "ROUND WINNER - " + winnerName.ToUpper();
        MessagePanel.SetActive(true);
    }

    /**********************************************************************************************/
    // Объявляем о том что игра поставлена на паузу
    //
    /**********************************************************************************************/
    public void SetPause(bool paused)
    {
        if (paused)
        {
            MessagePanel.SetActive(true);
            WinText.text = "PAUSE";
        }
        else
        {
            MessagePanel.SetActive(false);
            WinText.text = "";
        }

    }

    /**********************************************************************************************/
    // Скрываем панель
    //
    /**********************************************************************************************/
    public void HideMessagePanel()
    {
        MessagePanel.SetActive(false);
    }

    /**********************************************************************************************/
    // ведём обратный отсчёт
    //
    /**********************************************************************************************/
    public void SetTimerMessage(int SecondsToShow)
    {
        WinText.text = "ROUND START IN " + SecondsToShow.ToString().ToUpper();
        MessagePanel.SetActive(true);
    }

    /**********************************************************************************************/
    // Обновляем игровой счёт
    //
    /**********************************************************************************************/
    public void SetScore(PLAYER playerId, int scope)
    {
        if(playerId == PLAYER.PL1)
        {
            ScopePl1Text.text = "Score: " + scope.ToString("0000");
        }
        else if(playerId == PLAYER.PL2)
        {
            ScopePl2Text.text = "Score: " + scope.ToString("0000");
        }
    }

    /**********************************************************************************************/
    // отображаем диалог на экране
    //
    /**********************************************************************************************/
    public void SetDialog(Base.GO_TYPE icoForType, string textKey, string NameKey)
    {
        DialogController ctr = DialogPanel.GetComponent<DialogController>();
        ctr.SetDialog(icoForType, textKey, NameKey);
    }

    /**********************************************************************************************/
    // скрываем диалог
    //
    /**********************************************************************************************/
    public void HideDialog()
    {
        DialogController ctr = DialogPanel.GetComponent<DialogController>();
        ctr.HideDialog();
    }

    /**********************************************************************************************/
    // Обновляем игровой счёт на экране
    //
    /**********************************************************************************************/
    public void SetGameScore(int Pl1Score, int Pl2Score)
    {
        if(Pl1GameScoreText == null || Pl1GameScoreText == null)
        {
            Debug.LogError("Score Text didn't set!");
            return;
        }

        Pl1GameScoreText.text = Pl1Score.ToString().ToUpper();
        Pl2GameScoreText.text = Pl2Score.ToString().ToUpper();
    }

    /**********************************************************************************************/
    // функция старта
    // используем для начального контроля за элементами интерфейса
    //
    /**********************************************************************************************/
    // Use this for initialization
    void Start()
    {
        if (WinText == null || MessagePanel == null)
        {
            Debug.Log("ERROR! WinText is NULL!!!");
            return;
        }

        WinText.text = "";
        MessagePanel.SetActive(false);

        GameManager gmanager = GameManager.GetInstance();
        GameManager.GAME_MODE mode = gmanager.GameMode;
        if(mode == GameManager.GAME_MODE.SINGLE)
        {
            // во время одиночной игры отключаем одну из 2х панелей здоровья и аналогично поступаем с панелью перезарядки
            if(gmanager.GetPlayer().playerId == PLAYER.PL1)
            {
                HealthPanelPl2.SetActive(false);
                WeaponPanelPl2.SetActive(false);

                HealthPanelPl1.SetActive(true);
                WeaponPanelPl1.SetActive(true);
            }
            else //if(gmanager.GetPlayer().playerId == PLAYER.PL2)
            {
                HealthPanelPl1.SetActive(false);
                WeaponPanelPl1.SetActive(false);

                HealthPanelPl2.SetActive(true);
                WeaponPanelPl2.SetActive(true);
            }
        }
    }


}
