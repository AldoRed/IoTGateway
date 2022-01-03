﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// LoadMarkdown(FileName)
	/// </summary>
	public class LoadMarkdown : FunctionOneScalarVariable
    {
		/// <summary>
		/// LoadMarkdown(FileName)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadMarkdown(ScriptNode FileName, int Start, int Length, Expression Expression)
            : base(FileName, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => "loadmarkdown";

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			string Markdown = await Resources.ReadAllTextAsync(Argument);
			MarkdownSettings Settings = new MarkdownSettings()
			{
				Variables = Variables,
				ParseMetaData = true
			};

			if (Variables.TryGetVariable(" MarkdownSettings ", out Variable v) &&
				v.ValueObject is MarkdownSettings ParentSettings)
			{
				Settings.AllowScriptTag = ParentSettings.AllowScriptTag;
				Settings.AudioAutoplay = ParentSettings.AudioAutoplay;
				Settings.AudioControls = ParentSettings.AudioControls;
				Settings.EmbedEmojis = ParentSettings.EmbedEmojis;
				Settings.EmojiSource = ParentSettings.EmojiSource;
				Settings.HttpxProxy = ParentSettings.HttpxProxy;
				Settings.LocalHttpxResourcePath = ParentSettings.LocalHttpxResourcePath;
				Settings.RootFolder = ParentSettings.RootFolder;
				Settings.VideoAutoplay = ParentSettings.VideoAutoplay;
				Settings.VideoControls = ParentSettings.VideoControls;
			}

			KeyValuePair<string, bool> P = await MarkdownDocument.Preprocess(Markdown, Settings, Argument);
			Markdown = P.Key;

			Match M = MarkdownDocument.endOfHeader.Match(Markdown);
			if (M.Success)
			{
				string Header = Markdown.Substring(0, M.Index);
				string[] Rows = Header.Split(CommonTypes.CRLF);
				string s;
				bool IsHeader = true;

				foreach (string Row in Rows)
				{
					s = Row.Trim();
					if (string.IsNullOrEmpty(s))
						continue;

					if (s.IndexOf(':')<0)
					{
						IsHeader = false;
						break;
					}
				}

				if (IsHeader)
					Markdown = Markdown.Substring(M.Index).TrimStart();
			}

			return new StringValue(Markdown);
        }
    }
}
