﻿using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Shading
{
	/// <summary>
	/// Generates a <see cref="Graphs3D.PhongMaterial"/> object.
	/// </summary>
	public class PhongMaterial : FunctionMultiVariate
	{
		/// <summary>
		/// Generates a <see cref="Graphs3D.PhongMaterial"/> object.
		/// </summary>
		/// <param name="AmbientReflectionConstant">Ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="DiffuseReflectionConstant">Ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="SpecularReflectionConstant">Ratio of reflection of the specular term of incoming light.</param>
		/// <param name="Shininess">Larger for surfaces that are smoother and more mirror-like.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public PhongMaterial(ScriptNode AmbientReflectionConstant,
			ScriptNode DiffuseReflectionConstant, ScriptNode SpecularReflectionConstant,
			ScriptNode Shininess, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { AmbientReflectionConstant, DiffuseReflectionConstant, SpecularReflectionConstant, Shininess},
				  argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(PhongMaterial);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] 
		{
			"AmbientReflectionConstant",
			"DiffuseReflectionConstant",
			"SpecularReflectionConstant",
			"Shininess"
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return new ObjectValue(new Graphs3D.PhongMaterial(
				(float)Expression.ToDouble(Arguments[0].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[1].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[2].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[3].AssociatedObjectValue)));
		}
	}
}
