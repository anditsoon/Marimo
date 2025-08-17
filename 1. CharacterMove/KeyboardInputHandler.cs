using UnityEngine;

// PC/에디터 전용 키보드 입력
public class KeyboardInputHandler : IInputHandler
{
    public bool TryGetMoveDirection(out Vector3 moveDir, ref Vector3 targetPos, ref bool isMoving)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDir = (Vector3.right * h + Vector3.forward * v).normalized;
        return moveDir.sqrMagnitude > 0f;
    }
}
