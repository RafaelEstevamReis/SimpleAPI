using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Simple.API
{
    /// <summary>
    /// Helper class
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Builds an UrlEncoded url
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="values">Parameteres to be encoded</param>
        /// <returns>Url with parameters url encoded</returns>
        public static string BuildUrlEncodedUrl(string service, IEnumerable<KeyValuePair<string, string>> values)
            => buildUrl(service, values);
        /// <summary>
        /// Builds an UrlEncoded url
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="p">Object to extract parameters from</param>
        /// <returns>Url with parameters url encoded</returns>
        public static string BuildUrlEncodedUrl(string service, object p)
            => buildUrl(service, buildParams(p));

        internal static string buildUrl(string service, IEnumerable<KeyValuePair<string, string>> values)
        {
            string pars = string.Join("&", values.Select(pair => $"{pair.Key}={WebUtility.UrlEncode(pair.Value)}"));
            return $"{service}?{pars}";
        }
        internal static IEnumerable< KeyValuePair<string, string>> buildParams(object p)
        {
            if (p == null) yield break; // Empty list

#if NETSTANDARD1_1
            throw new System.NotSupportedException("Unsuported on NETSTANDARD 1.1");
#else
            var type = p.GetType();

            foreach (var prop in type.GetProperties())
            {
                var value = prop.GetValue(p);
                if (value == null) continue;

                string sValue;

                if (prop.PropertyType == typeof(decimal))
                {
                    sValue = ((decimal)value).ToString(CultureInfo.InvariantCulture);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    sValue = ((float)value).ToString(CultureInfo.InvariantCulture);
                }
                else if (prop.PropertyType == typeof(double))
                {
                    sValue = ((double)value).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    sValue = value.ToString();
                }

                yield return new KeyValuePair<string, string>(
                    key: prop.Name,
                    value: sValue
                );
            }
#endif
        }
    }
}
