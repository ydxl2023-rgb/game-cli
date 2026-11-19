using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Oathx.Entity;
using UnityEngine;

/// <summary>Owns validated device preferences independently of the settings page.</summary>
[Initialize]
public class SettingsEntity : XEntity
{
    /// <summary>Validated local values, never exposed for external mutation.</summary>
    private readonly Dictionary<SettingsOption, float> preferences = new Dictionary<SettingsOption, float>();

    /// <summary>Independent read-only snapshot for persistence and product adapters.</summary>
    public IReadOnlyDictionary<SettingsOption, float> values
    {
        get { return new ReadOnlyDictionary<SettingsOption, float>(new Dictionary<SettingsOption, float>(preferences)); }
    }

    /// <summary>Preset derived from all quality options, not from button state.</summary>
    public SettingsQuality qualityPreset
    {
        get
        {
            var quality = ReadQuality(SettingsOption.Resolution);
            foreach (SettingsOption option in Enum.GetValues(typeof(SettingsOption)))
            {
                if (IsQuality(option) && ReadQuality(option) != PresetQuality(option, quality))
                    return SettingsQuality.Custom;
            }

            return quality;
        }
    }

    /// <summary>Restore known preferences and replace malformed persisted data with defaults.</summary>
    public SettingsEntity()
    {
        foreach (SettingsOption option in Enum.GetValues(typeof(SettingsOption)))
            preferences.Add(option, DefaultValue(option));

        string json = PlayerPrefs.GetString(StorageKey.For<SettingsSystem, SettingsEntity>(entity => entity.values), string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var saved = JsonConvert.DeserializeObject<Dictionary<string, float>>(json);
                if (saved != null)
                {
                    foreach (var pair in saved)
                    {
                        if (Enum.TryParse(pair.Key, out SettingsOption option) && IsValid(option, pair.Value))
                            preferences[option] = pair.Value;
                    }
                }
            }
            catch (JsonException)
            {
                Debug.LogWarning("Settings preferences could not be restored; defaults are active.");
            }
        }
    }

    /// <summary>Check a supported boolean preference.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True only when the switch is enabled.</returns>
    public bool IsEnabled(SettingsOption option)
    {
        return IsSwitch(option) && preferences[option] == 1f;
    }

    /// <summary>Read a quality setting without converting it into display text.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>The selected tier, or Custom for a non-quality option.</returns>
    public SettingsQuality ReadQuality(SettingsOption option)
    {
        return IsQuality(option) ? (SettingsQuality)(int)preferences[option] : SettingsQuality.Custom;
    }

    /// <summary>Read the saved unmuted channel level.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>A normalized volume, or zero for an unrelated option.</returns>
    public float ReadVolume(SettingsOption option)
    {
        return IsVolume(option) ? preferences[option] : 0f;
    }

    /// <summary>Toggle a local preference and persist it atomically.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True when a supported switch changed.</returns>
    public bool TogglePreference(SettingsOption option)
    {
        if (!IsSwitch(option))
            return false;

        preferences[option] = IsEnabled(option) ? 0f : 1f;

        Save();

        return true;
    }

    /// <summary>Select a download policy without starting a download or requesting permission.</summary>
    /// <param name="wifiOnly">Whether downloads should use Wi-Fi only.</param>
    /// <returns>True when the local policy changed.</returns>
    public bool SelectNetwork(bool wifiOnly)
    {
        if (IsEnabled(SettingsOption.WifiOnly) == wifiOnly)
            return false;

        preferences[SettingsOption.WifiOnly] = wifiOnly ? 1f : 0f;

        Save();

        return true;
    }

    /// <summary>Select and save a validated quality tier.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="quality">Requested graphics tier.</param>
    /// <returns>True when the requested option changed.</returns>
    public bool SelectQuality(SettingsOption option, SettingsQuality quality)
    {
        if (!IsQuality(option) || !IsValid(option, (float)quality) || ReadQuality(option) == quality)
            return false;

        preferences[option] = (float)quality;

        Save();

        return true;
    }

    /// <summary>Apply one tier to every local graphics preference in one save.</summary>
    /// <param name="quality">Requested graphics tier.</param>
    /// <returns>True when a supported preset changed the settings.</returns>
    public bool SelectPreset(SettingsQuality quality)
    {
        if (quality < SettingsQuality.Low || quality > SettingsQuality.High || qualityPreset == quality)
            return false;

        foreach (SettingsOption option in Enum.GetValues(typeof(SettingsOption)))
        {
            if (IsQuality(option))
                preferences[option] = (float)PresetQuality(option, quality);
        }

        Save();

        return true;
    }

    /// <summary>Change one channel level, preserving its independent mute preference.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="volume">Requested normalized channel volume.</param>
    /// <returns>True when a finite volume changed.</returns>
    public bool ChangeVolume(SettingsOption option, float volume)
    {
        if (!IsVolume(option) || float.IsNaN(volume) || float.IsInfinity(volume))
            return false;

        float normalized = Mathf.Clamp01(volume);
        if (Mathf.Approximately(preferences[option], normalized))
            return false;

        preferences[option] = normalized;

        Save();

        return true;
    }

    /// <summary>Classify the original quality options.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True for a graphics tier.</returns>
    private bool IsQuality(SettingsOption option)
    {
        return option == SettingsOption.Resolution || option == SettingsOption.FrameRate
            || option == SettingsOption.Reflection || option == SettingsOption.Scene
            || option == SettingsOption.Texture || option == SettingsOption.Volume
            || option == SettingsOption.AntiAliasing || option == SettingsOption.Model;
    }

    /// <summary>Classify the three independently muted sound channels.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True for a volume option.</returns>
    private bool IsVolume(SettingsOption option)
    {
        return option == SettingsOption.MusicVolume || option == SettingsOption.SoundVolume || option == SettingsOption.VoiceVolume;
    }

    /// <summary>Reject undefined values before treating an option as a switch.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>True for a known boolean option.</returns>
    private bool IsSwitch(SettingsOption option)
    {
        return Enum.IsDefined(typeof(SettingsOption), option) && !IsQuality(option) && !IsVolume(option);
    }

    /// <summary>Validate persisted values without trusting their serialized enum names.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="value">Persisted value to validate.</param>
    /// <returns>True for a finite supported option value.</returns>
    private bool IsValid(SettingsOption option, float value)
    {
        if (!Enum.IsDefined(typeof(SettingsOption), option) || float.IsNaN(value) || float.IsInfinity(value))
            return false;

        if (option == SettingsOption.Model && value == (float)SettingsQuality.Medium)
            return false;

        return IsQuality(option) ? value >= 0f && value <= 2f && value == Mathf.Floor(value)
            : IsVolume(option) ? value >= 0f && value <= 1f : value == 0f || value == 1f;
    }

    /// <summary>Map the two-level model control onto the three graphics presets.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <param name="quality">Requested graphics tier.</param>
    /// <returns>The tier supported by the original control.</returns>
    private SettingsQuality PresetQuality(SettingsOption option, SettingsQuality quality)
    {
        return option == SettingsOption.Model && quality == SettingsQuality.Medium ? SettingsQuality.Low : quality;
    }

    /// <summary>Provide explicit local defaults rather than inferred service state.</summary>
    /// <param name="option">Validated preference identifier.</param>
    /// <returns>The default preference value.</returns>
    private float DefaultValue(SettingsOption option)
    {
        if (IsQuality(option))
            return (float)SettingsQuality.High;

        return option == SettingsOption.ShowFps || option == SettingsOption.NightPush
            || option == SettingsOption.ChatWorld || option == SettingsOption.ChatAssociation ? 0f : 1f;
    }

    /// <summary>Persist only this entity's preferences using the framework key contract.</summary>
    private void Save()
    {
        PlayerPrefs.SetString(StorageKey.For<SettingsSystem, SettingsEntity>(entity => entity.values), JsonConvert.SerializeObject(preferences));
        PlayerPrefs.Save();
    }
}
