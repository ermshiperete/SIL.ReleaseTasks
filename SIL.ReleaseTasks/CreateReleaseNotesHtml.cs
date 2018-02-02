// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using MarkdownDeep;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	/// <summary>
	/// Given a markdown file, this class will generate a release notes HTML file.
	/// </summary>
	/* Example uses:
	 * <CreateReleaseNotesHtml ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md"
	 *   HtmlFile="$(RootDir)\src\Installer\$(UploadFolder).htm" VersionNumber="$(Version)" ProductName="flexbridge"/>
	 *
	 * This generates a .htm file by creating a new file or by replacing the <div class='releasenotes'>
	 * in an existing .htm with a generated one.
	 */
	public class CreateReleaseNotesHtml : Task
	{
		public string HtmlFile { get; set; }

		[Required]
		public string ChangelogFile { get; set; }

		public override bool Execute()
		{
			if(!File.Exists(ChangelogFile))
			{
				Log.LogError($"The given markdown file ({ChangelogFile}) does not exist.");
				return false;
			}

			var markDownTransformer = new Markdown();
			try
			{
				var markdownHtml = markDownTransformer.Transform(File.ReadAllText(ChangelogFile));
				if(File.Exists(HtmlFile))
				{
					var htmlDoc = XDocument.Load(HtmlFile);
					var releaseNotesElement = htmlDoc.XPathSelectElement("//*[@class='releasenotes']");
					if (releaseNotesElement == null)
						return true;

					releaseNotesElement.RemoveNodes();
					var mdDocument = XDocument.Parse($"<div>{markdownHtml}</div>");
					// ReSharper disable once PossibleNullReferenceException - Will either throw or work
					releaseNotesElement.Add(mdDocument.Root.Elements());
					htmlDoc.Save(HtmlFile);
				}
				else
				{
					WriteBasicHtmlFromMarkdown(markdownHtml);
				}
				return true;
			}
			catch(Exception e)
			{
				Log.LogErrorFromException(e, true);
				return false;
			}
		}

		private void WriteBasicHtmlFromMarkdown(string markdownHtml)
		{
			File.WriteAllText(HtmlFile, $"<html><div class='releasenotes'>{markdownHtml}</div></html>");
		}
	}
}
