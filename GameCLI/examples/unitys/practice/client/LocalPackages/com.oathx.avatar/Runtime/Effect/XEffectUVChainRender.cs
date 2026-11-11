using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class XEffectUVChainRender : MonoBehaviour
    {
        public GameObject hitEffectPrefab;
        public Vector3 offset = new Vector3(0f, -0.5f, 0f);
        [HideInInspector]
        public GameObject hitEffect;

        public float detail = 1;
        public float displacement = 15;

        [HideInInspector]
        public Transform target;
        [HideInInspector]
        public Transform start;
        [HideInInspector]
        public float duration = 0;

        public float yOffset = 0;
        public bool autoDestroy = true;

        private LineRenderer _lineRender;
        private List<Vector3> _linePosList;
        private bool _fire = false;

        private void Awake()
        {
            _lineRender = GetComponent<LineRenderer>();
            _linePosList = new List<Vector3>();
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (!_fire)
                return;

            if (!start || !target)
            {
                Destroy(gameObject);
                return;
            }

            _linePosList.Clear();

            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            if (target != null)
            {
                endPos = target.position + Vector3.up * yOffset;
            }
            if (start != null)
            {
                startPos = start.position + Vector3.up * yOffset;
            }

            CollectLinPos(startPos, endPos, displacement);
            _linePosList.Add(endPos);

            _lineRender.positionCount = _linePosList.Count;
            for (int i = 0, n = _linePosList.Count; i < n; i++)
            {
                _lineRender.SetPosition(i, _linePosList[i]);
            }

            duration = duration - Time.deltaTime;
            if (duration <= 0)
            {
                if (autoDestroy)
                    Destroy(gameObject);
            }
        }


        private void CollectLinPos(Vector3 startPos, Vector3 destPos, float displace)
        {
            if (displace < detail)
            {
                _linePosList.Add(startPos);
            }
            else
            {
                float midX = (startPos.x + destPos.x) / 2;
                float midY = (startPos.y + destPos.y) / 2;
                float midZ = (startPos.z + destPos.z) / 2;

                midX += (float)(UnityEngine.Random.value - 0.5) * displace;
                midY += (float)(UnityEngine.Random.value - 0.5) * displace;
                midZ += (float)(UnityEngine.Random.value - 0.5) * displace;

                Vector3 midPos = new Vector3(midX, midY, midZ);

                CollectLinPos(startPos, midPos, displace / 2);
                CollectLinPos(midPos, destPos, displace / 2);
            }
        }

        public bool Fire(Transform startTrans, Transform endTrans, float time)
        {
            start = startTrans;
            target = endTrans;
            duration = time;

            _fire = true;

            if (hitEffectPrefab)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab);
                hitEffect.transform.position = endTrans.position + offset;
            }

            return true;
        }
    }
}

