﻿using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.ServerErrors
{
	/// <summary>
	/// Throws a <see cref="NotImplementedException"/>
	/// </summary>
	public class NotImplemented : FunctionOneVariable
	{
		/// <summary>
		/// Throws a <see cref="NotImplementedException"/>
		/// </summary>
		/// <param name="Content">Content to return</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NotImplemented(ScriptNode Content, int Start, int Length, Expression Expression)
			: base(Content, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "NotImplemented";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			throw new NotImplementedException(Argument.AssociatedObjectValue);
		}
	}
}
