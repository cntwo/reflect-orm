using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ReflectORM.Extensions
{
    /// <summary>
    /// String Extensions class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Surrounds the string with square brackets. An empty string is returned as an empty string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string SurroundWithSquareBrackets(this string source)
        {
            return SurroundWithCharacter(source, '[', ']');
        }

        /// <summary>
        /// Surrounds the string with the provided character. An empty string is returned as an empty string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string SurroundWithCharacter(this string source, char character)
        {
            return SurroundWithCharacter(source, character, character);
        }

        /// <summary>
        /// Surrounds the string with the provided characters. An empty string is returned as an empty string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startChar">The start char.</param>
        /// <param name="endChar">The end char.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string SurroundWithCharacter(this string source, char startChar, char endChar)
        {
            bool startAdd = false;
            bool endAdd = false;

            if (!source.Equals(string.Empty))
            {
                if (!source[0].Equals(startChar))
                    startAdd = true;
                if (!source[source.Length - 1].Equals(endChar))
                    endAdd = true;
            }

            if (startAdd)
                source = string.Format("{1}{0}", source, startChar);
            if (endAdd)
                source = string.Format("{0}{1}", source, endChar);
            return source;
        }

        /// <summary>
        /// Finds all links contained in the string.
        /// </summary>
        /// <param name="SearchText">The search text.</param>
        /// <returns></returns>
        public static MatchCollection FindLinks(this string SearchText)
        {
            // this will find links like:
            // http://www.mysite.com
            // as well as any links with other characters directly in front of it like:
            // href="http://www.mysite.com"
            // you can then use your own logic to determine which links to linkify
            Regex regx = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            SearchText = SearchText.Replace("&nbsp;", " ");
            MatchCollection matches = regx.Matches(SearchText);
            return matches;
        }

        /// <summary>
        /// Determines whether the string is a valid web location.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if the string is a valid web location; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidUrl(this string url)
        {
            bool valid = true;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;
            url = url.RemoveTags();
            using (var client = new UrlValidatorWebClient())
            {
                client.HeadOnly = true;
                try
                {
                    string s1 = client.DownloadString(url);
                }
                catch
                {
                    valid = false;
                }
            }
            return valid;
        }

        /// <summary>
        /// Removes all html tags from the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string RemoveTags(this string text)
        {
            bool hasTags = text.Contains('<');
            while (hasTags)
            {
                int startIndex = text.IndexOf('<');
                int endIndex = text.IndexOf('>');
                string toRemove = string.Empty;
                if (endIndex != -1)
                    text = text.Remove(startIndex, (endIndex - startIndex) + 1);
                else
                    text = text.Remove(startIndex);
                hasTags = text.Contains('<');
            }
            return text;
        }

        /// <summary>
        /// Determines whether [is null or white space (or empty)] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public static bool IsNumber(this string value)
        {
            int i;
            return int.TryParse(value, out i);
        }
    }
}
