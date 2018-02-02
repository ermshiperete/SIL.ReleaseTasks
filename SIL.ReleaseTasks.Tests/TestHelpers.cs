// // Copyright (c) 2018 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace SIL.ReleaseTasks.Tests
{
	internal sealed class MockEngine : IBuildEngine
	{
		public List<string> LoggedMessages = new List<string>();

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
			IDictionary                     targetOutputs)
		{
			throw new NotImplementedException();
		}

		public bool ContinueOnError { get; private set; }
		public int LineNumberOfTaskNode { get; private set; }
		public int ColumnNumberOfTaskNode { get; private set; }
		public string ProjectFileOfTaskNode { get; private set; }
	}

	/// <summary>
	/// This class is implemented to avoid a dependency on Palaso (which isn't strictly circular, but sure feels like it)
	/// The TempFile class that lives in SIL.IO is a more robust and generally preferred implementation.
	/// </summary>
	internal sealed class TwoTempFilesForTest : IDisposable
	{
		public string FirstFile { get; set; }
		public string SecondFile;

		public TwoTempFilesForTest(string firstFile, string secondFile)
		{
			FirstFile = firstFile;
			SecondFile = secondFile;
		}
		public void Dispose()
		{
			try
			{
				if(File.Exists(FirstFile))
				{
					File.Delete(FirstFile);
				}
				if(File.Exists(SecondFile))
				{
					File.Delete(SecondFile);
				}
			}
			catch(Exception)
			{
				// We try to clean up after ourselves, but we aren't going to fail tests if we couldn't
			}
		}
	}
}
