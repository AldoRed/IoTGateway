﻿using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// Direct reference to a data source.
	/// </summary>
	public class SourceReference : SourceDefinition
	{
		private ScriptNode source;
		private ScriptNode alias;

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, int Start, int Length, Expression Expression)
			: this(Source, null, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Alias">Alias definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, ScriptNode Alias, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.alias = Alias;
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override async Task<IDataSource> GetSource(Variables Variables)
		{
			string Alias;

			if (this.alias is null)
				Alias = string.Empty;
			else if (this.alias is VariableReference Ref)
				Alias = Ref.VariableName;
			else
				Alias = (await this.alias.EvaluateAsync(Variables)).AssociatedObjectValue?.ToString();

			if (this.source is VariableReference Ref2)
				return GetDataSource(Ref2, Alias, Variables);
			else
			{
				if (this.source is IEvaluateAsync AsyncSource)
					return GetDataSource(string.Empty, Alias, await AsyncSource.EvaluateAsync(Variables), this.source);
				else
					return GetDataSource(string.Empty, Alias, await this.source.EvaluateAsync(Variables), this.source);
			}
		}

		private static IDataSource GetDataSource(string Name, string Alias, IElement E, ScriptNode Source)
		{
			if (E.AssociatedObjectValue is Type T)
				return new TypeSource(T, Alias);
			else if (E is StringValue S)
				return new CollectionSource(S.Value, Alias);
			else if (E is ObjectMatrix OM && OM.HasColumnNames)
				return new VectorSource(Name, Alias, VectorSource.ToGenericObjectVector(OM), Source);
			else if (E is IVector V)
				return new VectorSource(Name, Alias, V, Source);
			else if (E is ObjectValue Value)
			{
				if (Value.AssociatedObjectValue is XmlDocument Doc)
					return new XmlSource(Name, Alias, Doc, Source);
				else if (Value.AssociatedObjectValue is XmlNode N)
					return new XmlSource(Name, Alias, N, Source);
				else if (Value.AssociatedObjectValue is IDataSource DataSource)
					return DataSource;
			}

			throw new ScriptRuntimeException("Data source type not supported: " + E.AssociatedObjectValue?.GetType()?.FullName, Source);
		}

		private static IDataSource GetDataSource(VariableReference Source, string Alias, Variables Variables)
		{
			string Name = Source.VariableName;

			if (Variables.TryGetVariable(Name, out Variable v))
				return GetDataSource(Name, Alias, v.ValueElement, Source);

			if (Expression.TryGetConstant(Name, Variables, out IElement ValueElement))
				return GetDataSource(Name, Alias, ValueElement, Source);

			if (Types.TryGetQualifiedNames(Name, out string[] QualifiedNames))
			{
				if (QualifiedNames.Length == 1)
				{
					Type T = Types.GetType(QualifiedNames[0]);

					if (!(T is null))
						return new TypeSource(T, Alias);
				}
			}

			return new CollectionSource(Name, Alias);
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
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			ScriptNode NewNode;
			bool b;

			if (!(this.source is null))
			{
				b = !Callback(this.source, out NewNode, State);
				if (!(NewNode is null))
					this.source = NewNode;

				if (b)
					return false;
			}

			if (!(this.alias is null))
			{
				b = !Callback(this.alias, out NewNode, State);
				if (!(NewNode is null))
					this.alias = NewNode;

				if (b)
					return false;
			}

			if (!DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is SourceReference O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.alias, O.alias) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.alias);

			return Result;
		}

	}
}
