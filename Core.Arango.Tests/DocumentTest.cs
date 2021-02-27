using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.Arango.Protocol;
using Core.Arango.Tests.Core;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Core.Arango.Tests
{
    public class DocumentTest : TestBase
    {
        [Theory]
        [ClassData(typeof(PascalCaseData))]
        public async Task Get(string serializer)
        {
            await SetupAsync(serializer);
            await Arango.Collection.CreateAsync("test", "test", ArangoCollectionType.Document);

            var createRes = await Arango.Document.CreateAsync("test", "test", new Entity
            {
                Key = "abc",
                Name = "a"
            });

            Assert.NotNull(createRes);
            Assert.NotNull(createRes.Id);
            Assert.NotNull(createRes.Key);
            Assert.NotNull(createRes.Revision);

            var doc = await Arango.Document.GetAsync<Entity>("test", "test", "abc");
            Assert.Equal("a", doc.Name);
            Assert.Equal(createRes.Id, doc.Id);
            Assert.Equal(createRes.Key, doc.Key);
            Assert.Equal(createRes.Revision, doc.Revision);

            var nodoc = await Arango.Document.GetAsync<Entity>("test", "test", "nonexistant", false);
            Assert.Null(nodoc);

            var exception = await Assert.ThrowsAsync<ArangoException>(
                async () => await Arango.Document.GetAsync<Entity>("test", "test", "nonexistant"));

            Assert.Contains("document not found", exception.Message);
            Assert.NotNull(exception.ErrorNumber);
            Assert.NotNull(exception.Code);
            Assert.Equal(ArangoErrorCode.ErrorArangoDocumentNotFound, exception.ErrorNumber);
            Assert.Equal(HttpStatusCode.NotFound, exception.Code);
        }

        [Theory]
        [ClassData(typeof(PascalCaseData))]
        public async Task Update(string serializer)
        {
            await SetupAsync(serializer);
            await Arango.Collection.CreateAsync("test", "test", ArangoCollectionType.Document);

            var createRes = await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Name = "a"
            });

            var res = await Arango.Document.UpdateAsync("test", "test", new
            {
                Key = "abc",
                Name = "c"
            }, returnNew: true, returnOld: true);

            Assert.Equal(createRes.Id, res.Id);
            Assert.Equal(createRes.Key, res.Key);
            Assert.Equal(createRes.Revision, res.OldRevision);

            var doc = await Arango.Document.GetAsync<Entity>("test", "test", "abc");
            Assert.Equal("c", doc.Name);
            Assert.Equal(res.Id, doc.Id);
            Assert.Equal(res.Key, doc.Key);
            Assert.Equal(res.Revision, doc.Revision);

            var obj = await Arango.Query.SingleOrDefaultAsync<Dictionary<string, string>>("test", "test", $"x._key == {"abc"}");

            Assert.Equal("c", obj["Name"]);
            Assert.Equal(res.Id, obj["_id"]);
            Assert.Equal(res.Key, obj["_key"]);
            Assert.Equal(res.Revision, obj["_rev"]);
        }

        [Theory]
        [ClassData(typeof(PascalCaseData))]
        public async Task CreateUpdateMode(string serializer)
        {
            await SetupAsync(serializer);
            if ((await Arango.GetVersionAsync()).SemanticVersion < Version.Parse("3.7"))
                return;

            await Arango.Collection.CreateAsync("test", "test", ArangoCollectionType.Document);

            var createRes = await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Name = "a"
            });

            var res = await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Value = "c"
            }, overwriteMode: ArangoOverwriteMode.Update);

            Assert.Equal(createRes.Id, res.Id);
            Assert.Equal(createRes.Key, res.Key);
            Assert.Equal(createRes.Revision, res.OldRevision);

            var obj = await Arango.Query.SingleOrDefaultAsync<Dictionary<string, string>>("test", "test", $"x._key == {"abc"}");

            Assert.Equal("a", obj["Name"]);
            Assert.Equal("c", obj["Value"]);
            Assert.Equal(res.Id, obj["_id"]);
            Assert.Equal(res.Key, obj["_key"]);
            Assert.Equal(res.Revision, obj["_rev"]);
        }

        [Theory]
        [ClassData(typeof(PascalCaseData))]
        public async Task CreateReplaceMode(string serializer)
        {
            await SetupAsync(serializer);
            if ((await Arango.GetVersionAsync()).SemanticVersion < Version.Parse("3.7"))
                return;

            await Arango.Collection.CreateAsync("test", "test", ArangoCollectionType.Document);

            var createRes = await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Name = "a"
            });

            var res = await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Value = "c"
            }, overwriteMode: ArangoOverwriteMode.Replace);

            Assert.Equal(createRes.Id, res.Id);
            Assert.Equal(createRes.Key, res.Key);
            Assert.Equal(createRes.Revision, res.OldRevision);

            var obj = await Arango.Query.SingleOrDefaultAsync<Dictionary<string, string>>("test", "test", $"x._key == {"abc"}");

            Assert.DoesNotContain("Name", obj.Keys);
            Assert.Equal("c", obj["Value"]);
            Assert.Equal(res.Id, obj["_id"]);
            Assert.Equal(res.Key, obj["_key"]);
            Assert.Equal(res.Revision, obj["_rev"]);
        }

        [Theory]
        [ClassData(typeof(PascalCaseData))]
        public async Task CreateConflictMode(string serializer)
        {
            await SetupAsync(serializer);
            if ((await Arango.GetVersionAsync()).SemanticVersion < Version.Parse("3.7"))
                return;

            await Arango.Collection.CreateAsync("test", "test", ArangoCollectionType.Document);

            await Arango.Document.CreateAsync("test", "test", new
            {
                Key = "abc",
                Name = "a"
            });

            var exception = await Assert.ThrowsAsync<ArangoException>(async () =>
            {
                await Arango.Document.CreateAsync("test", "test", new
                {
                    Key = "abc",
                    Value = "c"
                }, overwriteMode: ArangoOverwriteMode.Conflict);
            });

            Assert.Contains("unique constraint", exception.Message);
            Assert.Collection<ArangoError>(exception.Errors, 
                error => Assert.Equal(ArangoErrorCode.ErrorArangoUniqueConstraintViolated, error.ErrorNumber)
            );
        }
    }
}