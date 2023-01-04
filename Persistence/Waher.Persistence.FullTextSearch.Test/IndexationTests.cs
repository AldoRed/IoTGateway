using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class IndexationTests
	{
		private static FilesProvider? filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(FullTextSearchModule).Assembly,
				typeof(TestClass).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			Log.Register(new ConsoleEventSink());

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			Log.Terminate();

			await Types.StopAllModules();

			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestMethod]
		public async Task Test_01_InsertObject()
		{
			await this.CreateInstance();
		}

		private async Task<TestClass> CreateInstance()
		{
			TaskCompletionSource<bool> Result = new();
			Task ObjectIndexed(object Sender, ObjectReferenceEventArgs e)
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			};

			Search.ObjectAddedToIndex += ObjectIndexed;
			try
			{
				TestClass Obj = new()
				{
					IndexedProperty1 = "Hello World.",
					IndexedProperty2 = "Kilroy was here.",
					NonIndexedProperty1 = "This is a test.",
					NonIndexedProperty2 = "Testing indexation."
				};

				await Database.Insert(Obj);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);

				return Obj;
			}
			finally
			{
				Search.ObjectAddedToIndex -= ObjectIndexed;
			}
		}

		[TestMethod]
		public async Task Test_02_Search()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, 10,
				FullTextSearchOrder.Relevance, "Hello", "Clown", "Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.IsTrue(SearchResult.Length > 0, "No objects found. (Make sure you've run test 01 first, to insert at least one object.)");
		}

		[TestMethod]
		public async Task Test_03_DeleteObject()
		{
			await this.DeleteAllObjects(true);
		}

		private async Task DeleteAllObjects(bool ExceptionIfNone)
		{
			IEnumerable<TestClass> Objects = await Database.Find<TestClass>();
			int NrObjects = 0;

			foreach (TestClass Obj in Objects)
				NrObjects++;

			if (NrObjects == 0)
			{
				if (ExceptionIfNone)
					Assert.Fail("No objects to delete. (Make sure you've run test 01 first, to insert at least one object.)");

				return;
			}

			TaskCompletionSource<bool> Result = new();
			Task ObjectRemoved(object Sender, ObjectReferenceEventArgs e)
			{
				if (--NrObjects == 0)
					Result.TrySetResult(true);

				return Task.CompletedTask;
			};

			Search.ObjectRemovedFromIndex += ObjectRemoved;
			try
			{
				await Database.Delete(Objects);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);
			}
			finally
			{
				Search.ObjectRemovedFromIndex -= ObjectRemoved;
			}

			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, 10,
				FullTextSearchOrder.Relevance, "Hello", "Clown", "Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(0, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_04_UpdateObject()
		{
			await this.DeleteAllObjects(false);

			TestClass Obj = await this.CreateInstance();
			TaskCompletionSource<bool> Result = new();
			Task ObjectUpdated(object Sender, ObjectReferenceEventArgs e)
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			};

			Search.ObjectUpdatedInIndex += ObjectUpdated;
			try
			{
				Obj.IndexedProperty2 = "Roy was here.";

				await Database.Update(Obj);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);
			}
			finally
			{
				Search.ObjectUpdatedInIndex -= ObjectUpdated;
			}

			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, 10,
				FullTextSearchOrder.Relevance, "Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(0, SearchResult.Length);

			SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, 10,
				FullTextSearchOrder.Relevance, "Roy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(1, SearchResult.Length);
		}

		// TODO: GenericObject tests
		// TODO: Script-only tests
		// TODO: With/without accents in search
		// TODO: Tokens beginning with search keyword
		// TODO: Pagination & Multiple blocks/pages (> 100 objects)
		// TODO: Orders (> 100 objects)
		// TODO: Relevance, Occurrence, NewToOld, OldToNew
	}
}