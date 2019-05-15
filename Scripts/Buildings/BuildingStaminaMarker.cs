using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/**********************************************************************************/
// BuildingStaminaMarker
// контроллер для управления визуальным отображением уровня стамины здания
//
/**********************************************************************************/
public class BuildingStaminaMarker : MonoBehaviour
{

    public Sprite[] Stamina;
    public Sprite Invulnerable;
    public Color Pl1Color;
    public Color Pl2Color;
    public Color NeutralColor;

    protected SpriteRenderer m_renderer;

    bool m_initialized = false;
    protected int m_currentStamina = 0;
    protected PLAYER m_owner = PLAYER.NEUTRAL;

    /**********************************************************************************/
    // инициализация
    //
    /**********************************************************************************/
    private void Start()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_initialized = true;
        UpdateMarker();
    }

    /**********************************************************************************/
    // устанавливаем уровень стамины
    //
    /**********************************************************************************/
    public void SetStamina(int stamina)
    {
        m_currentStamina = stamina;
        if (m_initialized)
        {
            UpdateMarker();
        }
    }

    /**********************************************************************************/
    // обновляем маркер
    //
    /**********************************************************************************/
    private void UpdateMarker()
    {
        if (m_currentStamina <= 0)
        {
            return;
        }

        // выставляем количество очков сопротивляемости
        if (Stamina.Length < m_currentStamina)
        {
            m_renderer.sprite = Invulnerable;
        }
        else
        {
            m_renderer.sprite = Stamina[m_currentStamina - 1];
        }

        // устанавливаем цвет
        switch (m_owner)
        {
            case PLAYER.PL1:
                m_renderer.color = Pl1Color;
                break;
            case PLAYER.PL2:
                m_renderer.color = Pl2Color;
                break;
            case PLAYER.NEUTRAL:
                m_renderer.color = NeutralColor;
                break;
            default:
                Debug.LogError("We cant set owner: " + m_owner.ToString());
                break;
        }
    }

    /**********************************************************************************/
    // устанавливаем владельца
    //
    /**********************************************************************************/
    public void SetOwner(PLAYER ownerID)
    {
        m_owner = ownerID;
        if (m_initialized)
        {
            UpdateMarker();
        }
    }
}