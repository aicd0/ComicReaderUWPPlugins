// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using ComicReaderUWP.SDK.Models;

namespace DuplicationDetection;

internal class ImageInfoDatabase(IRegistryDatabase database)
{
    public bool TryGetImageInfo(string key, [NotNullWhen(true)] out ImageInfo? imageInfo)
    {
        IRegistryKey regKey = database.CreateKey(RegistryNames.IMAGE_INFO);
        if (!regKey.TryGet<string>(key, out string? json))
        {
            imageInfo = null;
            return false;
        }

        try
        {
            imageInfo = JsonSerializer.Deserialize<ImageInfo>(json);
        }
        catch (JsonException)
        {
            imageInfo = null;
            return false;
        }

        if (imageInfo is null)
        {
            return false;
        }

        return true;
    }

    public void SetImageInfo(string key, ImageInfo imageInfo)
    {
        IRegistryKey regKey = database.CreateKey(RegistryNames.IMAGE_INFO);
        string json = JsonSerializer.Serialize(imageInfo);
        regKey.Set(key, json);
    }

    public void RemoveImageInfo(string key)
    {
        IRegistryKey regKey = database.CreateKey(RegistryNames.IMAGE_INFO);
        regKey.Remove(key);
    }

    public class ImageInfo
    {
        public required string Signature { get; set; }
        public string PHash { get; set; } = string.Empty;
    }

    private class ImageInfoJsonModel
    {
        [JsonPropertyName("Signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("PHash")]
        public string? PHash { get; set; }
    }
}
