﻿using System;
using System.Text;
using SkiaSharp;
using Waher.Content.Markdown.GraphViz;
using Waher.Content.Markdown.Layout2D;
using Waher.Content.Markdown.PlantUml;
using Waher.IoTGateway.Setup;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Constants
{
	/// <summary>
	/// Theme constant.
	/// </summary>
	public class Theme : IConstant
	{
		private static ThemeDefinition currentDefinition = null;

		/// <summary>
		/// Theme constant.
		/// </summary>
		public Theme()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Theme"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			Variables["GraphBgColor"] = currentDefinition.GraphBgColor;
			Variables["GraphFgColor"] = currentDefinition.GraphFgColor;

			return new ObjectValue(currentDefinition);
		}

		/// <summary>
		/// Current theme.
		/// </summary>
		public static ThemeDefinition CurrentTheme
		{
			get => currentDefinition;
			internal set
			{
				bool DeleteCahces = !(currentDefinition is null) && currentDefinition.Id != value.Id;

				currentDefinition = value;

				string Color = ColorToString(currentDefinition.GraphBgColor);
				GraphViz.DefaultBgColor = Color;
				PlantUml.DefaultBgColor = Color;

				Color = ColorToString(currentDefinition.GraphFgColor);
				GraphViz.DefaultFgColor = Color;
				PlantUml.DefaultFgColor = Color;

				if (DeleteCahces)
				{
					GraphViz.DeleteOldFiles(TimeSpan.Zero, false);
					PlantUml.DeleteOldFiles(TimeSpan.Zero, false);
					XmlLayout.DeleteOldFiles(TimeSpan.Zero, false);
				}
			}
		}

		private static string ColorToString(SKColor Color)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("#");
			sb.Append(Color.Red.ToString("X2"));
			sb.Append(Color.Green.ToString("X2"));
			sb.Append(Color.Blue.ToString("X2"));

			if (Color.Alpha != 255)
				sb.Append(Color.Alpha.ToString("X2"));

			return sb.ToString();
		}

	}
}
