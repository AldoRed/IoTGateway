﻿using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Converts a <see cref="Measurement"/> to a double number.
	/// </summary>
	public class MeasurementToDouble : ITypeConverter
	{
		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(Measurement);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(double);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is Measurement Q)
				return Q.Magnitude;
			else
				throw new ArgumentException("Expected Measurement value.", nameof(Value));
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>, encapsulated in an <see cref="IElement"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public IElement ConvertToElement(object Value)
		{
			if (Value is Measurement Q)
				return new DoubleNumber(Q.Magnitude);
			else
				throw new ArgumentException("Expected Measurement value.", nameof(Value));
		}
	}
}
