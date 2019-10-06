﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field definition.
	/// </summary>
	public class Asn1FieldDefinition : Asn1Node
	{
		private readonly string fieldName;
		private readonly string typeName;
		private readonly Asn1Restriction restriction;
		private readonly Asn1NamedValue[] namedOptions;
		private readonly bool? optional;
		private readonly bool? unique;
		private readonly bool? present;
		private readonly bool? absent;
		private readonly Asn1Node _default;
		private int? tag;

		/// <summary>
		/// Represents an ASN.1 field definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Restriction">Optional restrictions.</param>
		/// <param name="Optional">If field is optional.</param>
		/// <param name="Unique">If field value is unique.</param>
		/// <param name="Present">If an optional field must be present.</param>
		/// <param name="Absent">If an optional field must be absent.</param>
		/// <param name="Default">Default value if field not provided.</param>
		/// <param name="NamedOptions">Named options.</param>
		public Asn1FieldDefinition(string FieldName, int? Tag, string TypeName, 
			Asn1Restriction Restriction, bool? Optional, bool? Unique, bool? Present,
			bool? Absent, Asn1Node Default, Asn1NamedValue[] NamedOptions)
			: base()
		{
			this.fieldName = FieldName;
			this.typeName = TypeName;
			this.restriction = Restriction;
			this.optional = Optional;
			this.unique = Unique;
			this.present = Present;
			this.absent = Absent;
			this._default = Default;
			this.namedOptions = NamedOptions;
			this.tag = Tag;
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName => this.fieldName;

		/// <summary>
		/// Type Name
		/// </summary>
		public string TypeName => this.typeName;

		/// <summary>
		/// Optional restrictions.
		/// </summary>
		public Asn1Restriction Restriction => this.restriction;

		/// <summary>
		/// If field is optional.
		/// </summary>
		public bool? Optional => this.optional;

		/// <summary>
		/// If field value is unique.
		/// </summary>
		public bool? Unique => this.unique;

		/// <summary>
		/// If an optional field must be present.
		/// </summary>
		public bool? Present => this.present;

		/// <summary>
		/// If an optional field must be absent.
		/// </summary>
		public bool? Absent => this.absent;

		/// <summary>
		/// Default value if field not provided.
		/// </summary>
		public Asn1Node Default => this._default;

		/// <summary>
		/// Named options.
		/// </summary>
		public Asn1NamedValue[] NamedOptions => this.namedOptions;

		/// <summary>
		/// Tag
		/// </summary>
		public int? Tag
		{
			get => this.tag;
			internal set => this.tag = value;
		}
	}
}
