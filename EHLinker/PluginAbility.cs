// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using EHLinker.UI;
using EHLinker.UI.Models;

using HtmlAgilityPack;

namespace EHLinker;

internal partial class PluginAbility : IPluginAbility
{
    private readonly object _clientLock = new();
    private ClassicHttpClient? _client;

    public async Task<ComicSearchResult> SearchComicsByKeyword(string keyword)
    {
        string url = $"https://e-hentai.org/?f_search={Uri.EscapeDataString(keyword)}";
        ClassicHttpClient client = await GetClient();
        string html = await client.GetHtmlAsync(url);
        return ParseComicSearchResult(html);
    }

    public async Task<ComicSearchResult> SearchComicsByLink(string link)
    {
        ClassicHttpClient client = await GetClient();
        string html = await client.GetHtmlAsync(link);
        return ParseComicSearchResult(html);
    }

    public async Task<ComicDetailedInfo> RequestComicInfo(string link)
    {
        ClassicHttpClient client = await GetClient();
        string html = await client.GetHtmlAsync(link);
        return ParseComicInfo(html);
    }

    private async Task<ClassicHttpClient> GetClient()
    {
        ClassicHttpClient? client = _client;
        if (client is not null)
        {
            return client;
        }

        lock (_clientLock)
        {
            client = _client;
            if (client is not null)
            {
                return client;
            }

            client = new();
            _client = client;
        }

        return client;
    }

    private static ComicSearchResult ParseComicSearchResult(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode node1 = doc.DocumentNode.SelectSingleNode("/html/body/div[@class='ido']/div[@style='position:relative; z-index:2']");
        HtmlNode tableNode = node1.SelectSingleNode("./table[@class='itg gltc']");
        HtmlNode searchnavNode = node1.SelectSingleNode("./div[@class='searchnav']");
        HtmlNode previousPageNode = searchnavNode.SelectSingleNode(".//*[@id='uprev']");
        HtmlNode nextPageNode = searchnavNode.SelectSingleNode(".//*[@id='unext']");

        List<HtmlNode> itemNodes = [.. tableNode.ChildNodes.Where(n => n.Name == "tr").Skip(1)];
        List<ComicBasicInfo> comics = [];
        foreach (HtmlNode itemNode in itemNodes)
        {
            HtmlNode? gl3cNode = itemNode.SelectSingleNode("./td[@class='gl3c glname']/a");
            if (gl3cNode is null)
            {
                continue;
            }

            string link = gl3cNode.Attributes["href"].Value;
            string title = gl3cNode.SelectSingleNode("./div[@class='glink']").InnerText;

            string categoryText = itemNode.SelectSingleNode("./td[@class='gl1c glcat']/div").InnerText;

            string pageCountText = itemNode.SelectSingleNode("./td[@class='gl2c']/div[@class='glthumb']/div[2]/div[2]/div[2]").InnerText;
            int pageCount = int.Parse(pageCountText[..^6]);

            ComicBasicInfo comic = new()
            {
                Title = title,
                Link = link,
                Category = ToComicCategory(categoryText),
                PageCount = pageCount,
            };
            comics.Add(comic);
        }

        string? previousPageLink = previousPageNode.Name == "a" ? previousPageNode.Attributes["href"].DeEntitizeValue : null;
        string? nextPageLink = nextPageNode.Name == "a" ? nextPageNode.Attributes["href"].DeEntitizeValue : null;

        return new()
        {
            Items = comics,
            PreviousPageLink = previousPageLink,
            NextPageLink = nextPageLink,
        };
    }

    private static ComicDetailedInfo ParseComicInfo(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("/html/body/div[@class='gm']/div[@id='gd2']");
        string title1 = titleNode.SelectSingleNode("./h1[@id='gn']").InnerText;
        string title2 = titleNode.SelectSingleNode("./h1[@id='gj']").InnerText;

        HtmlNode commentsNode = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='cdiv']");
        List<HtmlNode> commentNodes = [.. commentsNode.ChildNodes.Where(n =>
            n.Name == "div" &&
            n.Attributes.Contains("class") &&
            n.Attributes["class"].Value == "c1")];
        List<CommentInfo> comments = [];
        foreach (HtmlNode commentNode in commentNodes)
        {
            HtmlNode infoNode = commentNode.SelectSingleNode(".//div[contains(@class,'c3')]");
            HtmlNode contentNode = commentNode.SelectSingleNode(".//div[contains(@class,'c6')]");
            HtmlNode? scoreNode = commentNode.SelectSingleNode(".//span[starts-with(@id,'comment_score_')]");

            string sender = infoNode.SelectSingleNode(".//a[contains(@href,'uploader')]").InnerText.Trim();

            string infoText = HtmlEntity.DeEntitize(infoNode.InnerText);
            Match dtMatch = COMMENT_DATE_REGEX().Match(infoText);
            var postTime = DateTime.Parse(dtMatch.Value);

            string contentText = contentNode.InnerHtml
                .Replace("<br>", "\n")
                .Replace("<br/>", "\n")
                .Replace("<br />", "\n");
            string content = HtmlEntity.DeEntitize(contentText);

            var tmp = new HtmlDocument();
            tmp.LoadHtml(content);
            content = tmp.DocumentNode.InnerText.Trim();

            int vote = 0;
            if (scoreNode is not null)
            {
                string voteText = scoreNode.InnerText.Trim();
                vote = int.Parse(voteText);
            }

            comments.Add(new()
            {
                PostTime = postTime,
                Sender = sender,
                Content = content,
                Vote = vote,
                IsFromUploader = scoreNode is null,
            });
        }

        return new()
        {
            Title1 = title1,
            Title2 = title2,
            Comments = comments,
        };
    }

    private static ComicCategory ToComicCategory(string category)
    {
        return category switch
        {
            "Doujinshi" => ComicCategory.Doujinshi,
            "Manga" => ComicCategory.Manga,
            "Artist CG" => ComicCategory.ArtistCG,
            "Game CG" => ComicCategory.GameCG,
            "Western" => ComicCategory.Western,
            "Non-H" => ComicCategory.NonH,
            "Image Set" => ComicCategory.ImageSet,
            "Cosplay" => ComicCategory.Cosplay,
            "Asian Porn" => ComicCategory.AsianPorn,
            "Misc" => ComicCategory.Misc,
            _ => throw new ArgumentException($"Unknown category '{category}'.")
        };
    }

    [GeneratedRegex(@"\d{1,2}\s+\w+\s+\d{4},\s+\d{2}:\d{2}")]
    private static partial Regex COMMENT_DATE_REGEX();
}
