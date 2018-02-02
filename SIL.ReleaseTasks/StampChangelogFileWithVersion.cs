// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using MarkdownDeep;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

[assembly: InternalsVisibleTo("SIL.ReleaseTasks.Tests")]

namespace SIL.ReleaseTasks
{
	/// <summary>
	/// Replaces the first line in a Release.md with the version and date
	/// (Assumes that a temporary line is currently at the top: e.g. ## DEV_VERSION_NUMBER: DEV_RELEASE_DATE
	/// </summary>
	/* Example uses:
	 * <StampChangelogFileWithVersion ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md"
	 *   StampChangelogFile="true" VersionNumber="$(Version)"/>
	 *
	 * This stamps the ChangelogFile with the version numbers (replacing the first line with '## VERSION_NUMBER DATE')
	 */
	public class StampChangelogFileWithVersion : Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		[Required]
		public bool StampChangelogFile { get; set; }

		/// <summary>
		/// Finalize  markdown file (if <see cref="StampMarkdownFile"/> is true)
		/// for a release. Default: <c>true</c>.
		/// </summary>
		public bool Release { get; set; }

		public StampChangelogFileWithVersion()
		{
			Release = true;
		}

		public override bool Execute()
		{
			if (!StampChangelogFile || !Release)
				return true;

			var markdownLines = File.ReadAllLines(ChangelogFile);
			markdownLines[0] = $"## {VersionNumber} {DateTime.Today:dd/MMM/yyyy}";
			File.WriteAllLines(ChangelogFile, markdownLines);
			return true;
		}
	}
}
