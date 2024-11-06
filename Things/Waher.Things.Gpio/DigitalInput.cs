﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Windows.Devices.Gpio;

namespace Waher.Things.Gpio
{
	/// <summary>
	/// TODO
	/// </summary>
	public enum InputPinMode
	{
		/// <summary>
		/// TODO
		/// </summary>
		Input,

		/// <summary>
		/// TODO
		/// </summary>
		InputPullUp,

		/// <summary>
		/// TODO
		/// </summary>
		InputPullDown
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class DigitalInput : Pin, ISensor
	{
		private GpioPin pin = null;
		private InputPinMode mode = InputPinMode.Input;

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
		[Page(2, "GPIO")]
		[Header(7, "Mode:", 20)]
		[ToolTip(8, "Select drive mode of pin.")]
		[DefaultValue(InputPinMode.Input)]
		[Option(InputPinMode.Input, 9, "Input")]
		[Option(InputPinMode.InputPullUp, 10, "Input with Pull/Up")]
		[Option(InputPinMode.Input, 11, "Input with Pull/Down")]
		public InputPinMode Mode
		{
			get => this.mode;
			set
			{
				this.SetDriveMode(value);
				this.mode = value;
			}
		}

		private void SetDriveMode(InputPinMode Mode)
		{
			if (!(this.pin is null))
			{
				switch (Mode)
				{
					case InputPinMode.Input:
						this.pin.SetDriveMode(GpioPinDriveMode.Input);
						break;

					case InputPinMode.InputPullDown:
						this.pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
						break;

					case InputPinMode.InputPullUp:
						this.pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
						break;
				}
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 5, "Digital Input");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				if (this.pin is null)
				{
					GpioController Controller = await this.GetController();

					if (!(Controller is null) && !Controller.TryOpenPin(this.PinNr, GpioSharingMode.SharedReadOnly, out this.pin, out GpioOpenStatus Status))
					{
						string Id = Status.ToString();
						string s = this.GetStatusMessage(Status);

						await this.LogErrorAsync(Id, s);

						await Request.ReportErrors(true, new ThingError(this, s));
						return;
					}

					this.pin.ValueChanged += this.Pin_ValueChanged;

					this.SetDriveMode(this.mode);
				}

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new BooleanField(this, Now, "Value", this.pin.Read() == GpioPinValue.High, FieldType.Momentary, FieldQoS.AutomaticReadout,
						typeof(Controller).Namespace, 12));
				}

				if (Request.IsIncluded(FieldType.Identity))
				{
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout,
						typeof(Controller).Namespace, 13));

					this.AddIdentityReadout(Fields, Now);
				}

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new TimeField(this, Now, "Debounce Timeout", this.pin.DebounceTimeout, FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Controller).Namespace, 14));

					Fields.Add(new EnumField(this, Now, "Sharing Mode", this.pin.SharingMode, FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Controller).Namespace, 15));

					Fields.Add(new EnumField(this, Now, "Drive Mode", this.pin.GetDriveMode(), FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Controller).Namespace, 16));
				}

				await Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			this.NewMomentaryValues(new BooleanField(this, DateTime.Now, "Value", args.Edge == GpioPinEdge.RisingEdge, 
				FieldType.Momentary, FieldQoS.AutomaticReadout, typeof(Controller).Namespace, 12));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task DestroyAsync()
		{
			if (!(this.pin is null))
			{
				this.pin.Dispose();
				this.pin = null;
			}

			return base.DestroyAsync();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Controller), 23, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
