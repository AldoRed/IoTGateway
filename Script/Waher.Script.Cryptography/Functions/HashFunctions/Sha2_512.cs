﻿using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Script.Cryptography.Functions.HashFunctions
{
	/// <summary>
	/// Sha2_512(Data)
	/// </summary>
	public class Sha2_512 : FunctionOneScalarVariable
	{
		/// <summary>
		/// Sha2_512(Data)
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sha2_512(ScriptNode Data, int Start, int Length, Expression Expression)
			: base(Data, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Sha2_512);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			if (!(Argument.AssociatedObjectValue is byte[] Bin))
			{
				if (!(Argument.AssociatedObjectValue is string s))
					s = Expression.ToString(Argument.AssociatedObjectValue);

				return this.EvaluateScalar(s, Variables);
			}

			return new ObjectValue(Hashes.ComputeSHA512Hash(Bin));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			byte[] Bin = System.Text.Encoding.UTF8.GetBytes(Argument);
			return new ObjectValue(Hashes.ComputeSHA512Hash(Bin));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}

	}
}
