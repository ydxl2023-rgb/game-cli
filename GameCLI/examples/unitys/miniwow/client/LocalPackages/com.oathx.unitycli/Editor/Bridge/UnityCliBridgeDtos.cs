using System;
using Oathx.UnityCLI.Protocol;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Defines a set of data transfer objects (DTOs) used for communication between the Unity CLI bridge and the Unity editor.
    /// </summary>
    [Serializable]
    public sealed class PingResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public PingData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error;

        /// <summary>
        /// Creates a successful <see cref="PingResult"/> with the specified endpoint information and supported routes.
        /// This method is typically used to generate a response to a ping request from the Unity CLI bridge,
        /// indicating that the editor is responsive and providing details about the current endpoint and supported routes.
        /// </summary>
        /// <param name="endpoint">The active bridge endpoint.</param>
        /// <param name="supportedRoutes">The routes currently supported by the bridge.</param>
        /// <returns>A successful ping result.</returns>
        public static PingResult Pong(CLIEndpoint endpoint, string supportedRoutes)
        {
            return new PingResult
            {
                ok = true,
                status = "completed",
                message = "pong",
                data = new PingData
                {
                    endpoint = endpoint,
                    respondedAtUtc = DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                    supportedRoutes = supportedRoutes ?? string.Empty
                }
            };
        }

        /// <summary>
        /// Creates a failed PingResult with the specified error code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A failed ping result.</returns>
        public static PingResult Fail(string code, string message)
        {
            return new PingResult
            {
                ok = false,
                status = "error",
                message = message,
                error = new PingError { code = code, message = message }
            };
        }
    }

    /// <summary>
    /// Defines the data structure for a successful ping response from the Unity editor, including details about the current CLI endpoint and supported routes.
    /// </summary>
    [Serializable]
    public sealed class PingData
    {
        /// <summary>
        /// Stores the endpoint value.
        /// </summary>
        public CLIEndpoint endpoint;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
        /// <summary>
        /// Stores the supported routes value.
        /// </summary>
        public string supportedRoutes = string.Empty;
    }

    /// <summary>
    /// Defines the data structure for an error response to a ping request, including an error code and message that describe the nature of the failure.
    /// </summary>
    [Serializable]
    public sealed class PingError
    {
        /// <summary>
        /// Stores the code value.
        /// </summary>
        public string code;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
    }

    /// <summary>
    /// Defines the data structure for a response to a console logs request, including details about the total number of logs, the type of logs returned,
    /// </summary>
    [Serializable]
    public sealed class ConsoleResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public ConsoleData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static ConsoleResult Ok(ConsoleData data)
        {
            return new ConsoleResult
            {
                ok = true,
                status = "completed",
                message = "console logs",
                data = data
            };
        }
    }

    /// <summary>
    /// Console 日志查询的返回数据：totalCount 为命中过滤条件的条目数，
    /// totalEntries 为过滤前的 Console 原始总条数，returnedCount 为实际返回条数。
    /// </summary>
    [Serializable]
    public sealed class ConsoleData
    {
        /// <summary>
        /// 命中 type 与 search 过滤条件的 Console 条目数，不受返回条数上限影响。
        /// </summary>
        public int totalCount;
        /// <summary>
        /// 当前 Console 存储的总条目数，未经过任何 type 或 search 过滤。
        /// </summary>
        public int totalEntries;
        /// <summary>
        /// Stores the returned count value.
        /// </summary>
        public int returnedCount;
        /// <summary>
        /// Stores the type value.
        /// </summary>
        public string type;
        /// <summary>
        /// Stores the search value.
        /// </summary>
        public string search;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
        /// <summary>
        /// Stores the entries value.
        /// </summary>
        public ConsoleLogEntry[] entries;
    }

    /// <summary>
    /// Defines the data structure for an individual console log entry, including details about the log type, message, stack trace, file, line number, mode, and instance ID.
    /// </summary>
    [Serializable]
    public sealed class ConsoleLogEntry
    {
        /// <summary>
        /// Stores the type value.
        /// </summary>
        public string type;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the stack trace value.
        /// </summary>
        public string stackTrace;
        /// <summary>
        /// Stores the file value.
        /// </summary>
        public string file;
        /// <summary>
        /// Stores the line value.
        /// </summary>
        public int line;
        /// <summary>
        /// Stores the mode value.
        /// </summary>
        public int mode;
        /// <summary>
        /// Stores the instance id value.
        /// </summary>
        public int instanceId;
    }

    /// <summary>
    /// Defines the data structure for a response to a game object search request,
    /// including details about the total number of matched objects, the search query, and an array of matching game objects with their components and properties.
    /// </summary>
    [Serializable]
    public sealed class SearchResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public SearchData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static SearchResult Ok(SearchData data)
        {
            return new SearchResult
            {
                ok = true,
                status = "completed",
                message = "gameobject search",
                data = data
            };
        }
    }

    /// <summary>
    /// Defines the data structure for the results of a game object search request,
    /// including details about the total number of matched objects, the search query, and an array of matching game objects with their components and properties.
    /// </summary>
    [Serializable]
    public sealed class SearchData
    {
        /// <summary>
        /// Stores the total matched value.
        /// </summary>
        public int totalMatched;
        /// <summary>
        /// Stores the returned count value.
        /// </summary>
        public int returnedCount;
        /// <summary>
        /// Stores the search value.
        /// </summary>
        public string search;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
        /// <summary>
        /// Stores the objects value.
        /// </summary>
        public SearchGameObject[] objects;
    }

    /// <summary>
    /// Represents a Search Game Object class.
    /// </summary>
    [Serializable]
    public sealed class SearchGameObject
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string name;
        /// <summary>
        /// Stores the path value.
        /// </summary>
        public string path;
        /// <summary>
        /// Stores the scene name value.
        /// </summary>
        public string sceneName;
        /// <summary>
        /// Stores the scene path value.
        /// </summary>
        public string scenePath;
        /// <summary>
        /// Stores the instance id value.
        /// </summary>
        public int instanceId;
        /// <summary>
        /// Stores the tag value.
        /// </summary>
        public string tag;
        /// <summary>
        /// Stores the layer value.
        /// </summary>
        public int layer;
        /// <summary>
        /// Stores the active self value.
        /// </summary>
        public bool activeSelf;
        /// <summary>
        /// Stores the active in hierarchy value.
        /// </summary>
        public bool activeInHierarchy;
        /// <summary>
        /// Stores the components value.
        /// </summary>
        public SearchComponent[] components;
    }

    /// <summary>
    /// Represents a Search Component class.
    /// </summary>
    [Serializable]
    public sealed class SearchComponent
    {
        /// <summary>
        /// Stores the type value.
        /// </summary>
        public string type;
        /// <summary>
        /// Stores the full name value.
        /// </summary>
        public string fullName;
        /// <summary>
        /// Stores the assembly value.
        /// </summary>
        public string assembly;
        /// <summary>
        /// Stores the instance id value.
        /// </summary>
        public int instanceId;
        /// <summary>
        /// Stores the enabled value.
        /// </summary>
        public bool enabled;
        /// <summary>
        /// Stores the has enabled value.
        /// </summary>
        public bool hasEnabled;
        /// <summary>
        /// Stores the transform value.
        /// </summary>
        public SearchTransform transform;
        /// <summary>
        /// Stores the properties value.
        /// </summary>
        public InspectorProperty[] properties;
    }

    /// <summary>
    /// Represents a Search Transform class.
    /// </summary>
    [Serializable]
    public sealed class SearchTransform
    {
        /// <summary>
        /// Stores the local position value.
        /// </summary>
        public SerializedVector3 localPosition;
        /// <summary>
        /// Stores the local rotation value.
        /// </summary>
        public SerializedQuaternion localRotation;
        /// <summary>
        /// Stores the local euler angles value.
        /// </summary>
        public SerializedVector3 localEulerAngles;
        /// <summary>
        /// Stores the local scale value.
        /// </summary>
        public SerializedVector3 localScale;
        /// <summary>
        /// Stores the position value.
        /// </summary>
        public SerializedVector3 position;
        /// <summary>
        /// Stores the rotation value.
        /// </summary>
        public SerializedQuaternion rotation;
        /// <summary>
        /// Stores the euler angles value.
        /// </summary>
        public SerializedVector3 eulerAngles;
        /// <summary>
        /// Stores the lossy scale value.
        /// </summary>
        public SerializedVector3 lossyScale;
    }

    /// <summary>
    /// Represents a Serialized Vector3 class.
    /// </summary>
    [Serializable]
    public sealed class SerializedVector3
    {
        /// <summary>
        /// Stores the x value.
        /// </summary>
        public float x;
        /// <summary>
        /// Stores the y value.
        /// </summary>
        public float y;
        /// <summary>
        /// Stores the z value.
        /// </summary>
        public float z;

        /// <summary>
        /// Converts a Unity Vector3 into its serializable DTO form.
        /// </summary>
        /// <param name="value">The Unity vector to serialize.</param>
        /// <returns>The serialized vector.</returns>
        public static SerializedVector3 From(Vector3 value)
        {
            return new SerializedVector3
            {
                x = value.x,
                y = value.y,
                z = value.z
            };
        }
    }

    /// <summary>
    /// Represents a Serialized Quaternion class.
    /// </summary>
    [Serializable]
    public sealed class SerializedQuaternion
    {
        /// <summary>
        /// Stores the x value.
        /// </summary>
        public float x;
        /// <summary>
        /// Stores the y value.
        /// </summary>
        public float y;
        /// <summary>
        /// Stores the z value.
        /// </summary>
        public float z;
        /// <summary>
        /// Stores the w value.
        /// </summary>
        public float w;

        /// <summary>
        /// Converts a Unity Quaternion into its serializable DTO form.
        /// </summary>
        /// <param name="value">The Unity quaternion to serialize.</param>
        /// <returns>The serialized quaternion.</returns>
        public static SerializedQuaternion From(Quaternion value)
        {
            return new SerializedQuaternion
            {
                x = value.x,
                y = value.y,
                z = value.z,
                w = value.w
            };
        }
    }

    /// <summary>
    /// Represents an Inspector Property class.
    /// </summary>
    [Serializable]
    public sealed class InspectorProperty
    {
        /// <summary>
        /// Stores the path value.
        /// </summary>
        public string path;
        /// <summary>
        /// Stores the display name value.
        /// </summary>
        public string displayName;
        /// <summary>
        /// Stores the type value.
        /// </summary>
        public string type;
        /// <summary>
        /// Stores the value.
        /// </summary>
        public string value;
        /// <summary>
        /// Stores the editable value.
        /// </summary>
        public bool editable;
    }

    /// <summary>
    /// Represents a Modify Result class.
    /// </summary>
    [Serializable]
    public sealed class ModifyResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public ModifyData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static ModifyResult Ok(ModifyData data)
        {
            return new ModifyResult
            {
                ok = true,
                status = "completed",
                message = "component property modified",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Modify Data class.
    /// </summary>
    [Serializable]
    public sealed class ModifyData
    {
        /// <summary>
        /// Stores the asset path value when a prefab asset was modified.
        /// </summary>
        public string assetPath = string.Empty;
        /// <summary>
        /// Stores the instance id value.
        /// </summary>
        public int instanceId;
        /// <summary>
        /// Stores the object path value.
        /// </summary>
        public string objectPath;
        /// <summary>
        /// Stores the component value.
        /// </summary>
        public string component;
        /// <summary>
        /// Stores the property value.
        /// </summary>
        public string property;
        /// <summary>
        /// Stores the property type value.
        /// </summary>
        public string propertyType;
        /// <summary>
        /// Stores the old value.
        /// </summary>
        public string oldValue;
        /// <summary>
        /// Stores the new value.
        /// </summary>
        public string newValue;
    }

    /// <summary>
    /// Represents a Run Result class.
    /// </summary>
    [Serializable]
    public sealed class RunResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public RunData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static RunResult Ok(RunData data)
        {
            return new RunResult
            {
                ok = true,
                status = "completed",
                message = "editor run state changed",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Run Data class.
    /// </summary>
    [Serializable]
    public sealed class RunData
    {
        /// <summary>
        /// Stores the action value.
        /// </summary>
        public string action;
        /// <summary>
        /// Stores the is playing value.
        /// </summary>
        public bool isPlaying;
        /// <summary>
        /// Stores the is paused value.
        /// </summary>
        public bool isPaused;
        /// <summary>
        /// Stores the is playing or will change playmode value.
        /// </summary>
        public bool isPlayingOrWillChangePlaymode;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
    }

    /// <summary>
    /// Represents a Call Result class.
    /// </summary>
    [Serializable]
    public sealed class CallResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public CallData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static CallResult Ok(CallData data)
        {
            return new CallResult
            {
                ok = true,
                status = "completed",
                message = "reflection call completed",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Call Data class.
    /// </summary>
    [Serializable]
    public sealed class CallData
    {
        /// <summary>
        /// Stores the invoke type value.
        /// </summary>
        public string invokeType = string.Empty;
        /// <summary>
        /// Stores the invoke member value.
        /// </summary>
        public string invokeMember = string.Empty;
        /// <summary>
        /// Stores the resolved member value.
        /// </summary>
        public string resolvedMember = string.Empty;
        /// <summary>
        /// Stores the return is void value.
        /// </summary>
        public bool returnIsVoid;
        /// <summary>
        /// Stores the return type value.
        /// </summary>
        public string returnType = string.Empty;
        /// <summary>
        /// Stores the return summary value.
        /// </summary>
        public string returnSummary = string.Empty;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// Represents a Compile Result class.
    /// </summary>
    [Serializable]
    public sealed class CompileResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public CompileData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static CompileResult Ok(CompileData data)
        {
            return new CompileResult
            {
                ok = true,
                status = "completed",
                message = "script compilation requested",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Compile Data class.
    /// </summary>
    [Serializable]
    public sealed class CompileData
    {
        /// <summary>
        /// Stores the waited value.
        /// </summary>
        public bool waited;
        /// <summary>
        /// Stores the is compiling value.
        /// </summary>
        public bool isCompiling;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
    }

    /// <summary>
    /// Represents a Save Result class.
    /// </summary>
    [Serializable]
    public sealed class SaveResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public SaveData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static SaveResult Ok(SaveData data)
        {
            return new SaveResult
            {
                ok = true,
                status = "completed",
                message = "scene/asset save completed",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Save Data class.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>
        /// Stores the scenes saved count value.
        /// </summary>
        public int scenesSavedCount;
        /// <summary>
        /// Stores the scenes skipped count value.
        /// </summary>
        public int scenesSkippedCount;
        /// <summary>
        /// Stores the assets saved value.
        /// </summary>
        public bool assetsSaved;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc;
        /// <summary>
        /// Stores the scenes value.
        /// </summary>
        public SaveSceneEntry[] scenes;
    }

    /// <summary>
    /// Represents a Save Scene Entry class.
    /// </summary>
    [Serializable]
    public sealed class SaveSceneEntry
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string name;
        /// <summary>
        /// Stores the path value.
        /// </summary>
        public string path;
        /// <summary>
        /// Stores the was dirty value.
        /// </summary>
        public bool wasDirty;
        /// <summary>
        /// Stores the saved value.
        /// </summary>
        public bool saved;
        /// <summary>
        /// Stores the skip reason value.
        /// </summary>
        public string skipReason;
    }

    /// <summary>
    /// Represents a Components Result class.
    /// </summary>
    [Serializable]
    public sealed class ComponentsResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public ComponentsData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static ComponentsResult Ok(ComponentsData data)
        {
            return new ComponentsResult
            {
                ok = true,
                status = "completed",
                message = data != null && string.Equals(data.action, "remove", StringComparison.OrdinalIgnoreCase)
                    ? "component removed"
                    : "component added",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents a Components Data class.
    /// </summary>
    [Serializable]
    public sealed class ComponentsData
    {
        /// <summary>
        /// Stores the action value.
        /// </summary>
        public string action = string.Empty;
        /// <summary>
        /// Stores the asset path value when a prefab asset was modified.
        /// </summary>
        public string assetPath = string.Empty;
        /// <summary>
        /// Stores the instance id value.
        /// </summary>
        public int instanceId;
        /// <summary>
        /// Stores the object path value.
        /// </summary>
        public string objectPath = string.Empty;
        /// <summary>
        /// Stores the requested component value.
        /// </summary>
        public string requestedComponent = string.Empty;
        /// <summary>
        /// Stores the resolved component value.
        /// </summary>
        public string resolvedComponent = string.Empty;
        /// <summary>
        /// Stores the component instance id value.
        /// </summary>
        public int componentInstanceId;
        /// <summary>
        /// Stores the added value.
        /// </summary>
        public bool added;
        /// <summary>
        /// Stores the removed value.
        /// </summary>
        public bool removed;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// Represents an Asset Result class.
    /// </summary>
    [Serializable]
    public sealed class AssetResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public AssetData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <param name="message">The human-readable response message.</param>
        /// <returns>A completed bridge response.</returns>
        public static AssetResult Ok(AssetData data, string message)
        {
            return new AssetResult
            {
                ok = true,
                status = "completed",
                message = string.IsNullOrEmpty(message) ? "asset command completed" : message,
                data = data
            };
        }
    }

    /// <summary>
    /// Represents an Asset Data class.
    /// </summary>
    [Serializable]
    public sealed class AssetData
    {
        /// <summary>
        /// Stores the action value.
        /// </summary>
        public string action = string.Empty;
        /// <summary>
        /// Stores the src path value.
        /// </summary>
        public string srcPath = string.Empty;
        /// <summary>
        /// Stores the dst path value.
        /// </summary>
        public string dstPath = string.Empty;
        /// <summary>
        /// Stores the filter value.
        /// </summary>
        public string filter = string.Empty;
        /// <summary>
        /// Stores the total matched value.
        /// </summary>
        public int totalMatched;
        /// <summary>
        /// Stores the returned count value.
        /// </summary>
        public int returnedCount;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc = string.Empty;
        /// <summary>
        /// Asset entries returned by the asset command.
        /// </summary>
        public AssetEntry[] assets = Array.Empty<AssetEntry>();
    }

    /// <summary>
    /// Represents an Asset Entry class.
    /// </summary>
    [Serializable]
    public sealed class AssetEntry
    {
        /// <summary>
        /// Stores the guid value.
        /// </summary>
        public string guid = string.Empty;
        /// <summary>
        /// Stores the path value.
        /// </summary>
        public string path = string.Empty;
        /// <summary>
        /// Stores the main asset type value.
        /// </summary>
        public string mainAssetType = string.Empty;
        /// <summary>
        /// Stores the is folder value.
        /// </summary>
        public bool isFolder;
    }

    /// <summary>
    /// Represents an Ugui Result class.
    /// </summary>
    [Serializable]
    public sealed class UguiResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public UguiData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful bridge response using the supplied data payload.
        /// </summary>
        /// <param name="data">The response data payload.</param>
        /// <returns>A completed bridge response.</returns>
        public static UguiResult Ok(UguiData data)
        {
            return new UguiResult
            {
                ok = true,
                status = data.dryRun ? "validated" : "completed",
                message = data.message,
                data = data
            };
        }
    }

    /// <summary>
    /// Represents an Ugui Data class.
    /// </summary>
    [Serializable]
    public sealed class UguiData
    {
        /// <summary>
        /// Stores the view name value.
        /// </summary>
        public string uiName = string.Empty;
        /// <summary>
        /// Stores the json file value.
        /// </summary>
        public string jsonFile = string.Empty;
        /// <summary>
        /// Stores the asset manifest path value.
        /// </summary>
        public string assetManifestPath = string.Empty;
        /// <summary>
        /// Stores the prefab path value.
        /// </summary>
        public string prefabPath = string.Empty;
        /// <summary>
        /// Stores the assets folder value.
        /// </summary>
        public string assetsFolder = string.Empty;
        /// <summary>
        /// Stores the dry run value.
        /// </summary>
        public bool dryRun;
        /// <summary>
        /// Stores the scene instance replaced value.
        /// </summary>
        public bool sceneInstanceReplaced;
        /// <summary>
        /// Stores the node count value.
        /// </summary>
        public int nodeCount;
        /// <summary>
        /// Stores the assets copied value.
        /// </summary>
        public int assetsCopied;
        /// <summary>
        /// Stores the sprites referenced value.
        /// </summary>
        public int spritesReferenced;
        /// <summary>
        /// Validation issues found during the UGUI workflow.
        /// </summary>
        public string[] issues = Array.Empty<string>();
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message = string.Empty;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// Represents a particle prefab command result.
    /// </summary>
    [Serializable]
    public sealed class ParticleResult
    {
        public bool ok;
        public string status;
        public string message;
        public ParticleData data;
        public PingError error = null;

        public static ParticleResult Ok(ParticleData data)
        {
            return new ParticleResult
            {
                ok = true,
                status = data.dryRun ? "validated" : "completed",
                message = data.message,
                data = data
            };
        }
    }

    /// <summary>
    /// Describes the prefab and scene changes made by the particle command.
    /// </summary>
    [Serializable]
    public sealed class ParticleData
    {
        public string action = string.Empty;
        public string particleName = string.Empty;
        public string prefabPath = string.Empty;
        public string materialPath = string.Empty;
        public string texturePath = string.Empty;
        public string shape = string.Empty;
        public string startColor = string.Empty;
        public float duration;
        public bool loop;
        public float startLifetime;
        public float startSpeed;
        public float startSize;
        public float emissionRate;
        public int maxParticles;
        public bool dryRun;
        public bool prefabCreated;
        public bool materialCreated;
        public bool sceneInstanceReplaced;
        public bool sceneInstanceRemoved;
        public string objectPath = string.Empty;
        public string[] issues = Array.Empty<string>();
        public string message = string.Empty;
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// Represents a prefab composition command result.
    /// </summary>
    [Serializable]
    public sealed class PrefabResult
    {
        public bool ok;
        public string status;
        public string message;
        public PrefabData data;
        public PingError error = null;

        public static PrefabResult Ok(PrefabData data)
        {
            var isInspection = string.Equals(data.action, "inspect", StringComparison.Ordinal);
            var isValid = !isInspection || data.issues == null || data.issues.Length == 0;
            return new PrefabResult
            {
                ok = isValid,
                status = isInspection ? (isValid ? "inspected" : "invalid") : (data.dryRun ? "validated" : "completed"),
                message = data.message,
                data = data,
                error = isValid ? null : new PingError
                {
                    code = "prefab_validation_failed",
                    message = "Prefab inspection found " + data.issues.Length + " issue(s)."
                }
            };
        }
    }

    /// <summary>
    /// Describes the nested prefab attachment performed by the prefab command.
    /// </summary>
    [Serializable]
    public sealed class PrefabData
    {
        public string buildReportPath = string.Empty;
        public bool whitebox;
        public string deferredBindingsJson = "[]";
        public string textureSettingsJson = "[]";
        public string action = string.Empty;
        /// <summary>
        /// 已检查的 Prefab 资产路径。
        /// </summary>
        public string prefabPath = string.Empty;
        /// <summary>
        /// Prefab 检查档案。
        /// </summary>
        public string profile = string.Empty;
        public string parentPrefabPath = string.Empty;
        public string parentObjectPath = string.Empty;
        public string childPrefabPath = string.Empty;
        public string childName = string.Empty;
        /// <summary>
        /// group 动作新建的容器节点名。
        /// </summary>
        public string groupName = string.Empty;
        /// <summary>
        /// group 动作迁入容器的子节点名摘要（逗号分隔）。
        /// </summary>
        public string movedChildren = string.Empty;
        /// <summary>
        /// group 动作容器 RectTransform 参数摘要。
        /// </summary>
        public string groupRect = string.Empty;
        /// <summary>
        /// group 动作容器插入的兄弟序号，-1 表示自动。
        /// </summary>
        public int groupSiblingIndex = -1;
        /// <summary>
        /// group 动作实际迁入的子节点数量。
        /// </summary>
        public int movedChildCount;
        public string sourcePrefabPath = string.Empty;
        public string outputPrefabPath = string.Empty;
        public string scenePath = string.Empty;
        public string sceneRootName = string.Empty;
        public string localPosition = string.Empty;
        public string localRotation = string.Empty;
        public string localScale = string.Empty;
        public string layerName = string.Empty;
        public string layerMode = string.Empty;
        public bool replaceExistingChild;
        /// <summary>
        /// 是否递归检查子节点。
        /// </summary>
        public bool recursive;
        /// <summary>
        /// Prefab 检查是否通过。
        /// </summary>
        public bool valid = true;
        public bool dryRun;
        public bool prefabModified;
        public bool sceneInstanceReplaced;
        public int missingScriptRemovedCount;
        public int monoBehaviourRemovedCount;
        public int materialRepairedCount;
        public int prefabUnpackedCount;
        /// <summary>
        /// 已检查的层级节点数量。
        /// </summary>
        public int nodeCount;
        /// <summary>
        /// 已检查的组件数量。
        /// </summary>
        public int componentCount;
        /// <summary>
        /// 已解析的对象引用数量。
        /// </summary>
        public int objectReferenceCount;
        /// <summary>
        /// 缺失 MonoBehaviour 脚本数量。
        /// </summary>
        public int missingScriptCount;
        /// <summary>
        /// 缺失序列化对象引用数量。
        /// </summary>
        public int missingReferenceCount;
        /// <summary>
        /// 已检查的层级节点。
        /// </summary>
        public PrefabNodeData[] nodes = Array.Empty<PrefabNodeData>();
        public string[] issues = Array.Empty<string>();
        public string message = string.Empty;
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// 描述 Prefab 检查返回的单个层级节点。
    /// </summary>
    [Serializable]
    public sealed class PrefabNodeData
    {
        /// <summary>
        /// 相对于 Prefab 根节点的层级路径。
        /// </summary>
        public string path = string.Empty;
        /// <summary>
        /// 节点自身是否激活。
        /// </summary>
        public bool activeSelf;
        /// <summary>
        /// 节点 Layer 索引。
        /// </summary>
        public int layer;
        /// <summary>
        /// 节点 Layer 名称。
        /// </summary>
        public string layerName = string.Empty;
        /// <summary>
        /// 格式化后的 RectTransform 布局。
        /// </summary>
        public string rectTransform = string.Empty;
        /// <summary>
        /// 当前节点缺失的脚本数量。
        /// </summary>
        public int missingScriptCount;
        /// <summary>
        /// 当前节点挂载的组件。
        /// </summary>
        public PrefabComponentData[] components = Array.Empty<PrefabComponentData>();
    }

    /// <summary>
    /// 描述单个组件及其序列化对象引用。
    /// </summary>
    [Serializable]
    public sealed class PrefabComponentData
    {
        /// <summary>
        /// 组件 CLR 类型名称。
        /// </summary>
        public string type = string.Empty;
        /// <summary>
        /// 组件上的序列化对象引用。
        /// </summary>
        public PrefabObjectReferenceData[] objectReferences = Array.Empty<PrefabObjectReferenceData>();
    }

    /// <summary>
    /// 描述 Prefab 检查发现的单个 Unity 序列化对象引用。
    /// </summary>
    [Serializable]
    public sealed class PrefabObjectReferenceData
    {
        /// <summary>
        /// 序列化属性路径。
        /// </summary>
        public string propertyPath = string.Empty;
        /// <summary>
        /// 被引用对象的显示路径。
        /// </summary>
        public string target = string.Empty;
        /// <summary>
        /// 被引用对象的资产路径。
        /// </summary>
        public string assetPath = string.Empty;
        /// <summary>
        /// 序列化引用是否缺失。
        /// </summary>
        public bool missing;
    }

    /// <summary>
    /// 表示 Prefab 现有 Metadata 的校验结果。
    /// </summary>
    [Serializable]
    public sealed class WidgetMetadataValidationResult
    {
        /// <summary>
        /// 校验是否通过。
        /// </summary>
        public bool ok;
        /// <summary>
        /// 校验状态。
        /// </summary>
        public string status = string.Empty;
        /// <summary>
        /// 校验摘要。
        /// </summary>
        public string message = string.Empty;
        /// <summary>
        /// 详细校验数据。
        /// </summary>
        public WidgetMetadataValidationData data;
        /// <summary>
        /// 校验失败详情。
        /// </summary>
        public PingError error;

        /// <summary>
        /// 根据问题列表创建对应成功状态的响应。
        /// </summary>
        /// <param name="data">已完成的校验数据</param>
        /// <returns>可序列化的校验结果</returns>
        public static WidgetMetadataValidationResult Create(WidgetMetadataValidationData data)
        {
            var issueCount = data.issues == null ? 0 : data.issues.Length;
            var isValid = issueCount == 0;
            return new WidgetMetadataValidationResult
            {
                ok = isValid,
                status = isValid ? "validated" : "invalid",
                message = isValid ? "widget metadata validation passed" : "widget metadata validation found issues",
                data = data,
                error = isValid ? null : new PingError
                {
                    code = "widget_metadata_validation_failed",
                    message = "Widget metadata validation found " + issueCount + " issue(s)."
                }
            };
        }
    }

    /// <summary>
    /// 描述 Metadata 预期身份、实际身份和绑定健康状态。
    /// </summary>
    [Serializable]
    public sealed class WidgetMetadataValidationData
    {
        /// <summary>
        /// 已校验的 Prefab 路径。
        /// </summary>
        public string prefabPath = string.Empty;
        /// <summary>
        /// 请求的 UI 类型名称。
        /// </summary>
        public string uiName = string.Empty;
        /// <summary>
        /// 预期的 VM 类型名称。
        /// </summary>
        public string vmName = string.Empty;
        /// <summary>
        /// 预期的 Metadata 类名。
        /// </summary>
        public string expectedClassName = string.Empty;
        /// <summary>
        /// 预期的 VM MonoScript GUID。
        /// </summary>
        public string expectedClassGuid = string.Empty;
        /// <summary>
        /// 预期的 VM 脚本资产路径。
        /// </summary>
        public string expectedScriptPath = string.Empty;
        /// <summary>
        /// Metadata 中序列化的类名。
        /// </summary>
        public string actualClassName = string.Empty;
        /// <summary>
        /// Metadata 中序列化的类 GUID。
        /// </summary>
        public string actualClassGuid = string.Empty;
        /// <summary>
        /// 从序列化 GUID 解析出的资产路径。
        /// </summary>
        public string actualScriptPath = string.Empty;
        /// <summary>
        /// 从序列化 GUID 解析出的编译类型名称。
        /// </summary>
        public string actualScriptClassName = string.Empty;
        /// <summary>
        /// Prefab 层级中的 Metadata 组件数量。
        /// </summary>
        public int metadataComponentCount;
        /// <summary>
        /// 根 Metadata 的字段数量。
        /// </summary>
        public int metadataCount;
        /// <summary>
        /// 非空 Metadata Target 和 Component 的总数。
        /// </summary>
        public int boundTargetCount;
        /// <summary>
        /// 空或缺失的 Metadata 引用数量。
        /// </summary>
        public int missingReferenceCount;
        /// <summary>
        /// 语义校验问题列表。
        /// </summary>
        public string[] issues = Array.Empty<string>();
        /// <summary>
        /// UTC 响应时间戳。
        /// </summary>
        public string respondedAtUtc = string.Empty;
    }

    /// <summary>
    /// Represents a screenshot command result.
    /// </summary>
    [Serializable]
    public sealed class ScreenshotResult
    {
        /// <summary>
        /// Stores the ok value.
        /// </summary>
        public bool ok;
        /// <summary>
        /// Stores the status value.
        /// </summary>
        public string status;
        /// <summary>
        /// Stores the message value.
        /// </summary>
        public string message;
        /// <summary>
        /// Stores the data value.
        /// </summary>
        public ScreenshotData data;
        /// <summary>
        /// Stores the error value.
        /// </summary>
        public PingError error = null;

        /// <summary>
        /// Creates a successful screenshot result.
        /// </summary>
        /// <param name="data">The screenshot data.</param>
        /// <returns>The successful screenshot result.</returns>
        public static ScreenshotResult Ok(ScreenshotData data)
        {
            return new ScreenshotResult
            {
                ok = true,
                status = "completed",
                message = "game view screenshot captured",
                data = data
            };
        }
    }

    /// <summary>
    /// Represents metadata for a captured Game view screenshot.
    /// </summary>
    [Serializable]
    public sealed class ScreenshotData
    {
        /// <summary>
        /// Stores the absolute output path.
        /// </summary>
        public string path = string.Empty;
        /// <summary>
        /// Stores the project-root-relative output path.
        /// </summary>
        public string projectRelativePath = string.Empty;
        /// <summary>
        /// Stores the output directory.
        /// </summary>
        public string directory = string.Empty;
        /// <summary>
        /// Stores the output file name.
        /// </summary>
        public string fileName = string.Empty;
        /// <summary>
        /// Stores the screenshot width in pixels.
        /// </summary>
        public int width;
        /// <summary>
        /// Stores the screenshot height in pixels.
        /// </summary>
        public int height;
        /// <summary>
        /// Stores the PNG byte count.
        /// </summary>
        public long bytes;
        /// <summary>
        /// Stores the supersize multiplier.
        /// </summary>
        public int superSize;
        /// <summary>
        /// Stores the capture implementation used.
        /// </summary>
        public string captureMethod = string.Empty;
        /// <summary>
        /// Stores the responded at utc value.
        /// </summary>
        public string respondedAtUtc = string.Empty;
    }

    [Serializable]
    public sealed class AnimatorResult
    {
        public bool ok;
        public string status;
        public string message;
        public AnimatorData data;
        public PingError error = null;

        public static AnimatorResult Ok(AnimatorData data)
        {
            return new AnimatorResult
            {
                ok = true,
                status = data.dryRun ? "validated" : "completed",
                message = data.message,
                data = data
            };
        }
    }

    [Serializable]
    public sealed class AnimatorData
    {
        public string action = string.Empty;
        public string operation = string.Empty;
        public string controllerPath = string.Empty;
        public string clipPath = string.Empty;
        public string objectPath = string.Empty;
        public string layer = string.Empty;
        public string stateName = string.Empty;
        public string parameterName = string.Empty;
        public string parameterType = string.Empty;
        public string fromState = string.Empty;
        public string toState = string.Empty;
        public int parameterCount;
        public int layerCount;
        public int stateCount;
        public int transitionCount;
        public int curveCount;
        public bool dryRun;
        public bool assetModified;
        public bool sceneModified;
        public string[] details = Array.Empty<string>();
        public string[] issues = Array.Empty<string>();
        public string message = string.Empty;
        public string respondedAtUtc = string.Empty;
    }

    [Serializable]
    public sealed class ExcelResult
    {
        public bool ok;
        public string status;
        public string message;
        public ExcelData data;
        public PingError error = null;

        public static ExcelResult Ok(ExcelData data, string message)
        {
            return new ExcelResult
            {
                ok = true,
                status = data.dryRun ? "validated" : "completed",
                message = string.IsNullOrEmpty(message) ? "excel command completed" : message,
                data = data
            };
        }
    }

    [Serializable]
    public sealed class ExcelData
    {
        public string action = string.Empty;
        public string filePath = string.Empty;
        public string sheet = string.Empty;
        public int headerRow;
        public int totalMatched;
        public int returnedCount;
        public bool dryRun;
        public bool modified;
        public string respondedAtUtc = string.Empty;
        public ExcelTableEntry[] tables = Array.Empty<ExcelTableEntry>();
        public ExcelFieldEntry[] fields = Array.Empty<ExcelFieldEntry>();
        public ExcelRowEntry[] rows = Array.Empty<ExcelRowEntry>();
        public ExcelCellEntry cell;
        public ExcelIssueEntry[] issues = Array.Empty<ExcelIssueEntry>();
    }

    [Serializable]
    public sealed class ExcelTableEntry
    {
        public string name = string.Empty;
        public string filePath = string.Empty;
        public string sheet = string.Empty;
    }

    [Serializable]
    public sealed class ExcelFieldEntry
    {
        public string name = string.Empty;
        public string column = string.Empty;
        public int index;
    }

    [Serializable]
    public sealed class ExcelRowEntry
    {
        public int excelRow;
        public int dataIndex;
        public ExcelCellEntry[] cells = Array.Empty<ExcelCellEntry>();
    }

    [Serializable]
    public sealed class ExcelCellEntry
    {
        public string address = string.Empty;
        public string column = string.Empty;
        public string field = string.Empty;
        public string value = string.Empty;
    }

    [Serializable]
    public sealed class ExcelIssueEntry
    {
        public string severity = string.Empty;
        public string message = string.Empty;
        public int row;
        public string field = string.Empty;
    }

    [Serializable]
    public sealed class TerrainResult
    {
        public bool ok;
        public string status;
        public string message;
        public TerrainCommandData data;
        public PingError error = null;

        public static TerrainResult Ok(TerrainCommandData data)
        {
            return new TerrainResult
            {
                ok = true,
                status = data.dryRun ? "validated" : "completed",
                message = data.message,
                data = data
            };
        }
    }

    [Serializable]
    public sealed class TerrainCommandData
    {
        public string action = string.Empty;
        public string scenePath = string.Empty;
        public string objectPath = string.Empty;
        public string dataPath = string.Empty;
        public string layerPath = string.Empty;
        public string texturePath = string.Empty;
        public bool dryRun;
        public bool assetModified;
        public bool sceneModified;
        public bool sceneSaved;
        public int terrainCount;
        public int layerCount;
        public TerrainEntry[] terrains = Array.Empty<TerrainEntry>();
        public TerrainLayerEntry[] layers = Array.Empty<TerrainLayerEntry>();
        public string[] details = Array.Empty<string>();
        public string message = string.Empty;
        public string respondedAtUtc = string.Empty;
    }

    [Serializable]
    public sealed class TerrainEntry
    {
        public string objectPath = string.Empty;
        public string dataPath = string.Empty;
        public string size = string.Empty;
        public int heightmapResolution;
        public int alphamapResolution;
        public int baseMapResolution;
        public int layerCount;
        public TerrainLayerEntry[] layers = Array.Empty<TerrainLayerEntry>();
    }

    [Serializable]
    public sealed class TerrainLayerEntry
    {
        public int index;
        public string path = string.Empty;
        public string name = string.Empty;
        public string texturePath = string.Empty;
        public string normalPath = string.Empty;
        public string tileSize = string.Empty;
        public string tileOffset = string.Empty;
        public float metallic;
        public float smoothness;
    }

}
