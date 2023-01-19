using System;
using System.Text;

namespace Simple.API
{
    /// <summary>
    /// A base for JWT decoding
    /// </summary>
    public class JwtBase
    {
        protected JwtBase() { }
        /// <summary>
        /// Original parsed Token
        /// </summary>
        public string OriginalToken { get; protected set; }

        /// <summary>
        /// JWT Header text
        /// </summary>
        public string Header { get; protected set; }
        /// <summary>
        /// JWT Payload text
        /// </summary>
        public string Payload { get; protected set; }
        /// <summary>
        /// JWT signature bytes
        /// </summary>
        public byte[] Signature { get; protected set; }
        /// <summary>
        /// Parses JWT
        /// </summary>
        public static JwtBase ParseText(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3) new ArgumentException("Token must have 3 sections");

            return new JwtBase()
            {
                Header = getB64(parts[0]),
                Payload = getB64(parts[1]),
                Signature = convertFromBase64String(parts[2]),
                OriginalToken = token,
            };
        }
        private static string getB64(string b64)
        {
            var arr = convertFromBase64String(b64);
            return Encoding.UTF8.GetString(arr, 0, arr.Length);
        }

        private static byte[] convertFromBase64String(string input)
        {
            // B64 fix
            // https://github.com/dotnet/runtime/issues/26678
            if (string.IsNullOrWhiteSpace(input)) return null;
            try
            {
                string working = input.Replace('-', '+').Replace('_', '/'); ;
                while (working.Length % 4 != 0)
                {
                    working += '=';
                }
                return Convert.FromBase64String(working);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    /// <summary>
    /// A base for JWT decoding
    /// </summary>
    public class JWT<T>
        : JwtBase
    {
        /// <summary>
        /// JWT content
        /// </summary>
        public T Content { get; private set; }
        /// <summary>
        /// Parse JWT with T content
        /// </summary>
        public static JWT<T> Parse(string token)
        {
            var jwt = ParseText(token);

            var j = new JWT<T>()
            {
                Header = jwt.Header,
                Payload = jwt.Payload,
                Signature = jwt.Signature,
                Content = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jwt.Payload),
                OriginalToken = token,
            };
            return j;
        }
    }
    /// <summary>
    /// A JWT model for decoding
    /// </summary>
    public class JWT : JwtBase
    {
        /// <summary>
        /// JWT generic content
        /// </summary>
        public GenericJwt Content { get; private set; }
        /// <summary>
        /// Parse generic JWT
        /// </summary>
        public static JWT Parse(string token)
        {
            var jc = JWT<GenericJwt>.Parse(token);
            return new JWT()
            {
                Header = jc.Header,
                Payload = jc.Payload,
                Signature = jc.Signature,
                Content = jc.Content,
                OriginalToken = token,
            };
        }
    }
    /// <summary>
    /// JWT with rfc7519 recomended properties
    /// </summary>
    public class GenericJwt
    {
        /// <summary>
        /// Issuer
        /// </summary>
        public string iss { get; set; }
        /// <summary>
        /// Expiration Time
        /// </summary>
        public long exp { get; set; }
        /// <summary>
        /// Issued At
        /// </summary>
        public long iat { get; set; }
        /// <summary>
        /// Not Before
        /// </summary>
        public long nbf { get; set; }
        /// <summary>
        /// Subject
        /// </summary>
        public string sub { get; set; }
        /// <summary>
        /// Audience
        /// </summary>
        public string aud { get; set; }

#if !NETSTANDARD
        /// <summary>
        /// Expiration Time
        /// </summary>
        public DateTime GetExp => DateTime.UnixEpoch.AddSeconds(exp);
        /// <summary>
        /// Issued At
        /// </summary>
        public DateTime GetIat => DateTime.UnixEpoch.AddSeconds(iat);
        /// <summary>
        /// Not Before
        /// </summary>
        public DateTime GetNbf => DateTime.UnixEpoch.AddSeconds(nbf);
#endif
    }
}
