using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace OpenPlaDiC.WebApp.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlContent CreateApiResponse(this IHtmlHelper htmlHelper, string response)
        {
            return new HtmlString(Framework.Helper.Base64Encode(response));

        }
        public static IHtmlContent Image(this IHtmlHelper htmlHelper, string src, string altText, string width = "", string height = "")
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<img src='{0}' alt='{1}' width='{2}' height='{3}' />", src, altText, width, height);

            // Use HtmlString.Create or a new HtmlString instance in ASP.NET Core
            return new HtmlString(sb.ToString());
            // Alternatively, using TagBuilder is more robust
            /*
            var imgTag = new TagBuilder("img");
            imgTag.MergeAttribute("src", src);
            imgTag.MergeAttribute("alt", altText);
            if (!string.IsNullOrEmpty(width)) imgTag.MergeAttribute("width", width);
            if (!string.IsNullOrEmpty(height)) imgTag.MergeAttribute("height", height);
            // AppendHtml is used to build complex HTML content with TagBuilder
            return imgTag; 
            */
        }
    }
}
