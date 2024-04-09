﻿function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHtml();
}

function CreateHTML()
{
	var Segments = [
		"<div class='horizontalAlignLeft'>\r\n",
		"<p>",
		"This is paragraph 1",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignRight'>\r\n",
		"<p>",
		"This is paragraph 2",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignMargins'>\r\n",
		"<p>",
		"This is the third paragraph.",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignCenter'>\r\n",
		"<p>",
		"In this fourth paragraph",
		"<br/>\r\n",
		"a line break is inserted.",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignLeft'>\r\n",
		"<h1",
		" id=\"thisIsAnH1\"",
		">",
		"This is an H1",
		"</h1>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignRight'>\r\n",
		"<h2",
		" id=\"thisIsAnH2\"",
		">",
		"This is an H2",
		"</h2>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignMargins'>\r\n",
		"<h1",
		" id=\"thisIsASecondH1\"",
		">",
		"This is a second H1",
		"</h1>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignCenter'>\r\n",
		"<h2",
		" id=\"thisIsASecondH2\"",
		">",
		"This is a second H2",
		"</h2>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignLeft'>\r\n",
		"<h1",
		" id=\"thisIsAThirdH1\"",
		">",
		"This is a third H1",
		"</h1>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignRight'>\r\n",
		"<h2",
		" id=\"thisIsAThirdH2\"",
		">",
		"This is a third H2",
		"</h2>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignCenter'>\r\n",
		"<h3",
		" id=\"thisIsAThirdH3\"",
		">",
		"This is a third H3",
		"</h3>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignMargins'>\r\n",
		"<h4",
		" id=\"thisIsAThirdH4\"",
		">",
		"This is a third H4",
		"</h4>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignLeft'>\r\n",
		"<h5",
		" id=\"leftAlignedExample\"",
		">",
		"Left-aligned Example",
		"</h5>\r\n",
		"<p>",
		"This text is left-aligned. Left-alignment is done by placing ",
		"<code>&lt;&lt;</code>",
		" in the beginning of each block, or each row in each block, as appropriate.",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignRight'>\r\n",
		"<h5",
		" id=\"rightAlignedExample\"",
		">",
		"Right-aligned Example",
		"</h5>\r\n",
		"<p>",
		"This text is right-aligned. Right-alignment is done by placing ",
		"<code>&gt;&gt;</code>",
		" at the end of each block, or each row in each block, as appropriate.",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignCenter'>\r\n",
		"<h5",
		" id=\"centerAlignedExample\"",
		">",
		"Center-aligned Example",
		"</h5>\r\n",
		"<p>",
		"This text is center-aligned. Center-alignment is done by placing ",
		"<code>&gt;&gt;</code>",
		" in the beginning of each block, or each row in each block, and ",
		"<code>&lt;&lt;</code>",
		" at the end of each block, or at the end of each row in each block, as appropriate.",
		"</p>\r\n",
		"</div>\r\n",
		"<div class='horizontalAlignMargins'>\r\n",
		"<h5",
		" id=\"marginAlignedExample\"",
		">",
		"Margin-aligned Example",
		"</h5>\r\n",
		"<p>",
		"This text is margin-aligned. Margin-alignment is done by placing ",
		"<code>&lt;&lt;</code>",
		" in the beginning of each block, or each row in each block, and ",
		"<code>&gt;&gt;</code>",
		" at the end of each block, or at the end of each row in each block, as appropriate.",
		"</p>\r\n",
		"</div>\r\n"];
	return Segments.join("");
}
