using Main.Scripts.Utils;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Main.Scripts.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        // damp velocity of camera
        Vector3 _velocity;

        // camera target
        Transform? _target;

        // current camera distance
        private float distance = 20f;
        private float pitch = -40;
        private float cameraStretch = 0.1f;

        [SerializeField]
        Transform cam = default!;

        [SerializeField]
        float runningSmoothTime = 0.95f;

        [SerializeField]
        Transform dummyRig = default!;

        [SerializeField]
        Transform dummyTarget = default!;

        private Camera camComponent = default!;
        private Vector3 cursorPosition;

        void Awake()
        {
            camComponent = cam.GetComponent<Camera>();
        }

        void Update()
        {
            UpdateCamera(true);
        }

        void UpdateCamera(bool allowSmoothing)
        {
            if (_target != null)
            {
                // Cursor.lockState = CursorLockMode.Confined;
                Cursor.lockState = CursorLockMode.None;

                var plane = new Plane(_target.up, _target.position);

                CursorUtils.getCursorWorldPosition(camComponent, plane, out cursorPosition);

                CalculateCameraTransform(_target, pitch, distance, out var pos, out var rot);
                var deltaStretch = cameraStretch * (cursorPosition - _target.position);
                deltaStretch.z *= deltaStretch.z > 0 ? 0.4f : 2f; //fix fov difference stretching
                pos += deltaStretch;

                if (allowSmoothing)
                {
                    pos = Vector3.SmoothDamp(transform.position, pos, ref _velocity, runningSmoothTime);
                }

                transform.position = pos;
                transform.rotation = rot;

                cam.transform.localRotation = Quaternion.identity;
                cam.transform.localPosition = Vector3.zero;
            }
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            UpdateCamera(false);
        }

        private void CalculateCameraTransform(Transform target, float pitch, float distance, out Vector3 pos,
            out Quaternion rot)
        {
            // copy transform to dummy
            dummyTarget.position = target.position;

            // move position to where we want it
            dummyTarget.position += new Vector3(0, distance, 0);

            // clamp and calculate pitch rotation
            var pitchRotation = Quaternion.Euler(pitch, 0, 0);

            pos = target.position + pitchRotation * (dummyTarget.position - target.position);

            // calculate look-rotation by setting position and looking at target
            dummyRig.position = pos;
            dummyRig.LookAt(target.position);

            rot = dummyRig.rotation;
        }
    }
}