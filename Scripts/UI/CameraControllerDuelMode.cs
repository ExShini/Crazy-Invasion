using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************************/
// CameraControllerDuelMode класс
// управляет камерой
//
/**********************************************************************************************/
public class CameraControllerDuelMode : MonoBehaviour
{

    public float scalingTimeSec = 3.0f;
    public float scalingMinDist = 0.2f;
    public float scalingMaxDist = 0.8f;


    public static float ShakePower = 0.01f;
    public static float ShakeAttenuation = 0.015f;
    static float m_currentShakePower = 0.0f;

    public float minimalOrtographicSize = 1.0f;

    protected enum CameraState
    {
        PRE_INIT,   // ожидаем пока игроки проинициализируются прежде чем начинать работу камеры
        NORMAL,     // камера работает без изменений масштаба
        SCALING_UP,  // камера в процессе изменения масштаба
        SCALING_DOWN
    }

    protected Camera m_mainCamera = null;
    float m_targetCameraSize = 0;
    protected CameraState m_state = CameraState.PRE_INIT;
    protected float m_scalingStep = 0.0f;

    protected float m_verticalScreenSize;
    protected float m_horisontalScreenSize;

    protected int m_optimizationCnt = 0;
    public int optimizationFactor = 5;

    protected PlayerController m_pl1Ctr = null;
    protected PlayerController m_pl2Ctr = null;

    /**********************************************************************************************/
    // инициализируем контроллер
    //
    /**********************************************************************************************/
    protected void Start()
    {
        m_mainCamera = GetComponent<Camera>();

        Resolution resolutions = Screen.currentResolution;
        
        // устанавливаем дефолтный ортографический размер камеры
        CalculateOrtographicSize();
    }

    /**********************************************************************************************/
    // включаем трясение камеры
    //
    /**********************************************************************************************/
    public static void ShakeCamera(float power = 1)
    {
        m_currentShakePower += ShakePower * power;
    }

    /**********************************************************************************************/
    // вычисляем дефолтный ортографический размер для данного разрешения монитора
    //
    /**********************************************************************************************/
    void CalculateOrtographicSize()
    {
        // мы используем PPU = 100
        Resolution resolutions = Screen.currentResolution;
        float ortSize = (resolutions.height / 100) / 2;
        m_mainCamera.orthographicSize = ortSize;

        m_targetCameraSize = ortSize;

        UpdateScreenSizes();
    }

    /**********************************************************************************************/
    // вычисляем размеры экрана в unit-ах, это необходимо для подсчёта скейлинга камеры
    //
    /**********************************************************************************************/
    void UpdateScreenSizes()
    {
        m_verticalScreenSize = m_targetCameraSize * 2;
        m_horisontalScreenSize = ((float)Screen.width / (float)Screen.height) * m_verticalScreenSize;
    }

    /**********************************************************************************************/
    // уменьшаем ортографический размер камеры, приближая картинку
    //
    /**********************************************************************************************/
    void ScaleUp()
    {
        // мы пытаемся избежать слишком сильного приближения
        if(minimalOrtographicSize >= m_targetCameraSize / 2)
        {
            return;
        }

        m_targetCameraSize = m_targetCameraSize / 2;
        m_state = CameraState.SCALING_UP;
        m_scalingStep = Mathf.Abs(m_mainCamera.orthographicSize - m_targetCameraSize) / scalingTimeSec;
    }

    /**********************************************************************************************/
    // увеличиваем ортографический размер камеры, отдаляя картинку
    //
    /**********************************************************************************************/
    void ScaleDown()
    {
        m_targetCameraSize = m_targetCameraSize * 2;
        m_state = CameraState.SCALING_DOWN;
        m_scalingStep = Mathf.Abs(m_mainCamera.orthographicSize - m_targetCameraSize) / scalingTimeSec;
    }


