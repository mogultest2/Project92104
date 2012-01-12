﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using JabbR.ContentProviders.Core;

namespace JabbR.ContentProviders
{
    public class BBCContentProvider : CollapsibleContentProvider
    {
        private static readonly string ContentFormat = "<div class='bbc_wrapper'><div class=\"header\"><img src=\"/Content/images/contentproviders/bbcnews-masthead.png\" alt=\"\" width=\"84\" height=\"24\"></div><img src=\"{1}\" title=\"{2}\" alt=\"{3}\" class=\"newsimage\" /><h2>{0}</h2><div>{4}</div><div><a href=\"{5}\">View article</a></div></div>";

        protected override ContentProviderResultModel GetCollapsibleContent(HttpWebResponse response)
        {
            var pageInfo = ExtractFromResponse(response);
            return new ContentProviderResultModel()
                       {
                           Content = String.Format(ContentFormat, pageInfo.Title, pageInfo.ImageURL, pageInfo.Title, pageInfo.Title, pageInfo.Description, pageInfo.PageURL),
                           Title = pageInfo.Title
                       };
        }

        private PageInfo ExtractFromResponse(HttpWebResponse response)
        {
            var info = new PageInfo();
            using (var responseStream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(responseStream))
                {
                    var pageContext = HttpUtility.HtmlDecode(sr.ReadToEnd());
                    info.Title = ExtractUsingRegEx(new Regex(@"<meta\s.*property=""og:title"".*content=""(.*)"".*/>"), pageContext);
                    info.Description = ExtractUsingRegEx(new Regex(@"<meta\s.*name=""Description"".*content=""(.*)"".*/>"), pageContext);
                    info.ImageURL = ExtractUsingRegEx(new Regex(@"<meta.*property=""og:image"".*content=""(.*)"".*/>"), pageContext);
                    info.PageURL = response.ResponseUri.AbsoluteUri;
                }
            }
            return info;
        }

        private string ExtractUsingRegEx(Regex regularExpression, string content)
        {
            var matches = regularExpression.Match(content)
                .Groups
                .Cast<Group>()
                .Skip(1)
                .Select(g => g.Value)
                .Where(v => !String.IsNullOrEmpty(v));

            return matches.FirstOrDefault() ?? String.Empty;
        }

        private class PageInfo
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageURL { get; set; }
            public string PageURL { get; set; }
        }

        protected override bool IsValidContent(HttpWebResponse response)
        {
            return response.ResponseUri.AbsoluteUri.StartsWith("http://www.bbc.co.uk/news", StringComparison.OrdinalIgnoreCase);
        }
    }
}