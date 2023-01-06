using UnityEngine;

namespace Fragsurf.TraceUtil {
    public class Tracer {

        public static void GetCapsulePoints(CapsuleCollider capc, Vector3 origin, out Vector3 p1, out Vector3 p2) {

            var distanceToPoints = capc.height / 2f - capc.radius;
            p1 = origin + capc.center + Vector3.up * distanceToPoints;
            p2 = origin + capc.center - Vector3.up * distanceToPoints;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static Trace TraceCollider(Collider collider, Vector3 origin, Vector3 end, int layerMask, float colliderScale = 1f) {

            if (collider is BoxCollider) {

                // Box collider trace
                return TraceBox (origin, end, collider.bounds.extents, collider.contactOffset, layerMask, colliderScale);

            } else if (collider is CapsuleCollider) {

                // Capsule collider trace
                var capc = (CapsuleCollider)collider;

                Vector3 point1, point2;
                GetCapsulePoints (capc, origin, out point1, out point2);

                return TraceCapsule (point1, point2, capc.radius, origin, end, capc.contactOffset, layerMask, colliderScale);

            } else if (collider is SphereCollider) {

                var caps = (SphereCollider)collider;

                return TraceSphere(caps.radius, origin, end, caps.bounds.extents, caps.contactOffset, layerMask, colliderScale);
                
            }

            throw new System.NotImplementedException ("Trace missing for collider: " + collider.GetType ());

        }

        public static Trace TraceCapsule(Vector3 point1, Vector3 point2, float radius, Vector3 start, Vector3 destination, float contactOffset, int layerMask, float colliderScale = 1f) {

            var result = new Trace () {
                startPos = start,
                endPos = destination
            };

            var longSide = Mathf.Sqrt (contactOffset * contactOffset + contactOffset * contactOffset);
            radius *= (1f - contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance (start, destination);

            RaycastHit hit;
            if (Physics.CapsuleCast (
                point1: point1,
                point2: point2,
                radius: radius * colliderScale,
                direction: direction,
                hitInfo: out hit,
                maxDistance: maxDistance,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {

                result.fraction = hit.distance / maxDistance;
                result.hitCollider = hit.collider;
                result.hitPoint = hit.point;
                result.planeNormal = hit.normal;
                result.distance = hit.distance;
                
            } else
                result.fraction = 1;

            return result;

        }

        public static Trace TraceBox(Vector3 start, Vector3 destination, Vector3 extents, float contactOffset, int layerMask, float colliderScale = 1f) {

            var result = new Trace () {
                startPos = start,
                endPos = destination
            };

            var longSide = Mathf.Sqrt (contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance (start, destination) + longSide;
            extents *= (1f - contactOffset);

            RaycastHit hit;
            if (Physics.BoxCast (center: start,
                halfExtents: extents * colliderScale,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                hitInfo: out hit,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {

                result.fraction = hit.distance / maxDistance;
                result.hitCollider = hit.collider;
                result.hitPoint = hit.point;
                result.planeNormal = hit.normal;
                result.distance = hit.distance;
                
                RaycastHit normalHit;
                Ray normalRay = new Ray (hit.point - direction * 0.001f, direction);
                if (hit.collider.Raycast (normalRay, out normalHit, 0.002f)) {
                    
                    result.planeNormal = normalHit.normal;

                }
                
            } else
                result.fraction = 1;

            return result;

        }


        public static Trace TraceSphere(float radius, Vector3 start, Vector3 destination, Vector3 extents, float contactOffset, int layerMask, float colliderScale = 1f) {
            
            var result = new Trace () {
                startPos = start,
                endPos = destination
            };

            radius *= (1f - contactOffset);
            var longSide = Mathf.Sqrt (contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance (start, destination);

            RaycastHit hit;
            if (Physics.SphereCast (
                origin: start,
                radius: radius * colliderScale,
                direction: direction,
                hitInfo: out hit,
                maxDistance: maxDistance,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {

                result.fraction = hit.distance / maxDistance;
                result.hitCollider = hit.collider;
                result.hitPoint = hit.point;
                result.planeNormal = hit.normal;
                result.distance = hit.distance;            
                
            } else
                result.fraction = 1;

            return result;
        }
    }

}
