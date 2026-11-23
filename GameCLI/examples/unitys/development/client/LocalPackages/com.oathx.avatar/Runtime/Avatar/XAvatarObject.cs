using UnityEngine;

namespace Oathx.Avatar
{
    public class XAvatarObject
    {
        public string skeleton;
        public string[] elements;
        public string[] attachments;
        public string overrideController;

        public bool Compare(string skeletonName, string[] elementPaths)
        {
            if (skeleton == skeletonName && elements.Length == elementPaths.Length)
            {
                bool exsit = true;
                for (int i = 0; i < elementPaths.Length; i++)
                {
                    if (elements[i] != elementPaths[i])
                    {
                        exsit = false;
                        break;
                    }
                }

                return exsit;
            }

            return false;
        }
    }
}
