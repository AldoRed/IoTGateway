﻿function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHtml();
}

function CreateHTML()
{
	var Segments = [
		"<p>",
		"4*5 is ",
		4*5,
		".",
		"</p>\r\n",
		"<p>",
		"Result of execution: ",
		"<strong>",
		"some text",
		"</strong>",
		".",
		"</p>\r\n",
		x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y)];
	return Segments.join("");
}
