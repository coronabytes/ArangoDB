﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Modules.Internal
{
    internal abstract class ArangoModule
    {
        protected readonly IArangoContext Context;

        protected ArangoModule(IArangoContext context)
        {
            Context = context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string RealmPrefix(string name)
        {
            if (name == "_system")
                return "_system";

            var realm = string.IsNullOrWhiteSpace(Context.Configuration.Realm)
                ? string.Empty
                : Context.Configuration.Realm + "-";

            return UrlEncode(realm + name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ApiPath(ArangoHandle handle, string path)
        {
            return $"/_db/{RealmPrefix(handle)}/_api/{path}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ApiPath(string path)
        {
            return $"/_api/{path}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string UrlEncode(string value)
        {
            return UrlEncoder.Default.Encode(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SendAsync<T>(HttpMethod m, string url, object body = null,
            string transaction = null, bool throwOnError = true, bool auth = true,
            CancellationToken cancellationToken = default)
        {
            return Context.Configuration.Transport.SendAsync<T>(m, url, body, transaction, throwOnError, auth,
                cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> SendAsync(Type type, HttpMethod m, string url, object body = null,
            string transaction = null, bool throwOnError = true, bool auth = true,
            CancellationToken cancellationToken = default)
        {
            return Context.Configuration.Transport.SendAsync(type, m, url, body, transaction, throwOnError, auth,
                cancellationToken);
        }

        public string AddQueryString(string uri,
            IEnumerable<KeyValuePair<string, string>> queryString)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (queryString == null)
                throw new ArgumentNullException(nameof(queryString));

            var anchorIndex = uri.IndexOf('#');
            var uriToBeAppended = uri;
            var anchorText = "";

            if (anchorIndex != -1)
            {
                anchorText = uri.Substring(anchorIndex);
                uriToBeAppended = uri.Substring(0, anchorIndex);
            }

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            var sb = new StringBuilder();
            sb.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            sb.Append(anchorText);
            return sb.ToString();
        }

        private static Regex _regexParams = new Regex("(@)?\\{([0-9]+)\\}", RegexOptions.Compiled);

        public string Parameterize(FormattableString query, out Dictionary<string, object> parameter)
        {
            var i = 0;
            var j = 0;

            var set = new Dictionary<object, string>();
            var cols = new Dictionary<object, string>();
            var nullParam = string.Empty;

            var matches = _regexParams.Matches(query.Format);

            var args = query.GetArguments().Select(x =>
            {
                if (x == null)
                {
                    if (string.IsNullOrEmpty(nullParam))
                        nullParam = $"@P{++i}";

                    return nullParam;
                }

                if (matches[j++].Groups[1].Value == "@")
                {
                    if (cols.TryGetValue(x, out var p))
                        return (object) p;

                    p = $"@P{++i}";

                    cols.Add(x, "@"+ p);

                    return (object) p;
                }
                else
                {
                    if (set.TryGetValue(x, out var p))
                        return (object) p;

                    p = $"@P{++i}";

                    set.Add(x, p);

                    return (object) p;
                }
            }).ToArray();

            var queryExp = string.Format(query.Format, args);

            var res = set.ToDictionary(
                x => x.Value.Substring(1), 
                x => x.Key);

            foreach (var col in cols)
                res.Add(col.Value.Substring(1), col.Key);

            if (!string.IsNullOrEmpty(nullParam))
                res.Add(nullParam.Substring(1), null);

            parameter = res;

            return queryExp;
        }
    }
}