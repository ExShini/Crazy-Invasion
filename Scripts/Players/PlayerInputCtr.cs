using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// PlayerInputCtr класс
// отвечает за обработку ввода
//
/**********************************************************************************/
public class PlayerInputCtr
{

    enum CONTROLLER_MODE
    {
        NOT_INIT,       // не инициализирован
        TOW_JOY,        // используются 2 джойстика
        KEY_PLUS_JOY,   // используются джойстик + клавиатура
        ONLY_KEY        // используются только клавиатура
    };

    // кнопки
    public enum CTR_KEY : int
    {
        FIRE_1 = 0,         // основное оружие дальнего боя
        FIRE_2 = 1,         // оружие захвата
        FIRE_3 = 2,         // взрывчатка
        FIRE_4 = 3,         // спец-оружие
        ANY_KEY = 4,         // когда нам без разцницы какая кнопка
        PAUSE = 5,
        NUM_OF_CONTROLLED_KEYS = 6
    }

    private static CONTROLLER_MODE s_controllerMode = CONTROLLER_MODE.NOT_INIT;


    private string m_xAxisName = "Horizontal";
    private string m_yAxisName = "Vertical";
    private string m_fire1Button = "Fire1";
    private string m_fire2Button = "Fire2";
    private string m_fire3Button = "Fire3";
    private string m_fire4Button = "Fire4";
    private string m_pauseBtn = "Pause";

    private int m_lastDirection = 0;
    private float m_lastVerticalSpeed = 0.0f;
    private float m_lastHorizontalSpeed = 0.0f;
    private bool[] m_bloks = new bool[4];

    /**********************************************************************************/
    // PlayerInputCtr конструктор
    //
    /**********************************************************************************/
    public PlayerInputCtr()
    {
        m_bloks = new bool[(int)CTR_KEY.NUM_OF_CONTROLLED_KEYS];
        for (int i = 0; i < (int)CTR_KEY.NUM_OF_CONTROLLED_KEYS; i++)
        {
            m_bloks[i] = false;
        }
    }

    /**********************************************************************************/
    // определяем имена осей устройст ввода по ID игрока
    //
    /**********************************************************************************/
    public void DetermineAxisNameViaPlayerId(int playerID)
    {
        if (s_controllerMode == CONTROLLER_MODE.NOT_INIT)
        {
            DecideControllerMode();
        }

        switch (s_controllerMode)
        {
            case CONTROLLER_MODE.TOW_JOY:
                m_xAxisName += "_Joy" + playerID;
                m_yAxisName += "_Joy" + playerID;
                m_fire1Button += "_Joy" + playerID;
                m_fire2Button += "_Joy" + playerID;
                m_fire3Button += "_Joy" + playerID;
                m_fire4Button += "_Joy" + playerID;
                m_pauseBtn += "_Joy" + playerID;
                break;

            case CONTROLLER_MODE.KEY_PLUS_JOY:
                if (playerID == 1)
                {
                    m_xAxisName += "_Joy" + playerID;
                    m_yAxisName += "_Joy" + playerID;
                    m_fire1Button += "_Joy" + playerID;
                    m_fire2Button += "_Joy" + playerID;
                    m_fire3Button += "_Joy" + playerID;
                    m_fire4Button += "_Joy" + playerID;
                    m_pauseBtn += "_Joy" + playerID;
                }
                // второй игрок остаётся с дефолтными настройками для клавиатуры
                break;

            case CONTROLLER_MODE.ONLY_KEY:
                // даём игроку вторую клавиатурную раскладку
                if (playerID == 2)
                {
                    m_xAxisName += "_Sec";
                    m_yAxisName += "_Sec";
                    m_fire1Button += "_Sec";
                    m_fire2Button += "_Sec";
                    m_fire3Button += "_Sec";
                    m_fire4Button += "_Sec";

                    // Pause кнопка в этом случае одна на двоих, второй не надо
                }
                break;
            default:
                Debug.Log("DetermineAxisNameViaPlayerId: s_controllerMode is wrong!");
                break;
        }
    }


