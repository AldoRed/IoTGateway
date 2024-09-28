using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Raw;

namespace Waher.Things.Test
{
	[TestClass]
	public class Ieee1451_0_ParsingTests
	{
		[DataTestMethod]
		[DataRow("AgcCAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA")]
		[DataRow("AgcBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAACaAAAAAAAAAAA=")]
		[DataRow("AgECADkAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABASoAAGb4BJsD9YEn")]
		public void Test_01_ParseRawMessage(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseRaw(Bin, out RawMessage Message));
			Assert.IsNotNull(Message);
			Assert.IsNotNull(Message.Body);
			Assert.IsNotNull(Message.Tail);

			Console.Out.WriteLine("NetworkServiceType: " + Message.NetworkServiceType.ToString());
			Console.Out.WriteLine("NetworkServiceId: " + Message.NetworkServiceIdName + " (" + Message.NetworkServiceId.ToString() + ")");
			Console.Out.WriteLine("MessageType: " + Message.MessageType.ToString());
			Console.Out.WriteLine("Body Length: " + Message.Body.Length.ToString());
			Console.Out.WriteLine("Tail Length: " + Message.Tail.Length.ToString());
		}

	}
}
