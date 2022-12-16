﻿using System;
using System.Threading.Tasks;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Runtime.Settings;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource boolean value.
	/// </summary>
	public class Lwm2mResourceBoolean : Lwm2mResource
	{
		private readonly bool? defaultValue;
		private bool? value;

		/// <summary>
		/// Class managing an LWM2M resource boolean value.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		/// <param name="Value">Value of resource.</param>
		public Lwm2mResourceBoolean(string Name, ushort Id, ushort InstanceId, ushort ResourceId,
			bool CanWrite, bool Persist, bool? Value)
			: base(Name, Id, InstanceId, ResourceId, CanWrite, Persist)
		{
			this.defaultValue = this.value = Value;
		}

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public override async Task ReadPersistedValue()
		{
			this.value = await RuntimeSettings.GetAsync(this.Path, this.value.HasValue && this.value.Value);
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		/// <returns></returns>
		public override async Task WritePersistedValue()
		{
			if (this.value.HasValue)
				await RuntimeSettings.SetAsync(this.Path, this.value.Value);
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value => this.value;

		/// <summary>
		/// Resource value.
		/// 
		/// Use the <see cref="Set(bool?)"/> method to set the value of a persistent resource.
		/// </summary>
		public bool? BooleanValue
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Sets the resource value.
		/// </summary>
		/// <param name="Value">Value to set.</param>
		public async Task Set(bool? Value)
		{
			if (this.value != Value)
			{
				this.value = Value;

				if (this.Persist)
					await this.WritePersistedValue();

				await this.ValueUpdated();
			}

			base.Set();
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public override Task Read(TlvRecord Record)
		{
			return this.Set(Record.AsBoolean());
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.value.HasValue)
				Output.Write(IdentifierType.Resource, this.ResourceId, this.value.Value);
			else
				Output.Write(IdentifierType.Resource, this.ResourceId);
		}

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
			this.value = this.defaultValue;
		}
	}
}
