﻿using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines an option to display when editing the parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class OptionAttribute : Attribute
	{
		private readonly int stringId;
		private readonly string label;
		private readonly object option;

		/// <summary>
		/// Defines an option to display when editing the parameter.
		/// </summary>
		/// <param name="Option">Option.</param>
		public OptionAttribute(object Option)
			: this(Option, 0, Option.ToString())
		{
		}

		/// <summary>
		/// Defines an option to display when editing the parameter.
		/// </summary>
		/// <param name="Option">Option.</param>
		/// <param name="Label">Label string</param>
		public OptionAttribute(object Option, string Label)
			: this(Option, 0, Label)
		{
		}

		/// <summary>
		/// Defines an option to display when editing the parameter.
		/// </summary>
		/// <param name="Option">Option.</param>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Label">Default label string, in the default language defined for the class.</param>
		public OptionAttribute(object Option, int StringId, string Label)
		{
			this.option = Option;
			this.stringId = StringId;
			this.label = Label;
		}

		/// <summary>
		/// Option.
		/// </summary>
		public object Option => this.option;

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId => this.stringId;

		/// <summary>
		/// Default label string, in the default language defined for the class.
		/// </summary>
		public string Label => this.label;
	}
}