    /**********************************************************************************************/
    // процессинг камеры
    //
    /**********************************************************************************************/
    private void FixedUpdate()
    {
        if (m_state == CameraState.NORMAL)
        {
            // проверяем позицию, но не каждую итерацию (одну из optimizationFactor)
            if (m_optimizationCnt == optimizationFactor)
            {
                CheckPlayerDistance();
                m_optimizationCnt = 0;
            }

            m_optimizationCnt++;
        }
        // приближаем камеру
        else if (m_state == CameraState.SCALING_UP)
        {
            m_mainCamera.orthographicSize -= m_scalingStep * Time.deltaTime;
            if (m_mainCamera.orthographicSize <= m_targetCameraSize)
            {
                m_mainCamera.orthographicSize = m_targetCameraSize;
                m_state = CameraState.NORMAL;
                UpdateScreenSizes();
            }
        }
        // отдаляем камеру
        else if (m_state == CameraState.SCALING_DOWN)
        {
            m_mainCamera.orthographicSize += m_scalingStep * Time.deltaTime;
            if (m_mainCamera.orthographicSize >= m_targetCameraSize)
            {
                m_mainCamera.orthographicSize = m_targetCameraSize;
                m_state = CameraState.NORMAL;
                UpdateScreenSizes();
            }
        }
        // до сюда мы должны доходить только в случае если партия ещё не началась и мы ждём инициализации игроков
        else if (m_state == CameraState.PRE_INIT)
        {
            // полим на предмет инициализации игроков
            if(GetPlayersControllers())
            {
                m_state = CameraState.NORMAL;
            }
        }

    }

    /**********************************************************************************************/
    // получаем контроллеры игроков
    // возвращаем true в случае успеха
    //
    /**********************************************************************************************/
    protected virtual bool GetPlayersControllers()
    {
        GameManager gManager = GameManager.GetInstance();
        m_pl1Ctr = gManager.GetPlayers(PLAYER.PL1);
        m_pl2Ctr = gManager.GetPlayers(PLAYER.PL2);

        if(m_pl1Ctr != null && m_pl2Ctr != null)
        {
            return true;
        }

        return false;
    }


    /*******************************/
    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    /**********************************************************************************************/
    // обновляем позицию камеры
    //
    /**********************************************************************************************/
    void UpdateCameraPosition()
    {
        if (m_state != CameraState.PRE_INIT)
        {
            Vector2 centerPosition = (m_pl1Ctr.Position + m_pl2Ctr.Position) / 2;
            float xShaking = 0.0f;
            float yShaking = 0.0f;

            // проверяем дрожание камеры
            if (m_currentShakePower > 0)
            {
                xShaking = Random.Range(-m_currentShakePower, m_currentShakePower);
                yShaking = Random.Range(-m_currentShakePower, m_currentShakePower);

                m_currentShakePower -= ShakeAttenuation * Time.deltaTime;
                if(m_currentShakePower < 0)
                {
                    m_currentShakePower = 0;
                }
            }

            transform.position = new Vector3(centerPosition.x + xShaking, centerPosition.y + yShaking, -10);
        }
    }

    /**********************************************************************************************/
    // проверяем дистанцию между ироками
    // если большая - отдаляем камеру
    // если маленькая - приближаем
    //
    /**********************************************************************************************/
    void CheckPlayerDistance()
    {
        Vector2 pl1Pos = m_pl1Ctr.Position;
        Vector2 pl2Pos = m_pl2Ctr.Position;


        // вычисляем расстояние в долях от размера экрана
        float xDiff = Mathf.Abs(pl1Pos.x - pl2Pos.x) / m_horisontalScreenSize;
        float yDiff = Mathf.Abs(pl1Pos.y - pl2Pos.y) / m_verticalScreenSize;

        // выбираем наиболее значимое
        float dist = 0.0f;
        if (xDiff > yDiff)
        {
            dist = xDiff;
        }
        else
        {
            dist = yDiff;
        }

        // проверяем расстояние
        if (dist < scalingMinDist)
        {
            ScaleUp();
        }
        else if (dist > scalingMaxDist)
        {
            ScaleDown();
        }
    }
}
