using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class ParsingTests
	{
		public static Asn1Document ParseAsn1Document(string FileName)
		{
			return Asn1Document.FromFile(Path.Combine("Examples", FileName));
		}

		[TestMethod]
		public void Test_01_WorldSchema()
		{
			ParseAsn1Document("World-Schema.asn1");
		}

		[TestMethod]
		public void Test_02_MyShopPurchaseOrders()
		{
			ParseAsn1Document("MyShopPurchaseOrders.asn1");
		}

		[TestMethod]
		public void Test_03_RFC1155()
		{
			ParseAsn1Document("SNMPv1\\RFC1155-SMI.asn1");
		}

		[TestMethod]
		public void Test_04_RFC1157()
		{
			ParseAsn1Document("SNMPv1\\RFC1157-SNMP.asn1");
		}

		[TestMethod]
		public void Test_05_1451_1()
		{
			ParseAsn1Document("IEEE1451\\P21451-N1-T1-MIB.asn1");
		}

		[TestMethod]
		public void Test_06_1451_2()
		{
			ParseAsn1Document("IEEE1451\\P21451-N1-T2-MIB.asn1");
		}

		[TestMethod]
		public void Test_07_1451_3()
		{
			ParseAsn1Document("IEEE1451\\P21451-N1-T3-MIB.asn1");
		}
	}
}
