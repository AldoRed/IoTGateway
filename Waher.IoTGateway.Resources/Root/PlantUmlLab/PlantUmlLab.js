var EditTimer = null;

function UmlKeyDown(Control, Event)
{
	if (EditTimer)
		window.clearTimeout(EditTimer);

	EditTimer = window.setTimeout(UpdateGraph, 500);
}

function UpdateGraph()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			var GraphDiv = document.getElementById("GraphDiv");
			SetDynamicHtml(GraphDiv, xhttp.responseText);
		};
	}

	xhttp.open("POST", "PlantUmlLabGraph.md", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", "text/html");
	xhttp.send(document.getElementById("Uml").value);
}

function ShowExample(Name)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			document.getElementById("Uml").value = xhttp.responseText;
			UpdateGraph();
		}
	}

	xhttp.open("GET", "Examples/"+Name, true);
	xhttp.setRequestHeader("Accept", "text/vnd.plantuml");
	xhttp.send();
}