using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Simple.API
{
    internal class Helper
    {
        internal static string BuildUrl(string service, IEnumerable<KeyValuePair<string, string>> values)
        {
            string pars = string.Join("&", values.Select(pair => $"{pair.Key}={WebUtility.UrlEncode(pair.Value)}"));
            return $"{service}?{pars}";
        }
        internal static IEnumerable< KeyValuePair<string, string>> BuildParams(object p)
        {
#if NETSTANDARD1_1
            throw new System.NotSupportedException("Unsuported on NETSTANDARD 1.1");
#else
            var type = p.GetType();

            foreach (var prop in type.GetProperties())
            {
                var value = prop.GetValue(p);

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
