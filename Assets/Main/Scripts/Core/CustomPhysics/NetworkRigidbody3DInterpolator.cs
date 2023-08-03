using Fusion;
using UnityEngine;

namespace Main.Scripts.Core.CustomPhysics
{
    public class NetworkRigidbody3DInterpolator
    {
        private InterpolatedErrorCorrectionSettings settings;

        public NetworkRigidbody3DInterpolator(InterpolatedErrorCorrectionSettings settings)
        {
            this.settings = settings;
        }

        public void ApplyNewInterpolationDelta(ref Vector3 accumulatedError)
        {
            var interpolationDistance = accumulatedError.magnitude;

            if (interpolationDistance > settings.PosTeleportDistance)
            {
                accumulatedError = new Vector3();
            }

            var blendPosDelta = settings.PosBlendEnd - settings.PosBlendStart;
            var t = Mathf.Clamp01((interpolationDistance - settings.PosBlendStart) / blendPosDelta);
            var rate = Mathf.Lerp(settings.MinRate, settings.MaxRate, t);

            var movingFraction = 1f - Time.deltaTime * rate;
            if ((accumulatedError * movingFraction).magnitude < settings.PosMinCorrection)
            {
                UpdateMinPositionCorrection(ref accumulatedError);
            }
            else
            {
                accumulatedError *= movingFraction;
            }
        }

        private void UpdateMinPositionCorrection(
            ref Vector3 accumulatedError)
        {
            if (accumulatedError.x == 0f && accumulatedError.y == 0f && accumulatedError.z == 0f) return;

            var normalized = accumulatedError.normalized;
            var isPositiveX = accumulatedError.x >= 0f;
            var isPositiveY = accumulatedError.y >= 0f;
            var isPositiveZ = accumulatedError.z >= 0f;

            accumulatedError -= normalized * settings.PosMinCorrection;

            if (isPositiveX != accumulatedError.x >= 0f)
            {
                accumulatedError.x = 0f;
            }

            if (isPositiveY != accumulatedError.y >= 0f)
            {
                accumulatedError.y = 0f;
            }

            if (isPositiveZ != accumulatedError.z >= 0f)
            {
                accumulatedError.z = 0f;
            }
        }
    }
}