﻿using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// Double setting object.
	/// </summary>
	public class DoubleSetting : Setting
	{
		private double value = 0.0;

		/// <summary>
		/// Double setting object.
		/// </summary>
		public DoubleSetting()
		{
		}

		/// <summary>
		/// Double setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DoubleSetting(string Key, double Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValue(0.0)]
		public double Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public override object GetValueObject()
		{
			return this.value;
		}
	}
}
