using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CodeConverters.Core.Diagnostics
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static string ToLogFormat(this IDictionary<string, string> dictionary)
        {
            return string.Join(",", dictionary.Select(d => string.Format("{0}:{1}", d.Key, d.Value)));
        }

        /// <summary>
        /// Finds dictionary keys in the DefaultScrubParams list and replaces their values
        /// with asterisks. Key comparison is case insensitive.
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static IDictionary<string, string> Scrub(this NameValueCollection nvc)
        {
            return Scrub(nvc.ToDictionary(), LoggingConfig.DefaultScrubParams);
        }

        /// <summary>
        /// Finds dictionary keys in the DefaultScrubParams list and replaces their values
        /// with asterisks. Key comparison is case insensitive.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IDictionary<string, string> Scrub(this IDictionary<string, string> dict)
        {
            return Scrub(dict, LoggingConfig.DefaultScrubParams);
        }

        /// <summary>
        /// Finds dictionary keys in the <see cref="scrubParams"/> list and replaces their values
        /// with asterisks. Key comparison is case insensitive.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="scrubParams"></param>
        /// <returns></returns>
        public static IDictionary<string, string> Scrub(this IDictionary<string, string> dict, string[] scrubParams)
        {
            if (dict == null || !dict.Any())
                return dict;

            if (scrubParams == null || !scrubParams.Any())
                return dict;

            var itemsToUpdate = dict.Keys
                .Where(k => scrubParams.Contains(k, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();

            if (!itemsToUpdate.Any())
                return dict;
            foreach (var key in itemsToUpdate)
            {
                var len = dict[key] == null ? 8 : dict[key].Length;
                dict[key] = new string('*', len);
            }
            return dict;
        }
    }

}
