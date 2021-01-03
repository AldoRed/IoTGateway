﻿using System;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all unary operators.
	/// </summary>
	public abstract class UnaryOperator : ScriptNode
	{
		/// <summary>
		/// Operand.
		/// </summary>
		protected ScriptNode op;

		/// <summary>
		/// Base class for all unary operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UnaryOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.op = Operand;
		}

		/// <summary>
		/// Operand.
		/// </summary>
		public ScriptNode Operand
		{
			get { return this.op; }
		}

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public virtual string DefaultVariableName
		{
			get
			{
				if (this.op is IDifferentiable Differentiable)
					return Differentiable.DefaultVariableName;
				else
					return null;
			}
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
				if (!(this.op?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			if (!(this.op is null) && !Callback(ref this.op, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.op?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is UnaryOperator O &&
				this.op.Equals(O.op) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.op.GetHashCode();
			return Result;
		}

	}
}
