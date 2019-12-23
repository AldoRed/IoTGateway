﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.Sets;
using Waher.Script.Units;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptXmlTests
	{
		private void Test(string Script, string ExpectedOutput)
		{
			Variables v = new Variables();
			Expression Exp = new Expression(Script);
			XmlDocument Result = Exp.Evaluate(v) as XmlDocument;

			Assert.IsNotNull(Result, "XmlDocument expected.");
			Assert.AreEqual(ExpectedOutput ?? Script, Result.OuterXml);
		}

		[TestMethod]
		public void XML_Test_01_Text()
		{
			this.Test("<a>Hello</a>", null);
		}

		[TestMethod]
		public void XML_Test_02_Expression()
		{
			this.Test("<a><b>1</b><b><[1+2]></b></a>", "<a><b>1</b><b>3</b></a>");
		}

		[TestMethod]
		public void XML_Test_03_Attributes()
		{
			this.Test("<a><b value=\"1\" /><b value=\"2\" /></a>", "<a><b value=\"1\" /><b value=\"2\" /></a>");
		}

		[TestMethod]
		public void XML_Test_04_Attributes_Expression()
		{
			this.Test("x:=7;<a><b value=1 /><b value=2+x /></a>", "<a><b value=\"1\" /><b value=\"9\" /></a>");
		}

		[TestMethod]
		public void XML_Test_05_CDATA()
		{
			this.Test("<a><![CDATA[Hello World.]]></a>", null);
		}

		[TestMethod]
		public void XML_Test_06_Comment()
		{
			this.Test("<a><!--Hello World.--></a>", null);
		}

		[TestMethod]
		public void XML_Test_07_Mixed()
		{
			this.Test("<a>2+3=<[2+3]>.</a>", "<a>2+3=5.</a>");
		}

		[TestMethod]
		public void XML_Test_08_XML_Declaration()
		{
			this.Test("<?xml version=\"1.0\" encoding=\"UTF-8\" ?><a>Hello</a>", null);
		}

		[TestMethod]
		public void XML_Test_09_ProcessingInstruction()
		{
			this.Test("<?xml version=\"1.0\" encoding=\"UTF-8\" ?><?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?><a>Hello</a>", null);
		}

	}
}