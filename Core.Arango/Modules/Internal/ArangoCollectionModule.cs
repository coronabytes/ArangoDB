﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Arango.Protocol;
using Core.Arango.Protocol.Internal;
using Newtonsoft.Json.Linq;

namespace Core.Arango.Modules.Internal
{
    internal class ArangoCollectionModule : ArangoModule, IArangoCollectionModule
    {
        public ArangoCollectionModule(IArangoContext context) : base(context)
        {
        }

        public async Task CreateAsync(ArangoHandle database, string collection, ArangoCollectionType type,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Post,
                ApiPath(database, "collection"),
                new ArangoCollection
                {
                    Name = collection,
                    Type = type
                }, cancellationToken: cancellationToken);
        }

        public async Task CreateAsync(ArangoHandle database, ArangoCollection collection,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Post,
                ApiPath(database, "collection"),
                collection,
                cancellationToken: cancellationToken);
        }

        public async Task TruncateAsync(ArangoHandle database, string collection,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Put,
                ApiPath(database, $"collection/{UrlEncode(collection)}/truncate"),
                cancellationToken: cancellationToken);
        }

        public async Task<List<string>> ListAsync(ArangoHandle database,
            CancellationToken cancellationToken = default)
        {
            var res = await SendAsync<QueryResponse<ArangoCollection>>(HttpMethod.Get,
                ApiPath(database, "collection?excludeSystem=true"),
                cancellationToken: cancellationToken);
            return res.Result.Select(x => x.Name).ToList();
        }

        public async Task UpdateAsync(ArangoHandle database, string collection, ArangoCollectionUpdate update,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Put,
                ApiPath(database, $"collection/{collection}/properties"),
                update,
                cancellationToken: cancellationToken);
        }

        public async Task RenameAsync(ArangoHandle database, string oldname, string newname,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Put,
                ApiPath(database, $"collection/{oldname}/rename"),
                new
                {
                    name = newname
                },
                cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistAsync(ArangoHandle database, string collection,
            CancellationToken cancellationToken = default)
        {
            var collections = await ListAsync(database, cancellationToken);

            return collections.Contains(collection);
        }

        public async Task<ArangoCollection> GetAsync(ArangoHandle database, string collection,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<ArangoCollection>(
                HttpMethod.Get, 
                ApiPath(database, $"collection/{UrlEncode(collection)}"),
                null, database.Transaction, cancellationToken: cancellationToken);
        }

        public async Task DropAsync(ArangoHandle database, string collection,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(HttpMethod.Delete,
                ApiPath(database, $"collection/{UrlEncode(collection)}"),
                cancellationToken: cancellationToken);
        }
    }
}