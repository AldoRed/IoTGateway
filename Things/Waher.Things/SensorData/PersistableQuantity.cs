﻿using System.Text;
using Waher.Content;
using Waher.Persistence.Attributes;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Persistable quantity object.
	/// </summary>
	public class PersistableQuantity
	{
		/// <summary>
		/// Persistable quantity object.
		/// </summary>
		public PersistableQuantity()
		{
		}

		/// <summary>
		/// Persistable quantity object.
		/// </summary>
		/// <param name="Value">Magnitude</param>
		/// <param name="Unit">Unit</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		public PersistableQuantity(double Value, string Unit, byte NrDecimals)
		{
			this.Value = Value;
			this.Unit = Unit;
			this.NrDecimals = NrDecimals;
		}

		/// <summary>
		/// Magnitude
		/// </summary>
		[ShortName("v")]
		public double Value { get; set; }

		/// <summary>
		/// Unit
		/// </summary>
		[ShortName("d")]
		[DefaultValueStringEmpty]
		public string Unit { get; set; }

		/// <summary>
		/// Number of decimals.
		/// </summary>
		[DefaultValue(0)]
		public byte NrDecimals { get; set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is PersistableQuantity Q &&
				this.Value == Q.Value &&
				this.Unit == Q.Unit &&
				this.NrDecimals == Q.NrDecimals;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.Value.GetHashCode();
			Result ^= Result << 5 ^ (this.Unit?.GetHashCode() ?? 0);
			Result ^= Result << 5 ^ this.NrDecimals.GetHashCode();

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(CommonTypes.Encode(this.Value, this.NrDecimals));

			if (!string.IsNullOrEmpty(this.Unit))
			{
				sb.Append(' ');
				sb.Append(this.Unit);
			}

			return sb.ToString();
		}
	}
}
