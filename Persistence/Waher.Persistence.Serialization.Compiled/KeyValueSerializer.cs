﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Serializer for objects of type KeyValuePair&lt;string, object&gt;.
	/// </summary>
	public class KeyValueSerializer : IObjectSerializer
	{
		private readonly ISerializerContext context;
		private readonly GenericObjectSerializer genericSerializer;

		/// <summary>
		/// Serializer for objects of type KeyValuePair&lt;string, object&gt;.
		/// </summary>
		public KeyValueSerializer(ISerializerContext Context, GenericObjectSerializer GenericSerializer)
		{
			this.context = Context;
			this.genericSerializer = GenericSerializer;
		}

		/// <summary>
		/// If the underlying object is nullable.
		/// </summary>
		public bool IsNullable
		{
			get { return true; }
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public Type ValueType
		{
			get { return typeof(KeyValuePair<string, object>); }
		}

		/// <summary>
		/// Initializes the serializer before first-time use.
		/// </summary>
		public Task Init()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			return this.Deserialize(Reader, DataType, Embedded, true);
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="CheckFieldNames">If field names are to be extended.</param>
		/// <returns>Deserialized object.</returns>
		public async Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded, bool CheckFieldNames)
		{
			if (!Reader.ReadBit())
				return null;

			string Key = Reader.ReadString();
			object Value;

			DataType = Reader.ReadBits(6);

			if (DataType == ObjectSerializer.TYPE_MAX)
			{
				Reader.SkipVariableLengthInteger();
				DataType = Reader.ReadBits(6);
			}

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_OBJECT:
					Reader.FlushBits();

					int Pos = Reader.Position;
					ulong TypeCode = Reader.ReadVariableLengthUInt64();
					ulong CollectionCode = Reader.ReadVariableLengthUInt64();
					string CollectionName = await this.context.GetFieldName(null, CollectionCode);
					string TypeName = await this.context.GetFieldName(CollectionName, TypeCode);
					IObjectSerializer Serializer;

					if (string.IsNullOrEmpty(TypeName))
						Serializer = this.genericSerializer;
					else
					{
						Type T = Types.GetType(TypeName);
						if (T is null)
							Serializer = this.genericSerializer;
						else
							Serializer = await this.context.GetObjectSerializer(T);
					}

					Reader.Position = Pos;

					Value = await Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, true);
					break;

				case ObjectSerializer.TYPE_BOOLEAN:
					Value = Reader.ReadBit();
					break;

				case ObjectSerializer.TYPE_BYTE:
					Value = Reader.ReadByte();
					break;

				case ObjectSerializer.TYPE_INT16:
					Value = Reader.ReadInt16();
					break;

				case ObjectSerializer.TYPE_INT32:
					Value = Reader.ReadInt32();
					break;

				case ObjectSerializer.TYPE_INT64:
					Value = Reader.ReadInt64();
					break;

				case ObjectSerializer.TYPE_SBYTE:
					Value = Reader.ReadSByte();
					break;

				case ObjectSerializer.TYPE_UINT16:
					Value = Reader.ReadUInt16();
					break;

				case ObjectSerializer.TYPE_UINT32:
					Value = Reader.ReadUInt32();
					break;

				case ObjectSerializer.TYPE_UINT64:
					Value = Reader.ReadUInt64();
					break;

				case ObjectSerializer.TYPE_VARINT16:
					Value = Reader.ReadVariableLengthInt16();
					break;

				case ObjectSerializer.TYPE_VARINT32:
					Value = Reader.ReadVariableLengthInt32();
					break;

				case ObjectSerializer.TYPE_VARINT64:
					Value = Reader.ReadVariableLengthInt64();
					break;

				case ObjectSerializer.TYPE_VARUINT16:
					Value = Reader.ReadVariableLengthUInt16();
					break;

				case ObjectSerializer.TYPE_VARUINT32:
					Value = Reader.ReadVariableLengthUInt32();
					break;

				case ObjectSerializer.TYPE_VARUINT64:
					Value = Reader.ReadVariableLengthUInt64();
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					Value = Reader.ReadDecimal();
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					Value = Reader.ReadDouble();
					break;

				case ObjectSerializer.TYPE_SINGLE:
					Value = Reader.ReadSingle();
					break;

				case ObjectSerializer.TYPE_DATETIME:
					Value = Reader.ReadDateTime();
					break;

				case ObjectSerializer.TYPE_DATETIMEOFFSET:
					Value = Reader.ReadDateTimeOffset();
					break;

				case ObjectSerializer.TYPE_TIMESPAN:
					Value = Reader.ReadTimeSpan();
					break;

				case ObjectSerializer.TYPE_CHAR:
					Value = Reader.ReadChar();
					break;

				case ObjectSerializer.TYPE_STRING:
					Value = Reader.ReadString();
					break;

				case ObjectSerializer.TYPE_CI_STRING:
					Value = Reader.ReadCaseInsensitiveString();
					break;

				case ObjectSerializer.TYPE_ENUM:
					Value = Reader.ReadString();
					break;

				case ObjectSerializer.TYPE_BYTEARRAY:
					Value = Reader.ReadByteArray();
					break;

				case ObjectSerializer.TYPE_GUID:
					Value = Reader.ReadGuid();
					break;

				case ObjectSerializer.TYPE_NULL:
					Value = null;
					break;

				case ObjectSerializer.TYPE_ARRAY:
					throw new Exception("Arrays must be embedded in objects.");

				default:
					throw new Exception("Object or value expected.");
			}

			return new KeyValuePair<string, object>(Key, Value);
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State)
		{
			KeyValuePair<string, object> TypedValue = (KeyValuePair<string, object>)Value;
			IObjectSerializer Serializer = await this.context.GetObjectSerializer(TypedValue.Value?.GetType() ?? typeof(object));

			Writer.WriteBit(true);
			Writer.Write(TypedValue.Key);
			await Serializer.Serialize(Writer, true, true, TypedValue.Value, State);
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			KeyValuePair<string, object> TypedValue = (KeyValuePair<string, object>)Object;

			switch (FieldName)
			{
				case "Key":
					return Task.FromResult<object>(TypedValue.Key);

				case "Value":
					return Task.FromResult<object>(TypedValue.Value);

				default:
					return Task.FromResult<object>(null);
			}
		}
	}
}
