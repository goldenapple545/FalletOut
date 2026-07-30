using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleTouchButton : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler
    {
        public enum Control
        {
            Throttle,
            Steering,
            Handbrake,
            Boost
        }

        [Header("Binding")]
        [SerializeField] private Control control;
        [SerializeField] private float pressedValue = 1f;

        [Header("Visual")]
        [SerializeField] private bool changeScaleOnPressed = true;
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.85f;

        private VehicleInputSource _inputSource;
        private RectTransform _rectTransform;
        private Vector3 _initialScale;
        private bool _pressed;

        public void Bind(VehicleInputSource inputSource)
        {
            _inputSource = inputSource;
        }

        private void Awake()
        {
            _rectTransform = transform as RectTransform;

            if (_rectTransform != null)
                _initialScale = _rectTransform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            ApplyInput(true);
            SetPressedVisual(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        private void OnDisable()
        {
            Release();
        }

        private void OnDestroy()
        {
            Release();
        }

        private void Release()
        {
            if (!_pressed)
                return;

            _pressed = false;
            ApplyInput(false);
            SetPressedVisual(false);
        }

        private void ApplyInput(bool pressed)
        {
            if (_inputSource == null)
                return;

            switch (control)
            {
                case Control.Throttle:
                    _inputSource.SetTouchThrottle(
                        pressed ? pressedValue : 0f);
                    break;

                case Control.Steering:
                    _inputSource.SetTouchSteering(
                        pressed ? pressedValue : 0f);
                    break;

                case Control.Handbrake:
                    _inputSource.SetTouchHandbrake(pressed);
                    break;
                
                case Control.Boost:
                    _inputSource.SetTouchBoost(pressed);
                    break;
            }
        }

        private void SetPressedVisual(bool pressed)
        {
            if (!changeScaleOnPressed || _rectTransform == null)
                return;

            _rectTransform.localScale = pressed
                ? _initialScale * pressedScale
                : _initialScale;
        }
    }
}