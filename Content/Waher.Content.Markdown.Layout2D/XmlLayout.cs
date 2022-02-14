﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Layout.Layout2D;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Security;

namespace Waher.Content.Markdown.Layout2D
{
	/// <summary>
	/// Class managing 2D XML Layout integration into Markdown documents.
	/// </summary>
	public class XmlLayout : IImageCodeContent, IXmlVisualizer
	{
		private static readonly Random rnd = new Random();
		private static Scheduler scheduler = null;
		private static string layoutFolder = null;
		private static string contentRootFolder = null;

		/// <summary>
		/// Class managing 2D XML Layout integration into Markdown documents.
		/// </summary>
		public XmlLayout()
		{
		}

		/// <summary>
		/// Initializes the Layout2D-Markdown integration.
		/// </summary>
		/// <param name="ContentRootFolder">Content root folder. If hosting markdown under a web server, this would correspond
		/// to the roof folder for the web content.</param>
		public static void Init(string ContentRootFolder)
		{
			contentRootFolder = ContentRootFolder;
			layoutFolder = Path.Combine(contentRootFolder, "Layout");

			if (scheduler is null)
			{
				if (Types.TryGetModuleParameter("Scheduler", out object Obj) && Obj is Scheduler Scheduler)
					scheduler = Scheduler;
				else
				{
					scheduler = new Scheduler();

					Log.Terminating += (sender, e) =>
					{
						scheduler?.Dispose();
						scheduler = null;
					};
				}
			}

			if (!Directory.Exists(layoutFolder))
				Directory.CreateDirectory(layoutFolder);

			DeleteOldFiles(null);
		}

		private static void DeleteOldFiles(object P)
		{
			DeleteOldFiles(DateTime.Now.AddDays(-7));
		}

		/// <summary>
		/// Deletes generated files older than <paramref name="Limit"/>.
		/// </summary>
		/// <param name="Limit">Age limit.</param>
		public static void DeleteOldFiles(DateTime Limit)
		{
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(layoutFolder, "*.*"))
			{
				if (File.GetLastAccessTime(FileName) < Limit)
				{
					try
					{
						File.Delete(FileName);
						Count++;
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileName);
					}
				}
			}

			if (Count > 0)
				Log.Informational(Count.ToString() + " old file(s) deleted.", layoutFolder);

			lock (rnd)
			{
				scheduler.Add(DateTime.Now.AddDays(rnd.NextDouble() * 2), DeleteOldFiles, null);
			}
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			int i = Language.IndexOf(':');
			if (i > 0)
				Language = Language.Substring(0, i).TrimEnd();

			switch (Language.ToLower())
			{
				case "layout":
					return Grade.Excellent;
			}

			return Grade.NotAtAll;
		}

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
			// Do nothing.
		}

		/// <summary>
		/// If HTML is handled.
		/// </summary>
		public bool HandlesHTML => true;

		/// <summary>
		/// If Plain Text is handled.
		/// </summary>
		public bool HandlesPlainText => true;

		/// <summary>
		/// If XAML is handled.
		/// </summary>
		public bool HandlesXAML => true;

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			string FileName = Info.FileName.Substring(contentRootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');
			if (!FileName.StartsWith("/"))
				FileName = "/" + FileName;

			Output.Append("<figure>");
			Output.Append("<img src=\"");
			Output.Append(XML.HtmlAttributeEncode(FileName));

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));

				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));
			}

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Info.Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");

			return true;
		}

		private class GraphInfo
		{
			public string FileName;
			public string Title;
		}

		/// <summary>
		/// Generates an image, saves it, and returns the file name of the image file.
		/// </summary>
		/// <param name="Language">Language</param>
		/// <param name="Rows">Code Block rows</param>
		/// <param name="Session">Session variables.</param>
		/// <returns>File name</returns>
		private static async Task<GraphInfo> GetFileName(string Language, string[] Rows, Variables Session)
		{
			StringBuilder sb = new StringBuilder();
			GraphInfo Result = new GraphInfo();

			foreach (string Row in Rows)
				sb.AppendLine(Row);

			string Xml = sb.ToString();
			int i = Language.IndexOf(':');

			if (i > 0)
			{
				Result.Title = Language.Substring(i + 1).Trim();
				Language = Language.Substring(0, i).TrimEnd();
			}
			else
				Result.Title = string.Empty;

			sb.Append(Language);

			string Hash = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(sb.ToString()));

			string LayoutFolder = Path.Combine(contentRootFolder, "Layout");
			string FileName = Path.Combine(LayoutFolder, Hash);
			Result.FileName = FileName + ".png";

			if (!File.Exists(Result.FileName))
			{
				try
				{
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(Xml);

					Layout2DDocument LayoutDoc = await Layout2DDocument.FromXml(Doc);
					RenderSettings Settings = await LayoutDoc.GetRenderSettings(Session);

					KeyValuePair<SKImage, Map[]> P = await LayoutDoc.Render(Settings);
					using (SKImage Img = P.Key)   // TODO: Maps
					{
						using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
						{
							byte[] Bin = Data.ToArray();
							await Resources.WriteAllBytesAsync(Result.FileName, Bin);
						}
					}
				}
				catch (Exception)
				{
					return null;
				}
			}

			return Result;
		}

		/// <summary>
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			Output.AppendLine(Info.Title);

			return true;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.FileName);
			Output.WriteAttributeString("Stretch", "None");

			if (!string.IsNullOrEmpty(Info.Title))
				Output.WriteAttributeString("ToolTip", Info.Title);

			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.FileName);
			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		public async Task<PixelInformation> GenerateImage(string[] Rows, string Language, MarkdownDocument Document)
		{
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return null;

			byte[] Data = await Resources.ReadAllBytesAsync(Info.FileName);

			using (SKBitmap Bitmap = SKBitmap.Decode(Data))
			{
				return new PixelInformationPng(Data, Bitmap.Width, Bitmap.Height);
			}
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Xml">XML Document</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(XmlDocument Xml)
		{
			return Xml.DocumentElement.LocalName == Layout2DDocument.LocalName &&
				Xml.DocumentElement.NamespaceURI == Layout2DDocument.Namespace ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <summary>
		/// Transforms the XML document before visualizing it.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Session">Current variables.</param>
		/// <returns>Transformed object.</returns>
		public async Task<object> TransformXml(XmlDocument Xml, Variables Session)
		{
			Layout2DDocument LayoutDoc = await Layout2DDocument.FromXml(Xml, Session);
			RenderSettings Settings = await LayoutDoc.GetRenderSettings(Session);

			KeyValuePair<SKImage, Map[]> P = await LayoutDoc.Render(Settings);
			using (SKImage Img = P.Key)   // TODO: Maps
			{
				return PixelInformation.FromImage(Img);
			}
		}

	}
}
