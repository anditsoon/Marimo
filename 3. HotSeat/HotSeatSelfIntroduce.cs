using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotSeatSelfIntroduce : MonoBehaviourPun
{
    #region parameters 
    private BringPlayer bringPlayer;
    private HotSeatController hotSeatController;
    private SelectCharacter selectCharacter;
    private HotSeatStage stage;

    [SerializeField] private GameObject panel_waiting;
    [SerializeField] private GameObject stageGameObject;

    public TMP_Text[] CharacterNames;
    [SerializeField] private Button[] characterImages;
    [SerializeField] private Button btn_Submit;

    public List<int> playerNums = new List<int>();
    private int selfInt_count = 0;

    #region 모바일로 수업 시 창 크기 변화

    [SerializeField] private GameObject panel_selfIntroduce;
    public TMP_InputField selfIntroduceInput;
    [SerializeField] private TMP_Text Txt_TitleText;
    [SerializeField] private Image myAvatarImage;
    [SerializeField] private RectTransform inputFieldRect;

    private Vector2 expandedSize = new Vector2(1200, 400); // 확장된 크기
    private Vector2 expandedPos = new Vector2(-450, 180); // 확장됐을 때 위치
    private Vector2 expandedPosWordCnt = new Vector2(-480, -172);
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Vector2 originalSizeWordCnt;
    private Vector2 originalPositionWordCnt;

    [SerializeField] private TMP_Text wordCountText;
    [SerializeField] private TMP_Text txt_playerName;
    private bool over50 = false;

    #endregion

    private const int MIN_WORD_COUNT = 50;
    private const int TOTAL_PLAYERS = 5;

    #endregion

    void Start()
    {
        hotSeatController = GetComponent<HotSeatController>();
        stage = GetComponent<HotSeatStage>();
        bringPlayer = GameObject.Find("BookCanvas").GetComponent<BringPlayer>();
        selectCharacter = GameObject.Find("BookCanvas").GetComponent<SelectCharacter>();

        if (PhotonNetwork.IsMasterClient)
        {
            panel_waiting.SetActive(true);
            RPC_AllReady();
        }

        // 자기소개 인풋필드 터치 키보드 올라오면 위치/크기 변경할 준비
        originalSize = inputFieldRect.sizeDelta;
        originalPosition = inputFieldRect.gameObject.transform.localPosition;
        originalSizeWordCnt = wordCountText.GetComponent<RectTransform>().sizeDelta;
        originalPositionWordCnt = wordCountText.gameObject.transform.localPosition;

        // 자기소개 화면 본인 닉네임과 캐릭터, 이미지 표시
        txt_playerName.text = bringPlayer.MyAvatar.pv.Owner.NickName;

        if (selectCharacter.characterNum - 1 >= 0)
        {
            Txt_TitleText.text = CharacterNames[selectCharacter.characterNum - 1].text;
            myAvatarImage.sprite = characterImages[selectCharacter.characterNum - 1].GetComponent<Image>().sprite;
        }
        else
        {
            Txt_TitleText.text = "선생님";
        }

        btn_Submit.interactable = false;
    }

    void Update()
    {
        UpdateWordCount();
        CheckSubmitAvailability();
    }

    // 실시간으로 글자 수 업데이트
    void UpdateWordCount()
    {
        if (selfIntroduceInput != null && selfIntroduceInput.isActiveAndEnabled)
        {
            wordCountText.text = $"{selfIntroduceInput.text.Length}/{MIN_WORD_COUNT}";
        }
    }

    private void CheckSubmitAvailability()
    {
        if (!over50 && selfIntroduceInput.text.Length >= MIN_WORD_COUNT)
        {
            btn_Submit.interactable = true;
            btn_Submit.image.sprite = hotSeatController.sprites[5];
            over50 = true;
        }
    }

    // InputField가 선택되었을 때 호출
    public void OnSelect(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRect.sizeDelta = expandedSize;
            inputFieldRect.gameObject.transform.localPosition = expandedPos;
            wordCountText.gameObject.transform.localPosition = expandedPosWordCnt;
        }
    }

    // InputField가 선택 해제되었을 때 호출
    public void OnDeselect(BaseEventData eventData)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            inputFieldRect.sizeDelta = originalSize;
            inputFieldRect.gameObject.transform.localPosition = originalPosition;
            wordCountText.GetComponent<RectTransform>().sizeDelta = originalSizeWordCnt;
            wordCountText.gameObject.transform.localPosition = originalPositionWordCnt;
        }
    }

    // 제출하기 버튼
    public void Submit()
    {
        hotSeatController.selfIntroduce.SetActive(false);
        panel_waiting.SetActive(true);

        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);

        // 플레이어 순서 랜덤으로 섞음
        ShufflePlayers();

        // 작성한 순서대로 추가됨
        RPC_AddSelfIntroduce(PhotonNetwork.LocalPlayer.ActorNumber - 1, selfIntroduceInput.text);

        RPC_AllReady();
    }

    // 셔플 돌린 후 싱크 맞춤
    public void ShufflePlayers()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            playerNums = ShuffleList(bringPlayer.allPlayers);
            RPC_SyncPlayerNums(playerNums.ToArray());
        }
    }

    private List<int> ShuffleList(Dictionary<int, PhotonView> playerDict)
    {
        foreach (int key in playerDict.Keys)
        {
            playerNums.Add(key);
        }

        int n = playerNums.Count;

        for (int i = 0; i < n; i++)
        {
            int j = UnityEngine.Random.Range(0, n);

            int tmp = playerNums[i];
            playerNums[i] = playerNums[j];
            playerNums[j] = tmp;
        }

        return playerNums;
    }

    private void RPC_SyncPlayerNums(int[] syncedPlayerNums)
    {
        photonView.RPC(nameof(SyncPlayerNums), RpcTarget.All, syncedPlayerNums);
    }

    [PunRPC]
    private void SyncPlayerNums(int[] syncedPlayerNums)
    {
        playerNums = syncedPlayerNums.ToList();
    }

    private void RPC_AddSelfIntroduce(int actorNumber, string selfIntroduce)
    {
        photonView.RPC(nameof(AddSelfIntroduce), RpcTarget.All, actorNumber, selfIntroduce);
    }

    // 학생이 쓴 자기소개를 리스트에 넣어줌
    [PunRPC]
    private void AddSelfIntroduce(int actorNumber, string selfIntroduce)
    {
        if (actorNumber > 0)
            stage.selfIntroduces.Add(actorNumber, selfIntroduce);
    }

    private void RPC_AllReady()
    {
        photonView.RPC(nameof(startAllReadyCrt), RpcTarget.All);
    }

    [PunRPC]
    public void startAllReadyCrt()
    {
        StartCoroutine(AllReady());
    }

    public IEnumerator AllReady()
    {
        // 서브밋 누른 플레이어 수 늘림
        selfInt_count++;

        // 5명이 다 차면
        if (selfInt_count >= 5)
        {
            // 상단의 이름표, 중간의 캐릭터 애니메이션, 하단의 자기소개 순서 모두 랜덤으로 돌린 순서랑 맞춰 줌
            stage.MatchNameTags();
            stage.MatchPlayerPos();
            stage.MatchSelfIntroduce();

            yield return new WaitForSeconds(1f);

            // 순서 다 정렬하고 셋액티브
            panel_waiting.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                panel_selfIntroduce.SetActive(false);
            }
            stageGameObject.SetActive(true);

            // 먼저 자기의 자기소개 순서를 알아야 함
            int selfIntCount = 0;
            for (int i = 0; i < playerNums.Count; i++)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber - 2 == playerNums[i])
                {
                    selfIntCount = i;
                }
            }

            // 마스터가 아니면 자기 순서를 서버에 보내서 동기화
            if (!PhotonNetwork.IsMasterClient) HttpHotSeatServer.GetInstance().StartSendIntCoroutine(selfIntCount);

            // 마스터 클라이언트면 자기소개 진행 순서 시작
            if (PhotonNetwork.IsMasterClient) stage.RPC_StartSpeech(0);
        }
    }
}
