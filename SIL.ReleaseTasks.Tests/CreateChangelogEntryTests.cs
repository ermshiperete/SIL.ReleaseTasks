// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class CreateChangelogEntrytTests
	{
		[Test]
		public void UpdateDebianChangelogWorks()
		{
			var testMarkdown = new CreateChangelogEntry();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var mdFile = tempFiles.FirstFile;
				var changeLogFile = tempFiles.SecondFile;
				File.WriteAllLines(mdFile, new[] {"## 2.3.10: 4/Sep/2014", "* with some random content", "* does some things"});
				File.WriteAllLines(changeLogFile, new[]
				{
					"myfavoriteapp (2.1.0~alpha1) unstable; urgency=low", "", "  * Initial Release for Linux.", "",
					" -- Stephen McConnel <stephen_mcconnel@example.com>  Fri, 12 Jul 2013 14:57:59 -0500", ""
				});
				testMarkdown.ChangelogFile = mdFile;
				testMarkdown.VersionNumber = "2.3.11";
				testMarkdown.ProductName = "myfavoriteapp";
				testMarkdown.ChangelogAuthorInfo = "Steve McConnel <stephen_mcconnel@example.com>";
				testMarkdown.DebianChangelog = changeLogFile;
				Assert.That(testMarkdown.Execute(), Is.True);
				var newContents = File.ReadAllLines(changeLogFile);
				Assert.AreEqual(newContents.Length, 13, "New changelog entry was not the expected length");
				Assert.That(newContents[0], Does.StartWith("myfavoriteapp (2.3.11) unstable; urgency=low"));
				//Make sure that the author line matches debian standards for time offset and spacing around author name
				Assert.That(newContents[5], Does.Match(" -- " + testMarkdown.ChangelogAuthorInfo + "  .*[+-]\\d\\d\\d\\d"));
			}
		}

		[Test]
		public void UpdateDebianChangelogAllMdListItemsWork()
		{
			var testingTask = new CreateChangelogEntry();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var ChangelogFile = tempFiles.FirstFile;
				File.WriteAllLines(ChangelogFile, new[]
				{
					"## 3.0.97 Beta",
					"- Update French UI Translation",
					"+ When importing, Bloom no longer",
					"  1. makes images transparent when importing.",
					"  4. compresses images transparent when importing.",
					"  9. saves copyright/license back to the original files",
					"    * extra indented list",
					"* Fix insertion of unwanted space before bolded, underlined, and italicized portions of words",
				});
				var debianChangelog = tempFiles.SecondFile;
				File.WriteAllLines(debianChangelog, new[]
				{
					"Bloom (3.0.82 Beta) unstable; urgency=low", "", "  * Older release", "",
					" -- Stephen McConnel <stephen_mcconnel@example.com>  Fri, 12 Jul 2014 14:57:59 -0500", ""
				});
				testingTask.ChangelogFile = ChangelogFile;
				testingTask.VersionNumber = "3.0.97 Beta";
				testingTask.ProductName = "myfavoriteapp";
				testingTask.ChangelogAuthorInfo = "John Hatton <john_hatton@example.com>";
				testingTask.DebianChangelog = debianChangelog;
				Assert.That(testingTask.Execute(), Is.True);
				var newContents = File.ReadAllLines(debianChangelog);
				Assert.That(newContents[0], Does.Contain("3.0.97 Beta"));
				Assert.That(newContents[2], Does.StartWith("  *"));
				Assert.That(newContents[3], Does.StartWith("  *"));
				Assert.That(newContents[4], Does.StartWith("    *"));
				Assert.That(newContents[5], Does.StartWith("    *"));
				Assert.That(newContents[6], Does.StartWith("    *"));
				Assert.That(newContents[7], Does.StartWith("    *")); // The 3rd (and further) level indentation isn't currently supported
				Assert.That(newContents[8], Does.StartWith("  *"));
			}
		}
	}
}
