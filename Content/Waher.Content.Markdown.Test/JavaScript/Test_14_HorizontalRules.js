﻿function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHtml();
}

function CreateHTML()
{
	var Segments = [
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n"];
	return Segments.join("");
}
