using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviourPun
{
    #region parameters

    private PhotonView pv;
    [SerializeField] private GameObject ChooseCharacterUI;
    [SerializeField] private BookController bookController;
    [SerializeField] private BringPlayer bringPlayer;

    [SerializeField] private GameObject[] img_charNames;

    // 버튼 어떤 게 눌렸나 받아오기
    public int characterNum = 0;
    [SerializeField] private GameObject btn_chooseChar;
    [SerializeField] private GameObject btn_toMap;
    [SerializeField] private GameObject PaintUI;

    public int CurrentPlayerNum;

    private int selectedCount = 0;
    private int paintedCount = 0;

    #endregion

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    public void Select(int num)
    {
        // 방에 들어온 사람이 5명 미만이면 / 선생님이면 / 이미 선택된 아바타 번호라면 아무 일도 일어나지 않는다
        if (PhotonNetwork.CurrentRoom.PlayerCount < 5 || CurrentPlayerNum == -1 || IsCharacterSelected(num)) return;

        // 자신이 이미 선택한 번호를 다시 클릭했을 경우 선택 해제
        if (characterNum == num)
        {
            RPC_DeactivateNameUI(characterNum);
            RPC_DecreaseCnt();
            characterNum = 0; // 초기화 (아바타 선택 해제)
            bringPlayer.allPlayers[CurrentPlayerNum].GetComponent<PlayerAvatarSetting>().RPC_SelectChar(characterNum);
            SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);

            RPC_DisableChooseButton();

            return;
        }

        if (characterNum == 0) RPC_IncreaseCnt();

        RPC_DeactivateNameUI(characterNum);
        characterNum = num;
        RPC_ActivateNameUI(characterNum, CurrentPlayerNum);

        // 아바타(등장인물) 인덱스를 설정한다. 
        bringPlayer.allPlayers[CurrentPlayerNum].GetComponent<PlayerAvatarSetting>().RPC_SelectChar(characterNum);

        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
    }

    // 다른 플레이어가 이미 선택한 캐릭터 번호인지 확인
    private bool IsCharacterSelected(int num)
    {
        foreach (var player in bringPlayer.allPlayers)
        {
            int actorNumber = player.Key;
            // 현재 플레이어는 제외
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber - 2) continue;

            var avatarSetting = player.Value.GetComponent<PlayerAvatarSetting>();
            if (avatarSetting != null && (avatarSetting.avatarIndex + 1) == num)
            {
                return true; // 다른 플레이어가 이미 선택한 번호임
            }
        }
        return false;
    }

    void RPC_DisableChooseButton()
    {
        photonView.RPC(nameof(DisableChooseButton), RpcTarget.All);
    }

    [PunRPC]
    void DisableChooseButton()
    {
        // 만약 캐릭터 선택하기가 활성화되어 있으면 다시 비활성화
        btn_chooseChar.GetComponent<Image>().sprite = bookController.buttonSprites[7];
        btn_chooseChar.GetComponent<Button>().interactable = false;
    }

    public void RPC_DecreaseCnt()
    {
        photonView.RPC(nameof(DecreaseCnt), RpcTarget.All);
    }

    [PunRPC]
    void DecreaseCnt()
    {
        selectedCount--;
    }

    public void RPC_IncreaseCnt()
    {
        photonView.RPC(nameof(IncreaseCnt), RpcTarget.All);
    }

    [PunRPC]
    void IncreaseCnt()
    {
        selectedCount++;

        if (selectedCount >= 4)
        {
            btn_chooseChar.GetComponent<Image>().sprite = bookController.buttonSprites[6];
            btn_chooseChar.GetComponent<Button>().interactable = true;
        }
    }

    public void RPC_IncreaseClickSelectCount()
    {
        photonView.RPC(nameof(IncreaseClickSelectCount), RpcTarget.All);
    }

    [PunRPC]
    void IncreaseClickSelectCount()
    {
        if (PhotonNetwork.IsMasterClient) return;

        paintedCount++;

        if (paintedCount >= 4)
        {
            btn_toMap.GetComponent<Image>().sprite = bookController.buttonSprites[5];
            btn_toMap.GetComponent<Button>().interactable = true;
        }
    }

    public void RPC_ActivateNameUI(int characterIndex, int playerIndex)
    {
        pv.RPC(nameof(ActivateNameUI), RpcTarget.All, characterIndex, playerIndex);
    }

    [PunRPC]
    void ActivateNameUI(int characterIndex, int playerIndex)
    {
        string playerName = bringPlayer.playerNames.TryGetValue(playerIndex, out var name)
        ? name : "Not Found";

        var target = img_charNames[characterIndex - 1];
        target.SetActive(true);
        target.GetComponentInChildren<TMP_Text>().text = playerName;
    }

    public void RPC_DeactivateNameUI(int characterIndex)
    {
        pv.RPC(nameof(DeactivateNameUI), RpcTarget.All, characterIndex);
    }

    [PunRPC]
    private void DeactivateNameUI(int characterIndex)
    {
        img_charNames[characterIndex - 1].SetActive(false);
    }

    public void FinishSelect()
    {
        ChooseCharacterUI.SetActive(false);

        if (!PhotonNetwork.IsMasterClient)
        {
            PaintUI.SetActive(true);
        }
        else
        {
            ChooseCharacterUI.SetActive(true);
            // 방장(선생님)은 직접 호출 
            bookController.PaintToComplete();
        }

        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
    }
}
