using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vams2.Input
{
    // 플로팅 싱글 조이스틱
    // 화면 하단 60% 영역에서 터치하면 해당 위치에 조이스틱 표시
    // 드래그로 방향/강도 결정, 손가락 떼면 소멸
    public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("UI")]
        [SerializeField] private RectTransform mBaseRect;
        [SerializeField] private RectTransform mKnobRect;
        [SerializeField] private Image mBaseImage;
        [SerializeField] private Image mKnobImage;

        [Header("설정")]
        [SerializeField] private float mDeadZone = 10f;
        [SerializeField] private float mMaxRadius = 80f;
        [SerializeField] private float mActivationAreaRatio = 0.6f;

        private Canvas mCanvas;
        private RectTransform mCanvasRect;
        private Vector2 mOrigin;
        private Vector2 mDirection;
        private float mMagnitude;
        private bool mIsActive;

        public Vector2 Direction => mDirection;
        public float Magnitude => mMagnitude;

        private void Awake()
        {
            mCanvas = GetComponentInParent<Canvas>();
            mCanvasRect = mCanvas.GetComponent<RectTransform>();

            mDirection = Vector2.zero;
            mMagnitude = 0f;
            mIsActive = false;

            SetJoystickVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 터치 위치가 하단 60% 영역인지 확인
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect, eventData.position, eventData.pressEventCamera, out localPoint);

            float canvasHeight = mCanvasRect.rect.height;
            float activationTop = canvasHeight * mActivationAreaRatio;
            float pointFromBottom = localPoint.y + canvasHeight * 0.5f;

            if (pointFromBottom > activationTop)
            {
                return;
            }

            mIsActive = true;
            mOrigin = eventData.position;

            // 조이스틱을 터치 위치에 표시
            mBaseRect.position = eventData.position;
            mKnobRect.position = eventData.position;
            SetJoystickVisible(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!mIsActive)
            {
                return;
            }

            Vector2 delta = eventData.position - mOrigin;
            float distance = delta.magnitude;

            if (distance < mDeadZone)
            {
                mDirection = Vector2.zero;
                mMagnitude = 0f;
                mKnobRect.position = mOrigin;
                return;
            }

            // 방향과 강도 계산
            mDirection = delta.normalized;
            mMagnitude = Mathf.Clamp01((distance - mDeadZone) / (mMaxRadius - mDeadZone));

            // 손잡이 위치 업데이트 (최대 반경 제한)
            float clampedDistance = Mathf.Min(distance, mMaxRadius);
            mKnobRect.position = mOrigin + mDirection * clampedDistance;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            mIsActive = false;
            mDirection = Vector2.zero;
            mMagnitude = 0f;

            SetJoystickVisible(false);
        }

        private void SetJoystickVisible(bool visible)
        {
            if (mBaseImage != null)
            {
                mBaseImage.enabled = visible;
            }
            if (mKnobImage != null)
            {
                mKnobImage.enabled = visible;
            }
        }
    }
}
