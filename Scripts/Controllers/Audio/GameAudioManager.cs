using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using DigitalRuby.SoundManagerNamespace;


/**********************************************************************************/
// GameAudioManager
// контролирует все звуки и музыку использующуюся в игре
//
/**********************************************************************************/
public class GameAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioClipPackage
    {
        public string Key;
        public AudioClipBox[] Clips;
        public float DelayBeetwenSounds = 0.0f;
    }

    [System.Serializable]
    public class AudioClipBox
    {
        public AudioClip Clip;
        public float Volume = 1.0f;
    }

    // состояния менеджера, в зависимости от этого выбирается трек
    protected enum GAM_STATE
    {
        INIT,
        MENU,
        GAME
    }

    protected enum TRACK_STATE
    {
        STOP,
        PLAY,
        SWITCHING
    }

    public float MusicVolume = 1.0f;
    public float SoundVolume = 1.0f;
    public float MusicTrackFadeOut = 2.0f;

    public List<AudioClipPackage> SoundAudioSources = new List<AudioClipPackage>();
    public List<AudioClipPackage> MainMenuPlayListSources = new List<AudioClipPackage>();
    public List<AudioClipPackage> GamePlayListSources = new List<AudioClipPackage>();

    protected Dictionary<string, AudioClipPackage> m_soundEffectCollection = new Dictionary<string, AudioClipPackage>();
    LinkedList<AudioSource> m_freeSourceCollections = new LinkedList<AudioSource>();
    LinkedList<AudioSource> m_soundeSourceInUseCollections = new LinkedList<AudioSource>();
    Dictionary<string, float> m_soundTimers = new Dictionary<string, float>();
    Dictionary<string, int> m_soundQueue = new Dictionary<string, int>();

    public AudioSource MusicSource1;
    public AudioSource MusicSource2;
    public AudioSource AudioEffectLine;

    protected GAM_STATE m_state = GAM_STATE.INIT;
    protected TRACK_STATE m_trackState = TRACK_STATE.STOP;
    protected int m_currentTreckNumber = 0;

    protected float m_nextTreckSwitch = 0.0f;
    protected float m_currentTrackTiming = 0.0f;



    #region singleton
    public static GameAudioManager Instance { get { return s_instance; } }
    private static GameAudioManager s_instance;   // GameSystem local instance

    /**********************************************************************************/
    // делаем себя бессмертными
    //
    /**********************************************************************************/
    void Awake()
    {
        if (s_instance == null)
            s_instance = this;
        else if (s_instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    #endregion


    /**********************************************************************************/
    // парсим звуки, сохраняем всю необходимую информацию
    //
    /**********************************************************************************/
    private void Start()
    {
        // парсим все звуковые эффекты и сохраняем их по ключу
        for (int i = 0; i < SoundAudioSources.Count; i++)
        {
            string key = SoundAudioSources[i].Key;
            m_soundEffectCollection[key] = SoundAudioSources[i];

            // настраиваем таймера и очереди
            m_soundTimers[key] = 0.0f;
            m_soundQueue[key] = 0;
        }

        // настраиваем изначальное состояние
        m_trackState = TRACK_STATE.SWITCHING;
        m_state = GAM_STATE.MENU;

        // да будет диско!
        // запускаем музыку
        PlayNextTrack();
    }

    /**********************************************************************************/
    // проверяем, не надо ли включить следующий клип
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        if (m_state != GAM_STATE.INIT)
        {
            // обновляем таймера
            m_nextTreckSwitch -= Time.deltaTime;
            m_currentTrackTiming += Time.deltaTime;

            if (m_trackState == TRACK_STATE.PLAY)
            {
                // проверяем время до конца трека
                // если осталось меньше MusicTrackFadeOut - переключаем треки
                if (m_nextTreckSwitch <= MusicTrackFadeOut)
                {
                    PlayNextTrack();
                    m_trackState = TRACK_STATE.SWITCHING;
                }
            }
            else if (m_trackState == TRACK_STATE.SWITCHING)
            {
                // обновляем громкости в зависимости от прошедшего времени
                if (m_currentTrackTiming >= MusicTrackFadeOut)
                {
                    MusicSource1.volume = MusicVolume;
                    MusicSource2.volume = 0.0f;
                    MusicSource2.Stop();
                    m_trackState = TRACK_STATE.PLAY;
                }
                else
                {
                    float volumeChangeSpeed = (MusicVolume / MusicTrackFadeOut) * Time.deltaTime;
                    MusicSource1.volume += volumeChangeSpeed;
                    MusicSource2.volume -= volumeChangeSpeed;
                }
            }

            // проверяем состояние воспроизводимых эффектов
            bool process = true;
            while (process && m_soundeSourceInUseCollections.Count > 0)
            {
                AudioSource source = m_soundeSourceInUseCollections.First.Value;
                // если воспроизведение закончилось, перемещаем дорожку в список свободных
                if (!source.isPlaying)
                {
                    m_soundeSourceInUseCollections.RemoveFirst();
                    m_freeSourceCollections.AddLast(source);
                }
                else
                {
                    // в противном случае прерываем обработку
                    process = false;
                }
            }


            ProcessTimersAndQueue();

        }
    }


    /**********************************************************************************/
    // функция проверяет отложенные звуковые эффекты и запускает их в случае необходимости
    //
    /**********************************************************************************/
    protected void ProcessTimersAndQueue()
    {
        List<string> keys = new List<string>(m_soundTimers.Keys);
        foreach (string soundKey in keys)
        {
            int queue = m_soundQueue[soundKey];
            
            // если в очереди что-то есть - проверяем таймера
            if (queue > 0)
            {
                float timer = m_soundTimers[soundKey];
                timer -= Time.deltaTime;

                // если пришло время воспроизводить звук - делаем это
                if (timer <= 0.0f)
                {
                    m_soundTimers[soundKey] = timer;
                    PlaySound(soundKey);
                    queue--;

                    // выправляем значение таймера
                    if(queue > 0)
                    {
                        AudioClipPackage package = m_soundEffectCollection[soundKey];
                        timer += package.DelayBeetwenSounds;
                    }
                    else
                    {
                        timer = 0.0f;
                    }

                    m_soundQueue[soundKey] = queue;
                }

                // сохраняем обновлённый таймер со значениями для следующего цикла
                m_soundTimers[soundKey] = timer;
            }
            
            }
        }


    /**********************************************************************************/
    // включаем следующий трек  зависимости от того, где находимся
    //
    /**********************************************************************************/
    public void PlayNextTrack()
    {
        List<AudioClipPackage> musicSource = null;

        if (m_state == GAM_STATE.MENU)
        {
            musicSource = MainMenuPlayListSources;
        }
        else if (m_state == GAM_STATE.GAME)
        {
            musicSource = GamePlayListSources;
        }

        m_currentTreckNumber++;
        if (m_currentTreckNumber >= musicSource.Count)
        {
            m_currentTreckNumber = 0;
        }

        // меняем местами ссылки на источники звука
        AudioSource currentSource = MusicSource1;
        MusicSource1 = MusicSource2;
        MusicSource2 = currentSource;


        // устанавливаем следуюший трек в новый мажорный источник
        AudioClipPackage package = musicSource[m_currentTreckNumber];
        int clipIndex = Random.Range(0, package.Clips.Length);
        AudioClip nextClip = package.Clips[clipIndex].Clip;
        MusicSource1.clip = nextClip;
        MusicSource1.Play();

        m_currentTrackTiming = 0.0f;
        m_nextTreckSwitch = nextClip.length;
    }

    /**********************************************************************************/
    // воспроизводим звуковой эффект по ключу
    //
    /**********************************************************************************/
    public void PlaySound(string SoundKey)
    {
        if (!m_soundEffectCollection.ContainsKey(SoundKey))
        {
            Debug.LogError("We have no such music! Key: " + SoundKey);
            return;
        }

        // проверяем таймера
        if (m_soundTimers[SoundKey] > 0.0f)
        {
            // если таймер ещё не отработал - отправляем звук в очередь и выходим
            m_soundQueue[SoundKey] = m_soundQueue[SoundKey] + 1;
            return;
        }
        else
        {
            m_soundTimers[SoundKey] = m_soundEffectCollection[SoundKey].DelayBeetwenSounds;
        }


        // получаем аудио "дорожку" для воспроизведения
        AudioSource soundEffectAudioSource = null;

        // если есть свободная - используем её
        if (m_freeSourceCollections.Count > 0)
        {
            soundEffectAudioSource = m_freeSourceCollections.First.Value;
            m_freeSourceCollections.RemoveFirst();
        }
        // в противном случае создаём новую
        else
        {
            soundEffectAudioSource = Instantiate(AudioEffectLine, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as AudioSource;
            soundEffectAudioSource.transform.SetParent(this.transform);
        }

        // выбираем звуковой эффект для воспроизведения
        AudioClipPackage package = m_soundEffectCollection[SoundKey];
        int clipIndex = Random.Range(0, package.Clips.Length);

        // включаем звуковой эффект
        soundEffectAudioSource.clip = package.Clips[clipIndex].Clip;
        soundEffectAudioSource.Play();
        soundEffectAudioSource.volume = SoundVolume * package.Clips[clipIndex].Volume;
        m_soundeSourceInUseCollections.AddLast(soundEffectAudioSource);
    }

    /**********************************************************************************/
    // останавливаем музыку по ключу
    //
    /**********************************************************************************/
    private void StopMusic(string MusicKey)
    {
        /*
        if (!m_audioSourceCollections.ContainsKey(MusicKey))
        {
            Debug.LogError("We have no such music! Key: " + MusicKey);
        }

        
        AudioSource source = m_audioSourceCollections[MusicKey];
        //source.Stop();
        //source.StopLoopingMusicManaged();

        */
    }

    /**********************************************************************************/
    // переходим в игровой мод
    //
    /**********************************************************************************/
    public void SwitchToGameMode()
    {
        m_state = GAM_STATE.GAME;
        m_trackState = TRACK_STATE.SWITCHING;
        m_nextTreckSwitch = MusicTrackFadeOut;
    }

    /**********************************************************************************/
    // переходим в меню
    //
    /**********************************************************************************/
    public void SwitchToMenuMode()
    {
        m_state = GAM_STATE.MENU;
        m_trackState = TRACK_STATE.SWITCHING;
        m_nextTreckSwitch = MusicTrackFadeOut;
    }
}