using System;
using Fusion;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera? Instance { get; private set; } 
        
        // damp velocity of camera
        private Vector3 targetMoveVelocity;
        private Vector3 stretchVelocity;

        // camera target
        Transform? _target;

        [SerializeField]
        Transform cam = default!;
        [SerializeField]
        private float distance = 15f;
        [SerializeField]
        private float pitch = -40;
        [SerializeField]
        private float offsetZ;
        [SerializeField]
        private float cameraStretch = 0.1f;
        [SerializeField]
        private float maxOffset = 0.5f;

        [SerializeField]
        float targetMoveSmoothTime;
        [SerializeField]
        float stretchSmoothTime;

        [SerializeField]
        Transform dummyRig = default!;

        [SerializeField]
        Transform dummyTarget = default!;

        private Camera camComponent = default!;
        private Vector3 deltaStretch;

        void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            camComponent = cam.GetComponent<Camera>();
        }

        private void OnDestroy()
        {
            Instance = null;
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

                var cursorOffset = CursorUtils.GetCursorOffsetNormalized();
                cursorOffset = new Vector3(
                    Math.Clamp(cursorOffset.x / maxOffset, -1, 1),
                    0,
                    Math.Clamp(cursorOffset.z / maxOffset, -1, 1)
                );

                CalculateCameraTransform(_target, pitch, distance, out var pos, out var rot);
                var newDeltaStretch = cameraStretch * cursorOffset;
                // deltaStretch.z *= deltaStretch.z > 0 ? 0.4f : 2f; //fix fov difference stretching

                if (allowSmoothing)
                {
                    deltaStretch = Vector3.SmoothDamp(deltaStretch, newDeltaStretch, ref stretchVelocity, stretchSmoothTime);
                    pos += deltaStretch + Vector3.forward * offsetZ;
                    pos = Vector3.SmoothDamp(transform.position, pos, ref targetMoveVelocity, targetMoveSmoothTime);
                }

                transform.position = pos;
                transform.rotation = rot;

                cam.transform.localRotation = Quaternion.identity;
                cam.transform.localPosition = Vector3.zero;
            }
        }

        public void SetTarget(Transform target)
        {
            if (_target != target)
            {
                _target = target;
                UpdateCamera(false);
            }
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