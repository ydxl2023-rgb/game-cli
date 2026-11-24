using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// bundle location type
    /// </summary>
    public enum LocationType
    {
        Resource,
        Remote,
        Streaming,
        Bundle
    }

    /// <summary>
    /// entyry type
    /// </summary>
    public enum EntryType
    {
        None,
        Pack,
        File,
        Scene
    };

    /// <summary>
    /// asset file info
    /// </summary>
    [Serializable]
    public class AssetFileInfo
    {
        /// <summary>
        /// file path
        /// </summary>
        public string file;

        /// <summary>
        /// file size
        /// </summary>
        public long size;

        /// <summary>
        /// implicit operator bool
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AssetFileInfo exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// entry info
    /// </summary>
    [System.Serializable]
    public class Entry
    {
        /// <summary>
        /// entry location type
        /// </summary>
        public LocationType locationType;
        /// <summary>
        /// entry type
        /// </summary>
        public EntryType entryType;
        /// <summary>
        /// entry name
        /// </summary>
        public string name;
        /// <summary>
        /// entry checksum
        /// </summary>
        public uint checksum;
        /// <summary>
        /// entry size
        /// </summary>
        public uint size;        
        /// <summary>
        /// asset files
        /// </summary>
        public AssetFileInfo[] files;
        /// <summary>
        /// entry dependencies
        /// </summary>
        public string[] dependencies;
        /// <summary>
        /// check if no bundle
        /// </summary>
        /// <returns></returns>
        public bool Nobundle()
        {
            return locationType == LocationType.Resource;
        }

        /// <summary>
        /// implicit operator bool
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(Entry exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// bundle asset info
    /// </summary>
    public class AssetInfo
    {
        /// <summary>
        /// asset location type
        /// </summary>
        public LocationType locationType
        { get; set; }

        /// <summary>
        /// asset bundle name
        /// </summary>
        public string name
        { get; set; }

        /// <summary>
        /// assset path
        /// </summary>
        public string path
        { get; set; }

        /// <summary>
        /// asset size
        /// </summary>
        public long size
        { get; set; }

        /// <summary>
        /// implicit operator bool
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AssetInfo exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// bundle sheet info
    /// </summary>
    [System.Serializable]
    public class BundleSheetInfo
    {
        /// <summary>
        /// parse from json data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BundleSheetInfo Parse(string data)
        {
            return JsonConvert.DeserializeObject<BundleSheetInfo>(data);
        }

        /// <summary>
        /// try parse from json data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static bool TryParse(string text, out BundleSheetInfo sheet)
        {
            try
            {
                sheet = JsonConvert.DeserializeObject<BundleSheetInfo>(text);
                if (sheet)
                    return true;
            }
            catch (Exception e) 
            {
                Debug.LogException(e);
            }

            sheet = null;
            return false;
        }

        /// <summary>
        /// sheet entrys
        /// </summary>
        public List<Entry> entrys;
        /// <summary>
        /// sheet version
        /// </summary>
        public string version;

        /// <summary>
        /// bundle sheet info constructor
        /// </summary>
        public BundleSheetInfo()
        {
            entrys = new List<Entry>();
        }

        /// <summary>
        /// implicit operator bool
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(BundleSheetInfo exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// entry diff info
    /// </summary>
    public class EntryDiff
    {
        /// <summary>
        /// entry type
        /// </summary>
        public EntryType entryType = EntryType.None;
        /// <summary>
        /// entry local
        /// </summary>
        public Entry local;
        /// <summary>
        /// entry remote
        /// </summary>
        public Entry remote;

        /// <summary>
        /// implicit operator bool
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(EntryDiff exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// bundle sheet
    /// </summary>
    public sealed class XBundleSheet
    {
        /// <summary>
        /// sheet entrys
        /// </summary>
        private readonly Dictionary<string, Entry> entrys = new Dictionary<string, Entry>();

        /// <summary>
        /// sheet asset paths
        /// </summary>
        private readonly Dictionary<string, AssetInfo> 
            assetPaths = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// sheet fast asset paths
        /// </summary>
        private readonly Dictionary<string, AssetInfo> fastPaths = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// initialize bundle sheet from json text
        /// </summary>
        /// <param name="sheetJsonText"></param>
        /// <returns></returns>
        public bool Initialize(string sheetJsonText, System.Action<string> complete)
        {
            if (string.IsNullOrEmpty(sheetJsonText))
                return false;

            var info = BundleSheetInfo.Parse(sheetJsonText);
            foreach (var entry in info.entrys)
            {
                AddEntry(entry);
            }

#if UNITY_EDITOR
            Debug.Log($"[XBundleSheet.Initialize] Succeed version {info.version} asset count {assetPaths.Count} files {entrys.Count}");
#endif
            complete?.Invoke(info.version);

            return true;
        }

        /// <summary>
        /// get entrys
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Entry> GetEntrys()
        {
            return entrys;
        }

        /// <summary>
        /// add entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool AddEntry(Entry entry)
        {
            if (entrys.ContainsKey(entry.name))
                return false;

            entrys.Add(entry.name, entry);

#if UNITY_EDITOR
            Debug.Log($"[XBundleSheet.AddEntry] name {entry.name}");
#endif
            LocationType locationType = entry.Nobundle() ? LocationType.Resource : LocationType.Bundle;
            foreach (AssetFileInfo af in entry.files)
            {
                AddAssetPath(af.file, new AssetInfo() {
                    locationType = locationType, name = entry.name, path = af.file, size=af.size
                });
            }
            return true;
        }

        /// <summary>
        /// remove entry
        /// </summary>
        /// <param name="entry"></param>
        public void RemoveEntry(Entry entry)
        {
            if (entrys.ContainsKey(entry.name))
            {
#if UNITY_EDITOR
                Debug.Log($"[XBundleSheet.RemoveEntry] name {entry.name}");
#endif
                entrys.Remove(entry.name);
            }
        }

        /// <summary>
        /// add asset path, but using assets with the same name is prohibited
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assetInfo"></param>
        public void AddAssetPath(string name, AssetInfo assetInfo)
        {
            assetPaths[name] = assetInfo;

            var fastName = Path.GetFileName(assetInfo.path);
            if (!fastPaths.ContainsKey(fastName))
            {
                fastPaths.Add(fastName, assetInfo);
#if UNITY_EDITOR
                Debug.Log($"[XBundleSheet.AddAssetPath] fast {fastName} path {assetInfo.path} succeed");
#endif
            }
        }

        /// <summary>
        /// try get asset info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public bool TryGetAssetInfo(string name, out AssetInfo assetInfo)
        {
            bool result = fastPaths.TryGetValue(name, out assetInfo);
            if (!result)
                result = assetPaths.TryGetValue(name, out assetInfo);
            
            return result;
        }

        /// <summary>
        /// clear entrys and asset paths
        /// </summary>
        public void Clearup()
        {
            entrys.Clear();
            assetPaths.Clear();
            fastPaths.Clear();
        }

        /// <summary>
        /// check file exists
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        public bool Exists(string fullpath)
        {
            return assetPaths.ContainsKey(fullpath);
        }

        /// <summary>
        /// diff sheet, generate hotfix entrys
        /// </summary>
        /// <param name="newSheet"></param>
        /// <returns></returns>
        public Queue<EntryDiff> GenerateSheetDiff(BundleSheetInfo newSheet)
        {
            Queue<EntryDiff> diffs = new Queue<EntryDiff>();
            for (int i = 0; i < newSheet.entrys.Count; i++)
            {
                bool diff = true;

                Entry remoteEntry = newSheet.entrys[i];
                // if remote entry is resource, skip it
                if (remoteEntry.locationType == LocationType.Resource)
                    continue;

                if (entrys.TryGetValue(remoteEntry.name, out Entry localEntry))
                {
                    if (localEntry.checksum == remoteEntry.checksum 
                        || localEntry.locationType == LocationType.Resource)
                    {
                        diff = false;
                    }
                }

                if (diff)
                {
                    diffs.Enqueue(new EntryDiff()
                    {
                        entryType = EntryType.File,
                        local = localEntry,
                        remote = remoteEntry
                    });
                }
            }

            return diffs;
        }

        /// <summary>
        /// save bundle info to file, use json format
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path, bool pretty=true, string version="")
        {
            List<Entry> saveEntrys = new List<Entry>();
            foreach (var entry in entrys.Values)
            {
                saveEntrys.Add(entry);
            }

            BundleSheetInfo bundleSheetInfo = new BundleSheetInfo() {
                entrys = saveEntrys, version = version
            };

            StringBuilder sb = new StringBuilder();
            var json = JsonUtility.ToJson(bundleSheetInfo);
            sb.Append(json);

            byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());
            WriteAtomic(
                path,
                buff,
                candidatePath =>
                {
                    string candidateJson = File.ReadAllText(candidatePath);
                    return BundleSheetInfo.TryParse(candidateJson, out _);
                });
        }

        /// <summary>
        /// Writes and validates a sibling candidate file before atomically replacing
        /// the formal file.
        /// </summary>
        /// <param name="path">The formal file path.</param>
        /// <param name="content">The complete candidate file content.</param>
        /// <param name="validate">An optional candidate-file validator.</param>
        private static void WriteAtomic(
            string path,
            byte[] content,
            Func<string, bool> validate)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A formal file path is required.", nameof(path));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string newPath = path + ".new";
            if (File.Exists(newPath))
                File.Delete(newPath);

            try
            {
                using (FileStream stream = new FileStream(
                    newPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    stream.Write(content, 0, content.Length);
                    stream.Flush(true);
                }

                if (validate != null && !validate(newPath))
                    throw new InvalidDataException("The candidate file is invalid.");

                if (File.Exists(path))
                    File.Replace(newPath, path, null, true);
                else
                    File.Move(newPath, path);
            }
            finally
            {
                if (File.Exists(newPath))
                    File.Delete(newPath);
            }
        }
    }
}
