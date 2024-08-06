﻿using System;
using Waher.Content;
using Waher.Persistence.Attributes;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a boolean value that can be either true or false.
	/// </summary>
	public class BooleanField : Field
	{
		private bool value;

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		public BooleanField()
			: base(null, DateTime.MinValue, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout)
		{
			this.value = false;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS, bool Writable,
			string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS, string Module, 
			params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Thing, Timestamp, Name, Type, QoS, Writable)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a boolean value that can be either true or false.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public BooleanField(ThingReference Thing, DateTime Timestamp, string Name, bool Value, FieldType Type, FieldQoS QoS)
			: base(Thing, Timestamp, Name, Type, QoS)
		{
			this.value = Value;
		}

		/// <summary>
		/// Field Value
		/// </summary>
		[ShortName("v")]
		public bool Value
		{
			get => this.value;
			set => this.value = value; 
		}

		/// <summary>
		/// Field value, boxed as an object reference.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public override string ValueString
		{
			get { return CommonTypes.Encode(this.value); }
		}

		/// <summary>
		/// Provides a string identifying the data type of the field. Should conform to field value data types specified by the Neuro-Foundation, if possible:
		/// https://neuro-foundation.io/SensorData.md#fields
		/// </summary>
		public override string FieldDataTypeName
		{
			get { return "b"; }
		}

		/// <summary>
		/// Reference value. Can be used for change calculations, as outlined in 
		/// https://neuro-foundation.io/SensorDataEventSubscription.md
		/// 
		/// Possible values are either double values or string values.
		/// </summary>
		public override object ReferenceValue
		{
			get
			{
				return this.value ? 1.0 : 0.0;
			}
		}
	}
}
