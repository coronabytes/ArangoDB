﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Arango.Protocol;
using Newtonsoft.Json.Linq;

namespace Core.Arango.Modules
{
    /// <summary>
    ///     Document operations
    /// </summary>
    public interface IArangoDocumentModule
    {
        /// <summary>
        ///     creates documents
        /// </summary>
        Task<List<ArangoUpdateResult<TR>>> CreateMultipleAsync<T, TR>(ArangoHandle database,
            string collection,
            IEnumerable<T> docs,
            bool? waitForSync = null,
            bool? keepNull = null,
            bool? mergeObjects = null,
            bool? returnOld = null,
            bool? returnNew = null,
            bool? silent = null,
            ArangoOverwriteMode? overwriteMode = null,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        ///     creates documents
        /// </summary>
        Task<List<ArangoUpdateResult<JObject>>> CreateMultipleAsync<T>(ArangoHandle database,
            string collection,
            IEnumerable<T> docs,
            bool? waitForSync = null,
            bool? keepNull = null,
            bool? mergeObjects = null,
            bool? returnOld = null,
            bool? returnNew = null,
            bool? silent = null,
            ArangoOverwriteMode? overwriteMode = null,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        ///     create document
        /// </summary>
        Task<ArangoUpdateResult<TR>> CreateAsync<T, TR>(ArangoHandle database,
            string collection, T doc,
            bool? waitForSync = null,
            bool? keepNull = null,
            bool? mergeObjects = null,
            bool? returnOld = null,
            bool? returnNew = null,
            bool? silent = null,
            ArangoOverwriteMode? overwriteMode = null,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        ///     create document
        /// </summary>
        Task<ArangoUpdateResult<JObject>> CreateAsync<T>(ArangoHandle database,
            string collection, T doc,
            bool? waitForSync = null,
            bool? keepNull = null,
            bool? mergeObjects = null,
            bool? returnOld = null,
            bool? returnNew = null,
            bool? silent = null,
            ArangoOverwriteMode? overwriteMode = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<List<ArangoUpdateResult<TR>>> DeleteMultipleAsync<T, TR>(ArangoHandle database, string collection,
            IEnumerable<T> docs, bool? waitForSync = null, bool? returnOld = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<ArangoUpdateResult<TR>> DeleteAsync<TR>(ArangoHandle database, string collection, string key,
            bool? waitForSync = null, bool? returnOld = null, bool? silent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Note: this API is currently not supported on cluster coordinators.
        /// </summary>
        IAsyncEnumerable<List<JObject>> ExportAsync(ArangoHandle database, string collection, bool? flush = null,
            int? flushWait = null, int? batchSize = null, int? ttl = null,
            CancellationToken cancellationToken = default);

        Task ImportAsync<T>(ArangoHandle database, string collection, IEnumerable<T> docs, bool complete = true,
            CancellationToken cancellationToken = default) where T : class;

        Task<List<ArangoUpdateResult<TR>>> ReplaceMultipleAsync<T, TR>(ArangoHandle database, string collection,
            IEnumerable<T> docs, bool? waitForSync = null, bool? returnOld = null, bool? returnNew = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<List<ArangoUpdateResult<JObject>>> ReplaceMultipleAsync<T>(ArangoHandle database, string collection,
            IEnumerable<T> docs, bool? waitForSync = null, bool? returnOld = null, bool? returnNew = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<ArangoUpdateResult<TR>> ReplaceAsync<T, TR>(ArangoHandle database, string collection, T doc,
            bool waitForSync = false, bool? returnOld = null, bool? returnNew = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<ArangoUpdateResult<JObject>> ReplaceAsync<T>(ArangoHandle database, string collection, T doc,
            bool waitForSync = false, bool? returnOld = null, bool? returnNew = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<List<ArangoUpdateResult<JObject>>> UpdateMultipleAsync<T>(ArangoHandle database, string collection,
            IEnumerable<T> docs, bool? waitForSync = null, bool? keepNull = null, bool? mergeObjects = null,
            bool? returnOld = null, bool? returnNew = null, bool? silent = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<List<ArangoUpdateResult<TR>>> UpdateMultipleAsync<T, TR>(ArangoHandle database, string collection,
            IEnumerable<T> docs, bool? waitForSync = null, bool? keepNull = null, bool? mergeObjects = null,
            bool? returnOld = null, bool? returnNew = null, bool? silent = null,
            CancellationToken cancellationToken = default) where T : class;

        Task<ArangoUpdateResult<JObject>> UpdateAsync<T>(ArangoHandle database, string collection, T doc,
            bool? waitForSync = null, bool? keepNull = null, bool? mergeObjects = null, bool? returnOld = null,
            bool? returnNew = null, bool? silent = null, CancellationToken cancellationToken = default) where T : class;

        Task<ArangoUpdateResult<TR>> UpdateAsync<T, TR>(ArangoHandle database, string collection, T doc,
            bool? waitForSync = null, bool? keepNull = null, bool? mergeObjects = null, bool? returnOld = null,
            bool? returnNew = null, bool? silent = null, CancellationToken cancellationToken = default) where T : class;
    }
}