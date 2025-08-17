using Photon.Pun;
using Photon.Voice.PUN;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotSeatStage : MonoBehaviourPun
{
    private HotSeatController hotSeatController;
    private HotSeatSelfIntroduce selfInt;
    private BringPlayer bringPlayer;
    private HotSeatVoiceRecord voiceRecords;

    [SerializeField] private List<Image> images = new List<Image>();
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    public Dictionary<int, string> selfIntroduces = new Dictionary<int, string>();

    [SerializeField] private TMP_Text[] characterNames;
    public GameObject speechGuide;
    [SerializeField] private Vector2 playerPos;
    [SerializeField] private Transform stagePos;
    [SerializeField] private Image[] stageScriptImgs;
    [SerializeField] private TMP_Text[] stageScriptTxts;

    [Header("UI 조절")]
    [SerializeField] private GameObject[] characterNametagImgs;
    [SerializeField] private GameObject[] playerNametagImgs;
    [SerializeField] private TMP_Text[] characterNameTags;
    [SerializeField] private TMP_Text[] playerNameTags;
    [SerializeField] private RawImage[] rawImages;
    [SerializeField] private RenderTexture[] renderTextures;
    [SerializeField] private Material[] materials;
    [SerializeField] private GameObject spotlight;
    [SerializeField] private GameObject[] timerImgs;
    [SerializeField] private TMP_Text[] timerTxts;
    [SerializeField] private Image[] speakingUIOns;
    [SerializeField] private Image[] speakingUIOffs;

    private bool isEnd = false;
    private Vector3 originalPos;
    private RectTransform rtStage;
    private float currTime = 0;
    private int interviewTime = 15;
    private int answerTime = 15;

    void Start()
    {
        hotSeatController = GetComponent<HotSeatController>();
        selfInt = GetComponent<HotSeatSelfIntroduce>();
        voiceRecords = GetComponent<HotSeatVoiceRecord>();
        bringPlayer = GameObject.Find("BookCanvas").GetComponent<BringPlayer>();

        // stage 부분 사전 준비/저장
        playerPos = players[0].transform.position;
    }

    // Stage 단계의 상단 이름표 부분 구현 : 랜덤으로 순서 섞어서 보여주기
    public void MatchNameTags()
    {
        // playerNums 순서대로 이름표 안의 텍스트 배치
        for (int i = 0; i < characterNameTags.Length; i++)
        {
            string name = bringPlayer.allPlayers[selfInt.playerNums[i]].Owner.NickName;
            int avatarIndex = bringPlayer.allPlayers[selfInt.playerNums[i]].GetComponent<PlayerAvatarSetting>().avatarIndex;
            characterNameTags[i].text = characterNames[avatarIndex].text; // 캐릭터 이름
            playerNameTags[i].text = name; // 플레이어 이름
        }
    }

    // Stage 단계의 플레이어 이미지를 상단 이름표의 순서에 맞춰 배치
    public void MatchPlayerPos()
    {
        // playerNums 순서대로 캐릭터 MP4 순서대로 배치
        for (int i = 0; i < rawImages.Length; i++)
        {
            materials[selfInt.playerNums[i]].mainTexture = renderTextures[selfInt.playerNums[i]];
            rawImages[i].material = materials[selfInt.playerNums[i]];
        }
    }

    // 각 플레이어가 쓴 자기소개를 순서에 따라 넣어놓기
    public void MatchSelfIntroduce()
    {
        for (int i = 0; i < selfInt.playerNums.Count; i++)
        {
            int playerNum = selfInt.playerNums[i];

            stageScriptTxts[i].text = selfIntroduces[playerNum + 1];
        }
    }

    public void RPC_StartSpeech(int index)
    {
        photonView.RPC(nameof(StartSpeech), RpcTarget.All, index);
    }

    [PunRPC]
    // 순서대로 자기소개 - 질문
    public void StartSpeech(int index)
    {
        if (index - 1 >= 0 && index - 1 < images.Count)
        {
            images[index - 1].sprite = hotSeatController.sprites[2]; // 전 플레이어는 이름표 색 원래 색으로
            characterNametagImgs[index - 1].GetComponent<Image>().sprite = hotSeatController.sprites[0];
            players[index - 1].GetComponent<RectTransform>().anchoredPosition = originalPos; // 이미지 위치도 원위치
            stageScriptImgs[index - 1].gameObject.SetActive(false); // 전 플레이어의 자기소개 끄기
            spotlight.SetActive(false); // 스포트라이트 끔
        }

        if (index < players.Count)
        {
            // 이름 UI 색깔 바꾸고
            images[index].sprite = hotSeatController.sprites[3];
            playerNametagImgs[index].GetComponent<Image>().sprite = hotSeatController.sprites[1]; ////////////

            // 플레이어 무대로 가게 한다
            originalPos = players[index].GetComponent<RectTransform>().anchoredPosition;
            playerPos = players[index].GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(ChangePos(playerPos, index));
        }

        if (index == 4 && PhotonNetwork.IsMasterClient && !isEnd)
        {
            RPC_startLastCrt();
            isEnd = true;
        }
    }

    public IEnumerator ChangePos(Vector2 playerPos, int i)
    {
        rtStage = stagePos.GetComponent<RectTransform>();
        RectTransform rtPlayer = players[i].GetComponent<RectTransform>();
        while (true)
        {
            if (Vector3.Distance(playerPos, stagePos.position) > 160f)
            {
                // 플레이어가 무대로 가게 한다
                rtPlayer.anchoredPosition = Vector2.Lerp(playerPos, rtStage.anchoredPosition, 0.05f);
                playerPos = rtPlayer.anchoredPosition;
                yield return null;
            }
            else
            {
                rtPlayer.anchoredPosition = rtStage.anchoredPosition;
                break;
            }
        }

        if (PhotonNetwork.IsMasterClient) RPC_OnStage(i);
    }

    void RPC_OnStage(int i)
    {
        photonView.RPC(nameof(OnStage), RpcTarget.All, i);
    }

    [PunRPC]
    void OnStage(int i)
    {
        playerPos = rtStage.anchoredPosition; // 도착점에 위치 맞춰준다

        spotlight.SetActive(true); // 스포트라이트 켜준다

        stageScriptImgs[i].gameObject.SetActive(true);

        // "친구들에게 말로 자기소개를 해 봅시다" UI
        speechGuide.SetActive(true);
        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_INFO);
        StartCoroutine(hotSeatController.Deactivate(speechGuide));

        // 처음 순서면 30초, 아니면 15초 타이머 시작
        if (i == 0 && PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.IsMasterClient) RPC_StartTimer(i, 30);

        }
        else if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.IsMasterClient) RPC_StartTimer(i, 15);
        }

        // 소리 나머지 뮤트
        voiceRecords.MuteOtherPlayers(selfInt.playerNums[i] + 1);

        // 자기소개 켜 줌
        stageScriptImgs[i].gameObject.SetActive(true);
    }

    void RPC_StartTimer(int i, int time)
    {
        photonView.RPC(nameof(StartTimerCoroutine), RpcTarget.All, i, time);
    }

    [PunRPC]
    void StartTimerCoroutine(int i, int time)
    {
        StartCoroutine(StartTimer(i, time));
    }

    // 타이머 시작
    IEnumerator StartTimer(int i, int time)
    {
        yield return new WaitForSeconds(3f); // UI 사라질 때까지 기다리기

        Image speakingUIOn = speakingUIOns[i];
        Image speakingUIOff = speakingUIOffs[i];
        speakingUIOn.gameObject.SetActive(true);
        speakingUIOff.gameObject.SetActive(true);
        float timescale = 1 / (time / Time.deltaTime);

        while (currTime < time)
        {
            // DeltaTime을 사용하여 경과 시간 계산
            currTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기

            // 경과 시간을 분:초 형식으로 변환
            TimeSpan timeSpan = TimeSpan.FromSeconds(currTime);

            speakingUIOn.fillAmount = 1 - (currTime / time);
        }

        timerImgs[i].gameObject.SetActive(false);
        speakingUIOn.fillAmount = 1;
        speakingUIOff.fillAmount = 1;
        speakingUIOn.gameObject.SetActive(false);
        speakingUIOff.gameObject.SetActive(false);

        // 인터뷰로 넘어감
        if (PhotonNetwork.IsMasterClient) RPC_InterviewCrt(i);

        // "자기소개를 듣고 궁금한 것들을 질문해봅시다" -> 2초 뒤 자동으로 deactivate
        hotSeatController.ActivateStageGuide(3);

        currTime = 0;
    }

    void RPC_InterviewCrt(int index)
    {
        photonView.RPC(nameof(StartInterviewCrt), RpcTarget.All, index);
    }

    [PunRPC]
    void StartInterviewCrt(int index)
    {
        StartCoroutine(InterviewCoroutine(index));
    }

    IEnumerator InterviewCoroutine(int index)
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i <= players.Count; i++)
        {
            if (i < players.Count && i != index)
            {
                speakingUIOffs[i].gameObject.SetActive(true);
                speakingUIOns[i].gameObject.SetActive(true);

                // 질문하는 사람 보이스 켜주고 녹음 시작
                voiceRecords.MuteOtherPlayers(selfInt.playerNums[i] + 1);
                voiceRecords.RecordVoice(selfInt.playerNums[i] + 2);

                // 원래는 30초인데 테스트용 10초
                float currTime = 0;
                while (currTime < interviewTime)
                {

                    speakingUIOns[i].fillAmount = 1 - (currTime / interviewTime);
                    currTime += Time.deltaTime;
                    yield return null;
                }
                currTime = 0;

                // 녹음 종료
                voiceRecords.StopRecordVoice(selfInt.playerNums[i] + 2, index);

                speakingUIOns[i].fillAmount = 1;
                speakingUIOns[i].gameObject.SetActive(false);
                speakingUIOffs[i].gameObject.SetActive(false);

                speakingUIOffs[index].gameObject.SetActive(true);
                speakingUIOns[index].gameObject.SetActive(true);

                // 자기소개 한 사람(답변할 사람) 보이스 켜주고 녹음 시작
                voiceRecords.MuteOtherPlayers(selfInt.playerNums[index] + 1);
                voiceRecords.RecordVoice(selfInt.playerNums[index] + 2);

                // 원래는 60초인데 일단 10초
                while (currTime < answerTime)
                {
                    speakingUIOns[index].fillAmount = 1 - (currTime / answerTime);
                    currTime += Time.deltaTime;
                    yield return null;
                }
                currTime = 0;
                // 녹음 종료
                voiceRecords.StopRecordVoice(selfInt.playerNums[index] + 2, index);

                speakingUIOns[index].fillAmount = 1;
                speakingUIOns[index].gameObject.SetActive(false);
                speakingUIOffs[index].gameObject.SetActive(false);
            }
        }
    }

    public void RPC_startLastCrt()
    {
        photonView.RPC(nameof(StartLastCrt), RpcTarget.All);
    }

    [PunRPC]
    void StartLastCrt()
    {
        StartCoroutine(LastCoroutine());
    }

    // 최종 단계
    public IEnumerator LastCoroutine()
    {
        yield return null;
        VoiceManager.Instance.recorder.TransmitEnabled = true;
        if (PhotonNetwork.IsMasterClient) hotSeatController.RPC_ActivateStageGuide(4);
    }
}
