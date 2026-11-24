using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public class XEffectSpriteComponent : MonoBehaviour
    {
        public SpriteRenderer spriteRender;

        public void SetSprite(Sprite sprite)
        {
            if (spriteRender)
            {
                spriteRender.sprite = sprite;
            }
        }

        public void SetSpriteURL(string url, System.Action<SpriteRenderer> complete)
        {
            if (!string.IsNullOrEmpty(url))
            {
                XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(url, (Object obj) => {
                    GameObject srGo = obj as GameObject;
                    if (srGo)
                    {
                        SpriteRenderer sr = srGo.GetComponent<SpriteRenderer>();
                        if (sr)
                        {
                            spriteRender.sprite = sr.sprite;
                            if (complete != null)
                            {
                                complete(sr);
                            }
                        }
                    }
                });
            }
        }
    }
}
