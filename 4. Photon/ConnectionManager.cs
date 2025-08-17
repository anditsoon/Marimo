using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] buttons = new GameObject[3];
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    void Start()
    {
        SoundManager.instance.PlayBgmSound(SoundManager.EBgmType.BGM_LOGIN);
    }

    public void StartLogin()
    {
        
        if (LobbyController.LobbyUI.Input_nickName.text.Length > 0)
        {
            // 접속을 위한 설정
            PhotonNetwork.GameVersion = "1.0.0";
            PhotonNetwork.NickName = LobbyController.LobbyUI.Input_nickName.text;
            PhotonNetwork.AutomaticallySyncScene = false;
            print("접속 설정 완료");

            // 접속을 서버에 요청하기
            PhotonNetwork.ConnectUsingSettings(); // 위 정보를 바탕으로 네임 서버에서 마스터 서버로 연결하는 함수.
                                                  // 네임 서버: 커넥션 관리, NetID 구분하여 클라이언트 구분.
                                                  // 마스터 서버: 유저들 간의 Match making 을 해주는 공간. 룸을 만들고 룸에 조인을 하고 룸의 플레이어끼리 플레이를 하는 식. 방장의 씬을 기준으로 설정하고 나의 씬에 동기화
            print("서버에 요청 중");
        }
        
    }

    public override void OnConnected()
    {
        base.OnConnected();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        // 실패 원인을 출력한다.
        Debug.LogError("서버에서 Disconnected 됨 - " + cause);
        LobbyController.LobbyUI.Btn_login.interactable = true;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        // 서버의 로비로 들어간다.
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }


    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("방 생성 완료!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError($"방 생성 실패: {message} (Code : {returnCode})");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("LoadScene1 이동 호출");

        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        // 룸에 입장이 실패한 이유를 출력한다.
        Debug.LogError("입장 실패 : " + message);

    }

    // 룸에 다른 플레이어가 입장했을 때의 콜백 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        string playerMsg = $"{newPlayer.NickName}님이 입장하셨습니다.";
        Debug.Log(playerMsg);

        if(PhotonNetwork.CurrentRoom.PlayerCount == 5)
        {
            // 포톤 RPC로 전체 호출
            StartCoroutine(HttpRoomSetUp.GetInstance().GetUserIdList());
        }
    }

    // 룸에 있던 다른 플레이어가 퇴장했을 때의 콜백 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        string playerMsg = $"{otherPlayer.NickName}님이 퇴장하셨습니다.";
        Debug.Log(playerMsg);
    }

    // 현재 로비에서 룸의 변경사항을 알려주는 콜백 함수
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (HttpLogIn.GetInstance().IsTeacher) return;

        base.OnRoomListUpdate(roomList);

        buttons = GameObject.FindGameObjectsWithTag("StudentButtons");

        if (buttons.Length > 0)
        {
            // 배열을 정렬하고 리스트에 추가, 이름순으로 정렬
            System.Array.Sort(buttons, (a, b) => a.name.CompareTo(b.name));
        }
        else
        {
            Debug.LogError("태그가 'Button'인 오브젝트를 찾을 수 없습니다.");
        }

        foreach (RoomInfo room in roomList)
        {
            // 만일, 갱신된 룸 정보가 제거 리스트에 있다면
            if (room.RemovedFromList)
            {
                // cachedRoomList에서 해당 룸을 제거한다.
                cachedRoomList.Remove(room);
            }
            // 그렇지 않다면
            else
            {
                // 만일, 이미 cachedRoomList에 있는 방이라면
                if (cachedRoomList.Contains(room))
                {
                    // 기존 룸 정보를 제거한다.
                    cachedRoomList.Remove(room);
                }
                // 새 룸을 cachedRoomList에 추가한다.
                cachedRoomList.Add(room);
            }
        }

        // 버튼 UI 업데이트
        for (int i = 0; i < buttons.Length; i++)
        {
            // 버튼 클릭 리스너 초기화
            buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();

            // 룸이 있는 경우 버튼 설정
            if (i < cachedRoomList.Count)
            {
                buttons[i].SetActive(true);
                RoomInfo room = cachedRoomList[i];
                buttons[i].GetComponentInChildren<TMP_Text>().text = room.Name;

                // 클릭 리스너에 방 입장 기능 추가
                buttons[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    PhotonNetwork.JoinRoom(room.Name);
                });
            }
        }
    }

    public void FinishLesson()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        ReturnLobby();
    }

    public void ReturnLobby()
    {
        if (HttpLogIn.GetInstance().IsLoggedIn)
        {
            SignUpPageUIController y_SignUp = FindAnyObjectByType<SignUpPageUIController>();

            // 씬 초기화 후 UI 상태를 다시 세팅
            if (HttpLogIn.GetInstance().IsTeacher)
            {
                y_SignUp.CreatorUI.SetActive(true);
                y_SignUp.LogInUI.SetActive(false);
                GameObject.Find("Canvas_CreatorTool").GetComponent<P_CreatorToolController>().titleNickname.text = HttpLogIn.GetInstance().UserNickName;
            }
            else
            {
                y_SignUp.TitleUI.SetActive(true);
                y_SignUp.LogInUI.SetActive(false);
                y_SignUp.TitleNickname.text = HttpLogIn.GetInstance().UserNickName;
            }
        }
    }
}
