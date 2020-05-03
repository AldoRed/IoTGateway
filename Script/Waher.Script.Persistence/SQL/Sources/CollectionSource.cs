﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data Source defined by a collection name
	/// </summary>
	public class CollectionSource : IDataSource
	{
		private readonly Dictionary<string, bool> isLabel = new Dictionary<string, bool>();
		private readonly string collectionName;
		private readonly string alias;

		/// <summary>
		/// Data Source defined by a collection name
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Alias">Alias for source.</param>
		public CollectionSource(string CollectionName, string Alias)
		{
			this.collectionName = CollectionName;
			this.alias = Alias;
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			return Find(this.collectionName, Offset, Top, Where, Variables, Order, Node,
				this.alias ?? this.collectionName);
		}

		internal static async Task<IResultSetEnumerator> Find(string Collection, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node, string Name)
		{
			object[] FindParameters = new object[] { Collection, Offset, Top, 
				TypeSource.Convert(Where, Variables, Name), TypeSource.Convert(Order) };
			object Obj = FindMethod.Invoke(null, FindParameters);
			if (!(Obj is Task Task))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			await Task;

			PropertyInfo PI = Task.GetType().GetRuntimeProperty("Result");
			if (PI is null)
				throw new ScriptRuntimeException("Unexpected response.", Node);

			Obj = PI.GetValue(Task);
			if (!(Obj is IEnumerable Enumerable))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			return new SynchEnumerator(Enumerable.GetEnumerator());
		}

		private static MethodInfo findMethod = null;

		/// <summary>
		/// Generic object database Find method: <see cref="Database.Find(string, int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo FindMethod
		{
			get
			{
				if (findMethod is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("Find"))
					{
						if (MI.ContainsGenericParameters)
							continue;

						ParameterInfo[] Parameters = MI.GetParameters();
						if (Parameters.Length != 5 ||
							Parameters[0].ParameterType != typeof(string) ||
							Parameters[1].ParameterType != typeof(int) ||
							Parameters[2].ParameterType != typeof(int) ||
							Parameters[3].ParameterType != typeof(Filter) ||
							Parameters[4].ParameterType != typeof(string[]))
						{
							continue;
						}

						findMethod = MI;
					}

					if (findMethod is null)
						throw new InvalidOperationException("Appropriate Database.Find method not found.");
				}

				return findMethod;
			}
		}

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		public Task Update(IEnumerable<object> Objects)
		{
			return Database.Update(Objects);
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			return Database.Delete(Objects);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(object Object)
		{
			return Database.Insert(Object);
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get { return this.collectionName; }
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Checks if the name refers to the source.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name refers to the source.</returns>
		public bool IsSource(string Name)
		{
			return
				string.Compare(this.collectionName, Name, true) == 0 ||
				string.Compare(this.alias, Name, true) == 0;
		}

		/// <summary>
		/// Checks if the label is a label in the source.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>If the label is a label in the source.</returns>
		public async Task<bool> IsLabel(string Label)
		{
			bool Result;

			lock (this.isLabel)
			{
				if (this.isLabel.TryGetValue(Label, out Result))
					return Result;
			}

			Result = await Database.IsLabel(this.collectionName, Label);

			lock (this.isLabel)
			{
				this.isLabel[Label] = Result;
			}

			return Result;
		}

	}
}
