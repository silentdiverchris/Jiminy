using Jiminy.Helpers;
using System;
using System.Collections.Generic;

namespace Jiminy.Classes
{
    /// <summary>
    /// Stores the cached SVG definitions for each icon
    /// </summary>
    public class SvgCache
    {
        private Dictionary<string, string> _cache = new();

        /// <summary>
        /// Get the cached svg html for the named icon, returns a default icon if it is not found
        /// </summary>
        /// <param name="iconFileName"></param>
        /// <returns></returns>
        public string Get(string? iconFileName)
        {
            ValidateKey(iconFileName, "Get");

            if (_cache.ContainsKey(iconFileName!))
            {
                return _cache[iconFileName!];
            }
            else
            {
                return Constants.DEFAULT_SVG_ICON_HTML;
            }
        }

        /// <summary>
        /// Adds an svg html to the cache, optionally overwriting if it already exists
        /// </summary>
        /// <param name="iconFileName"></param>
        /// <param name="svgHtml"></param>
        /// <param name="overwrite"></param>
        public void Add(string iconFileName, string svgHtml, bool overwrite = false)
        {
            ValidateKey(iconFileName, "Add");

            if (!_cache.ContainsKey(iconFileName))
            {
                _cache.Add(iconFileName, svgHtml);
            }
            else
            {
                if (overwrite)
                {
                    _cache[iconFileName] = svgHtml;
                }
            }
        }

        private void ValidateKey(string? key, string interfaceName)
        {
            if (key.IsEmpty())
            {
                throw new ArgumentException($"SvgCache.{interfaceName} was supplied with an empty key");
            }
        }
    }
}
