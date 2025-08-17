using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LobbyController : MonoBehaviour
{
    [SerializeField] public GameObject Panel_login;
    [SerializeField] public Button Btn_login;
    [SerializeField] public TMP_InputField Input_nickName;
    [SerializeField] private GameObject panel_Title;
    [SerializeField] public static LobbyController LobbyUI;

    [SerializeField] private GameObject enterRoom;
    [SerializeField] private GameObject album;

    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private RectTransform inputFieldRectPw;
    [SerializeField] private RectTransform inputFieldRectNickname;
    private Vector2 originalSize;
    private Vector2 originalPositionPw;
    private Vector2 originalPositionNickname;
    private Vector2 expandedSize = new Vector2(1200, 100); // 확장된 크기
    private Vector2 expandedPos = new Vector2(0, 637); // 확장됐을 때 위치

    private void Awake()
    {
        if(LobbyUI == null)
        {
            LobbyUI = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        originalSize = inputFieldRectPw.sizeDelta;
        originalPositionPw = inputFieldRectPw.gameObject.transform.localPosition;
        originalPositionNickname = inputFieldRectNickname.gameObject.transform.localPosition;
    }

    public void ShowRoomPanel()
    {
        Btn_login.interactable = true;
        Panel_login.gameObject.SetActive(false);
        panel_Title.SetActive(true);
    }

    public void ShowAlbum()
    {
        buttons[0].GetComponent<Image>().sprite = sprites[2];
        buttons[1].GetComponent<Image>().sprite = sprites[1];

        enterRoom.SetActive(false);
        album.SetActive(true);
    }

    public void ShowEnterRoom()
    {
        buttons[0].GetComponent<Image>().sprite = sprites[3];
        buttons[1].GetComponent<Image>().sprite = sprites[0];

        album.SetActive(false);
        enterRoom.SetActive(true);
    }

    public void OnSelectNickname(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRectNickname.sizeDelta = expandedSize;
            inputFieldRectNickname.gameObject.transform.localPosition = expandedPos;
        }
    }

    // InputField가 선택 해제되었을 때 호출
    public void OnDeselectNickname(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRectNickname.sizeDelta = originalSize;
            inputFieldRectNickname.gameObject.transform.localPosition = originalPositionNickname;
        }
    }

    public void OnSelectPw(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRectPw.sizeDelta = expandedSize;
            inputFieldRectPw.gameObject.transform.localPosition = expandedPos;
        }
    }

    // InputField가 선택 해제되었을 때 호출
    public void OnDeselectPw(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRectPw.sizeDelta = originalSize;
            inputFieldRectPw.gameObject.transform.localPosition = originalPositionPw;
        }
    }
}
