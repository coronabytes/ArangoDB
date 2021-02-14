﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Arango.Protocol;
using Core.Arango.Protocol.Internal;

namespace Core.Arango.Modules.Internal
{
    internal class ArangoTransactionModule : ArangoModule, IArangoTransactionModule
    {
        internal ArangoTransactionModule(IArangoContext context) : base(context)
        {
        }

        public async Task<T> ExecuteAsync<T>(ArangoHandle database, ArangoTransaction request,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<T>(HttpMethod.Post,
                ApiPath(database, "transaction"),
                request, cancellationToken: cancellationToken);
        }

        public async Task<ArangoHandle> BeginAsync(ArangoHandle database, ArangoTransaction request,
            CancellationToken cancellationToken = default)
        {
            var res = await SendAsync<SingleResult<TransactionResponse>>(HttpMethod.Post,
                ApiPath(database, "transaction/begin"),
                request, cancellationToken: cancellationToken);

            var transaction = res.Result.Id;
            return new ArangoHandle(database, transaction);
        }

        public async Task CommitAsync(ArangoHandle database,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(database.Transaction))
                throw new ArangoException("no transaction handle");

            await SendAsync<ArangoVoid>(HttpMethod.Put,
                ApiPath(database, $"transaction/{database.Transaction}"),
                cancellationToken: cancellationToken);
        }

        public async Task AbortAsync(ArangoHandle database, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(database.Transaction))
                throw new ArangoException("no transaction handle");

            await SendAsync<ArangoVoid>(HttpMethod.Delete,
                ApiPath(database, $"transaction/{database.Transaction}"),
                cancellationToken: cancellationToken);
        }

        public ArangoHandle CreateBatch(ArangoHandle database)
        {
            return new ArangoHandle(database, true);
        }

        public async Task ExecuteBatch(ArangoHandle handle)
        {
            const string boundary = "ArangoBatch";
            var m = new MultipartFormDataContent(boundary);

            foreach (var batch in handle.Batches)
            {
                var s = new StringContent(batch.Request);
                s.Headers.ContentType = new MediaTypeHeaderValue("application/x-arango-batchpart");
                if (handle.Transaction != null)
                    s.Headers.Add("x-arango-trx-id", handle.Transaction);
                s.Headers.Add("Content-Id", batch.ContentId.ToString("D"));

                m.Add(s, "a");
                s.Headers.Remove("Content-Disposition");
            }

            var q = await m.ReadAsStringAsync();

            var result = await Context.Configuration.Transport.SendContentAsync(HttpMethod.Post, ApiPath(handle, "batch"), m,
                handle.Transaction);

            // ReadAsMultipartAsync does not exist in .NET Core
            
            var dbg = await result.ReadAsStringAsync();
            var stream = await result.ReadAsStreamAsync();

            var sr = new StreamReader(stream, Encoding.UTF8);
            string line;
            Guid? contentId = null;
            string content = null;
            bool hasErrors = false;
            var lookup = handle.Batches.ToDictionary(x => x.ContentId, x => x);

            while ((line = await sr.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("--"))
                {
                    if (contentId.HasValue && !string.IsNullOrWhiteSpace(content))
                    {
                        if (lookup.TryGetValue(contentId.Value, out var batch))
                        {
                            try
                            {
                                if (hasErrors)
                                {
                                    var errors = Context.Configuration.Serializer.Deserialize<IEnumerable<ErrorResponse>>(content)
                                        .Select(error => new ArangoError(error.ErrorMessage, (ArangoErrorCode)error.ErrorNum));

                                    batch.Fail(new ArangoException(content, errors));
                                }
                                else
                                {
                                    batch.Complete(content);
                                }
                            }
                            catch (Exception)
                            {
                                //
                            }

                            batch.Completed = true;
                        }
                    }

                    contentId = null;
                    content = null;
                    hasErrors = false;
                    continue;
                }

                if (line.StartsWith("Content-Id: "))
                {
                    contentId = Guid.Parse(line.Substring(12));
                    continue;
                }

                if (line.StartsWith("X-Arango-Error-Codes:"))
                {
                    hasErrors = true;
                    continue;
                }

                if (line.StartsWith("{") || line.StartsWith("[") )
                {
                    content = line;
                    continue;
                }
            }

            foreach (var batch in handle.Batches.Where(x=>!x.Completed))
            {
                try
                {
                    batch.Cancel();
                }
                catch (Exception)
                {
                    // 
                }
            }
        }
    }
}