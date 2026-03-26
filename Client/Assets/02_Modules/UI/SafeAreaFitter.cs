using UnityEngine;

namespace Vams2.UI
{
    // 모바일 Safe Area에 맞춰 RectTransform을 조정
    // Canvas 바로 아래 Panel에 붙여서 사용
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform mRectTransform;
        private Rect mLastSafeArea;

        private void Awake()
        {
            mRectTransform = GetComponent<RectTransform>();
            mLastSafeArea = Rect.zero;
        }

        private void Update()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea != mLastSafeArea)
            {
                ApplySafeArea(safeArea);
                mLastSafeArea = safeArea;
            }
        }

        private void ApplySafeArea(Rect safeArea)
        {
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            mRectTransform.anchorMin = anchorMin;
            mRectTransform.anchorMax = anchorMax;
            mRectTransform.offsetMin = Vector2.zero;
            mRectTransform.offsetMax = Vector2.zero;
        }
    }
}
