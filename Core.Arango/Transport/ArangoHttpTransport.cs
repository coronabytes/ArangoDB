using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Core.Arango.Protocol;
using Core.Arango.Protocol.Internal;
using Newtonsoft.Json;

namespace Core.Arango.Transport
{
    /// <summary>
    ///     Arango HTTP 1.1/2.0 Transport Implementation
    /// </summary>
    public class ArangoHttpTransport : IArangoTransport
    {
        private static readonly HttpClient DefaultHttpClient = new();
        private readonly IArangoConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string _auth;
        private DateTime _authValidUntil = DateTime.MinValue;

        /// <summary>
        ///     Arango HTTP 1.1/2.0 Transport Implementation
        /// </summary>
        public ArangoHttpTransport(IArangoConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = configuration.HttpClient ?? DefaultHttpClient;
        }

        /// <inheritdoc />
        public async Task<T> SendAsync<T>(HttpMethod m, string url, object body = null,
            string transaction = null, bool throwOnError = true, bool auth = true,
            IDictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            await Authenticate(auth, cancellationToken);

            var msg = new HttpRequestMessage(m, _configuration.Server + url);
            ApplyHeaders(transaction, auth, msg, headers);

            if (body != null)
            {
                var json = _configuration.Serializer.Serialize(body);
                msg.Content = new ByteArrayContent(json);
            }
            else
            {
                msg.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");
            }

            msg.Headers.Remove("Content-Type");
            msg.Headers.Add("Content-Type", _configuration.Serializer.ContentType);

            var res = await _httpClient.SendAsync(msg, cancellationToken);

            if (!res.IsSuccessStatusCode)
                if (throwOnError)
                {
                    var errorContent = await res.Content.ReadAsByteArrayAsync();
                    var error = _configuration.Serializer.Deserialize<ErrorResponse>(errorContent);
                    throw new ArangoException(Encoding.UTF8.GetString(errorContent), error.ErrorMessage,
                        (HttpStatusCode) error.Code, (ArangoErrorCode) error.ErrorNum);
                }
                else
                {
                    return default;
                }

            var content = await res.Content.ReadAsByteArrayAsync();

            if (res.Headers.Contains("X-Arango-Error-Codes"))
            {
                var errors = _configuration.Serializer.Deserialize<IEnumerable<ErrorResponse>>(content)
                    .Select(error => new ArangoError(error.ErrorMessage, (ArangoErrorCode) error.ErrorNum));
                throw new ArangoException(Encoding.UTF8.GetString(content), errors);
            }

            if (content == null || content.Length == 0 )
                return default;

            return _configuration.Serializer.Deserialize<T>(content);
        }

        /// <inheritdoc />
        public async Task<HttpContent> SendContentAsync(HttpMethod m, string url, HttpContent body = null,
            string transaction = null,
            bool throwOnError = true, bool auth = true, IDictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            await Authenticate(auth, cancellationToken);

            var msg = new HttpRequestMessage(m, _configuration.Server + url);
            ApplyHeaders(transaction, auth, msg, headers);
            msg.Content = body;

            var res = await _httpClient.SendAsync(msg, cancellationToken);

            if (!res.IsSuccessStatusCode)
                if (throwOnError)
                {
                    var errorContent = await res.Content.ReadAsByteArrayAsync();
                    var error = _configuration.Serializer.Deserialize<ErrorResponse>(errorContent);
                    throw new ArangoException(Encoding.UTF8.GetString(errorContent), error.ErrorMessage,
                        (HttpStatusCode) error.Code, (ArangoErrorCode) error.ErrorNum);
                }

            return res.Content;
        }

        /// <inheritdoc />
        public async Task<object> SendAsync(Type type, HttpMethod m, string url, object body = null,
            string transaction = null, bool throwOnError = true, bool auth = true,
            IDictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            await Authenticate(auth, cancellationToken);

            var msg = new HttpRequestMessage(m, _configuration.Server + url);
            ApplyHeaders(transaction, auth, msg, headers);

            if (body != null)
                msg.Content = new ByteArrayContent(_configuration.Serializer.Serialize(body));
            else
                msg.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");

            msg.Headers.Remove("Content-Type");
            msg.Headers.Add("Content-Type", _configuration.Serializer.ContentType);

            var res = await _httpClient.SendAsync(msg, cancellationToken);

            if (!res.IsSuccessStatusCode)
                if (throwOnError)
                    throw new ArgumentException(await res.Content.ReadAsStringAsync());
                else return default;

            var content = await res.Content.ReadAsByteArrayAsync();

            if (content == null || content.Length == 0)
                return default;

            return _configuration.Serializer.Deserialize(content, type);
        }

        private void ApplyHeaders(string transaction, bool auth, HttpRequestMessage msg,
            IDictionary<string, string> headers)
        {
            msg.Headers.Add(HttpRequestHeader.KeepAlive.ToString(), "true");

            if (auth)
                msg.Headers.Add(HttpRequestHeader.Authorization.ToString(), _auth);

            if (transaction != null)
                msg.Headers.Add("x-arango-trx-id", transaction);

            if (_configuration.AllowDirtyRead)
                msg.Headers.Add("x-arango-allow-dirty-read", "true");

            if (headers != null)
                foreach (var header in headers)
                    msg.Headers.Add(header.Key, header.Value);
        }

        private async Task Authenticate(bool auth, CancellationToken cancellationToken)
        {
            if (auth && (_auth == null || _authValidUntil < DateTime.UtcNow.AddMinutes(-10)))
            {
                var authResponse = await SendAsync<AuthResponse>(HttpMethod.Post,
                    "/_open/auth",
                    new AuthRequest
                    {
                        Username = _configuration.User,
                        Password = _configuration.Password ?? string.Empty
                    }, auth: false, cancellationToken: cancellationToken);

                var jwt = authResponse.Jwt;
                var token = new JwtSecurityToken(jwt.Replace("=", ""));
                _auth = $"Bearer {jwt}";
                _authValidUntil = token.ValidTo;
            }
        }

        private class AuthRequest
        {
            [JsonProperty("username")]
            [JsonPropertyName("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            [JsonPropertyName("password")]
            public string Password { get; set; }
        }

        private class AuthResponse
        {
            [JsonProperty("jwt")]
            [JsonPropertyName("jwt")]
            public string Jwt { get; set; }
        }
    }
}