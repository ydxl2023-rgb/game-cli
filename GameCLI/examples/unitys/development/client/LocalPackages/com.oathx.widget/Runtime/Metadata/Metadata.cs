using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
#pragma warning disable IDE1006 // 命名样式
    /// <summary>
    /// binding property field
    /// </summary>
    [System.Serializable]
    public class MetadataField
    {
        /// <summary>
        /// metatdata filed type hash code, only use by sditor inspector
        /// </summary>
        public int hash;

        /// <summary>
        /// field type
        /// </summary>
        public string type;

        /// <summary>
        /// field name
        /// </summary>
        public string name;

        /// <summary>
        /// bind multiple objects
        /// </summary>
        public List<GameObject> targets;

        /// <summary>
        /// bind multiple components
        /// </summary>
        public List<Component> components;
    }

    /// <summary>
    /// binding metadata info
    /// </summary>

    public class Metadata : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public string className;

        /// <summary>
        /// metadata fields
        /// </summary>
        public List<MetadataField> metadatas = new List<MetadataField>();

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public string classGuid;
    }

#pragma warning restore IDE1006 // 命名样式
}
