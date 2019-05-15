using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************/
// BuildingController класс
// базовый класс для всех зданий
//
/**********************************************************************************/
public class BuildingController : MonoBehaviour
{
    public Base.GO_TYPE BuildingType;
    public float RateOfProduction = 0.0f;
    public float RateOfProductionNeutral = 0.0f;
    public int MaxNumberOfProd = 1;
    protected HashSet<int> m_createdUnitIDs = new HashSet<int>();
    public Base.GO_TYPE UnitToProduce_Pl1 = Base.GO_TYPE.NONE_TYPE;
    public Base.GO_TYPE UnitToProduce_Pl2 = Base.GO_TYPE.NONE_TYPE;
    public Base.GO_TYPE UnitToProduce_Neutral = Base.GO_TYPE.NONE_TYPE;
    public int MaxVirusStamina = 2;
    public int NeutralVirusStamina = 1;
    protected int m_currentVirusStamina = 1;
    protected bool m_isDestroyed = false;
    protected BuildingStaminaMarker m_virusMarker = null;


    public int XBuildingSize = 2;
    public int YBuildingSize = 2;
    public int CuptureScope = 1;

    public Point RoadPoint;

    private Animator m_animator = null;
    private Animator m_IIFlAnimator = null;
    private Rigidbody2D m_rb2d;
    private int m_ownerId = (int)PLAYER.NEUTRAL;
    private Base.GO_TYPE m_unitProdactionType = Base.GO_TYPE.NONE_TYPE;
    private float m_currentProductionTime = 0.0f;
    private Vector2 m_productionPosition;

    private bool m_isInitialized = false;

    /**********************************************************************************/
    // функция инициализации (Unity)
    //
    /**********************************************************************************/
    protected void Start()
    {
        InitializeBuilding();
    }


    /**********************************************************************************/
    // функция инициализации здания
    //
    /**********************************************************************************/
    void InitializeBuilding()
    {
        if (!m_isInitialized)
        {
            m_animator = GetComponent<Animator>();
            m_rb2d = GetComponent<Rigidbody2D>();

            // ставим дефолтного юнита на производство
            m_unitProdactionType = UnitToProduce_Neutral;

            if (m_ownerId == (int)PLAYER.NEUTRAL)
            {
                // устанавливаем ранодное стартовое значение для того, чтобы нейтральные войска выходили не в один момент
                m_currentProductionTime = Random.Range(1.0f, RateOfProductionNeutral);
            }
            else
            {
                m_currentProductionTime = RateOfProduction;
            }

            // получаем контроллер маркеров
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            for (int ind = 0; ind < allChildren.Length; ind++)
            {
                
                GameObject component = allChildren[ind].gameObject;
                if (component.tag == "VirusStaminaMarker")
                {
                    m_virusMarker = component.GetComponent<BuildingStaminaMarker>();
                    if(m_virusMarker == null)
                    {
                        Debug.LogError("We have a problem with Virus Marker!");
                    }
                }

                // получаем аниматор 2 этажа для контроля
                if(component.tag == "II_Floor")
                {
                    m_IIFlAnimator = component.GetComponent<Animator>();
                }
            }

            CalculateProductionPoint();

            m_currentVirusStamina = NeutralVirusStamina;
            m_virusMarker.SetStamina(m_currentVirusStamina);
            m_virusMarker.SetOwner(PLAYER.NEUTRAL);
            m_isInitialized = true;

            // регистрируем здание в контроллере
            GameObjectMapController.GetInstance().RegistBuilding(this);
        }
    }

    /**********************************************************************************/
    // возвращаем влядельца
    //
    /**********************************************************************************/
    public PLAYER GetOwnerId()
    {
        return (PLAYER)m_ownerId;
    }

    /**********************************************************************************/
    // функция возвращает состояние экземпляра контроллера
    //
    /**********************************************************************************/
    public bool IsInitialized()
    {
        return m_isInitialized;
    }

    /**********************************************************************************/
    // функция захвата строения
    // здесь определяется сменит ли здание владельца и устанавливается тип производимых юнитов
    //
    /**********************************************************************************/
    public virtual void Capture(CaptureData cdata)
    {
        if (m_ownerId != cdata.OwnweID)
        {
            m_currentVirusStamina -= cdata.CapturePower;
            if (m_currentVirusStamina <= 0)
            {
                m_ownerId = cdata.OwnweID;
                Debug.Log("Building was captured by pl# " + m_ownerId.ToString());

                m_animator.SetInteger("ownerID", m_ownerId);
                m_animator.SetTrigger("isCuptured");

                if(m_IIFlAnimator != null)
                {
                    m_IIFlAnimator.SetInteger("ownerID", m_ownerId);
                    m_IIFlAnimator.SetTrigger("isCuptured");
                }

                // выбираем юнита для производства
                if (m_ownerId == (int)PLAYER.PL1)
                {
                    m_unitProdactionType = UnitToProduce_Pl1;
                }
                else if (m_ownerId == (int)PLAYER.PL2)
                {
                    m_unitProdactionType = UnitToProduce_Pl2;
                }
                else
                {
                    m_unitProdactionType = UnitToProduce_Neutral;
                }

                // при захвате обновляем счётчик производства, чтобы не воровать чужих юнитов
                m_currentProductionTime = RateOfProduction;

                // выставляем новую сопротивляемость
                if(m_ownerId == (int)PLAYER.NEUTRAL)
                {
                    m_currentVirusStamina = NeutralVirusStamina;
                }
                else
                {
                    m_currentVirusStamina = MaxVirusStamina;
                }

                // увеличиваем счётчик очков за захват
                GameManager.GetInstance().IncreasePlayerScopeBuildingCuptured((PLAYER)m_ownerId, CuptureScope);
                m_virusMarker.SetOwner((PLAYER)m_ownerId);

                m_createdUnitIDs.Clear();

                // помечаем здание как разрушенное - больше оно не будет производить нейтральных юнитов
                m_isDestroyed = true;
            }

            m_virusMarker.SetStamina(m_currentVirusStamina);
        }
    }

