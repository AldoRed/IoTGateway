﻿using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Script.Functions.Scalar;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes
{
	/// <summary>
	/// TEDS identification header (§6.3)
	/// </summary>
	public class TedsId : TedsRecord
	{
		/// <summary>
		/// TEDS identification header (§6.3)
		/// </summary>
		public TedsId()
			: base()
		{
		}

		/// <summary>
		/// This field identifies the member of the IEEE 1451 family of standards 
		/// that defines this TEDS.See Table 13 for examples.
		/// </summary>
		public byte FamilyMember { get; set; }

		/// <summary>
		/// This field identifies the submember of the IEEE 1451 family of standards 
		/// that defines this TEDS.Value 255 (0xFF) shall be reserved to an empty 
		/// submember.See Table 13 for examples.
		/// </summary>
		public byte FamilySubMember { get; set; }

		/// <summary>
		/// This field identifies the TEDS being accessed. The value is the TEDS access 
		/// code found in Table 72.
		/// </summary>
		public byte Class { get; set; }

		/// <summary>
		/// This field identifies the TEDS version. The value is the version number 
		/// identified in the standard.A value of zero in this field indicates that the 
		/// TEDS do not conform to any released standard. Table 12 lists the allowable 
		/// values for this field.
		/// </summary>
		public byte Version { get; set; }

		/// <summary>
		/// This field gives the number of octets in the length field of all tuples in the 
		/// TEDS except this tuple.
		/// 
		/// For most TEDS, the number of octets in the length field of the tuples is one, 
		/// meaning that there are 255 or less octets in the value field.However, some cases 
		/// may require more than 8 bits for the number of octets in the value field, so 
		/// this field specifies the number of octets in the length field of a tuple. All 
		/// tuples within a TEDS, except the TEDS Identifier, shall have the same number of 
		/// octets in the length field.
		/// </summary>
		public byte TupleLength { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="FieldType">TEDS field type.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(byte FieldType)
		{
			return FieldType == 3 ? Grade.Perfect : Grade.NotAtAll;
		}

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="Type">Field Type</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <returns>Parsed TEDS record.</returns>
		public override TedsRecord Parse(byte Type, Ieee1451_0Binary RawValue)
		{
			return new TedsId()
			{
				Type = Type,
				RawValue = RawValue.Body,
				FamilyMember = RawValue.NextUInt8(),
				FamilySubMember = RawValue.NextUInt8(),
				Class = RawValue.NextUInt8(),
				Version = RawValue.NextUInt8(),
				TupleLength = RawValue.NextUInt8()
			};
		}

		/// <summary>
		/// Adds fields to a collection of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Fields">Parsed fields.</param>
		public override void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields)
		{
			Fields.Add(new StringField(Thing, Timestamp, "Family",
				this.FamilyMember.ToString() + "." + this.FamilySubMember.ToString(),
				SensorData.FieldType.Identity, FieldQoS.AutomaticReadout));

			Fields.Add(new Int32Field(Thing, Timestamp, "Class", this.Class,
				SensorData.FieldType.Identity, FieldQoS.AutomaticReadout));

			Fields.Add(new Int32Field(Thing, Timestamp, "Version", this.Version,
				SensorData.FieldType.Identity, FieldQoS.AutomaticReadout));
		}
	}
}