    /**********************************************************************************/
    // функция определяющая режим работы контроллера
    // проверяет кол-во подключённых устройств и выбирает режим работы всех PlayerInputCtr
    //
    /**********************************************************************************/
    protected static void DecideControllerMode()
    {
        // determine number of connected joystick
        string[] joystickNames = Input.GetJoystickNames();
        int numOfJoy = joystickNames.Length;

        if (numOfJoy >= 2)
        {
            s_controllerMode = CONTROLLER_MODE.TOW_JOY;
        }
        else if (numOfJoy == 1)
        {
            s_controllerMode = CONTROLLER_MODE.KEY_PLUS_JOY;
        }
        else
        {
            s_controllerMode = CONTROLLER_MODE.ONLY_KEY;
        }

        Debug.Log("Input controller set to " + s_controllerMode.ToString());
    }

    /**********************************************************************************/
    // функция определяющая направление движения (взгляда в случае, если движения нет) игрока
    // считывает показания с клавиатуры или джойстиков (кнопки: asdw, стрелки, рычажки на джойстиках)
    //
    /**********************************************************************************/
    public Vector2 GetDirection(out int direction, out bool isMoved)
    {
        float moveHorizontal = 0;
        float moveVertical = 0;

        //GetAxisRaw
        moveHorizontal = Input.GetAxisRaw(m_xAxisName);
        moveVertical = Input.GetAxisRaw(m_yAxisName);

        float absMoveHorizontal = Mathf.Abs(moveHorizontal);
        float absMoveVertical = Mathf.Abs(moveVertical);

        if (absMoveHorizontal == absMoveVertical)
        {
            // если движения нет - останавливаем всё
            if (absMoveHorizontal == 0)
            {
                isMoved = false;
                moveHorizontal = 0;
                moveVertical = 0;
                m_lastHorizontalSpeed = 0;
                m_lastVerticalSpeed = 0;
            }
            // если есть инпут сразу с двух кнопок - продолжаем движение в прошлом направлении
            else
            {
                moveHorizontal = m_lastHorizontalSpeed;
                moveVertical = m_lastVerticalSpeed;
                isMoved = true;
            }

            direction = m_lastDirection;

        }
        else if (absMoveHorizontal > absMoveVertical)
        {
            moveVertical = 0.0f;

            // определяем направление движения по горизонтали
            if (moveHorizontal > 0)
            {
                direction = (int)Base.DIREC.RIGHT;
            }
            else
            {
                direction = (int)Base.DIREC.LEFT;
            }
            isMoved = true;
            m_lastHorizontalSpeed = moveHorizontal;
            m_lastVerticalSpeed = 0.0f;
        }
        else
        {
            moveHorizontal = 0.0f;

            // определяем направление движения по вертикали
            if (moveVertical > 0)
            {
                direction = (int)Base.DIREC.UP;
            }
            else
            {
                direction = (int)Base.DIREC.DOWN;
            }
            isMoved = true;
            m_lastHorizontalSpeed = 0.0f;
            m_lastVerticalSpeed = moveVertical;
        }

        //Use the two store floats to create a new Vector2 variable movement.
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // save direction for next iteration of animation
        m_lastDirection = direction;

        return movement;
    }

    /**********************************************************************************/
    // функция проверяющая не нажата ли конкретная кнопка
    //
    /**********************************************************************************/
    public bool IsKeyPressed(CTR_KEY key)
    {
        bool pressed = false;
        bool isBlocked = m_bloks[(int)key];

        switch (key)
        {
            case CTR_KEY.FIRE_1:
                pressed = Input.GetButton(m_fire1Button);
                break;
            case CTR_KEY.FIRE_2:
                pressed = Input.GetButton(m_fire2Button);
                break;
            case CTR_KEY.FIRE_3:
                pressed = Input.GetButton(m_fire3Button);
                break;
            case CTR_KEY.FIRE_4:
                pressed = Input.GetButton(m_fire4Button);
                break;
            case CTR_KEY.PAUSE:
                pressed = Input.GetButton(m_pauseBtn);
                break;
            case CTR_KEY.ANY_KEY:
                pressed = Input.anyKey;
                break;
        }

        // проверяем блокировку
        if (isBlocked)
        {
            // если блокируем - инвертируем нажатие
            if (pressed)
            {
                pressed = false;
            }
            else
            {
                // снимаем блокировку
                m_bloks[(int)key] = false;
            }
        }

        return pressed;
    }

    /**********************************************************************************/
    // функция блокирующая кнопку
    //
    /**********************************************************************************/
    public void BlockButton(CTR_KEY key)
    {
        m_bloks[(int)key] = true;
    }


}
