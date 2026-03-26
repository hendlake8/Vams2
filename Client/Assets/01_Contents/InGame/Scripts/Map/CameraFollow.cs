using UnityEngine;

namespace Vams2.InGame.Map
{
    // 플레이어를 추적하는 Orthographic 카메라
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform mTarget;
        [SerializeField] private float mSmoothSpeed = 0.125f;
        [SerializeField] private float mCameraZ = -10f;

        public void SetTarget(Transform target)
        {
            mTarget = target;

            // 즉시 카메라 위치 초기화 (Lerp 지연 방지)
            if (mTarget != null)
            {
                transform.position = new Vector3(mTarget.position.x, mTarget.position.y, mCameraZ);
            }
        }

        private void LateUpdate()
        {
            if (mTarget == null)
            {
                return;
            }

            Vector3 desiredPos = new Vector3(mTarget.position.x, mTarget.position.y, mCameraZ);
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, mSmoothSpeed);
            transform.position = smoothedPos;
        }
    }
}
