using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Oathx.Avatar.Editor
{
    public class XAvatarAnimationClipTab : ITabPage
    {
        AnimationClip _targetClip;
        AnimationClip _lastClip;

        string _clipPath = string.Empty;
        string _clipName = string.Empty;
        ModelImporter _importer;
        SerializedObject _serializedObject;
        SerializedProperty _clipAnimations;
        XAvatarAnimationClipInfoProperties _clipInfoProperties;
        AnimationEvent[] _events;
        AnimationEvent _noneEvent = new AnimationEvent();
        Vector2 _scroll = Vector2.zero;
        int _frameLength = 0;

        bool _ignoreEvenGen = false;
        Rect _position = new Rect();

        public XAvatarAnimationClipTab(EditorWindow window)
        {
            _noneEvent.functionName = XAvatarActor.GetFunctionName(XAnimationEventName.Event);
        }

        public void OnEnable(Rect rect)
        {

        }

        public void OnDisable()
        {

        }

        public void OnReload()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringParameter"></param>
        /// <returns></returns>
        public static XAnimationEventType GetEventType(string stringParameter)
        {
            XAnimationEventType eventType = XAnimationEventType.None;
            string[] types = System.Enum.GetNames(typeof(XAnimationEventType));
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == stringParameter)
                {
                    eventType = (XAnimationEventType)i;
                }
            }

            return eventType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringParameter"></param>
        /// <returns></returns>
        public static XAnimationAttackType GetEventAttackType(string stringParameter)
        {
            XAnimationAttackType eventType = XAnimationAttackType.None;
            string[] types = System.Enum.GetNames(typeof(XAnimationAttackType));
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == stringParameter)
                {
                    eventType = (XAnimationAttackType)i;
                }
            }
            return eventType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Color GetColor(XAnimationEventName name)
        {
            Color color = Color.white;
            switch (name)
            {
                case XAnimationEventName.AttackWarnStart:
                case XAnimationEventName.AttackWarnEnd:
                    color = Color.yellow;
                    break;
                case XAnimationEventName.AttackStart:
                case XAnimationEventName.AttackEnd:
                    color = Color.red;
                    break;
                case XAnimationEventName.Event:
                    color = new Color(0.62f, 0.84f, 1.0f); ;
                    break;
                case XAnimationEventName.Effect:
                    color = Color.cyan;
                    break;
                case XAnimationEventName.Sound:
                    color = Color.gray;
                    break;
                default:
                    color = Color.white;
                    break;
            }
            color.a = 1f;
            return color;
        }

        bool UpdateTargetClip()
        {
            if (_lastClip != _targetClip || _serializedObject == null)
            {
                Selection.activeObject = _targetClip;

                _clipPath = AssetDatabase.GetAssetPath(_targetClip);
                if (!string.IsNullOrEmpty(_clipPath))
                {
                    _importer = AssetImporter.GetAtPath(_clipPath) as ModelImporter;
                    if (!_importer)
                        return false;

                    _serializedObject = new SerializedObject(_importer);

                    _clipAnimations = _serializedObject.FindProperty("m_ClipAnimations");
                    if (!_clipAnimations.isArray || _clipAnimations.arraySize < 1)
                    {
                        return false;
                    }
                }

                _lastClip = _targetClip;
                _events = null;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        void InitClipEvents()
        {
            if (_events == null)
            {
                _clipInfoProperties = new XAvatarAnimationClipInfoProperties(_clipAnimations.GetArrayElementAtIndex(0));
                _events = AnimationUtility.GetAnimationEvents(_targetClip);

                List<AnimationEvent> events = new List<AnimationEvent>();
                foreach (AnimationEvent e in _events)
                {
                    events.Add(e);
                }

                _events = events.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DrawClipTitle()
        {
            if (string.IsNullOrEmpty(_clipPath))
                return false;

            _clipName = Path.GetFileName(_clipPath);

            EditorGUILayout.LabelField("Animation Clip Name", _clipName);

            _frameLength = (int)(_targetClip.length * _targetClip.frameRate);
            EditorGUILayout.LabelField("Animation Length", _frameLength.ToString());

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <returns></returns>
        private bool DrawEvent(AnimationEvent animationEvent)
        {
            int width = Mathf.FloorToInt(_position.width - 20);
            EditorGUILayout.BeginHorizontal();

            var eventName = XAvatarActor.GetFunctionType(animationEvent.functionName);
            GUI.backgroundColor = GetColor(eventName);

            eventName = (XAnimationEventName)EditorGUILayout.EnumPopup(eventName, GUILayout.Width(100));
            
            animationEvent.functionName = XAvatarActor.GetFunctionName(eventName);
            animationEvent.floatParameter = EditorGUILayout.FloatField(animationEvent.floatParameter, GUILayout.Width(width * 0.1f));
            animationEvent.intParameter = EditorGUILayout.IntField(animationEvent.intParameter, GUILayout.Width(width * 0.1f));
            animationEvent.stringParameter = EditorGUILayout.TextField(animationEvent.stringParameter, GUILayout.Width(width * 0.1f));

            ShowEvent(animationEvent, eventName, width);
            ShowAttackStart(animationEvent, eventName, width);
            ShowAttackEnd(animationEvent, eventName, width);
            ShowAttackWarnStart(animationEvent, eventName, width);
            ShowAttackWarnEnd(animationEvent, eventName, width);
            ShowSound(animationEvent, eventName, width);
            ShowEffect(animationEvent, eventName, width);
            ShowBackwardStart(animationEvent, eventName, width);
            ShowBackwardEnd(animationEvent, eventName, width);
            ShowForwardStart(animationEvent, eventName, width);
            ShowForwardEnd(animationEvent, eventName, width);
            ShowJumpStart(animationEvent, eventName, width);
            ShowJumpEnd(animationEvent, eventName, width);
            ShowRenderSetting(animationEvent, eventName, width);

            ShowTime(animationEvent, width);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="width"></param>
        private void ShowTime(AnimationEvent animationEvent, int width)
        {
            if (_targetClip)
            {
                float value = (float)System.Math.Round(animationEvent.time / _targetClip.length * 100f, 2);
                animationEvent.time = EditorGUILayout.FloatField(value, 
                    GUILayout.Width(width * 0.2f)) * _targetClip.length / 100f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowEvent(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.Event)
                return false;

            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(ScriptableObject), false, GUILayout.Width(width * 0.2f));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowAttackStart(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.AttackStart)
                return false;

            if (_targetClip)
                animationEvent.stringParameter = _targetClip.name;

            float sacle = animationEvent.objectReferenceParameter ? 0.2f : 0.1f;
            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(XAvatarAttackFrame), false, GUILayout.Width(width * sacle));

            if (!animationEvent.objectReferenceParameter)
            {
                if (GUILayout.Button("+", GUILayout.Width(width * sacle)))
                {
                    animationEvent.objectReferenceParameter = CreateAttackFrame(_targetClip, _clipPath, eventName.ToString());
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowAttackEnd(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.AttackEnd)
                return false;
            
            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter, 
                typeof(GameObject), false, GUILayout.Width(width * 0.2f));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetClip"></param>
        /// <param name="clipPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private XAvatarAttackFrame CreateAttackFrame(AnimationClip targetClip, string clipPath, string title)
        {
            string savePath = GetSavePath(targetClip, clipPath, title, typeof(XAvatarAttackFrame));

            XAvatarAttackFrame attackFrame = ScriptableObject.CreateInstance<XAvatarAttackFrame>();
            attackFrame.attackName = targetClip.name;

            AssetDatabase.CreateAsset(attackFrame, savePath);
            AssetDatabase.Refresh();

            attackFrame = AssetDatabase.LoadAssetAtPath<XAvatarAttackFrame>(savePath);
            if (!attackFrame)
                throw new System.NullReferenceException();

            EditorUtility.SetDirty(attackFrame);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return attackFrame;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowAttackWarnStart(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.AttackWarnStart)
                return false;

            if (_targetClip)
                animationEvent.stringParameter = _targetClip.name;

            float sacle = animationEvent.objectReferenceParameter ? 0.2f : 0.1f;
            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(XAvatarAttackFrame), false, GUILayout.Width(width * sacle));

            if (!animationEvent.objectReferenceParameter)
            {
                if (GUILayout.Button("+", GUILayout.Width(width * 0.1f)))
                    animationEvent.objectReferenceParameter = CreateWarnFrame(_targetClip, _clipPath, eventName.ToString());
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowAttackWarnEnd(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.AttackWarnEnd)
                return false;

            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(GameObject), false, GUILayout.Width(width * 0.2f));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetClip"></param>
        /// <param name="clipPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private XAvatarWarnFrameObject CreateWarnFrame(AnimationClip targetClip, string clipPath, string title)
        {
            string savePath = GetSavePath(targetClip, clipPath, title, typeof(XAvatarWarnFrameObject));

            XAvatarWarnFrameObject attackFrame = ScriptableObject.CreateInstance<XAvatarWarnFrameObject>();
            attackFrame.attackName = targetClip.name;

            AssetDatabase.CreateAsset(attackFrame, savePath);
            AssetDatabase.Refresh();

            attackFrame = AssetDatabase.LoadAssetAtPath<XAvatarWarnFrameObject>(savePath);
            if (!attackFrame)
                throw new System.NullReferenceException();

            EditorUtility.SetDirty(attackFrame);
            AssetDatabase.SaveAssets();

            return attackFrame;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowSound(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.Sound)
                return false;

            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(AudioClip), false, GUILayout.Width(width * 0.2f));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowEffect(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.Effect)
                return false;

            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(XEffectConfigObject), false, GUILayout.Width(width * 0.2f));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowBackwardStart(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.BackwardStart)
            {
                ShowCurve(animationEvent, eventName, width);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowBackwardEnd(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.BackwardEnd)
            {
                animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                    typeof(GameObject), false, GUILayout.Width(width * 0.2f));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowForwardStart(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.ForwardStart)
            {
                ShowCurve(animationEvent, eventName, width);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowForwardEnd(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.ForwardEnd)
            {
                animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                    typeof(GameObject), false, GUILayout.Width(width * 0.2f));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowJumpStart(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.JumpStart)
            {
                ShowCurve(animationEvent, eventName, width);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowJumpEnd(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName == XAnimationEventName.JumpEnd)
            {
                animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                    typeof(GameObject), false, GUILayout.Width(width * 0.2f));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        private void ShowCurve(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            float sacle = animationEvent.objectReferenceParameter ? 0.2f : 0.1f;
            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(XAvatarMoveCurve), false, GUILayout.Width(width * sacle));
            if (!animationEvent.objectReferenceParameter)
            {
                if (GUILayout.Button("+", GUILayout.Width(width * 0.1f)))
                    animationEvent.objectReferenceParameter = CreateCurve(_targetClip, _clipPath, eventName.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="clipPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private XAvatarMoveCurve CreateCurve(AnimationClip animationClip, string clipPath, string title)
        {
            string savePath = GetSavePath(animationClip, clipPath, title, typeof(XAvatarMoveCurve));

            XAvatarMoveCurve curve = ScriptableObject.CreateInstance<XAvatarMoveCurve>();
            
            AssetDatabase.CreateAsset(curve, savePath);
            AssetDatabase.Refresh();

            curve = AssetDatabase.LoadAssetAtPath<XAvatarMoveCurve>(savePath);
            if (!curve)
                throw new System.NullReferenceException();

            EditorUtility.SetDirty(curve);
            AssetDatabase.SaveAssets();

            return curve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationEvent"></param>
        /// <param name="eventName"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool ShowRenderSetting(AnimationEvent animationEvent, XAnimationEventName eventName, int width)
        {
            if (eventName != XAnimationEventName.RenderSetting)
                return false;

            if (_targetClip)
                animationEvent.stringParameter = _targetClip.name;

            float sacle = animationEvent.objectReferenceParameter ? 0.2f : 0.1f;
            animationEvent.objectReferenceParameter = EditorGUILayout.ObjectField(animationEvent.objectReferenceParameter,
                typeof(XAvatarAttackFrame), false, GUILayout.Width(width * sacle));

            if (!animationEvent.objectReferenceParameter)
            {
                if (GUILayout.Button("+", GUILayout.Width(width * 0.1f)))
                    animationEvent.objectReferenceParameter = CreateRenderSetting(_targetClip, _clipPath, eventName.ToString());
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="clipPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private XAvatarRenderSetting CreateRenderSetting(AnimationClip animationClip, string clipPath, string title)
        {
            string savePath = GetSavePath(animationClip, clipPath, title, typeof(XAvatarRenderSetting));

            XAvatarRenderSetting renderSetting = ScriptableObject.CreateInstance<XAvatarRenderSetting>();
            renderSetting.clipName = animationClip.name;

            AssetDatabase.CreateAsset(renderSetting, savePath);
            AssetDatabase.Refresh();

            renderSetting = AssetDatabase.LoadAssetAtPath<XAvatarRenderSetting>(savePath);
            if (!renderSetting)
                throw new System.NullReferenceException();

            EditorUtility.SetDirty(renderSetting);
            AssetDatabase.SaveAssets();

            return renderSetting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="clipPath"></param>
        /// <param name="folder"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSavePath(AnimationClip clip, string clipPath, string folder, System.Type type)
        {
            Debug.LogError(clipPath);
            
            string[] assetPaths = clipPath.Split(new string[] { "/" }, System.StringSplitOptions.RemoveEmptyEntries);
            string assetPath = string.Concat(assetPaths[1], "/", assetPaths[2], "/", assetPaths[3], "/EventObject/" + folder);

            string dirPath = Path.Combine(Application.dataPath, assetPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string saveName = Path.GetFileName(Path.GetDirectoryName(clipPath)).ToLower() + clip.name.ToLower();

            string assetName = Path.GetFileNameWithoutExtension(clipPath).Replace("@", "_").ToLower();
            int counter = 1;
            string savePath = "Assets/" + assetPath + "/" + assetName + ".asset";
            while (AssetDatabase.LoadAssetAtPath(savePath, type))
            {
                savePath = "Assets/" + assetPath + "/" + saveName + counter.ToString() + ".asset";
                counter++;
            }

            return savePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        private void ProcessAttackDamageCount(AnimationEvent[] events)
        {
            int dmgTotal = 0;
            foreach (AnimationEvent aniEvent in events)
            {
                if (XAvatarActor.GetFunctionType(aniEvent.functionName) == XAnimationEventName.AttackStart)
                {
                    XAvatarAttackFrame checkPoint = aniEvent.objectReferenceParameter as XAvatarAttackFrame;
                    dmgTotal++;
                }
            }

            int dmgCurrent = 1;
            foreach (AnimationEvent aniEvent in events)
            {
                if (XAvatarActor.GetFunctionType(aniEvent.functionName) == XAnimationEventName.AttackStart)
                {
                    aniEvent.intParameter = dmgTotal;
                    if (dmgTotal > 1)
                        aniEvent.floatParameter = dmgCurrent + 1.0f / dmgTotal;
                    else
                        aniEvent.floatParameter = (float)dmgCurrent;

                    XAvatarAttackFrame checkPoint = aniEvent.objectReferenceParameter as XAvatarAttackFrame;
                    dmgCurrent++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private bool DrawAddButton()
        {
            int width = (int)(_position.width - 20);
            
            GUI.color = GetColor(XAnimationEventName.None);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField("Function Name", GUILayout.Width(width*0.2f));
            EditorGUILayout.LabelField("Float", GUILayout.Width(width * 0.1f));
            EditorGUILayout.LabelField("Int", GUILayout.Width(width * 0.1f));
            EditorGUILayout.LabelField("String", GUILayout.Width(width * 0.2f));
            EditorGUILayout.LabelField("Object", GUILayout.Width(width * 0.2f));
            EditorGUILayout.LabelField("Time");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            XAnimationEventName eventName = (XAnimationEventName)EditorGUILayout.EnumPopup(XAvatarActor.GetFunctionType(_noneEvent.functionName), GUILayout.Width(100));
            _noneEvent.functionName = XAvatarActor.GetFunctionName(eventName);
            _noneEvent.floatParameter = EditorGUILayout.FloatField(_noneEvent.floatParameter, GUILayout.Width(width * 0.1f));
            _noneEvent.intParameter = EditorGUILayout.IntField(_noneEvent.intParameter, GUILayout.Width(width * 0.1f));
            _noneEvent.stringParameter = EditorGUILayout.TextField(_noneEvent.stringParameter, GUILayout.Width(width * 0.1f));
            _noneEvent.objectReferenceParameter = EditorGUILayout.ObjectField("", _noneEvent.objectReferenceParameter,
                typeof(GameObject), false, GUILayout.Width(width * 0.2f));
            _noneEvent.time = EditorGUILayout.FloatField(_noneEvent.time / _targetClip.length * 100f) * _targetClip.length / 100f;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add"))
            {
                List<AnimationEvent> events = new List<AnimationEvent>(_targetClip.events);
                events.Add(new AnimationEvent() {
                    functionName = XAvatarActor.GetFunctionName(eventName)
                });

                AnimationEvent[] arrays = new AnimationEvent[events.Count];
                for(int idx=0; idx<events.Count; idx++)
                {
                    arrays[idx] = new AnimationEvent() {
                        functionName = events[idx].functionName,
                        floatParameter = events[idx].floatParameter,
                        intParameter = events[idx].intParameter,
                        stringParameter = events[idx].stringParameter,
                        objectReferenceParameter = events[idx].objectReferenceParameter,
                        time = events[idx].time / _targetClip.length
                    };
                }

                _clipInfoProperties = new XAvatarAnimationClipInfoProperties(_clipAnimations.GetArrayElementAtIndex(0));
                _clipInfoProperties.SetEvents(arrays);

                _serializedObject.ApplyModifiedProperties();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetClip));

                _lastClip = null;
                _events = null;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private bool DrawRemoveButton(int idx)
        {
            if (!GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
                return false;
            
            List<AnimationEvent> events = new List<AnimationEvent>(_events);
            events.RemoveAt(idx);

            AnimationEvent[] newEvents = new AnimationEvent[events.Count];
            for (int ix = 0; ix < events.Count; ix++)
            {
                if (events[ix] != null)
                {
                    newEvents[ix] = new AnimationEvent();
                    newEvents[ix].functionName = events[ix].functionName.Trim();
                    newEvents[ix].floatParameter = events[ix].floatParameter;
                    newEvents[ix].intParameter = events[ix].intParameter;
                    newEvents[ix].stringParameter = events[ix].stringParameter.Trim();
                    newEvents[ix].objectReferenceParameter = events[ix].objectReferenceParameter;
                    newEvents[ix].time = events[ix].time / _targetClip.length;
                }
            }

            _clipInfoProperties.SetEvents(newEvents);
            _serializedObject.ApplyModifiedProperties();

            var assetPath = AssetDatabase.GetAssetPath(_targetClip);
            AssetDatabase.ImportAsset(assetPath);

            _lastClip = null;
            _events = null;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DrawAnimationEvents()
        {
            int width = (int)(_position.width - 20);
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField("Function Name", GUILayout.Width(100));
            EditorGUILayout.LabelField("Float", GUILayout.Width(width * 0.1f));
            EditorGUILayout.LabelField("Int", GUILayout.Width(width * 0.1f));
            EditorGUILayout.LabelField("String", GUILayout.Width(width * 0.2f));
            EditorGUILayout.LabelField("Object", GUILayout.Width(width * 0.2f));
            EditorGUILayout.LabelField("Time");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            for (int idx = 0; idx < _events.Length; idx++)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(25));
                if (DrawRemoveButton(idx))
                {
                    GUILayout.EndHorizontal();
                    break;
                }
                else
                {
                    DrawEvent(_events[idx]);
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            return true;
        }

        private float GetTime(XAnimationEventName eventName, float startTime, AnimationEvent[] events)
        {
            foreach(var evt in events)
            {
                var type = XAvatarActor.GetFunctionType(evt.functionName);
                if (type == eventName)
                {
                    return evt.time - startTime;
                }
            }

            return 0f;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool Apply()
        {
            if (_events == null)
                return false;
            
            ProcessAttackDamageCount(_events);

            List<AnimationEvent> events = new List<AnimationEvent>();
            for (int i = 0; i < _events.Length; i++)
            {
                var eventName = XAvatarActor.GetFunctionType(_events[i].functionName);
                if (eventName != XAnimationEventName.None)
                {
                    if (eventName == XAnimationEventName.BackwardStart)
                    {
                        if (_events[i].objectReferenceParameter is XAvatarMoveCurve c)
                        {
                            c.endTime = GetTime(XAnimationEventName.BackwardEnd, _events[i].time, _events);
                            _events[i].floatParameter = c.endTime;

                            EditorUtility.SetDirty(c);
                        }
                            
                    }
                    else if (eventName == XAnimationEventName.ForwardStart)
                    {
                        if (_events[i].objectReferenceParameter is XAvatarMoveCurve c)
                        {
                            c.endTime = GetTime(XAnimationEventName.ForwardEnd, _events[i].time, _events);
                            _events[i].floatParameter = c.endTime;

                            EditorUtility.SetDirty(c);
                        }
                            
                    }
                    else if (eventName == XAnimationEventName.JumpStart)
                    {
                        if (_events[i].objectReferenceParameter is XAvatarMoveCurve c)
                        {
                            c.endTime = GetTime(XAnimationEventName.JumpEnd, _events[i].time, _events);
                            _events[i].floatParameter = c.endTime;

                            EditorUtility.SetDirty(c);
                        }
                    }

                    events.Add(new AnimationEvent() {
                        functionName = _events[i].functionName,
                        floatParameter = _events[i].floatParameter,
                        intParameter = _events[i].intParameter,
                        stringParameter = _events[i].stringParameter,
                        objectReferenceParameter = _events[i].objectReferenceParameter,
                        time = CalFixedEventTime(_events[i].time, _targetClip, _frameLength) / _targetClip.length
                    });
                }   
            }

            if (_clipAnimations == null)
                _clipAnimations = _serializedObject.FindProperty("m_ClipAnimations");
            
            _clipInfoProperties = new XAvatarAnimationClipInfoProperties(_clipAnimations.GetArrayElementAtIndex(0));
            _clipInfoProperties.SetEvents(events.ToArray());

            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            var assetPath = AssetDatabase.GetAssetPath(_targetClip);
            if (!string.IsNullOrEmpty(assetPath))
                AssetDatabase.ImportAsset(assetPath);

            _lastClip = null;
            _events = null;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ApplyEvents()
        {
            if (GUILayout.Button("Refresh"))
                _events = null;

            if (GUILayout.Button("Apply"))
            {
                Apply();
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="targetClip"></param>
        /// <param name="frameLength"></param>
        /// <returns></returns>
        public static float CalFixedEventTime(float time, AnimationClip targetClip, int frameLength)
        {
            int countRate = (int)(time / targetClip.length * frameLength);
            float half = (countRate == frameLength || time == 0) ? 0f : 0.5f;
            float timeFixed = ((float)countRate + half) * targetClip.length / frameLength;

            return timeFixed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CopyKeyFrame(AnimationClip source, AnimationClip target)
        {
            if (!source && !target)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnGUI(Rect windowPosition)
        {
            _position = windowPosition;
            _targetClip = EditorGUILayout.ObjectField("Target", _targetClip, typeof(AnimationClip), true) as AnimationClip;
            if (_targetClip)
            {
                _ignoreEvenGen = EditorGUILayout.Toggle("ignoreEvenGen", _ignoreEvenGen);
                if (!UpdateTargetClip())
                    return;

                DrawClipTitle();
                InitClipEvents();

                GUILayout.Space(10);
                _scroll = EditorGUILayout.BeginScrollView(_scroll);

                DrawAnimationEvents();

                EditorGUILayout.EndScrollView();
                DrawAddButton();

                if (GUILayout.Button("Calculate Attack Damage Count"))
                {
                    ProcessAttackDamageCount(_events);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                ApplyEvents();
            }
        }
    }
}
