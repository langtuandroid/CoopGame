using System.Numerics;
using Bolt.Samples.AdvancedTutorial.scripts;
using Photon.Bolt;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Bolt.AdvancedTutorial
{
	public class PlayerCamera : BoltSingletonPrefab<PlayerCamera>
	{
		// damp velocity of camera
		Vector3 _velocity;

		// camera target
		Transform _target;

		// current camera distance
		private float distance = 20f;
		private float pitch = -40;
		private float cameraStretch = 0.1f;

		[SerializeField]
		Transform cam;

		[SerializeField]
		float aimingDistance = 1f;

		[SerializeField]
		float runningSmoothTime = 0.95f;

		[SerializeField]
		Transform dummyRig;

		[SerializeField]
		Transform dummyTarget;

		private Camera camComponent;
		private Vector3 cursorPosition;


		public System.Func<int> getHealth;

		void Awake ()
		{
			DontDestroyOnLoad (gameObject);
			camComponent = cam.GetComponent<Camera> ();;
		}

		void LateUpdate ()
		{
			UpdateCamera (true);
		}

		void UpdateCamera (bool allowSmoothing)
		{
			if (_target) {
				Cursor.lockState = CursorLockMode.Confined;

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

		public void SetTarget (BoltEntity entity)
		{
			_target = entity.transform;
			UpdateCamera (false);
		}

		public void CalculateCameraAimTransform (Transform target, float pitch, out Vector3 pos, out Quaternion rot)
		{
			CalculateCameraTransform (target, pitch, aimingDistance, out pos, out rot);
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
