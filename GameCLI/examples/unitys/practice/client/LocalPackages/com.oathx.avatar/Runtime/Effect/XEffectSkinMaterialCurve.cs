using UnityEngine;
using System.Collections;

namespace Oathx.Avatar
{
    public class XEffectSkinMaterialCurve : MonoBehaviour
    {
        public string[] param_s = null;
        public AnimationCurve curve_s = null;
        public bool IsMulti = false;
        public float Multipler
        {
            get
            {
                return IsMulti ? _multipler : 1;
            }
            set
            {
                _multipler = value;
            }
        }
        private float _multipler;

        private Material[] mat_s = null;
        private float elapse = 0;
        Renderer render;

        void Start()
        {
            if (mat_s == null)
                InitDefaultMaterialValue();
        }

        public void InitDefaultMaterialValue()
        {
            if (curve_s == null || param_s == null)
            {
                return;
            }
            render = gameObject.GetComponent<Renderer>();
            if (render != null)
            {
                mat_s = new Material[render.sharedMaterials.Length];
                System.Array.Copy(render.materials, mat_s, mat_s.Length);

                for (int i = 0; i < mat_s.Length; i++)
                {
                    for (int x = 0; x < param_s.Length; x++)
                    {
                        mat_s[i].SetFloat(param_s[x], curve_s.Evaluate(0) * Multipler);
                        render.materials = mat_s;
                    }
                }
            }
        }

        void Update()
        {
            if (mat_s == null)
                return;

            elapse += Time.deltaTime;
            for (int i = 0; i < mat_s.Length; i++)
            {
                for (int x = 0; x < param_s.Length; x++)
                {
                    if (param_s[x].Contains("v4"))
                    {
                        string temp_param_name = param_s[x].Substring(2);
                        char last_word = temp_param_name[temp_param_name.Length - 1];

                        string src_param_name = temp_param_name.Substring(0, temp_param_name.Length - 2);

                        Vector4 vec4 = mat_s[i].GetVector(src_param_name);
                        if (last_word == 'x' || last_word == 'r')
                        {
                            vec4.x = curve_s.Evaluate(elapse);
                        }
                        else if (last_word == 'y' || last_word == 'g')
                        {
                            vec4.y = curve_s.Evaluate(elapse);
                        }
                        else if (last_word == 'z' || last_word == 'b')
                        {
                            vec4.z = curve_s.Evaluate(elapse);
                        }
                        else
                        {
                            vec4.w = curve_s.Evaluate(elapse);
                        }
                        mat_s[i].SetVector(src_param_name, vec4);
                    }
                    else
                    {
                        mat_s[i].SetFloat(param_s[x], curve_s.Evaluate(elapse) * Multipler);
                        render.materials = mat_s;
                    }
                }
            }
        }
    }

}
