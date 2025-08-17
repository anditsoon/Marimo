using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;

public class PlayerMove : MonoBehaviour, IPunObservable
{
    private CharacterController cc;
    private PhotonVoiceView voiceView;
    private SetCamera setCamera;
    public PhotonView Pv;

    [SerializeField] private GameObject test;

    private float speed = 10;
    private float trackingSpeed = 10;
    float pullStrength = 100f; // 당겨지는 강도 
    
    private Vector3 targetPosition;
    private Vector3 networkPosition;
    private float playerYPos;
    public float moveDistance;
    public bool Movable = false;
    private bool isMoving;
    public bool IsFive;

    // 입력 핸들러
    private IInputHandler inputHandler;

    private void Awake()
    {
        Pv = GetComponent<PhotonView>();
        if(Pv == null)
        {
            Debug.LogError("Pv 를 찾을 수 없습니다");
        }
        voiceView = GetComponent<PhotonVoiceView>();
        if (voiceView == null)
        {
            Debug.LogError("voiceView 를 찾을 수 없습니다");
        }
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("cc 를 찾을 수 없습니다");
        }
        setCamera = GetComponent<SetCamera>();
        if (setCamera == null)
        {
            Debug.LogError("setCamera 를 찾을 수 없습니다");
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        inputHandler = new TouchInputHandler(Camera.main, groundMask);
#else
        inputHandler = new KeyboardInputHandler();
#endif
    }

    void FixedUpdate()
    {
        if (Pv.Owner.IsMasterClient && IsFive)
        {
            transform.position = setCamera.PlayerAverage.position;
        }

        if (Pv.IsMine)
        {
            // 로컬 플레이어만 이동 처리
            if (Movable)
            {
                HandleInput();
                ClampToScreen();
            }
        }
        else
        {
            SyncPosition();
        }
    }

    private void HandleInput()
    {
        if (transform.position.y != playerYPos)
            transform.position = new Vector3(transform.position.x, playerYPos, transform.position.z);

        Vector3 inputDir;
        if (inputHandler.TryGetMoveDirection(out inputDir, ref targetPosition, ref isMoving))
        {
            cc.Move(inputDir * speed * Time.fixedDeltaTime);
            moveDistance = inputDir.sqrMagnitude;
        }

        if (isMoving) MoveTowardsTarget();
    }

    private void ClampToScreen()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

        if (viewPos.x < 0.1f)
            transform.position += Vector3.right * (0.1f - viewPos.x) * pullStrength * Time.deltaTime;
        else if (viewPos.x > 0.9f)
            transform.position += Vector3.right * (0.9f - viewPos.x) * pullStrength * Time.deltaTime;

        if (viewPos.y < 0.1f)
            transform.position += Vector3.forward * (0.1f - viewPos.y) * (pullStrength * 0.5f) * Time.deltaTime;
        else if (viewPos.y > 0.7f)
            transform.position += Vector3.forward * (0.7f - viewPos.y) * (pullStrength * 2f) * Time.deltaTime;
    }

    private void SyncPosition()
    {
        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * trackingSpeed);
    }

    // 목표 지점으로 이동
    void MoveTowardsTarget()
    {
        // 방향 벡터 계산 및 이동
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        cc.Move(direction * speed * Time.fixedDeltaTime);

        moveDistance = direction.magnitude;

        // 이동 후 거리 계산
        float newDistanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 목표 지점에 도달했거나 초과했는지 확인
        if (newDistanceToTarget <= 0.1f)
        {
            transform.position = targetPosition; 
            isMoving = false;                 
            moveDistance = 0;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 만일 데이터를 서버에 전송(PhotonView.IsMine == true)하는 상태라면
        if(stream.IsWriting)
        {
            // iterable 데이터를 보낸다 
            stream.SendNext(transform.position);
            stream.SendNext(voiceView.IsRecording);
            stream.SendNext(moveDistance);
        }

        // 그렇지 않고 만일 데이터를 서버로부터 읽어오는 상태라면
        else if (stream.IsReading)
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            moveDistance = (float)stream.ReceiveNext();
        }
    }
}
