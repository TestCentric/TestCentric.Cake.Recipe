// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

// This file contains classes used to interpret the result XML that is
// produced by test runs of the GUI.

using System.Xml;

public abstract class ResultSummary
{
	public string OverallResult { get; set; }
	public int Total { get; set; }
	public int Passed { get; set; }
	public int Failed { get; set; }
	public int Warnings { get; set; }
	public int Inconclusive { get; set; }
	public int Skipped { get; set; }

	public AssemblyResult[] Assemblies { get; set; }
}

public class AssemblyResult : ResultSummary
{
	public string Name { get; set; }
	public string Runtime { get; set; }
}

public class ExpectedResult : ResultSummary
{
	public ExpectedResult(string overallResult)
	{
		if (string.IsNullOrEmpty(overallResult))
			throw new ArgumentNullException(nameof(overallResult));

		OverallResult = overallResult;
		// Initialize counters to -1, indicating no expected value.
		// Set properties of those items to be checked.
		Total = Passed = Failed = Warnings = Inconclusive = Skipped = -1;

		Assemblies = new AssemblyResult[0];
	}
}

public class ActualResult : ResultSummary
{
	public ActualResult(string resultFile)
	{
		var doc = new XmlDocument();
		doc.Load(resultFile);

		Xml = doc.DocumentElement;
		if (Xml.Name != "test-run")
			throw new Exception("The test-run element was not found.");

		OverallResult = GetAttribute(Xml, "result");
		Total = IntAttribute(Xml, "total");
		Passed = IntAttribute(Xml, "passed");
		Failed = IntAttribute(Xml, "failed");
		Warnings = IntAttribute(Xml, "warnings");
		Inconclusive = IntAttribute(Xml, "inconclusive");
		Skipped = IntAttribute(Xml, "skipped");

		var assemblies = Xml.SelectNodes("test-suite[@type='Assembly']");
		Assemblies = new AssemblyResult[assemblies.Count];

		for (int i = 0; i < assemblies.Count; i++)
		{
			XmlNode assembly = assemblies[i];
			var env = assembly.SelectSingleNode("environment");
			string name = assembly.Attributes["name"].Value;
			string clrVersion = env.Attributes["clr-version"].Value;
			// This agent is only used with the .NET Framework.
			string runtime = "net" + clrVersion.Substring(0, 3).Replace(".", "");
			Assemblies[i] = new AssemblyResult() { Name = name, Runtime = runtime };
		}
	}

	public XmlNode Xml { get; }

	private string GetAttribute(XmlNode node, string name)
	{
		return node.Attributes[name]?.Value;
	}

	private int IntAttribute(XmlNode node, string name)
	{
		string s = GetAttribute(node, name);
		// TODO: We should replace 0 with -1, representing a missing counter
		// attribute, after issue #904 is fixed.
		return s == null ? 0 : int.Parse(s);
	}
}
