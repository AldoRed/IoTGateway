﻿using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="UInt16"/> value.
	/// </summary>
	public class NullableUInt16Serializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="UInt16"/> value.
		/// </summary>
		public NullableUInt16Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(ushort?);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>(Reader.ReadBoolean() ? (ushort?)1 : (ushort?)0);
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>((ushort?)Reader.ReadByte());
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>((ushort?)Reader.ReadInt16());
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>((ushort?)Reader.ReadInt32());
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>((ushort?)Reader.ReadInt64());
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>((ushort?)Reader.ReadSByte());
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>((ushort?)Reader.ReadUInt16());
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>((ushort?)Reader.ReadUInt32());
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>((ushort?)Reader.ReadUInt64());
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>((ushort?)Reader.ReadDecimal());
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>((ushort?)Reader.ReadDouble());
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>((ushort?)Reader.ReadSingle());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>((ushort?)ushort.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_MIN: return Task.FromResult<object>(ushort.MinValue);
				case ObjectSerializer.TYPE_MAX: return Task.FromResult<object>(ushort.MaxValue);
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a nullable UInt16 value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			ushort? Value2 = (ushort?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return Task.CompletedTask;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_UINT16, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);

			return Task.CompletedTask;
		}

	}
}
