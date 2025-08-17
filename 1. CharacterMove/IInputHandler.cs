using UnityEngine;

public interface IInputHandler
{
    bool TryGetMoveDirection(out Vector3 moveDir, ref Vector3 targetPos, ref bool isMoving);
}