﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**********************************************************************************************/
// MainMenuController класс
// контролирует главного меню
// содержит все необходимые функции загрузки игры и выхода
//
/**********************************************************************************************/
public class MainMenuController : MonoBehaviour {

    /**********************************************************************************************/
    // загружаем дуэль
    //
    /**********************************************************************************************/
    public void LoadDuel()
    {
        GameManager.GetInstance().InitDuelGame();
        //SceneManager.LoadScene("DuelLevel", LoadSceneMode.Single);
    }

    /**********************************************************************************************/
    // загружаем одиночную игру для Гизмо
    //
    /**********************************************************************************************/
    public void LoadSingleGame_Gizmo()
    {
        GameManager.GetInstance().InitSingleGame(PLAYER.PL2);
    }


    /**********************************************************************************************/
    // загружаем одиночную игру для Ксерга 3-его
    //
    /**********************************************************************************************/
    public void LoadSingleGame_XsergIII()
    {
        GameManager.GetInstance().InitSingleGame(PLAYER.PL1);
    }

    /**********************************************************************************************/
    // выходим из игры
    //
    /**********************************************************************************************/
    public void ExitGame()
    {
        Application.Quit();
    }
}