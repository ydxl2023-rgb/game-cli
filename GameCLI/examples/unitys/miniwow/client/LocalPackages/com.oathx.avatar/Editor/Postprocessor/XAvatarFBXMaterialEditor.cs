using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Oathx.Avatar.Editor
{
    public class XAvatarFBXMaterialEditor : EditorWindow
    {
        [MenuItem("Avatar/Materials Generator")]
        public static void OpenAutoMaterialsWindow()
        {
            XAvatarFBXMaterialEditor avatarWindow = GetWindow<XAvatarFBXMaterialEditor>();
            avatarWindow.titleContent = new GUIContent("Materials Generator");
            avatarWindow.Show();
        }

        private GameObject _fbxGo;
        private GameObject _lastFbxGo;
        private Shader _shader;
        private Shader _lastShader;
        private Material _sourceMaterial;
        private List<Material> _materials = new List<Material>();
        private Vector2 _position;
        private bool _toggle = true;
        private bool _clear = true;

        public void OnGUI()
        {
            _fbxGo = (GameObject)EditorGUILayout.ObjectField("FBX", _fbxGo, typeof(GameObject), false);
            if (_fbxGo != _lastFbxGo)
            {
                _lastFbxGo = _fbxGo;
                _materials.Clear();

                var path = AssetDatabase.GetAssetPath(_fbxGo);
                var materialPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "Materials");

                var files = System.IO.Directory.GetFiles(materialPath, "*.mat");
                foreach (var file in files)
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(file);
                    _materials.Add(material);

                    if (_shader == null)
                        _shader = material.shader;
                }
            }

            _shader = (Shader)EditorGUILayout.ObjectField("Shader", _shader, typeof(Shader), false);
            if (_lastShader != _shader)
            {
                _lastShader = _shader;
                if (_shader != null)
                    _sourceMaterial = new Material(_shader);
            }

            if (_sourceMaterial != null)
            {
                var names = _sourceMaterial.GetTexturePropertyNames();
                for (int index = 0; index < names.Length; index++)
                {
                    var texture = _sourceMaterial.GetTexture(names[index]);
                    var newTexture = (Texture2D)EditorGUILayout.ObjectField(names[index], texture, typeof(Texture2D), false);
                    _sourceMaterial.SetTexture(names[index], newTexture);
                }
            }

            _toggle = EditorGUILayout.Toggle("Single Material", _toggle);
            _clear = EditorGUILayout.Toggle("Clear Materials", _clear);
            if (GUILayout.Button("Generate Render Materials"))
            {
                if (_fbxGo)
                {
                    var path = AssetDatabase.GetAssetPath(_fbxGo);

                    var outpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "Materials");
                    if (!System.IO.Directory.Exists(outpath))
                        System.IO.Directory.CreateDirectory(outpath);

                    _materials.Clear();

                    if (_clear)
                    {
                        if (System.IO.Directory.Exists(outpath))
                        {
                            System.IO.Directory.Delete(outpath, true);
                            System.IO.Directory.CreateDirectory(outpath);
                        }    
                    }

                    if (_toggle)
                    {
                        string name = _sourceMaterial.name;
                        var textureNames = _sourceMaterial.GetTexturePropertyNames();
                        foreach(var textureName in textureNames)
                        {
                            var texture = _sourceMaterial.GetTexture(textureName);
                            if (texture != null)
                            {
                                name = texture.name;
                                break;
                            }
                        }

                        Material asset = new Material(_sourceMaterial);
                        _materials.Add(asset);

                        AssetDatabase.CreateAsset(asset, $"{outpath}/{_fbxGo.name.ToLower()}{name.ToLower()}.mat");

                        Renderer[] renderers = _fbxGo.GetComponentsInChildren<Renderer>();
                        foreach(var render in renderers)
                        {
                            render.sharedMaterials = _materials.ToArray();
                        }
                    }
                    else
                    {
                        Renderer[] renderers = _fbxGo.GetComponentsInChildren<Renderer>();
                        foreach (var render in renderers)
                        {
                            Material asset = new Material(_sourceMaterial);
                            _materials.Add(asset);

                            AssetDatabase.CreateAsset(asset, $"{outpath}/{render.name}.mat");
                        }
                    }

                    AssetDatabase.SaveAssets();
                }
            }

            if (_materials.Count != 0)
            {
                if (XAvatarUtility.DrawHeader("Materials"))
                {
                    _position = EditorGUILayout.BeginScrollView(_position);

                    foreach (var material in _materials)
                    {
                        XAvatarUtility.BeginContents();
                        EditorGUILayout.ObjectField(material.name, material, typeof(Material), false);

                        if (XAvatarUtility.DrawHeader(material.name, false))
                        {
                            var textures = material.GetTexturePropertyNames();
                            for (int index = 0; index < textures.Length; index++)
                            {
                                Texture2D texture = material.GetTexture(textures[index]) as Texture2D;

                                var texture2D = (Texture2D)EditorGUILayout.ObjectField(textures[index], texture, typeof(Texture2D), false);
                                if (texture2D)
                                    material.SetTexture(textures[index], texture2D);
                            }
                        }
                        XAvatarUtility.EndContents();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
