// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	//NB: if c# ever allows us to add static exension methods,
	//then all this could be an extension on nunit's Assert class.

	public static class Guard
	{
		public static void AgainstNull(object value, string valueName)
		{
			if (value == null)
				throw new ArgumentNullException(valueName);
		}
	}

	public class AssertThatXmlIn
	{
		public static AssertFile File(string path)
		{
			return new AssertFile(path);
		}
	}

	public class AssertFile : AssertXmlCommands
	{
		private readonly string _path;

		public AssertFile(string path)
		{
			_path = path;
		}

		protected override XmlNode NodeOrDom
		{
			get
			{
				var dom = new XmlDocument();
				dom.Load(_path);
				return dom;
			}
		}
	}

	public abstract class AssertXmlCommands
	{
		protected abstract XmlNode NodeOrDom { get; }

		public void HasAtLeastOneMatchForXpath(string xpath, XmlNamespaceManager nameSpaceManager)
		{
			XmlNode node = GetNode(xpath, nameSpaceManager);
			if (node == null)
			{
				Console.WriteLine("Could not match " + xpath);
				PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNotNull(node, "Not matched: " + xpath);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public  void HasAtLeastOneMatchForXpath(string xpath)
		{
			XmlNode node = GetNode(xpath);
			if (node == null)
			{
				Console.WriteLine("Could not match " + xpath);
				PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNotNull(node, "Not matched: " + xpath);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public void HasSpecifiedNumberOfMatchesForXpath(string xpath, int count, XmlNamespaceManager nameSpaceManager = null)
		{
			HasSpecifiedNumberOfMatchesForXpath(xpath, count, true, nameSpaceManager);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public void HasSpecifiedNumberOfMatchesForXpath(string xpath, int count, bool verbose, XmlNamespaceManager nameSpaceManager = null)
		{
			var nodes = nameSpaceManager == null ? NodeOrDom.SafeSelectNodes(xpath) : NodeOrDom.SafeSelectNodes(xpath, nameSpaceManager);
			if (nodes==null)
			{
				if (count > 0)
				{
					Console.WriteLine("Expected {0} but got 0 matches for {1}", count,  xpath);
					if (verbose)
						PrintNodeToConsole(NodeOrDom);
				}
				Assert.AreEqual(count, 0);
			}
			else if (nodes.Count != count)
			{
				Console.WriteLine("Expected {0} but got {1} matches for {2}", count, nodes.Count, xpath);
				if (verbose)
					PrintNodeToConsole(NodeOrDom);
				Assert.AreEqual(count, nodes.Count, "matches for " + xpath);
			}
		}

		public static void PrintNodeToConsole(XmlNode node)
		{
			var settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlWriter writer = XmlWriter.Create(Console.Out, settings))
			{
				node.WriteContentTo(writer);
				writer.Flush();
				Console.WriteLine();
			}
		}

		public void HasNoMatchForXpath(string xpath, XmlNamespaceManager nameSpaceManager = null, string message = null, bool print = true)
		{
			if (nameSpaceManager == null)
			{
				nameSpaceManager = new XmlNamespaceManager(new NameTable());
			}
			var node = GetNode(xpath, nameSpaceManager);
			if (node != null)
			{
				if (message != null)
					Console.WriteLine(message);
				Console.WriteLine(@"Was not supposed to match " + xpath);
				if (print)
					PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNull(node, "Should not have matched: {0}{1}{2}", xpath, Environment.NewLine, message);
		}

		private XmlNode GetNode(string xpath)
		{
			// Mono: Currently the method XmlNodeExtensions.GetPrefixedPath doesn't allow for / in a literal string
			return Environment.OSVersion.Platform != PlatformID.Unix
				? NodeOrDom.SelectSingleNodeHonoringDefaultNS(xpath)
				: NodeOrDom.SelectSingleNode(xpath);
		}

		private XmlNode GetNode(string xpath, XmlNamespaceManager nameSpaceManager)
		{
			return NodeOrDom.SelectSingleNode(xpath, nameSpaceManager);
		}
	}
}
