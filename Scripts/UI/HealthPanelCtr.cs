using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**********************************************************************************************/
// HealthPanelCtr класс
// контролирует все объекты в панели здоровья конкретного игрока
//
/**********************************************************************************************/
public class HealthPanelCtr : MonoBehaviour
{

    public Image HeartIco = null;
    public Image ShieldIco = null;

    protected LinkedList<Image> m_heartIcons = new LinkedList<Image>();
    protected LinkedList<Image> m_activeHeartIcons = new LinkedList<Image>();

    protected LinkedList<Image> m_shieldIcons = new LinkedList<Image>();
    protected LinkedList<Image> m_disabledShieldIcons = new LinkedList<Image>();
    protected LinkedList<Image> m_activeShieldIcons = new LinkedList<Image>();


    /**********************************************************************************************/
    // инициализация
    //
    /**********************************************************************************************/
    void Start()
    {

        if (HeartIco == null || ShieldIco == null)
        {
            Debug.Log("ERROR! HeartIco or ShieldIco is NULL!");
            return;
        }
    }

    /**********************************************************************************************/
    // функция устанавливает кол-во жизней игрока
    //
    /**********************************************************************************************/
    public void SetHearts(int number)
    {
        for (int i = 0; i < number; i++)
        {
            AddNewHeartIcon();
        }
    }

    /**********************************************************************************************/
    // функция устанавливает кол-во потенциальных очков щита
    //
    /**********************************************************************************************/
    public void SetShieldPoints(int number)
    {
        for (int i = 0; i < number; i++)
        {
            AddNewShildIcon();
        }
    }

    /**********************************************************************************************/
    // функция заряжает значки щитов
    //
    /**********************************************************************************************/
    public void ChargeShieldIcon(int number)
    {
        int currentNumberOfActiveShield = m_activeShieldIcons.Count;    // получаем кол-во активных щитов

        // проверяем, можем ли мы зарядить такое кол-во щитов
        int shieldToRecharge = number;
        if(number > m_disabledShieldIcons.Count)
        {
            shieldToRecharge = m_disabledShieldIcons.Count;
        }

        // заряжаем щиты
        for(int chargedShield = 0; chargedShield < shieldToRecharge; chargedShield++)
        {
            Image shieldIco = m_disabledShieldIcons.First.Value;
            m_disabledShieldIcons.RemoveFirst();

            // активируем
            Animator animator = shieldIco.GetComponent<Animator>();
            animator.SetBool("isActive", true);

            // сохраняем в пуле активных
            m_activeShieldIcons.AddLast(shieldIco);
        }
    }

    /**********************************************************************************************/
    // функция устанавливает кол-во жизней игрока
    //
    /**********************************************************************************************/
    public void LoseHeart(int number = 1)
    {
        if (m_activeHeartIcons.Count - number < 0)
        {
            // снимаем все оставшиеся очки жизни
            number = m_activeHeartIcons.Count;
        }

        for (int healthToLose = number; healthToLose > 0; healthToLose--)
        {
            Image hearttIco = m_activeHeartIcons.Last.Value;
            m_activeHeartIcons.RemoveLast();

            Animator animator = hearttIco.GetComponent<Animator>();
            animator.SetBool("isActive", false);
        }
    }


    /**********************************************************************************************/
    // функция устанавливает кол-во очков щита игрока
    //
    /**********************************************************************************************/
    public void LoseShield(int number = 1)
    {
        if (m_activeShieldIcons.Count - number < 0)
        {
            // снимаем все оставшиеся очки щита
            number = m_activeShieldIcons.Count;
        }

        for (int healthToLose = number; healthToLose > 0; healthToLose--)
        {
            // убираем из активных
            Image ShieldIco = m_activeShieldIcons.Last.Value;
            m_activeShieldIcons.RemoveLast();

            // выключаем
            Animator animator = ShieldIco.GetComponent<Animator>();
            animator.SetBool("isActive", false);

            // добавляем в отключённые
            m_disabledShieldIcons.AddFirst(ShieldIco);
        }
    }

    /**********************************************************************************************/
    // функция добавляет новую иконку сердечка в панель
    //
    /**********************************************************************************************/
    void AddNewHeartIcon()
    {
        if (HeartIco == null)
        {
            return;
        }

        Image hearttIco = Instantiate(HeartIco, new Vector3(0f, 0f, 0f), Quaternion.identity) as Image;
        hearttIco.transform.SetParent(gameObject.transform);

        // устанавливаем якорь в левый верхний угол
        hearttIco.rectTransform.anchorMax = new Vector2(0f, 1f);
        hearttIco.rectTransform.anchorMin = new Vector2(0f, 1f);

        // рассчитываем позицию сердечка в зависимости от кол-ва уже созданных
        int count = m_heartIcons.Count;
        hearttIco.rectTransform.anchoredPosition = new Vector2(12f + 32f * count, -12f);

        // сохраняем ссылку на иконку
        m_heartIcons.AddLast(hearttIco);
        m_activeHeartIcons.AddLast(hearttIco);
    }


    /**********************************************************************************************/
    // функция добавляет новую иконку щита в панель
    //
    /**********************************************************************************************/
    void AddNewShildIcon()
    {
        if (ShieldIco == null)
        {
            return;
        }

        Image shieldIco = Instantiate(ShieldIco, new Vector3(0f, 0f, 0f), Quaternion.identity) as Image;
        shieldIco.transform.SetParent(gameObject.transform);

        // устанавливаем якорь в левый верхний угол
        shieldIco.rectTransform.anchorMax = new Vector2(0f, 1f);
        shieldIco.rectTransform.anchorMin = new Vector2(0f, 1f);

        // рассчитываем позицию щитка в зависимости от кол-ва уже созданных
        int count = m_shieldIcons.Count;
        shieldIco.rectTransform.anchoredPosition = new Vector2(12f + 32f * count, -48f);

        // по дефолту у игроков все щиты разряжены
        Animator animator = shieldIco.GetComponent<Animator>();
        animator.SetBool("isActive", false);

        // сохраняем ссылку на иконку
        m_shieldIcons.AddLast(shieldIco);
        m_disabledShieldIcons.AddLast(shieldIco);

    }
}
