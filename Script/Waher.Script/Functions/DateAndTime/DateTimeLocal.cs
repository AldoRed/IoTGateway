﻿using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
	/// <summary>
	/// Creates a Local DateTime value.
	/// </summary>
	public class DateTimeLocal : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a Local DateTime value.
		/// </summary>
		/// <param name="String">String representation to be parsed.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeLocal(ScriptNode String, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeLocal(ScriptNode Year, ScriptNode Month, ScriptNode Day, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeLocal(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second }, argumentTypes6Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="MSecond">Millisecond</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeLocal(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second,
			ScriptNode MSecond, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second, MSecond }, argumentTypes7Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(DateTimeLocal);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Year", "Month", "Day", "Hour", "Minute", "Second", "MSecond" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int i, c = this.Arguments.Length;

			if (c == 1)
			{
				object Obj = Arguments[0].AssociatedObjectValue;

				if (Obj is long L)
					return new DateTimeValue(new System.DateTime(L, DateTimeKind.Local));
				else if (Obj is double Dbl)
					return new DateTimeValue(new System.DateTime((long)Dbl, DateTimeKind.Local));
				else if (!(Obj is null) && DateTime.TryParse(Obj.ToString(), out System.DateTime TP))
				{
					if (TP.Kind != DateTimeKind.Local)
						TP = new System.DateTime(TP.Year, TP.Month, TP.Day, TP.Hour, TP.Minute, TP.Second, TP.Millisecond, DateTimeKind.Local);
					
					return new DateTimeValue(TP);
				}
				else
					throw new ScriptRuntimeException("Unable to parse DateTime value.", this);
			}

			double[] d = new double[c];
			DoubleNumber n;

			for (i = 0; i < c; i++)
			{
				n = Arguments[i] as DoubleNumber;
				if (n is null)
					throw new ScriptRuntimeException("Expected number arguments.", this);

				d[i] = n.Value;
			}

			switch (c)
			{
				case 3:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2], 0, 0, 0, DateTimeKind.Local));

				case 6:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5], DateTimeKind.Local));

				case 7:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5], (int)d[6], DateTimeKind.Local));

				default:
					throw new ScriptRuntimeException("Invalid number of parameters.", this);
			}
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			object Obj = CheckAgainst.AssociatedObjectValue;

			if (!(Obj is System.DateTime TP))
			{
				if (Obj is double d)
					TP = DateTime.FromInteger((long)d, DateTimeKind.Local);
				else
				{
					string s = CheckAgainst.AssociatedObjectValue?.ToString() ?? string.Empty;

					if (!System.DateTime.TryParse(s, out TP))
					{
						if (long.TryParse(s, out long Ticks))
							TP = DateTime.FromInteger(Ticks, DateTimeKind.Local);
						else
							return PatternMatchResult.NoMatch;
					}
				}
			}

			TP = TP.ToLocalTime();

			int c = this.Arguments.Length;
			if (c == 1)
				return this.Arguments[0].PatternMatch(new DateTimeValue(TP), AlreadyFound);

			if (c < 3)
				return PatternMatchResult.NoMatch;

			PatternMatchResult Result = this.Arguments[0].PatternMatch(new DoubleNumber(TP.Year), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[1].PatternMatch(new DoubleNumber(TP.Month), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[2].PatternMatch(new DoubleNumber(TP.Day), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c == 3)
				return TP.TimeOfDay == System.TimeSpan.Zero ? PatternMatchResult.Match : PatternMatchResult.NoMatch;

			if (c < 6)
				return PatternMatchResult.NoMatch;

			Result = this.Arguments[3].PatternMatch(new DoubleNumber(TP.Hour), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[4].PatternMatch(new DoubleNumber(TP.Minute), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[5].PatternMatch(new DoubleNumber(TP.Second), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c == 6)
				return TP.Millisecond == 0 ? PatternMatchResult.Match : PatternMatchResult.NoMatch;

			if (c != 7)
				return PatternMatchResult.NoMatch;

			return this.Arguments[6].PatternMatch(new DoubleNumber(TP.Millisecond), AlreadyFound);
		}
	}
}
