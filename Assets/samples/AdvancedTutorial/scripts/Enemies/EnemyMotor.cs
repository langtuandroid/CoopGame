using UnityEngine;
using Photon.Bolt;

namespace Bolt.AdvancedTutorial
{
	[RequireComponent(typeof(CharacterController))]
	public class EnemyMotor : MonoBehaviour
	{

		public struct State
		{
			public Vector3 position;
			public Vector3 velocity;
			public bool isGrounded;
		}

		State _state;
		CharacterController _cc;

		[SerializeField]
		float skinWidth = 0.08f;

		[SerializeField]
		float gravityForce = -9.81f;

		[SerializeField]
		float movingSpeed = 6f;

		[SerializeField]
		float maxVelocity = 32f;

		[SerializeField]
		Vector3 drag = new Vector3(1f, 0f, 1f);

		[SerializeField]
		LayerMask layerMask;

		Vector3 sphere
		{
			get
			{
				Vector3 p;

				p = transform.position;
				p.y += _cc.radius;
				p.y -= (skinWidth * 2);

				return p;
			}
		}

		Vector3 waist
		{
			get
			{
				Vector3 p;

				p = transform.position;
				p.y += _cc.height / 2f;

				return p;
			}
		}
		
		public bool isGrounded => _state.isGrounded;


		void Awake()
		{
			_cc = GetComponent<CharacterController>();
			_state = new State();
			_state.position = transform.localPosition;
		}

		public void SetState(Vector3 position, Vector3 velocity, bool isGrounded)
		{
			// assign new state
			_state.position = position;
			_state.velocity = velocity;
			_state.isGrounded = isGrounded;

			// assign local position
			_cc.Move(_state.position - transform.localPosition);
		}

		void Move(Vector3 velocity)
		{
			bool isGrounded = false;

			isGrounded = isGrounded || _cc.Move(velocity * BoltNetwork.FrameDeltaTime) == CollisionFlags.Below;
			isGrounded = isGrounded || _cc.isGrounded;
			isGrounded = isGrounded || Physics.CheckSphere(sphere, _cc.radius, layerMask);

			if (isGrounded && !_state.isGrounded)
			{
				_state.velocity = new Vector3();
			}

			_state.isGrounded = isGrounded;
			_state.position = transform.localPosition;
		}

		public State MoveTo(Vector3 movingDir)
		{
			var moving = false;
			movingDir.y = 0;
			movingDir.Normalize();

			if (movingDir.sqrMagnitude > 0f)
			{
				moving = true;
			}

			//
			if (!_state.isGrounded)
			{
				_state.velocity.y += gravityForce * BoltNetwork.FrameDeltaTime;
			}

			if (moving)
			{
				Move(movingDir * movingSpeed);
			}

			// clamp velocity
			_state.velocity = Vector3.ClampMagnitude(_state.velocity, maxVelocity);

			// apply drag
			_state.velocity.x = ApplyDrag(_state.velocity.x, drag.x);
			_state.velocity.y = ApplyDrag(_state.velocity.y, drag.y);
			_state.velocity.z = ApplyDrag(_state.velocity.z, drag.z);

			// this might seem weird, but it actually gets around a ton of issues - we basically apply 
			// gravity on the Y axis on every frame to simulate instant gravity if you step over a ledge
			_state.velocity.y = Mathf.Min(_state.velocity.y, gravityForce);

			// apply movement
			Move(_state.velocity);

			// set local rotation
			var yaw = Vector3.SignedAngle(Vector3.forward, movingDir, Vector3.up);
			transform.localRotation = Quaternion.Euler(0, 0, 0);

			// detect tunneling
			DetectTunneling();

			// update position
			_state.position = transform.localPosition;

			// done
			return _state;
		}

		float ApplyDrag(float value, float drag)
		{
			if (value < 0)
			{
				return Mathf.Min(value + (drag * BoltNetwork.FrameDeltaTime), 0f);
			}
			else if (value > 0)
			{
				return Mathf.Max(value - (drag * BoltNetwork.FrameDeltaTime), 0f);
			}

			return value;
		}

		void DetectTunneling()
		{
			RaycastHit hit;

			if (Physics.Raycast(waist, Vector3.down, out hit, _cc.height / 2, layerMask))
			{
				transform.position = hit.point;
			}
		}
	}
}
