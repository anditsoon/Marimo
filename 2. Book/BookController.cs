using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookController : MonoBehaviourPun
{
    public GameObject[] characterButtons;
    public Sprite[] buttonSprites;
    [SerializeField] private GameObject[] isReadyImgs;

    [SerializeField] private GameObject btn_chooseChar;
    [SerializeField] private GameObject btn_toMap;

    [SerializeField] private SelectCharacter selectCharacter;
    private int mapCnt = 0;

    private void Start()
    {
        SoundManager.instance.StopBgmSound();
    }

    public void PaintToComplete()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            TMP_Text textComponent = characterButtons[i].GetComponentInChildren<TMP_Text>();
            Color color = textComponent.color;
            color.a = 0;
            textComponent.color = color;
        }

        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
        SoundManager.instance.PlayBgmSound(SoundManager.EBgmType.BGM_MAIN);

        btn_chooseChar.SetActive(false);
        btn_toMap.SetActive(true);
    }

    public void ToMap()
    {
        // 이 때 캐릭터 받아와서 내 플레이어에 동기화
        if (!PhotonNetwork.IsMasterClient)
        {
            RPC_IncreaseMapCount(selectCharacter.characterNum);
        }
        btn_toMap.GetComponent<Image>().sprite = buttonSprites[4];
        btn_toMap.GetComponent<Button>().interactable = false;
    }

    public void RPC_IncreaseMapCount(int characterNum)
    {
        photonView.RPC(nameof(IncreaseMapCount), RpcTarget.All, characterNum);
    }

    [PunRPC]
    private void IncreaseMapCount(int characterNum)
    {
        mapCnt++;
        isReadyImgs[characterNum - 1].SetActive(true);

        if (mapCnt >= 4)
        {
            // 맵 소개 UI 실행
            GameManager.instance.afterbook = true;
            SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);

            gameObject.SetActive(false);
        }
    }
}   
