using UnityEngine;
using UnityEngine.InputSystem;
using RTGLite;

namespace LiDARMimic
{
    /// <summary>
    /// DCC 스타일 오빗 카메라 컨트롤러.
    ///  - 좌클릭 드래그  : 회전 (orbit)
    ///  - 중클릭/우클릭 드래그 : 패닝 (pan)
    ///  - 스크롤 휠      : 줌 (dolly)
    ///
    /// 새 Input System 을 직접 사용합니다 (이 프로젝트는 Active Input Handling = Input System Package).
    /// 카메라는 항상 <see cref="focusPoint"/> 를 바라보며, 패닝은 이 초점을 이동시킵니다.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class OrbitCameraController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("카메라가 공전하는 초점. 패닝 시 이동합니다.")]
        public Vector3 focusPoint = Vector3.zero;

        [Header("Orbit")]
        [Tooltip("마우스 이동 1px 당 회전 각도(도).")]
        public float orbitSensitivity = 0.25f;
        [Tooltip("피치(위/아래) 각도 제한.")]
        public float minPitch = -5f;
        public float maxPitch = 85f;

        [Header("Pan")]
        [Tooltip("패닝 속도 배율. 거리에 비례해 자동 보정됩니다.")]
        public float panSensitivity = 0.0015f;

        [Header("Zoom")]
        [Tooltip("스크롤 1노치 당 거리 변화 배율.")]
        public float zoomSensitivity = 0.3f;
        public float minDistance = 1.5f;
        public float maxDistance = 100f;

        [Header("Smoothing")]
        [Tooltip("0 이면 즉시, 값이 클수록 부드럽게 따라갑니다.")]
        public float smoothTime = 0.06f;

        [Header("State (read-only)")]
        [SerializeField] private float distance = 12f;
        [SerializeField] private float yaw = 45f;
        [SerializeField] private float pitch = 30f;

        // 스무딩용 목표/현재 값
        private float _distanceVel;
        private Vector3 _focusVel;
        private float _currentDistance;
        private Vector3 _currentFocus;

        private void Awake()
        {
            _currentDistance = distance;
            _currentFocus = focusPoint;
        }

        private void OnValidate()
        {
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void LateUpdate()
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue();

                // 기즈모 드래그 중에는 카메라 입력을 무시한다 (좌클릭이 기즈모 조작과 겹침).
                bool gizmoDragging = RTGizmos.get != null && RTGizmos.get.draggedGizmo != null;

                // 회전: 좌클릭 드래그
                if (mouse.leftButton.isPressed && !gizmoDragging)
                {
                    yaw += delta.x * orbitSensitivity;
                    pitch -= delta.y * orbitSensitivity;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }

                // 패닝: 중클릭 또는 우클릭 드래그
                if ((mouse.middleButton.isPressed || mouse.rightButton.isPressed) && !gizmoDragging)
                {
                    // 화면 우/상 방향으로 초점 이동 (거리에 비례해 체감 속도 일정하게)
                    float panScale = panSensitivity * distance;
                    Vector3 right = transform.right;
                    Vector3 up = transform.up;
                    focusPoint -= (right * delta.x + up * delta.y) * panScale;
                }

                // 줌: 스크롤 휠 (Windows 는 보통 노치당 ±120)
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    float notches = scroll / 120f;                       // 노치 단위로 정규화
                    float step = notches * distance * zoomSensitivity;   // 현재 거리에 비례한 지수형 줌
                    distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
                }
            }

            // 스무딩
            if (smoothTime > 0f)
            {
                _currentDistance = Mathf.SmoothDamp(_currentDistance, distance, ref _distanceVel, smoothTime);
                _currentFocus = Vector3.SmoothDamp(_currentFocus, focusPoint, ref _focusVel, smoothTime);
            }
            else
            {
                _currentDistance = distance;
                _currentFocus = focusPoint;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 position = _currentFocus - (rotation * Vector3.forward) * _currentDistance;

            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>외부(초기화/버튼 등)에서 카메라 상태를 설정할 때 사용.</summary>
        public void SetView(Vector3 focus, float yawDeg, float pitchDeg, float dist)
        {
            focusPoint = focus;
            yaw = yawDeg;
            pitch = Mathf.Clamp(pitchDeg, minPitch, maxPitch);
            distance = Mathf.Clamp(dist, minDistance, maxDistance);
        }
    }
}
