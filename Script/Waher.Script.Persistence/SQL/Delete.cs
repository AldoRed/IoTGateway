﻿using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a DELETE statement against the object database.
	/// </summary>
	public class Delete : ScriptNode
	{
		private SourceDefinition source;
		private ScriptNode where;

		/// <summary>
		/// Executes a DELETE statement against the object database.
		/// </summary>
		/// <param name="Source">Source to delete from.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Delete(SourceDefinition Source, ScriptNode Where, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.where = Where;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IDataSource Source = this.source.GetSource(Variables);
			IEnumerator e = Source.Find(0, int.MaxValue, this.where, Variables, new	KeyValuePair<VariableReference, bool>[0], this);
			LinkedList<object> ToDelete = new LinkedList<object>();
			int Count = 0;

			while (e.MoveNext())
			{
				ToDelete.AddLast(e.Current);
				Count++;
			}

			Source.Delete(ToDelete);

			return new DoubleNumber(Count);
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			ScriptNode Node = this.source;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.source)
			{
				if (Node is SourceDefinition Source2)
					this.source = Source2;
				else
					return false;
			}

			if (!(this.where is null) && !Callback(ref this.where, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				if (!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Delete O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.where, O.where) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			return Result;
		}


	}
}
