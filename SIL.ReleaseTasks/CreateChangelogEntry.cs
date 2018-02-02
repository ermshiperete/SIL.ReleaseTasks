// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	/// <summary>
	/// Given a Changelog file, this task will add an entry to the debian changelog.
	/// </summary>
	/* Example use:
	 * <CreateChangelogEntry ChangelogFile="$(RootDir)/CHANGELOG.md"
	 *   VersionNumber="4.1" ProductName="bloom" Stability="stable" Urgency="medium"
	 *   DebianChangeLog="$(RootDir)/debian/changelog" ChangeLogAuthorInfo="Stephen McConnel &lt;stephen_mcconnel@example.com&gt;" />
	 *
	 * This uses the ChangelogFile and VersionNumber to generate a changelog entry in the DebianChangeLog file giving the author credit to
	 * Steve.
	*/
	public class CreateChangelogEntry: Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		[Required]
		public string ProductName { get; set; }

		[Required]
		public string DebianChangelog { get; set; }

		public string Stability { get; set; }

		public string Urgency { get; set; }

		/// <summary>
		/// Name and e-mail string
		/// </summary>
		public string ChangelogAuthorInfo { get; set; }

		/// <summary>
		/// Finalize the changelog for a release. Default: <c>true</c>.
		/// </summary>
		public bool Release { get; set; }

		public CreateChangelogEntry()
		{
			Release = true;
		}

		public override bool Execute()
		{
			if (string.IsNullOrEmpty(Stability))
				Stability = Release ? "unstable" : "UNRELEASED";
			if (string.IsNullOrEmpty(Urgency))
				Urgency = "low";
			var oldChangeLog = Path.ChangeExtension(DebianChangelog, ".old");
			File.Delete(oldChangeLog);
			File.Move(DebianChangelog, oldChangeLog);
			WriteMostRecentMarkdownEntryToChangelog();
			File.AppendAllLines(DebianChangelog, File.ReadAllLines(oldChangeLog));
			return true;
		}
		
		private void WriteMostRecentMarkdownEntryToChangelog()
		{
			if(string.IsNullOrEmpty(ChangelogAuthorInfo))
			{
				ChangelogAuthorInfo = "Annonymous <annonymous@example.com>";
			}
			var markdownLines = File.ReadAllLines(ChangelogFile);
			var newEntryLines = new List<string>();
			// Write out the first markdown line as the changelog version line
			newEntryLines.Add($"{ProductName} ({VersionNumber}) {Stability}; urgency={Urgency}");
			newEntryLines.Add(string.Empty);
			for(var i = 1; i < markdownLines.Length; ++i)
			{
				if(markdownLines[i].StartsWith("##"))
					break;
				ConvertMarkdownLineToChangelogLine(markdownLines[i], newEntryLines);
			}
			newEntryLines.Add(string.Empty);
			// The debian changelog needs RFC 2822 format (Thu, 15 Oct 2015 08:25:16 -0500), which is not quite what .NET can provide
			var debianDate = $"{DateTime.Now:ddd, dd MMM yyyy HH'|'mm'|'ss zzz}".Replace(":", "").Replace('|', ':');
			newEntryLines.Add($" -- {ChangelogAuthorInfo}  {debianDate}");
			newEntryLines.Add(string.Empty);
			File.AppendAllLines(DebianChangelog, newEntryLines);
		}

		private static void ConvertMarkdownLineToChangelogLine(string markdownLine, List<string> newEntryLines)
		{
			if (string.IsNullOrEmpty(markdownLine))
				return;

			switch(markdownLine[0])
			{
				case '*':
				case '-':
				case '+':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '0': // treat all unordered and ordered list items the same in the changelog
					newEntryLines.Add($"  *{markdownLine.Substring(1)}");
					break;
				case ' ': // Handle lists within lists, only second level items are handled, any further indentation is currently ignored
					newEntryLines.Add($"    *{markdownLine.Trim().Substring(1).Trim('.')}");
					break;
			}
		}

	}
}
