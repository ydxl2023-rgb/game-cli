using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

namespace Oathx.Avatar.Editor
{
    [Serializable]
    public class XAvatarComplite
    {
        public string inputDirectory;
        public string outDirectory;
        public AnimatorController animatorController;
        public ModelImporterAnimationType importerAnimationType;
    }

    [FilePath("ProjectSettings/AvatarSettings.asset")]
    public class XAvatarEditorSetting : ScriptableSingleton<XAvatarEditorSetting>
    {
        public List<XAvatarComplite> complites = new List<XAvatarComplite>();
        public List<string> boneNogPoints = new List<string>();
        public string currentEditAnimationClipPath;
    }
}
