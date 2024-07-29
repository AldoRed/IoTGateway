﻿using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with string data.
	/// </summary>
	public class StringData : Data
	{
		private string value;

		/// <summary>
		/// Represents an MQTT topic with string data.
		/// </summary>
		public StringData(MqttTopic Topic, string Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void DataReported(MqttContent Content)
		{
			this.value = CommonTypes.GetString(Content.Data, Encoding.UTF8);
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 40, "String");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new StringField(ThingReference, this.timestamp, this.Append(Prefix, "Value"), 
				this.value, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override bool IsControllable => true;

		/// <summary>
		/// TODO
		/// </summary>
		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new StringControlParameter("Value", "Publish", "Value:", "String value of topic.",
					(n) => Task.FromResult<string>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString());
		}
	}
}
