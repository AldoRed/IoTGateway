﻿using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Performs a sequence of type conversions to convert an object from one type to another.
	/// </summary>
	public class ConversionSequence : ITypeConverter
	{
		private readonly ITypeConverter[] converters;
		private readonly Type from;
		private readonly Type to;
		private readonly int c;

		/// <summary>
		/// Performs a sequence of type conversions to convert an object from one type to another.
		/// </summary>
		/// <param name="Converters">Sequence of converters.</param>
		public ConversionSequence(params ITypeConverter[] Converters)
		{
			this.converters = Converters;
			this.c = this.converters.Length;
			this.from = this.converters[0].From;
			this.to = this.converters[this.c - 1].To;
		}

		/// <summary>
		/// Sequence of converters.
		/// </summary>
		public ITypeConverter[] Converters => this.converters;

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => this.from;

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => this.to;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>. If conversion not possible, null is returned.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			object Value2;
			int i;

			for (i = 0; i < c; i++)
			{
				Value2 = this.converters[i].Convert(Value);
				if (Value2 is null && !(Value is null))
					return null;

				Value = Value2;
			}

			return Value;
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>, encapsulated in an <see cref="IElement"/>.
		/// If conversion not possible, null is returned.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public IElement ConvertToElement(object Value)
		{
			int i;
			int c1 = c - 1;
			object Value2;

			for (i = 0; i < c1; i++)
			{
				Value2 = this.converters[i].Convert(Value);
				if (Value2 is null && !(Value is null))
					return null;

				Value = Value2;
			}

			return this.converters[c1].ConvertToElement(Value);
		}
	}
}
