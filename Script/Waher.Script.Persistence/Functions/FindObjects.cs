﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Operators.Logical;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Finds objects in the object database.
	/// </summary>
	public class FindObjects : FunctionMultiVariate
	{
		private static readonly MethodInfo findMethodGeneric = GetFindMethod();

		/// <summary>
		/// Finds object in the object database.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="Offset">Offset</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Search filter.</param>
		/// <param name="SortOrder">Sort order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FindObjects(ScriptNode Type, ScriptNode Offset, ScriptNode MaxCount, ScriptNode Filter, ScriptNode SortOrder, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Type, Offset, MaxCount, Filter, SortOrder }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector }, 
				  Start, Length, Expression)
		{
		}

		private static MethodInfo GetFindMethod()
		{
			Type T = typeof(Database);
			foreach (MethodInfo MI in T.GetTypeInfo().GetDeclaredMethods("Find"))
			{
				ParameterInfo[] P = MI.GetParameters();
				if (P.Length != 4 ||
					P[0].ParameterType != typeof(int) ||
					P[1].ParameterType != typeof(int) ||
					P[2].ParameterType != typeof(Filter) ||
					P[3].ParameterType != typeof(string[]))
				{
					continue;
				}

				return MI;
			}

			return null;
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[]
				{
					"Type",
					"Offset",
					"MaxCount",
					"Filter",
					"SortOrder"
				};
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "FindObjects";
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is Type T))
				throw new ScriptRuntimeException("First parameter must be a type.", this);

			int Offset = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			int MaxCount = (int)Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			object FilterObj = Arguments[3].AssociatedObjectValue;
			Filter Filter = FilterObj as Filter;
			IVector V = Arguments[4] as IVector;
			int i, c = V.Dimension;
			string[] SortOrder = new string[c];

			for (i = 0; i < c; i++)
				SortOrder[i] = V.GetElement(i).AssociatedObjectValue.ToString();

			if (Filter is null && !(FilterObj is null))
			{
				Expression Exp = new Expression(FilterObj.ToString());
				Filter = this.Convert(Exp.Root, Variables);
			}

			MethodInfo MI = findMethodGeneric.MakeGenericMethod(new Type[] { T });
			object Result = MI.Invoke(null, new object[] { Offset, MaxCount, Filter, SortOrder });
			if (Result is Task Task)
			{
				Task.Wait();

				PropertyInfo PI = Task.GetType().GetRuntimeProperty("Result");
				Result = PI.GetMethod.Invoke(Task, null);
			}

			if (Result is IEnumerable E)
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();
				IEnumerator e = E.GetEnumerator();
				while (e.MoveNext())
					Elements.AddLast(Expression.Encapsulate(e.Current));

				return new ObjectVector(Elements);
			}
			else
				return Expression.Encapsulate(Result);
		}

		private Filter Convert(ScriptNode Node, Variables Variables)
		{
			string FieldName;
			object Value;

			if (Node is null)
				return null;
			else if (Node is And And)
				return new FilterAnd(this.Convert(And.LeftOperand, Variables), this.Convert(And.RightOperand, Variables));
			else if (Node is Or Or)
				return new FilterOr(this.Convert(Or.LeftOperand, Variables), this.Convert(Or.RightOperand, Variables));
			else if (Node is Operators.Dual.And And2)
				return new FilterAnd(this.Convert(And2.LeftOperand, Variables), this.Convert(And2.RightOperand, Variables));
			else if (Node is Operators.Dual.Or Or2)
				return new FilterOr(this.Convert(Or2.LeftOperand, Variables), this.Convert(Or2.RightOperand, Variables));
			else if (Node is Not Not)
				return new FilterNot(this.Convert(Not.Operand, Variables));
			else if (Node is EqualTo EQ)
			{
				this.CheckBinaryOperator(EQ, Variables, out FieldName, out Value);
				return new FilterFieldEqualTo(FieldName, Value);
			}
			else if (Node is NotEqualTo NEQ)
			{
				this.CheckBinaryOperator(NEQ, Variables, out FieldName, out Value);
				return new FilterFieldNotEqualTo(FieldName, Value);
			}
			else if (Node is LesserThan LT)
			{
				this.CheckBinaryOperator(LT, Variables, out FieldName, out Value);
				return new FilterFieldLesserThan(FieldName, Value);
			}
			else if (Node is GreaterThan GT)
			{
				this.CheckBinaryOperator(GT, Variables, out FieldName, out Value);
				return new FilterFieldGreaterThan(FieldName, Value);
			}
			else if (Node is LesserThanOrEqualTo LTE)
			{
				this.CheckBinaryOperator(LTE, Variables, out FieldName, out Value);
				return new FilterFieldLesserOrEqualTo(FieldName, Value);
			}
			else if (Node is GreaterThanOrEqualTo GTE)
			{
				this.CheckBinaryOperator(GTE, Variables, out FieldName, out Value);
				return new FilterFieldGreaterOrEqualTo(FieldName, Value);
			}
			else if (Node is Range Range)
			{
				this.CheckTernaryOperator(Range, Variables, out FieldName, 
					out object Min, out object Max);

				Filter[] Filters = new Filter[2];

				if (Range.LeftInclusive)
					Filters[0] = new FilterFieldGreaterOrEqualTo(FieldName, Min);
				else
					Filters[0] = new FilterFieldGreaterThan(FieldName, Min);

				if (Range.RightInclusive)
					Filters[1] = new FilterFieldLesserOrEqualTo(FieldName, Max);
				else
					Filters[1] = new FilterFieldLesserThan(FieldName, Max);

				return new FilterAnd(Filters);
			}
			else if (Node is Like)
			{
				this.CheckBinaryOperator((BinaryOperator)Node, Variables, out FieldName, out Value);
				string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "*");
				return new FilterFieldLikeRegEx(FieldName, RegEx);
			}
			else if (Node is NotLike)
			{
				this.CheckBinaryOperator((BinaryOperator)Node, Variables, out FieldName, out Value);
				string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "*");
				return new FilterNot(new FilterFieldLikeRegEx(FieldName, RegEx));
			}
			else
				throw new ScriptRuntimeException("Invalid operation for filters: " + Node.GetType().FullName, this);
		}

		private void CheckBinaryOperator(BinaryOperator Operator, Variables Variables, out string FieldName, out object Value)
		{
			if (!(Operator.LeftOperand is VariableReference v))
				throw new ScriptRuntimeException("Left operands in binary filter operators need to be a variable references, as they refer to field names.", this);

			FieldName = v.VariableName;
			Value = Operator.RightOperand.Evaluate(Variables).AssociatedObjectValue;
		}

		private void CheckTernaryOperator(TernaryOperator Operator, Variables Variables, out string FieldName, out object Left, out object Right)
		{
			if (!(Operator.MiddleOperand is VariableReference v))
				throw new ScriptRuntimeException("Middle operands in ternary filter operators need to be a variable references, as they refer to field names.", this);

			FieldName = v.VariableName;
			Left = Operator.LeftOperand.Evaluate(Variables).AssociatedObjectValue;
			Right = Operator.RightOperand.Evaluate(Variables).AssociatedObjectValue;
		}
	}
}
