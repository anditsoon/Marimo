using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringPlayer : MonoBehaviourPun
{
    private PhotonView pv;
    public PlayerAvatarSetting MyAvatar;

    public Dictionary<int, string> playerNames = new Dictionary<int, string>();
    public Dictionary<int, PhotonView> allPlayers = new Dictionary<int, PhotonView>();

    private bool isSync = true;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!isSync) return;

        SyncAllPlayers();

        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            SyncAllPlayerNames();
        }

        if (PhotonNetwork.PlayerList.Length == 5)
        {
            StartCoroutine(DisableSyncAfterDelay(1f));
        }
    }

    private IEnumerator DisableSyncAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSync = false;
    }

    // 플레이어 닉네임 동기화
    // 플레이어가 참여할 때 호출
    public void RPC_AddPlayerNames(int playerIndex, string nickName)
    {
        if (playerNames.ContainsKey(playerIndex)) return;

        // ActorNumber ≥ 2 → 전체 동기화
        if (PhotonNetwork.LocalPlayer.ActorNumber >= 2)
        {
            SyncAllPlayerNames();
            return;
        }

        // 마스터 클라이언트(선생님)는 별도 처리 없음 (플레이어가 아니라서 이름 저장하면 안 됨)
        if (PhotonNetwork.IsMasterClient) return;

        // 그 외 → 자기 정보만 전송
        pv.RPC(nameof(AddPlayerNames), RpcTarget.All, playerIndex, nickName);

    }

    // 전체 플레이어 동기화
    public void SyncAllPlayerNames()
    {
        // 현재 룸의 모든 플레이어 정보 전송
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int idx = player.ActorNumber - 1;
            string name = player.NickName;
            if (idx > 0) pv.RPC(nameof(AddPlayerNames), RpcTarget.All, idx - 1, name);
        }
    }

    [PunRPC]
    void AddPlayerNames(int playerIndex, string nickName)
    {
        if (playerIndex >= 0) playerNames[playerIndex] = nickName;
    }

    public void AddAllPlayer(PhotonView pv)
    {
        if (pv.Owner.ActorNumber - 2 >= 0)
        {
            allPlayers[pv.Owner.ActorNumber - 2] = pv;
        }

        if(pv.IsMine)
        {
            MyAvatar = pv.GetComponent<PlayerAvatarSetting>();
        }
    }

    public void SyncAllPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if(player.ActorNumber - 2 == i)
                {
                    PhotonView playerPV = GetPhotonViewByActorNumber(player.ActorNumber);
                    allPlayers[i] = playerPV;
                }
            }
        }
    }

    // ActorNumber로 PhotonView 찾기
    private PhotonView GetPhotonViewByActorNumber(int actorNumber)
    {
        foreach (var view in FindObjectsOfType<PlayerMove>())
        {
            if (view.Pv.Owner != null && view.Pv.Owner.ActorNumber == actorNumber)
            {
                return view.Pv;
            }
        }
        return null;
    }
}
