using UnityEngine;

public class WeaponRotationFixer : MonoBehaviour
{
    public Vector3 localRotationOffset = new Vector3(90, 0, 0); // Polygon standard
    public Vector3 localPositionOffset = Vector3.zero;

    void LateUpdate()
    {
        // 애니메이션이 끝난 후 강제로 로컬 변환을 고정합니다.
        transform.localRotation = Quaternion.Euler(localRotationOffset);
        transform.localPosition = localPositionOffset;
    }
}
