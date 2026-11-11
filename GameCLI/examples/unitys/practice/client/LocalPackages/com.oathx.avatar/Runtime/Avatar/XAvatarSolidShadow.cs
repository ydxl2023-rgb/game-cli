using UnityEngine;

namespace Oathx.Avatar
{
    public class XAvatarSolidShadow : MonoBehaviour
    {
        public float groundHeight = .02f;
        public float heightScale = .2f;
        public bool localPositionY = false;

        public float alpha = .5f;
        public float scale = 1.0f;
        public Transform owner = null;

        void Start()
        {
            if (localPositionY)
                gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, alpha));
        }
    }
}