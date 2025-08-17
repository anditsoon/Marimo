using Photon.Pun;
using UnityEngine;

public class SetCamera : MonoBehaviour
{
    // 카메라 위치 Transform
    [SerializeField] private Transform cameraTransform;

    private PhotonView pv;
    public Transform PlayerAverage;
    private ThirdPersonCamera tpc;

    public bool isFive = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogError("pv 오브젝트를 찾을 수 없습니다.");
        }

        // 플레이어간의 평균값 찾기
        PlayerAverage = GameObject.Find("PlayerAverage").transform;
        if (PlayerAverage == null)
        {
            Debug.LogError("PlayerAverage 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Start()
    {
        // 메인 카메라 찾기
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera 오브젝트를 찾을 수 없습니다.");
        }
        // 버츄얼 카메라 찾기
        tpc = FindObjectOfType<ThirdPersonCamera>();
        if (tpc == null)
        {
            Debug.LogError("tpc 오브젝트를 찾을 수 없습니다.");
        }

        if (pv.IsMine)
        {
            cameraTransform = mainCamera.transform;

            tpc.SetPlayer(this.transform);
        }

        GameManager.instance.SetPlayerObject(pv);
    }

    void Update()
    {
        if (isFive && pv.IsMine)
        {
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (PlayerAverage == null)
        {
            Debug.LogWarning("playerAverage가 null입니다. 초기화되지 않았습니다.");
            return; // playerAverage 또는 tpc가 null일 경우, 더 이상 진행하지 않음
        }
        if(tpc == null)
        {
            Debug.LogWarning("tpc가 null입니다. 초기화되지 않았습니다.");
            return; // playerAverage 또는 tpc가 null일 경우, 더 이상 진행하지 않음
        }


        // 모든 플레이어의 위치 가져오기
        Vector3[] playerPositions = new Vector3[4];
        for(int i = 1; i <= playerPositions.Length; i++)
        {
            playerPositions[i-1] = GameManager.instance.students[i-1].transform.position;
        }

        // 플레이어들의 평균 위치 계산
        Vector3 averagePosition = Vector3.zero;
        foreach (var pos in playerPositions)
        {
            averagePosition += pos;
        }
        averagePosition /= (float)playerPositions.Length;

        // playerAverage의 위치 업데이트
        PlayerAverage.position = averagePosition;

        // ThirdPersonCamera에 새로운 위치 설정
        tpc.SetPlayer(PlayerAverage);
    }

    public PhotonView FindPlayerObjectByActorNumber(int actorNumber)
    {
        PhotonView pview = null;
        PlayerMove[] playerMoves = FindObjectsOfType<PlayerMove>();
        
        for (int i = 0; i < playerMoves.Length; i++)
        {
            if (playerMoves[i].GetComponent<PhotonView>().Owner != null && (playerMoves[i].GetComponent<PhotonView>().Owner.ActorNumber == actorNumber))
            {
                pview = playerMoves[i].GetComponent<PhotonView>();
            }
        }
        return pview;
    }
}
