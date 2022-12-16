﻿using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines a compound index for the collection holding objects of the corresponding class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class IndexAttribute : Attribute
	{
		private readonly string[] fieldNames;

		/// <summary>
		/// This attribute defines a compound index for the collection holding objects of the corresponding class.
		/// </summary>
		/// <param name="FieldNames">Names of the fields to build the index on. Prefix field names with a hyphen (-)
		/// to illustrate descending order.</param>
		public IndexAttribute(params string[] FieldNames)
		{
			if (FieldNames.Length == 0)
				throw new ArgumentException("No field names listed.", nameof(FieldNames));

			this.fieldNames = FieldNames;
		}

		/// <summary>
		/// Names of the fields to build the index on. Prefix field names with a hyphen (-)
		/// to illustrate descending order.
		/// </summary>
		public string[] FieldNames => this.fieldNames;
	}
}
