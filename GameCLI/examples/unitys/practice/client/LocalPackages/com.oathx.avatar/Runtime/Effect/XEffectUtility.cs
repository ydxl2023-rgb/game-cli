using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Oathx.Avatar
{
    public class XEffectUtility
    {
        public static bool Dissolve(Material material, GameObject go, float duration, bool isIn, System.Action callback, int renderQueue)
        {
            if (!material)
            {
                if (callback != null)
                    callback();

                return false;
            }

            List<Material[]> sourceMaterialList = new List<Material[]>();

            Renderer[] skins = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer skin in skins)
            {
                sourceMaterialList.Add(skin.materials);

                List<Material>
                    tempMaterials = new List<Material>();
                foreach (Material srcMat in skin.materials)
                {
                    if (srcMat.renderQueue <= renderQueue)
                    {
                        Material mat = Object.Instantiate(material);
                        if (srcMat.HasProperty("_MainTex"))
                        {
                            mat.SetTexture("_MainTex", srcMat.GetTexture("_MainTex"));
                        }

                        tempMaterials.Add(mat);
                    }
                    else
                    {
                        tempMaterials.Add(srcMat);
                    }
                }

                skin.materials = tempMaterials.ToArray();

                int finished = 0;
                for (int i = 0; i < tempMaterials.Count; i++)
                {
                    Material dissolve = tempMaterials[i];
                    if (dissolve.renderQueue <= renderQueue)
                    {
                        dissolve.SetFloat("_Clip", isIn ? 2 : 0);

                        Tweener tweener = dissolve.DOFloat(isIn ? 0 : 2, "_Clip", duration).SetEase(isIn ? Ease.OutCubic : Ease.InCubic);
                        tweener.OnComplete(delegate
                        {
                            finished = finished + 1;
                            if (finished == tempMaterials.Count)
                            {
                                for (int j = 0; j < skins.Length; j++)
                                {
                                    skins[j].materials = sourceMaterialList[j];
                                }

                                if (callback != null)
                                    callback();
                            }
                        });
                    }
                }
            }

            return true;
        }

        public static bool ReplaceMaterials(GameObject target, Material mat, string[] param, AnimationCurve curve, bool isMultipy, float multipler)
        {
            ParticleSystem ps = target.GetComponent<ParticleSystem>();
            if (ps)
                return false;

            Material[] temp = target.GetComponent<Renderer>().materials;
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = mat;
            }

            target.GetComponent<Renderer>().materials = temp;

            XEffectSkinMaterialCurve esc = target.GetComponent<XEffectSkinMaterialCurve>();
            if (!esc)
                esc = target.AddComponent<XEffectSkinMaterialCurve>();

            esc.param_s = param;
            esc.curve_s = curve;
            esc.Multipler = multipler;
            esc.IsMulti = isMultipy;
            esc.InitDefaultMaterialValue();

            return true;
        }

        public static void DestroyEffect(int effectId)
        {
            XEffectManager.GetSingleton().DestroyEffect(effectId);
        }

        public static void PlayEffectAsync(GameObject owner, Vector3 position, Quaternion rotation, string effectPath, float time,
            System.Action<XEffectComponent> callback)
        {
            XEffectManager instance = XEffectManager.GetSingleton();
            instance.DoGenerateAsync(effectPath, delegate (XEffectCache effectCache, XEffectComponent effect)
            {
                if (effect != null)
                    effect.Initialize(owner, position, rotation, effectCache.prefab, time);

                if (callback != null)
                    callback(effect);
            });
        }

        public static void PlayEffectAsync(GameObject parent, string effectPath, float time,
            System.Action<XEffectComponent> callback)
        {
            XEffectManager instance = XEffectManager.GetSingleton();
            instance.DoGenerateAsync(effectPath, delegate (XEffectCache effectCache, XEffectComponent effect)
            {
                if (effect != null)
                {
                    effect.Initialize(parent, effectCache.prefab, time);
                }

                if (callback != null)
                    callback(effect);
            });
        }

        public static void PlayEffectAsync(GameObject effectPrefab, float time,
            System.Action<XEffectComponent> callback)
        {
            XEffectManager instance = XEffectManager.GetSingleton();
            XEffectCoroutine.Run(instance.GetEffect(effectPrefab, delegate (XEffectCache effectCache, XEffectComponent effect)
            {
                if (effect != null)
                    effect.Initialize(effect.gameObject, effectCache.prefab, time);

                if (callback != null)
                    callback(effect);
            }));
        }

        public static XEffectComponent GetEffect(int gid)
        {
            XEffectManager instance = XEffectManager.GetSingleton();
            if (!instance.effectMap.TryGetValue(gid, out XEffectComponent effect))
                throw new System.NullReferenceException(gid.ToString());

            return effect;
        }

        public static void PlayLineAsync(GameObject parent,
            Vector3 offset, Transform[] targets, string path, XEffectLineMode mode, float time, System.Action<XEffectLineRender> callback)
        {
            XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(path, delegate (Object prefab)
            {
                GameObject lineGo = GameObject.Instantiate(prefab) as GameObject;
                lineGo.transform.SetParent(parent.transform, false);
                lineGo.transform.position = parent.transform.position + offset;

                XEffectLineRender render = lineGo.GetComponent<XEffectLineRender>();
                render.Fire(targets, time, mode);

                if (callback != null)
                {
                    callback(render);
                }
            });
        }

        public static void PlayLineAsync(GameObject parent,
           Transform[] targets, string path, XEffectLineMode mode, float time, System.Action<XEffectLineRender> callback)
        {
            XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(path, delegate (Object prefab)
            {
                GameObject lineGo = GameObject.Instantiate(prefab) as GameObject;
                lineGo.transform.SetParent(parent.transform, false);
                lineGo.transform.position = parent.transform.position;

                XEffectLineRender render = lineGo.GetComponent<XEffectLineRender>();
                render.Fire(targets, time, mode);

                if (callback != null)
                {
                    callback(render);
                }
            });
        }

        public static void PlayUVChainAsync(GameObject parent,
            Transform startTrans, Transform endTrans, string path, float time, System.Action<XEffectUVChainRender> callback)
        {
            XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(path, delegate (Object prefab)
            {
                GameObject lineGo = GameObject.Instantiate(prefab) as GameObject;
                lineGo.transform.SetParent(parent.transform, false);
                lineGo.transform.position = parent.transform.position;

                XEffectUVChainRender render = lineGo.GetComponent<XEffectUVChainRender>();
                render.Fire(startTrans, endTrans, time);

                if (callback != null)
                {
                    callback(render);
                }
            });
        }
    }
}
