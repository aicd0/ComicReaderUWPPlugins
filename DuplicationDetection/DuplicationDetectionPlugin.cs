// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Models;
using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.UI;
using ComicReaderUWP.SDK.Plugins.UI.Menu;

using DuplicationDetection.Utils;

using Microsoft.UI.Xaml.Controls;

namespace DuplicationDetection;

public class DuplicationDetectionPlugin : IPlugin
{
    string IPlugin.Name => "DuplicationDetection";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Description => "";

    IconSource? IPlugin.Icon => null;

    string IPlugin.Version => "1.0.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
        context.ComicMenuItemCreator = new MainPageMoreMenuItemCreator(this);
    }

    private async Task DetectDuplication(IComicModel comic)
    {
        using IComicConnection? connection = await comic.Open();
        if (connection is null)
        {
            DialogOptions options = new DialogOptions.Builder()
                .SetTitle("Error")
                .SetContent("Failed to open the comic.")
                .SetPrimaryButtonText("OK")
                .Build();
            await PluginService.PluginContext.EnqueueDialog(options);
            return;
        }

        ImageInfoDatabase database = new(PluginService.PluginContext.RegistryDatabase);
        CoenM.ImageHash.HashAlgorithms.PerceptualHash algorithm = new();
        int imageCount = connection.ImageCount;
        List<ImageModel> detectingImages = [];
        List<ImageModel> errorImages = [];

        for (int i = 0; i < imageCount; i++)
        {
            string imageKey = $"{comic.Id}#{i}";
            string signature = connection.GetImageSignature(i);
            string imageName = connection.GetImageName(i);
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = $"Page {i + 1}";
            }

            ImageModel imageModel = new() { Name = imageName };

            if (comic.Id >= 0 &&
                database.TryGetImageInfo(imageKey, out ImageInfoDatabase.ImageInfo? info) &&
                info.Signature == signature &&
                ulong.TryParse(info.PHash, System.Globalization.NumberStyles.HexNumber, null, out ulong pHash))
            {
                imageModel.PHash = pHash;
                detectingImages.Add(imageModel);
                continue;
            }

            using Stream? imageStream = connection.OpenImageStream(i);
            if (imageStream is null)
            {
                errorImages.Add(imageModel);
                continue;
            }

            try
            {
                using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imageStream);
                pHash = algorithm.Hash(image);
            }
            catch (Exception)
            {
                errorImages.Add(imageModel);
                continue;
            }

            imageModel.PHash = pHash;
            detectingImages.Add(imageModel);

            if (comic.Id >= 0)
            {
                info = new()
                {
                    Signature = signature,
                    PHash = pHash.ToString("X")
                };
                database.SetImageInfo(imageKey, info);
            }
        }

        PriorityQueue<Tuple<ImageModel, ImageModel>, double> similarities = new();
        for (int i = 0; i < detectingImages.Count; i++)
        {
            for (int j = i + 1; j < detectingImages.Count; j++)
            {
                ImageModel image1 = detectingImages[i];
                ImageModel image2 = detectingImages[j];
                double similarity = CalculateSimilarity(image1, image2);

                if (similarities.Count < 100)
                {
                    similarities.Enqueue(new(image1, image2), similarity);
                    continue;
                }

                if (similarities.TryPeek(out _, out double minSimilarity) && similarity < minSimilarity)
                {
                    continue;
                }

                similarities.EnqueueDequeue(new(image1, image2), similarity);
            }
        }

        var top100 = similarities.UnorderedItems
            .OrderByDescending(x => x.Priority)
            .ToList();

        {
            StringBuilder resultBuilder = new();
            if (errorImages.Count > 0)
            {
                resultBuilder.AppendLine("Error images:");
                foreach (ImageModel item in errorImages)
                {
                    resultBuilder.AppendLine(item.Name);
                }
            }

            resultBuilder.AppendLine("Most similar images:");
            foreach ((Tuple<ImageModel, ImageModel> element, double priority) in top100)
            {
                resultBuilder.AppendLine($"{element.Item1.Name}, {element.Item2.Name} ({priority})");
            }

            string result = resultBuilder.ToString();
            DialogOptions options = new DialogOptions.Builder()
                .SetTitle("Detection result")
                .SetContent(result)
                .SetPrimaryButtonText("OK")
                .Build();
            await PluginService.PluginContext.EnqueueDialog(options);
        }
    }

    private static double CalculateSimilarity(ImageModel info1, ImageModel info2)
    {
        return CoenM.ImageHash.CompareHash.Similarity(info1.PHash, info2.PHash);
    }

    private class ImageModel
    {
        public required string Name { get; set; }
        public ulong PHash { get; set; }
    }

    private class MainPageMoreMenuItemCreator(DuplicationDetectionPlugin plugin) : IComicMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IWindowContext windowContext, IComicModel primary, IEnumerable<IComicModel> selection)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Detect duplication",
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.WithBusyState(async () =>
                    {
                        await plugin.DetectDuplication(primary);
                    })),
                }
            ];
        }
    }
}
