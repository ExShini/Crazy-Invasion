using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrenadeCtr : Bullet
{
    public int ExplosionRadius = 2;
    public float FlyDistance = 1.28f;
    public float Gravity = 1.0f;
    public ParticleSystem BurstSystem;

    protected float m_currentFlyTime = 0.0f;

    float m_horisontalSpeed = 0.0f;
    List<float> m_additionalHeight = new List<float>();
    List<float> m_heightDiff = new List<float>();
    List<float> m_speedYHashDown = new List<float>();
    List<float> m_speedYHashUp = new List<float>();


    protected List<CIGameObject> m_players = new List<CIGameObject>();
    protected float m_timeStep;
    protected int m_appliedStep = 0;
    protected float m_numOfSpeedHeshes = 20.0f;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    new public void Start()
    {
        base.Start();

        // скорость гранаты зависит от параметров дистанции полёта и коэффициента гравитации

        // нулевая скорость
        float V0 = Mathf.Sqrt(FlyDistance * Gravity);

        // полётное время (T = ( 2 * V0 * sin45) / g)
        float TFly = (V0 * Mathf.Sqrt(2.0f)) / Gravity;

        // горизонтальная скорость (не меняется в течении полёта)
        float Vx = V0 / 1.4142f;
        m_horisontalSpeed = Vx;

        // начальная вертикальная скорость равна начальной горизонтальной
        float Vy0 = Vx;


        // рассчитываем количество шагов для нашего хеширования
        m_timeStep = TFly / m_numOfSpeedHeshes;

        // производим рассчёт добавочной высоты для каждой из наших хеш точек
        float YSummDebug = 0.0f;
        float lastHeight = 0.0f;

        for(int tStep = 0; tStep < m_numOfSpeedHeshes + 1; tStep++)
        {
            float cTime = tStep * m_timeStep;
            float Y = Vy0 * cTime - (Gravity * Mathf.Pow(cTime, 2.0f)) / 2;
            m_additionalHeight.Add(Y);
            

            // считаем разницу высот
            float Ydiff = Y - lastHeight;
            m_heightDiff.Add(Ydiff);
            YSummDebug += Ydiff;

            lastHeight = Y;

            float Vy = Vy0 - Gravity * cTime;
            m_speedYHashDown.Add(Vx - Vy);
            m_speedYHashUp.Add(Vx + Vy);
        }


        // запоминаем объекты игроков
        GameManager.GAME_MODE mode = GameManager.GetInstance().GameMode;
        if (mode == GameManager.GAME_MODE.SINGLE)
        {
            m_players.Add(GameManager.GetInstance().GetPlayer());
        }
        else if (mode == GameManager.GAME_MODE.DUEL)
        {
            m_players.Add(GameManager.GetInstance().GetPlayers(PLAYER.PL1));
            m_players.Add(GameManager.GetInstance().GetPlayers(PLAYER.PL2));
        }
    }


    /**********************************************************************************/
    // процессинг
    //
    /**********************************************************************************/
    protected override void FixedUpdate()
    {
        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        // манипулируем скоростью
        SpeedManipualation();

        // вызываем родительсикий метод
        base.FixedUpdate();
    }


    /**********************************************************************************/
    // функция контроля скорости
    // снаряд в процессе полёта замедляется
    //
    /**********************************************************************************/
    private void SpeedManipualation()
    {
        if (m_state != BULLET_STATE.BURN)
        {
            m_currentFlyTime += Time.deltaTime;
            int currentStep = (int)Mathf.Floor(m_currentFlyTime / m_timeStep);

            // проверяем, надо ли обновить скорость и высоту полёта гранаты
            if(currentStep > m_appliedStep)
            {
                if (currentStep >= m_numOfSpeedHeshes)
                {
                    m_state = BULLET_STATE.BURN;
                    speed = 0.0f;
                    m_animator.SetBool("Burn", true);
                    BurstSystem.Play();
                    PlayBurstSoundEffect();

                    BlowUpGrenade();
                    return;
                }

                Point directionVector = new Point((int)m_direction.x, (int)m_direction.y);
                Base.DIREC direction = directionVector.ToDirection();

                switch (direction)
                {
                    case Base.DIREC.LEFT:
                    case Base.DIREC.RIGHT:
                        speed = m_horisontalSpeed;
                        m_rb2d.position = new Vector2(m_rb2d.position.x, m_rb2d.position.y + m_heightDiff[currentStep]);
                        break;
                    case Base.DIREC.UP:
                        speed = m_speedYHashUp[currentStep];
                        break;
                    case Base.DIREC.DOWN:
                        speed = m_speedYHashDown[currentStep];
                        break;
                }

                m_appliedStep = currentStep;
            }
        }
    }

    /**********************************************************************************/
    // взрываем гранату
    //
    /**********************************************************************************/
    protected virtual void BlowUpGrenade()
    {
        Point position = GetGlobalPosition();
        List<CIGameObject> unitsInExplosion = GameObjectMapController.GetInstance().SearchEnemiesInRadius(position, ExplosionRadius, PLAYER.NO_PLAYER, false);
        DamageData dd = new DamageData(damage, DamageData.DAMAGE_TYPE.PHYSICAL, this, DamageData.RESPONSE.NOT_EXPECTED);

        // наносим урон всем причастным
        foreach(CIGameObject gmo in unitsInExplosion)
        {
            gmo.ApplyDamage(dd);
        }

        List<GeneratedEnvironmentCtr> envInExplosion = GameObjectMapController.GetInstance().SearchEnvironmentInRadius(position, ExplosionRadius, false);

        // наносим урон всем причастным
        foreach (CIGameObject env in envInExplosion)
        {
            env.ApplyDamage(dd);
        }

        // проверяем игроков
        foreach (CIGameObject plObject in m_players)
        {
            Point plPosition = plObject.GetGlobalPosition();
            Point diff = position - plPosition;

            if (diff.GetSimpleLength() <= ExplosionRadius)
            {
                plObject.ApplyDamage(dd);
            }
        }

        // трясём камеру
        CameraControllerDuelMode.ShakeCamera(ShakePower);
    }

    /**********************************************************************************/
    // сброс настроек на дефолтные
    //
    /**********************************************************************************/
    public override void ResetGObject()
    {
        base.ResetGObject();
        m_collider.enabled = false;
        m_appliedStep = 0;
        m_currentFlyTime = 0.0f;
    }
}
