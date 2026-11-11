using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public enum XAnimationEventName
    {
        None = 0,
        Event,
        AttackStart,
        AttackEnd,
        AttackWarnStart,
        AttackWarnEnd,
        Sound,
        Effect,
        RenderSetting,
        ForwardStart,
        ForwardEnd,
        BackwardStart,
        BackwardEnd,
        JumpStart,
        JumpEnd,
    }

    public enum XAnimationAttackType
    {
        None,
    }

    public enum XAnimationEventType
    {
        None,
        Hide,
        Show,
    }

    [System.Serializable]
    public class XBoneNogPoint
    {
        public string name;
        public Transform bone;
    }

    public class XAvatarActor : MonoBehaviour
    {
        protected readonly Dictionary<string, AnimationClip>
            overridedAnimation = new Dictionary<string, AnimationClip>();
        protected Animator animator;

        public List<XBoneNogPoint> boneNogPoints = new List<XBoneNogPoint>();

        [Header("Actor ID")]
        public int cid = 0;
        public float radius = 2;

        [Header("Draw debug info")]
        public bool isDrawGL;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            isDrawGL = false;

            if (!animator)
                animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void BoneTransformToNog()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            var enumValues = System.Enum.GetValues(typeof(HumanBodyBones));
            foreach(var value in enumValues)
            {
                HumanBodyBones humanBodyBones = (HumanBodyBones)value;
                if (humanBodyBones < HumanBodyBones.LastBone)
                {
                    var boneNogTrans = animator.GetBoneTransform(humanBodyBones);
                    if (boneNogTrans != null)
                    {
                        var boneNogName = value.ToString();

                        var index = boneNogPoints.FindIndex((v) => v.name == boneNogName);
                        if (index < 0)
                        {
#if UNITY_EDITOR
                            Debug.Log($"Add bone poine {boneNogName}");
#endif
                            boneNogPoints.Add(new XBoneNogPoint()
                            {
                                name = boneNogName,
                                bone = boneNogTrans
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linkBone"></param>
        /// <returns></returns>
        public Transform GetBoneNogPoint(string linkBone)
        {
            var nogPoint = boneNogPoints.Find((v) => v.name.ToLower() == linkBone.ToLower());
            if (nogPoint == null)
                throw new System.NullReferenceException($"Can't find body bone {linkBone}");

            return nogPoint.bone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nogPoint"></param>
        public void AddBoneNogPoint(string name, Transform nogPoint)
        {
            var trans = boneNogPoints.Find((v) => v.name.ToLower() == name.ToLower());
            if (trans == null)
            {
                boneNogPoints.Add(new XBoneNogPoint() {
                    name = name, bone = nogPoint
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        private bool FindAllNogPoint(Transform root, string boneNogPointName)
        {
            var boneNogPointTrans = root.Find(boneNogPointName);
            if (boneNogPointTrans != null)
            {
                var index = boneNogPoints.FindIndex((v) => v.name == boneNogPointName);
                if (index < 0)
                {
#if UNITY_EDITOR
                    Debug.Log($"Add bone poine {boneNogPointName}");
#endif
                    boneNogPoints.Add(new XBoneNogPoint() { 
                        name=boneNogPointName, bone=boneNogPointTrans 
                    });
                }

                return true;
            }

            foreach (Transform child in root)
            {
                FindAllNogPoint(child, boneNogPointName);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContextMenu("Generate Bone NogPoint")]
        public void GenerateBoneNogPoint(string[] boneNames)
        {
            ClearBoneNogPoints();
            BoneTransformToNog();

            foreach(var boneName in boneNames)
            {
                FindAllNogPoint(transform, boneName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearBoneNogPoints()
        {
            boneNogPoints.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        public void SetLayer(int layer)
        {
            foreach (Transform trans in GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clip"></param>
        public void SetAnimatorOverrideAnimation(string name, AnimationClip clip)
        {
            if (!animator)
                return;

            AnimatorOverrideController controller = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (controller != null && clip != null)
            {
                AnimationClip old = controller[name];
                controller[name] = clip;

                if (!overridedAnimation.ContainsKey(name))
                    overridedAnimation.Add(name, old);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clip"></param>
        public void SetAnimatorOverrideAnimation(string name, string clip)
        {
            if (!animator)
                return;

            AnimatorOverrideController controller = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (controller != null && !string.IsNullOrEmpty(clip))
            {
                AnimationClip old = controller[name];
                controller[name] = controller[clip];

                if (!overridedAnimation.ContainsKey(name))
                    overridedAnimation.Add(name, old);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void RevertOverrideAnimation(string name)
        {
            if (!animator)
                return;

            AnimationClip clip;
            overridedAnimation.TryGetValue(name, out clip);

            if (clip != null)
            {
                AnimatorOverrideController controller = animator.runtimeAnimatorController as AnimatorOverrideController;
                if (controller != null && clip != null)
                {
                    controller[name] = clip;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AnimatorStateInfo GetCurrentAnimatorState()
        {
            if (animator.IsInTransition(0))
                return animator.GetNextAnimatorStateInfo(0);

            return animator.GetCurrentAnimatorStateInfo(0);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        enum SectorType
        {
            Sector,
        }

        /// <summary>
        /// 
        /// </summary>
        public class SectorMesh
        {
            public Material material;
            public Mesh mesh;
            public float duration;
        }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<SectorType, SectorMesh>
            sectorMeshs = new Dictionary<SectorType, SectorMesh>();

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (isDrawGL)
            {
                DrawSectorMeshGL();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            foreach (KeyValuePair<SectorType, SectorMesh> it in sectorMeshs)
            {
                Destroy(it.Value.mesh);
                Destroy(it.Value.material);
            }

            sectorMeshs.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        private SectorMesh CreateSectorMesh(List<Vector3> vertices, Material material)
        {
            if (!material)
                throw new System.NullReferenceException();

            Mesh mesh = new Mesh();
            int triangleAmount = vertices.Count - 2;
            int[] triangles = new int[3 * triangleAmount];

            Vector2[] uvs = new Vector2[vertices.Count];
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (Vector3 vertex in vertices)
            {
                if (vertex.x < minX) minX = vertex.x;
                if (vertex.x > maxX) maxX = vertex.x;
                if (vertex.z < minZ) minZ = vertex.z;
                if (vertex.z > maxZ) maxZ = vertex.z;
            }

            float centerX = (minX + maxX) / 2f;
            float centerZ = (minZ + maxZ) / 2f;
            float sizeX = maxX - minX;
            float sizeZ = maxZ - minZ;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                uvs[i] = new Vector2(
                    sizeX > 0 ? (vertex.x - centerX) / sizeX + 0.5f : 0.5f,
                    sizeZ > 0 ? (vertex.z - centerZ) / sizeZ + 0.5f : 0.5f
                );
            }

            for (int i = 0; i < triangleAmount; i++)
            {
                triangles[3 * i] = 0;
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles;
            mesh.uv = uvs;

            SectorMesh trace = new SectorMesh() {
                mesh = mesh, material = Instantiate(material)
            };

            return trace;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="distance"></param>
        /// <param name="material"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public bool DrawSectorRect(float width, float distance, Material material, float duration)
        {
            if (!isDrawGL)
                return false;

            Vector3 forward = transform.forward;
            List<Vector3> vertices = new List<Vector3>();

            float halfWidth = width / 2f;

            Vector3 center = transform.localPosition;
            vertices.Add(-transform.right * halfWidth + center);
            vertices.Add(-transform.right * halfWidth + forward * distance + center);
            vertices.Add(transform.right * halfWidth + forward * distance + center);
            vertices.Add(transform.right * halfWidth + center);

            SectorMesh mesh;
            if (!sectorMeshs.ContainsKey(SectorType.Sector))
            {
                mesh = CreateSectorMesh(vertices, material);
                sectorMeshs.Add(SectorType.Sector, mesh);
            }
            else
            {
                mesh = sectorMeshs[SectorType.Sector];
                mesh.mesh.Clear();

                mesh.mesh.vertices = vertices.ToArray();
                mesh.mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                mesh.duration = duration;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="center"></param>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        public bool BeginSector(float angle, float radius, Material material, float duration)
        {
            if (!isDrawGL)
                return false;

            int pointAmmount = 100;
            float eachAngle = angle / pointAmmount;

            Vector3 forward = transform.forward;
            List<Vector3> vertices = new List<Vector3>();
            Vector3 center = transform.localPosition;

            vertices.Add(center);
            for (int i = 0; i < pointAmmount; i++)
            {
                Vector3 pos = Quaternion.Euler(0f, -angle / 2 + eachAngle * (i - 1), 0f) * forward * radius + center;
                vertices.Add(pos);
            }

            int[] triangles;
            int triangleAmount = vertices.Count - 2;
            triangles = new int[3 * triangleAmount];

            for (int i = 0; i < triangleAmount; i++)
            {
                triangles[3 * i] = 0;
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;
            }

            SectorMesh mesh;
            if (!sectorMeshs.ContainsKey(SectorType.Sector))
            {
                mesh = CreateSectorMesh(vertices, material);
                sectorMeshs.Add(SectorType.Sector, mesh);
            }
            else
            {
                mesh = sectorMeshs[SectorType.Sector];
                mesh.mesh.Clear();

                mesh.mesh.vertices = vertices.ToArray();
                mesh.mesh.triangles = triangles;
                mesh.duration = duration;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndSector()
        {
            if (sectorMeshs.ContainsKey(SectorType.Sector))
            {
                SectorMesh mesh = sectorMeshs[SectorType.Sector];
                Destroy(mesh.mesh);
                Destroy(mesh.material);

                sectorMeshs.Remove(SectorType.Sector);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawSectorMeshGL()
        {
            foreach (KeyValuePair<SectorType, SectorMesh> mesh in sectorMeshs)
            {
                Graphics.DrawMesh(mesh.Value.mesh, Vector3.zero, Quaternion.identity, mesh.Value.material, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XAnimationEventName GetFunctionType(string name)
        {
            XAnimationEventName type = XAnimationEventName.None;
            switch (name)
            {
                case "OnAnimationEvent":
                    type = XAnimationEventName.Event;
                    break;
                case "OnAnimationAttackStart":
                    type = XAnimationEventName.AttackStart;
                    break;
                case "OnAnimationAttackEnd":
                    type = XAnimationEventName.AttackEnd;
                    break;
                case "OnAnimationAttackWarnStart":
                    type = XAnimationEventName.AttackWarnStart;
                    break;
                case "OnAnimationAttackWarnEnd":
                    type = XAnimationEventName.AttackWarnEnd;
                    break;
                case "OnAnimationSound":
                    type = XAnimationEventName.Sound;
                    break;
                case "OnAnimationEffect":
                    type = XAnimationEventName.Effect;
                    break;
                case "OnAnimationForwardStart":
                    type = XAnimationEventName.ForwardStart;
                    break;
                case "OnAnimationForwardEnd":
                    type = XAnimationEventName.ForwardEnd;
                    break;
                case "OnAnimationBackwardStart":
                    type = XAnimationEventName.BackwardStart;
                    break;
                case "OnAnimationBackwardEnd":
                    type = XAnimationEventName.BackwardEnd;
                    break;
                case "OnAnimationJumpStart":
                    type = XAnimationEventName.JumpStart;
                    break;
                case "OnAnimationJumpEnd":
                    type = XAnimationEventName.JumpEnd;
                    break;
                case "OnAnimationRenderSetting":
                    type = XAnimationEventName.RenderSetting;
                    break;
            }

            return type;
        }

        public static string GetFunctionName(XAnimationEventName type)
        {
            string name = string.Empty;
            switch (type)
            {
                case XAnimationEventName.Event:
                    name = "OnAnimationEvent";
                    break;
                case XAnimationEventName.AttackStart:
                    name = "OnAnimationAttackStart";
                    break;
                case XAnimationEventName.AttackEnd:
                    name = "OnAnimationAttackEnd";
                    break;
                case XAnimationEventName.AttackWarnStart:
                    name = "OnAnimationAttackWarnStart";
                    break;
                case XAnimationEventName.AttackWarnEnd:
                    name = "OnAnimationAttackWarnEnd";
                    break;
                case XAnimationEventName.Sound:
                    name = "OnAnimationSound";
                    break;
                case XAnimationEventName.Effect:
                    name = "OnAnimationEffect";
                    break;
                case XAnimationEventName.ForwardStart: 
                    name = "OnAnimationForwardStart";
                    break;
                case XAnimationEventName.ForwardEnd:
                    name = "OnAnimationForwardEnd";
                    break;
                case XAnimationEventName.BackwardStart:
                    name = "OnAnimationBackwardStart";
                    break;
                case XAnimationEventName.BackwardEnd:
                    name = "OnAnimationBackwardEnd";
                    break;
                case XAnimationEventName.JumpStart:
                    name = "OnAnimationJumpStart";
                    break;
                case XAnimationEventName.JumpEnd:
                    name = "OnAnimationJumpEnd";
                    break;
                case XAnimationEventName.RenderSetting:
                    name = "OnAnimationRenderSetting";
                    break;
            }
            return name;
        }
    }
}
