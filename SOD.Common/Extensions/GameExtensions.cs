using UnityEngine;

namespace SOD.Common.Extensions
{
    public static class GameExtensions
    {
        /// <summary>
        /// Determines if the actor sees the target actor.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Sees(this Actor actor, Actor target)
        {
            if (actor == null || target == null || actor.GetHashCode() == target.GetHashCode() || 
                actor.isDead || actor.isStunned || actor.isAsleep || !target.isSeenByOthers)
                return false;

            float distance = Vector3.Distance(actor.lookAtThisTransform.position, target.lookAtThisTransform.position);
            float maxDistance = Mathf.Min(GameplayControls.Instance.citizenSightRange, target.stealthDistance);
            if (distance <= maxDistance)
            {
                if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                    actor.ActorRaycastCheck(target, distance + 3f, out RaycastHit _, false, Color.green, Color.red, Color.white, 1f))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
