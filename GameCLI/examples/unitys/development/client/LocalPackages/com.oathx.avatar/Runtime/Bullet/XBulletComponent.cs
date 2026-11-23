using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public enum BulletTrajectory
    {
        Straight,
        Parabolic
    }

    public class XBulletComponent : MonoBehaviour
    {
        public GameObject fireEffectPrefab;
        [HideInInspector]
        public GameObject fireEffect;

        public GameObject hitEffectPrefab;
        [HideInInspector]
        public GameObject hitEffect;

        public BulletTrajectory trajectory = BulletTrajectory.Straight;
        public float speed = 10f;

        [Header("Parabolic")]
        public float parabolaHeight = 5f;
        public Vector3 startPoint = Vector3.zero;
        public Vector3 endPoint = Vector3.forward;
        public float distance = 10f;
        public float minHeight = 0f;

        [Header("Track")]
        public bool canTrackTarget = false;
        public Transform target;
        public float trackingSpeed = 5f;
        public float maxTrackingAngle = 45f;

        [Header("Collider")]
        public BoxCollider boxCollider;
        public LayerMask collisionMask = -1;

        public System.Func<XBulletComponent, Transform, bool> hitCallback;

        private float progress = 0f;
        private Vector3 currentDirection;
        private bool isInitialized = false;
        private float totalDistance;
        private Vector3 calculatedEndPoint;

        private void LateUpdate()
        {
            if (isInitialized)
            {
                MoveBullet();
                CheckCollisions();

                if (progress >= 1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Initialize()
        {
            if (!boxCollider)
            {
                boxCollider = GetComponent<BoxCollider>();
                if (!boxCollider)
                    boxCollider = gameObject.AddComponent<BoxCollider>();  
                
                boxCollider.isTrigger = true;
            }

            transform.position = startPoint;
            calculatedEndPoint = endPoint;

            if (canTrackTarget && target != null)
            {
                calculatedEndPoint = target.position;
            }

            totalDistance = Vector3.Distance(startPoint, calculatedEndPoint);
            currentDirection = (calculatedEndPoint - startPoint).normalized;

            isInitialized = true;
        }

        public void Initialize(Vector3 start, Vector3 end, float fireSpeed, 
            Transform targetTransform, System.Func<XBulletComponent, Transform, bool> callback)
        {
            startPoint = start;
            endPoint = end;
            target = targetTransform;
            speed = fireSpeed;
            hitCallback = callback;

            Initialize();
        }

        public void Initialize(Transform firePoint, Vector3 end, float fireSpeed,
            Transform targetTransform, System.Func<XBulletComponent, Transform, bool> callback)
        {
            startPoint = firePoint.position;
            endPoint = end;
            target = targetTransform;
            speed = fireSpeed;
            hitCallback = callback;

            PlayFireEffect(firePoint);

            Initialize();
        }

        public void PlayFireEffect(Transform firePoint)
        {
            if (fireEffectPrefab)
            {
                fireEffect = GameObject.Instantiate<GameObject>(fireEffectPrefab);
               
                fireEffect.transform.position = firePoint.position;
            }
        }

        private void MoveBullet()
        {
            float deltaProgress = (speed * Time.deltaTime) / totalDistance;
            if (canTrackTarget && target != null)
            {
                UpdateTracking();
            }

            switch (trajectory)
            {
                case BulletTrajectory.Straight:
                    MoveStraight(deltaProgress);
                    break;
                case BulletTrajectory.Parabolic:
                    MoveParabolic(deltaProgress);
                    break;
            }

            progress += deltaProgress;
        }

        private void MoveStraight(float deltaProgress)
        {
            Vector3 newPosition = Vector3.Lerp(startPoint, calculatedEndPoint, progress + deltaProgress);
            transform.position = newPosition;

            if (currentDirection != Vector3.zero)
            {
                transform.forward = currentDirection;
            }
        }

        private void MoveParabolic(float deltaProgress)
        {
            float t = progress + deltaProgress;
            t = Mathf.Clamp01(t);

            Vector3 linearPosition = Vector3.Lerp(startPoint, calculatedEndPoint, t);

            float height = Mathf.Sin(t * Mathf.PI) * parabolaHeight;
            height = Mathf.Max(height, minHeight);

            Vector3 parabolicOffset = Vector3.up * height;
            Vector3 newPosition = linearPosition + parabolicOffset;

            transform.position = newPosition;

            UpdateParabolicRotation(t, newPosition);
        }

        private void UpdateParabolicRotation(float t, Vector3 newPosition)
        {
            float nextT = Mathf.Clamp01(t + 0.01f);
            Vector3 nextLinearPosition = Vector3.Lerp(startPoint, calculatedEndPoint, nextT);
            float nextHeight = Mathf.Sin(nextT * Mathf.PI) * parabolaHeight;
            nextHeight = Mathf.Max(nextHeight, minHeight);
            Vector3 nextPosition = nextLinearPosition + Vector3.up * nextHeight;

            Vector3 direction = (nextPosition - newPosition).normalized;
            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }

        private bool UpdateTracking()
        {
            if (!target)
                return false;

            calculatedEndPoint = target.position;

            Vector3 targetDirection = (calculatedEndPoint - transform.position).normalized;

            float angle = Vector3.Angle(currentDirection, targetDirection);
            if (angle <= maxTrackingAngle)
            {
                currentDirection = Vector3.RotateTowards(
                    currentDirection,
                    targetDirection,
                    trackingSpeed * Mathf.Deg2Rad * Time.deltaTime,
                    0f
                ).normalized;
            }

            totalDistance = Vector3.Distance(startPoint, calculatedEndPoint);
            return true;
        }

        private bool CheckCollisions()
        {
            if (!boxCollider)
                return false;

            Vector3 boxCenter = transform.TransformPoint(boxCollider.center);
            Vector3 halfExtents = boxCollider.size * 0.5f;
            Quaternion orientation = transform.rotation;

            Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfExtents, orientation, collisionMask);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider != boxCollider)
                {
                    OnHit(hitCollider);
                    break;
                }
            }

            return true;
        }

        private void OnHit(Collider other)
        {
           // Destroy(gameObject);
           //if (hitEffectPrefab)
           // {
           //     hitEffect = GameObject.Instantiate(hitEffectPrefab);
           //     hitEffect.transform.position = transform.position;
           // }
        }

        public void SetTrajectory(BulletTrajectory newTrajectory)
        {
            trajectory = newTrajectory;
        }

        public void SetParabolaHeight(float height)
        {
            parabolaHeight = Mathf.Max(height, minHeight);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (canTrackTarget && target != null)
            {
                calculatedEndPoint = target.position;
                totalDistance = Vector3.Distance(startPoint, calculatedEndPoint);
            }
        }

        public void EnableTracking(bool enable)
        {
            canTrackTarget = enable;
        }
    }
}
