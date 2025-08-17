using UnityEngine;

// 모바일 전용 터치 입력
public class TouchInputHandler : IInputHandler
{
    private Camera cam;
    private LayerMask groundMask;

    public TouchInputHandler(Camera cam, LayerMask groundMask)
    {
        this.cam = cam;
        this.groundMask = groundMask;
    }

    public bool TryGetMoveDirection(out Vector3 moveDir, ref Vector3 targetPos, ref bool isMoving)
    {
        moveDir = Vector3.zero;

        if (Input.touchCount == 0) return false;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(touch.position), out RaycastHit hit, 9999f, groundMask))
            {
                targetPos = new Vector3(hit.point.x, 3f, hit.point.z);
                isMoving = true;
            }
        }

        return false;
    }
}
