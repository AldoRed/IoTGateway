﻿using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Fractals
{
    public class JuliaRoot1Variation : FlameVariationOneComplexParameter
    {
        public JuliaRoot1Variation(ScriptNode Parameter1, ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public JuliaRoot1Variation(ScriptNode Parameter1, int Start, int Length, Expression Expression)
			: base(Parameter1, null, Start, Length, Expression)
		{
		}

		private JuliaRoot1Variation(Complex z, ScriptNode Parameter, int Start, int Length, Expression Expression)
            : base(z, Parameter, Start, Length, Expression)
        {
        }

        private JuliaRoot1Variation(double Re, double Im, ScriptNode Parameter1, ScriptNode Parameter2,
            int Start, int Length, Expression Expression)
            : base(Re, Im, Parameter1, Parameter2, Start, Length, Expression)
        {
        }

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments[1] is null || Arguments[1].AssociatedObjectValue is null)
			{
				return new JuliaRoot1Variation(Expression.ToComplex(Arguments[0].AssociatedObjectValue),
					this.Arguments[0], this.Start, this.Length, this.Expression);
			}
			else
			{
				return new JuliaRoot1Variation(
					Expression.ToDouble(Arguments[0].AssociatedObjectValue),
					Expression.ToDouble(Arguments[1].AssociatedObjectValue),
					this.Arguments[0], this.Arguments[1], Start, Length, this.Expression);
			}
		}

		public override void Operate(ref double x, ref double y)
        {
            // sqrt(x+iy-z)

            double re = x - this.re;
            double im = y - this.im;

            double argz = Math.Atan2(im, re);
            double amp = Math.Pow(re * re + im * im, 0.25);
            double phi = 0.5 * argz;

            x = amp * Math.Cos(phi);
            y = amp * Math.Sin(phi);
        }

        public override string FunctionName
        {
            get { return "JuliaRoot1Variation"; }
        }
    }
}
