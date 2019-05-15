using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************************************************/
// DialogController контроллер диалога
// отображает сценарыне тексты
//
/**********************************************************************************************/
public class DialogController : MonoBehaviour
{
    [System.Serializable]
    public class IcoContainer
    {
        public Base.GO_TYPE Type;
        public Sprite Ico;
    }

    public Text DialogText = null;
    public Image DialogIco = null;
    public Text NameText = null;

    public Sprite Player1Ico;
    public Sprite Player2Ico;
    public IcoContainer[] IconCollection;
    protected Dictionary<Base.GO_TYPE, Sprite> m_icoDictionary = new Dictionary<Base.GO_TYPE, Sprite>();

    protected LocalizedText m_dialogTextCtr = null;
    protected LocalizedText m_nameTextCtr = null;

    private void Awake()
    {
        m_dialogTextCtr = DialogText.GetComponent<LocalizedText>();
        if (m_dialogTextCtr == null)
        {
            Debug.LogError("m_dialogTextCtr is null!");
            return;
        }

        m_nameTextCtr = NameText.GetComponent<LocalizedText>();
        if (m_nameTextCtr == null)
        {
            Debug.LogError("m_nameTextCtr is null!");
            return;
        }

        if (IconCollection == null)
        {
            Debug.LogError("IconCollection is null!");
            return;
        }

        // сохраняем информацию о иконках в Dictionary для организации быстрого доступа
        for (int i = 0; i < IconCollection.Length; i++)
        {
            IcoContainer container = IconCollection[i];
            m_icoDictionary[container.Type] = container.Ico;
        }

        // прячем диалог по дефолту
        HideDialog();
    }

    /**********************************************************************************************/
    // устанавливаем новые значения для диалога
    //
    /**********************************************************************************************/
    public void SetDialog(Base.GO_TYPE icoForType, string textKey, string NameKey)
    {
        m_dialogTextCtr.SetNewKey(textKey);

        // определяемся с иконкой
        if(icoForType == Base.GO_TYPE.PLAYER)
        {
            if(GameManager.GetInstance().GetPlayer().playerId == PLAYER.PL1)
            {
                Sprite spriteToSet = Player1Ico;
                DialogIco.sprite = spriteToSet;
                NameKey += "_1";
            }
            else
            {
                Sprite spriteToSet = Player2Ico;
                DialogIco.sprite = spriteToSet;
                NameKey += "_2";
            }
        }
        else
        {
            Sprite spriteToSet = m_icoDictionary[icoForType];
            DialogIco.sprite = spriteToSet;
        }

        m_nameTextCtr.SetNewKey(NameKey);

        // активируем диалог, если он был спрятан
        gameObject.SetActive(true);
    }

    /**********************************************************************************************/
    // прячем диалог
    //
    /**********************************************************************************************/
    public void HideDialog()
    {
        // прячем диалог
        gameObject.SetActive(false);
    }
}
