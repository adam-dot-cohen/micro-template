using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Laso.TableStorage.Tests
{
    public class AzureTableStorageServiceTests
    {
        [Fact]
        public async Task Should_insert_and_retrieve_entity()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Id = id });

                var entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Id.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task Should_insert_and_retrieve_entity_with_projection()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Id = id, Name = "test" });

                var name = await tableStorageService.GetAsync<TestEntity, string>(id, x => x.Name);
                name.ShouldBe("test");
            }
        }

        [Fact]
        public async Task Should_insert_and_retrieve_entities()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Id = id1 }, new TestEntity { Id = id2 } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();
                entities.ShouldContain(x => x.Id == id1);
                entities.ShouldContain(x => x.Id == id2);
            }
        }

        [Fact]
        public async Task Should_insert_and_retrieve_entities_with_projection()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" } });

                var names = await tableStorageService.GetAllAsync<TestEntity, string>(x => x.Name);
                names.ShouldContain("test1");
                names.ShouldContain("test2");
            }
        }

        [Fact]
        public async Task Should_insert_and_find_entities()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" } });

                var entities = await tableStorageService.FindAllAsync<TestEntity>(x => x.Name == "test2");
                entities.ShouldNotContain(x => x.Name == "test1");
                entities.ShouldContain(x => x.Name == "test2");
            }
        }

        [Fact]
        public async Task Should_insert_and_find_entities_with_projection()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" } });

                var names = await tableStorageService.FindAllAsync<TestEntity, string>(x => x.Name == "test2", x => x.Name);
                names.ShouldNotContain("test1");
                names.ShouldContain("test2");
            }
        }

        [Fact]
        public async Task Should_replace_entity()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertOrReplaceAsync(new TestEntity { Id = id, Name = "test1" });
                await tableStorageService.InsertOrReplaceAsync(new TestEntity { Id = id, Name = "test2" });

                var entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Name.ShouldBe("test2");
            }
        }

        [Fact]
        public async Task Should_replace_entities()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertOrReplaceAsync(new[] { new TestEntity { Id = id1, Name = "test11" }, new TestEntity { Id = id2, Name = "test21" } });
                await tableStorageService.InsertOrReplaceAsync(new[] { new TestEntity { Id = id1, Name = "test12" }, new TestEntity { Id = id2, Name = "test22" } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();
                entities.Count.ShouldBe(2);
                entities.ShouldContain(x => x.Id == id1 && x.Name == "test12");
                entities.ShouldContain(x => x.Id == id2 && x.Name == "test22");
            }
        }

        [Fact]
        public async Task Should_delete_entity1()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Id = id });
                await tableStorageService.DeleteAsync<TestEntity>(id);

                var entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.ShouldBeNull();
            }
        }

        [Fact]
        public async Task Should_delete_entity2()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Id = id });

                var entity = await tableStorageService.GetAsync<TestEntity>(id);

                await tableStorageService.DeleteAsync(entity);

                (await tableStorageService.GetAsync<TestEntity>(id)).ShouldBeNull();
            }
        }

        [Fact]
        public async Task Should_delete_entities()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Id = id1 }, new TestEntity { Id = id2 } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();

                await tableStorageService.DeleteAsync(entities);

                (await tableStorageService.GetAllAsync<TestEntity>()).Count.ShouldBe(0);
            }
        }

        private class TestEntity : TableStorageEntity
        {
            public override string PartitionKey => Id;
            public override string RowKey => string.Empty;

            public string Id { get; set; } = Guid.NewGuid().ToString("D");
            public string Name { get; set; }
        }
    }
}
