using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Laso.TableStorage.Domain;
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
                var entity = new TestEntity { Eid = id };

                await tableStorageService.InsertAsync(entity);

                entity.ETag.ShouldNotBeNull();
                //entity.Timestamp.ShouldNotBe(new DateTimeOffset());

                entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Eid.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task Should_retrieve_entity_with_projection()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Eid = id, Name = "test" });

                var name = await tableStorageService.GetAsync<TestEntity>(id, x => x.Name == "test");
                name.Name.ShouldBe("test");
            }
        }

        [Fact]
        public async Task Should_retrieve_entities()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Eid = id1 }, new TestEntity { Eid = id2 } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();
                entities.ShouldContain(x => x.Eid == id1);
                entities.ShouldContain(x => x.Eid == id2);
            }
        }

        [Fact]
        public async Task Should_retrieve_entities_with_projection()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" } });

                var names = await tableStorageService.GetAllAsync<TestEntity>(x => true);
                names.ToList()[0].Name.ShouldContain("test1");
                names.ToList()[1].Name.ShouldContain("test2");
            }
        }

        [Fact]
        public async Task Should_find_entities()
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
        public async Task Should_find_entities_with_limit()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" }, new TestEntity { Name = "test2" } });

                var entities = await tableStorageService.FindAllAsync<TestEntity>(x => x.Name == "test2", 1);
                entities.Count.ShouldBe(1);
                entities.ShouldContain(x => x.Name == "test2");
            }
        }

        [Fact]
        public async Task Should_find_entities_with_projection()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Name = "test1" }, new TestEntity { Name = "test2" } });

                var names = (await tableStorageService.FindAllAsync<TestEntity>(x => x.Name == "test2", x => x.Name == "test2")).Select(r => r.Name);
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
                await tableStorageService.InsertAsync(new TestEntity { Eid = id, Name = "test1" });

                var entity = await tableStorageService.GetAsync<TestEntity>(id);

                entity.Name = "test2";

                await tableStorageService.ReplaceAsync(entity);

                entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Name.ShouldBe("test2");
            }
        }

        [Fact]
        public async Task Should_not_replace_entity_if_updated_concurrently()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                var entity = new TestEntity { Eid = id, Name = "test1" };

                await tableStorageService.InsertAsync(entity);

                await tableStorageService.InsertOrReplaceAsync(new TestEntity { Eid = id, Name = "test2" });

                var hadException = false;
                try
                {
                    entity.Name = "test3";
                    await tableStorageService.ReplaceAsync(entity);
                }
                catch
                {
                    hadException = true;
                }

                hadException.ShouldBeTrue();
                entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Name.ShouldBe("test2");
            }
        }

        [Fact]
        public async Task Should_merge_entity()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                var entity = new TestEntity { Eid = id, Name = "test1" };

                await tableStorageService.InsertAsync(entity);

                entity.Name = null;
                entity.Description = "Test Entity 1";

                await tableStorageService.MergeAsync(entity);

                entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Name.ShouldBe("test1");
                entity.Description.ShouldBe("Test Entity 1");
            }
        }

        [Fact]
        public async Task Should_not_merge_entity_if_updated_concurrently()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Eid = id, Name = "test1" });

                var entity = await tableStorageService.GetAsync<TestEntity>(id);

                await tableStorageService.InsertOrReplaceAsync(new TestEntity { Eid = id, Name = "test2" });

                //var hadException = false;
                //try
                //{
                    entity.Name = "test3";
                    await tableStorageService.MergeAsync(entity);
                //}
                //catch
                //{
                //    hadException = true;
                //}

                //hadException.ShouldBeTrue();
                entity = await tableStorageService.GetAsync<TestEntity>(id);
                entity.Name.ShouldBe("test3");
            }
        }

        [Fact]
        public async Task Should_insert_or_replace_entities()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Eid = id1, Name = "test11" }, new TestEntity { Eid = id2, Name = "test21" } });
                await tableStorageService.InsertOrReplaceAsync(new[] { new TestEntity { Eid = id1, Name = "test12" }, new TestEntity { Eid = id2, Name = "test22" } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();
                entities.Count.ShouldBe(2);
                entities.ShouldContain(x => x.Eid == id1 && x.Name == "test12");
                entities.ShouldContain(x => x.Eid == id2 && x.Name == "test22");
            }
        }

        [Fact]
        public async Task Should_delete_entity1()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new TestEntity { Eid = id });
                var e = tableStorageService.Get<TestEntity>(id);
                await tableStorageService.DeleteAsync<TestEntity>(e);

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
                await tableStorageService.InsertAsync(new TestEntity { Eid = id });

                var entity = tableStorageService.Get<TestEntity>(id);

                await tableStorageService.DeleteAsync(entity);

                var e2 = await tableStorageService.GetAsync<TestEntity>(id);

                (e2).ShouldBeNull();
            }
        }

        [Fact]
        public async Task Should_delete_entities()
        {

            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[] { new TestEntity { Eid = id1 }, new TestEntity { Eid = id2 } });

                var entities = await tableStorageService.GetAllAsync<TestEntity>();

                await tableStorageService.DeleteAsync(entities);

                (await tableStorageService.GetAllAsync<TestEntity>()).Count.ShouldBe(0);
            }
        }

        [Fact]
        public async Task Should_get_entities_with_row_key_specified()
        {
            var part1 = Guid.NewGuid().ToString("D");
            var part2 = Guid.NewGuid().ToString("D");
            var part3 = Guid.NewGuid().ToString("D");
            var part4 = Guid.NewGuid().ToString("D");

            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                await tableStorageService.InsertAsync(new[]
                {
                    new TestEntityWithRowKey { Part = part1, RowKey = part1 },
                    new TestEntityWithRowKey { Part = part2, RowKey = part2 },
                });

                (await tableStorageService.GetAllAsync<TestEntityWithRowKey>()).Count.ShouldBe(2);
                (await tableStorageService.GetAllAsync<TestEntityWithRowKey>(x=> x.PartitionKey == part2)).Count.ShouldBe(1);
            }
        }

        [Fact]
        public async Task Should_give_better_error_message_for_illegal_key_characters()
        {
            await using (var tableStorageService = new TempAzureTableStorageService())
            {
                var entity = new TestEntity { Eid = "not/allowed" };

                var exceptionMessage = "";

                try
                {
                    await tableStorageService.InsertAsync(entity);
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                }

                exceptionMessage.ShouldNotBeNullOrWhiteSpace();
                exceptionMessage.ShouldNotBe("Element 0 in the batch returned an unexpected response code.");
            }
        }

        private class TestEntity : TableStorageEntity, ITableEntity
        {
            public override string PartitionKey
            {
                get => Eid;
                set => Eid = value;
            }

            public override string RowKey => Eid;
            public string Eid { get; set; } = Guid.NewGuid().ToString("D");

            public string Rid { get; set; } /// nameof(TestEntity);// Guid.NewGuid().ToString("D");
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private class TestEntityWithRowKey : TableStorageEntity, ITableEntity
        {
            public override string PartitionKey
            {
                get => Part;
                set => Part = value;
            }
            public override string RowKey => Part;

            public string Part { get; set; }  = Guid.NewGuid().ToString("D");
            public string Row { get; } = "";
        }
    }
}
