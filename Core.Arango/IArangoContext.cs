﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Arango.Modules;
using Core.Arango.Protocol;

namespace Core.Arango
{
    public interface IArangoContext
    {
        IArangoConfiguration Configuration { get; }
        IArangoUserModule User { get; }
        IArangoCollectionModule Collection { get; }
        IArangoGraphModule Graph { get; }
        IArangoTransactionModule Transaction { get; }
        IArangoDocumentModule Document { get; }
        IArangoQueryModule Query { get; }
        IArangoDatabaseModule Database { get; }
        IArangoViewModule View { get; }
        IArangoIndexModule Index { get; }
        IArangoAnalyzerModule Analyzer { get; }
        IArangoFunctionModule Function { get; }
        IArangoFoxxModule Foxx { get; }
        IArangoBackupModule Backup { get; }
        IArangoBatchModule Batch { get; }
        Task<ArangoVersion> GetVersionAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<string>> GetEndpointsAsync(CancellationToken cancellationToken = default);
    }
}