﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Waher.Things.ControlParameters;

namespace Waher.Things.Gpio
{
	public enum OutputPinMode
	{
		Output,
		OutputOpenDrain,
		OutputOpenDrainPullUp,
		OutputOpenSource,
		OutputOpenSourcePullDown
	}

	public class DigitalOutput : Pin, ISensor, IActuator
	{
		private GpioPin pin = null;
		private OutputPinMode mode = OutputPinMode.Output;

		public DigitalOutput()
			: base()
		{
		}

		[Page(2, "GPIO")]
		[Header(7, "Mode:", 20)]
		[ToolTip(8, "Select drive mode of pin.")]
		[DefaultValue(OutputPinMode.Output)]
		[Option(OutputPinMode.Output, 17, "Output")]
		[Option(OutputPinMode.OutputOpenDrain, 18, "Output with open drain")]
		[Option(OutputPinMode.OutputOpenDrainPullUp, 19, "Output with open drain and pull/up")]
		[Option(OutputPinMode.OutputOpenSource, 20, "Output with open source")]
		[Option(OutputPinMode.OutputOpenSourcePullDown, 21, "Output with open source and pull down")]
		public OutputPinMode Mode
		{
			get => this.mode;
			set
			{
				this.SetDriveMode(value);
				this.mode = value;
			}
		}

		private void SetDriveMode(OutputPinMode Mode)
		{
			if (this.pin != null)
			{
				switch (Mode)
				{
					case OutputPinMode.Output:
						this.pin.SetDriveMode(GpioPinDriveMode.Output);
						break;

					case OutputPinMode.OutputOpenDrain:
						this.pin.SetDriveMode(GpioPinDriveMode.OutputOpenDrain);
						break;

					case OutputPinMode.OutputOpenDrainPullUp:
						this.pin.SetDriveMode(GpioPinDriveMode.OutputOpenDrainPullUp);
						break;

					case OutputPinMode.OutputOpenSource:
						this.pin.SetDriveMode(GpioPinDriveMode.OutputOpenSource);
						break;

					case OutputPinMode.OutputOpenSourcePullDown:
						this.pin.SetDriveMode(GpioPinDriveMode.OutputOpenSourcePullDown);
						break;
				}
			}
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 6, "Digital Output");
		}

		public Task StartReadout(ISensorReadout Request)
		{
			try
			{
				if (this.pin is null)
				{
					if (!this.Controller.TryOpenPin(this.PinNr, GpioSharingMode.Exclusive, out this.pin, out GpioOpenStatus Status))
					{
						string Id = Status.ToString();
						string s = this.GetStatusMessage(Status);

						this.LogErrorAsync(Id, s);

						Request.ReportErrors(true, new ThingError(this, s));
						return Task.CompletedTask;
					}

					this.SetDriveMode(this.mode);
				}

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new BooleanField(this, Now, "Value", this.pin.Read() == GpioPinValue.High, FieldType.Momentary, FieldQoS.AutomaticReadout,
						true, typeof(Controller).Namespace, 12));
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

				Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}

			return Task.CompletedTask;
		}

		public override Task DestroyAsync()
		{
			if (this.pin != null)
			{
				this.pin.Dispose();
				this.pin = null;
			}

			return base.DestroyAsync();
		}

		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult<ControlParameter[]>(new ControlParameter[]
			{
				new BooleanControlParameter("Value", "Output", "Value:", "Value of output.", this.GetValue, this.SetValue)
			});
		}

		private Task<bool?> GetValue(IThingReference Node)
		{
			if (this.pin is null)
			{
				if (!this.Controller.TryOpenPin(this.PinNr, GpioSharingMode.Exclusive, out this.pin, out GpioOpenStatus _))
					return null;

				this.SetDriveMode(this.mode);
			}

			return Task.FromResult<bool?>(this.pin.Read() == GpioPinValue.High);
		}

		private Task SetValue(IThingReference Node, bool Value)
		{
			if (this.pin is null)
			{
				if (!this.Controller.TryOpenPin(this.PinNr, GpioSharingMode.Exclusive, out this.pin, out GpioOpenStatus Status))
					throw new Exception(this.GetStatusMessage(Status));

				this.SetDriveMode(this.mode);
			}

			this.pin.Write(Value ? GpioPinValue.High : GpioPinValue.Low);
		
			return Task.CompletedTask;
		}

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Controller), 23, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
