using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public enum XEffectLineMode
    {
        None,
        Sequence,
    }

    public class XEffectLineRender : MonoBehaviour
    {
        [HideInInspector]
        public int gid = 0;

        public XEffectLineMode mode;
        public bool autoDestroy = true;

        /// <summary>
        /// 
        /// </summary>
        private LineRenderer _line;
        private List<Transform> _targets = new List<Transform>();
        private float _duration = 0;
        private float _time = 0;
        private int _old = 0;
        private int _cur = 1;
        private bool _fire = false;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            if (!_line)
                throw new System.NullReferenceException();
        }

        private void Update()
        {
            if (!_fire || _targets.Count < 2)
                return;

            if (mode == XEffectLineMode.Sequence)
            {
                _duration -= Time.deltaTime;
                if (_duration <= 0)
                {
                    _duration = _time;

                    int old = _cur;
                    int cur = _cur + 1;

                    if (cur < _targets.Count)
                    {
                        _line.SetPosition(0, _targets[old].position);
                        _line.SetPosition(1, _targets[cur].position);

                        _cur = cur;
                    }
                    else
                    {
                        if (autoDestroy)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _targets.Count; i++)
                {
                    if (!_targets[i])
                    {
                        _targets.RemoveAt(i);
                        break;
                    }

                    _line.SetPosition(i, _targets[i].position);
                }

                _duration -= Time.deltaTime;
                if (_duration <= 0 && autoDestroy)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Fire(Transform[] targtes, float time, XEffectLineMode lineMode)
        {
            _targets.Clear();
            foreach (Transform trans in targtes)
            {
                _targets.Add(trans);
            }

            _time = time;
            _duration = time;
            _fire = true;

            mode = lineMode;
            switch (mode)
            {
                case XEffectLineMode.None:
                    _line.positionCount = targtes.Length;
                    for (int i = 0; i < targtes.Length; i++)
                    {
                        _line.SetPosition(i, targtes[i].position);
                    }
                    break;

                case XEffectLineMode.Sequence:
                    _line.SetPosition(_old, targtes[_old].position);
                    _line.SetPosition(_cur, targtes[_cur].position);
                    break;
            }
        }
    }
}
