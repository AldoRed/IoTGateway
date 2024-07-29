﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Arduino
{
	/// <summary>
	/// TODO
	/// </summary>
	public enum DigitalInputPinMode
	{
		/// <summary>
		/// TODO
		/// </summary>
		Input,

		/// <summary>
		/// TODO
		/// </summary>
		InputPullUp
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class DigitalInput : DigitalPin, ISensor
	{
		private DigitalInputPinMode mode = DigitalInputPinMode.Input;
		private bool initialized = false;

		/// <summary>
		/// TODO
		/// </summary>
		public DigitalInput()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(9, "Mode:", 20)]
		[ToolTip(10, "Select drive mode of pin.")]
		[DefaultValue(DigitalInputPinMode.Input)]
		[Option(DigitalInputPinMode.Input, 11, "Input")]
		[Option(DigitalInputPinMode.InputPullUp, 12, "Input with Pull/Up")]
		public DigitalInputPinMode Mode
		{
			get => this.mode;
			set
			{
				this.mode = value;
				this.Initialize();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Initialize()
		{
			RemoteDevice Device = this.Device;

			if (!(Device is null))
			{
				switch (Mode)
				{
					case DigitalInputPinMode.Input:
						Device.pinMode(this.PinNr, PinMode.INPUT);
						break;

					case DigitalInputPinMode.InputPullUp:
						Device.pinMode(this.PinNr, PinMode.PULLUP);
						break;
				}

				this.initialized = true;
			}
			else
				this.initialized = false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 8, "Digital Input");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public Task StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = this.Device;
				if (Device is null)
					throw new Exception("Device not ready.");

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (!this.initialized)
					this.Initialize();

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new BooleanField(this, Now, "Value", Device.digitalRead(this.PinNr) == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 13));
				}
				
				if (Request.IsIncluded(FieldType.Identity))
				{
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 14));

					this.AddIdentityReadout(Fields, Now);
				}

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new EnumField(this, Now, "Drive Mode", Device.getPinMode(this.PinNr), FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 15));
				}

				Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Pin_ValueChanged(PinState NewState)
		{
			this.NewMomentaryValues(new BooleanField(this, DateTime.Now, "Value", NewState == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
				typeof(Module).Namespace, 13));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Module), 19, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