    /**********************************************************************************/
    // возвращаем локальную позицию объекта
    //
    /**********************************************************************************/
    public Point GetLocalPosition()
    {
        Point position = new Point(Mathf.FloorToInt(gameObject.transform.localPosition.x / Base.SIZE_OF_CELL), Mathf.FloorToInt(gameObject.transform.localPosition.y / Base.SIZE_OF_CELL));
        return position;
    }

    /**********************************************************************************/
    // возвращаем глобальную позицию объекта
    //
    /**********************************************************************************/
    public Point GetGlobalPosition()
    {
        Point position = new Point(Mathf.FloorToInt(gameObject.transform.position.x / Base.SIZE_OF_CELL), Mathf.FloorToInt(gameObject.transform.position.y / Base.SIZE_OF_CELL));
        return position;
    }

    /**********************************************************************************/
    // возвращаем позицию девого нижнего угла
    //
    /**********************************************************************************/
    public Point GetPositionOfLeftDownСorner()
    {
        Point position = GetGlobalPosition();
        position.x -= XBuildingSize / 2;
        position.y -= YBuildingSize / 2;

        return position;
    }

    /**********************************************************************************/
    // рассчитываем позицию в которой будет появляться продукция строения
    //
    /**********************************************************************************/
    public void CalculateProductionPoint()
    {
        Vector2 centerPosition = m_rb2d.position;
        Vector2 leftDownAnglePosition = new Vector2(centerPosition.x - ((float)XBuildingSize / 2) * Base.SIZE_OF_CELL,
                                                    centerPosition.y - ((float)YBuildingSize / 2) * Base.SIZE_OF_CELL);

        // пассчитываем точку производства
        m_productionPosition = leftDownAnglePosition + RoadPoint.GetUnityPoint();
        m_productionPosition += new Vector2(Base.HALF_OF_CELL, Base.HALF_OF_CELL);
    }

    /**********************************************************************************/
    // процессинг строения, производство юнитов
    //
    /**********************************************************************************/
    private void FixedUpdate()
    {
        // стопим все процессы, если игра поставлена на паузу
        if (GameManager.GamePaused)
        {
            return;
        }

        if (IsInitialized() && m_unitProdactionType != Base.GO_TYPE.NONE_TYPE)
        {

            // разрушенные и незахваченные здания ничего не производят
            if(m_isDestroyed && m_ownerId == (int)PLAYER.NEUTRAL)
            {
                return;
            }

            // блокируем производство если у здания уже есть достигли предела по количеству существ
            if(m_createdUnitIDs.Count >= MaxNumberOfProd)
            {
                return;
            }

            // рассчитываем текущий прогресс производства
            m_currentProductionTime -= Time.deltaTime;

            if (m_currentProductionTime <= 0)
            {
                GameObject production = ObjectFactory.GetInstance().CreateGObject(m_productionPosition, Base.DIREC.DOWN, m_unitProdactionType, false);

                // бонусы - недвижимые объекты, потому GMovingObject не имеют
                if (production.tag != "Bonus")
                {
                    CIGameObject gmo = production.GetComponent<CIGameObject>();
                    gmo.Owner = m_ownerId;
                    m_createdUnitIDs.Add(gmo.ID);
                    gmo.ProductionBase = this;
                }
                else
                {
                    BonusCtr bctr = production.GetComponent<BonusCtr>();
                    bctr.ProductionBase = this;
                }

                if (m_ownerId == (int)PLAYER.NEUTRAL)
                {
                    // устанавливаем ранодное стартовое значение для того, чтобы нейтральные войска выходили не в один момент
                    m_currentProductionTime += RateOfProductionNeutral;
                }
                else
                {
                    m_currentProductionTime += RateOfProduction;
                }


                // увеличиваем счётчик очков за производство
                // TODO: производство пока приносит строго 1 очко, подумать над этим на досуге
                GameManager.GetInstance().IncreasePlayerScopeUnitProduction((PLAYER)m_ownerId, 1);
            }
        }
    }

    /**********************************************************************************/
    // регистрируем "гибель" произведённого юнита
    // после этого снова возобновиться производство
    //
    /**********************************************************************************/
    public void ReleaseProduction(int id)
    {
        if(m_createdUnitIDs.Contains(id))
        {
            m_createdUnitIDs.Remove(id);
        }
    }
}
