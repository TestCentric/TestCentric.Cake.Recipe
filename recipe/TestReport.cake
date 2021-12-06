// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

public class TestReport
{
	public List<string> Errors;

	public TestReport(ExpectedResult expected, ActualResult result)
	{
		Errors = new List<string>();

		ReportMissingFiles(result);

		if (result.OverallResult == null)
			Errors.Add("   The test-run element has no result attribute.");
		else if (expected.OverallResult != result.OverallResult)
			Errors.Add($"   Expected: Overall Result = {expected.OverallResult}\r\n    But was: {result.OverallResult}");
		CheckCounter("Test Count", expected.Total, result.Total);
		CheckCounter("Passed", expected.Passed, result.Passed);
		CheckCounter("Failed", expected.Failed, result.Failed);
		CheckCounter("Warnings", expected.Warnings, result.Warnings);
		CheckCounter("Inconclusive", expected.Inconclusive, result.Inconclusive);
		CheckCounter("Skipped", expected.Skipped, result.Skipped);

		if (expected.Assemblies.Length != result.Assemblies.Length)
			Errors.Add($"   Expected: {expected.Assemblies.Length} assemblies\r\n    But was: {result.Assemblies.Length}");
		for (int i = 0; i < expected.Assemblies.Length && i < result.Assemblies.Length; i++)
        {
			var expectedAssembly = expected.Assemblies[i];
			var resultAssembly = result.Assemblies[i];
            if (expectedAssembly.Name != resultAssembly.Name)
                Errors.Add($"   Expected: Assembly name {expectedAssembly.Name}\r\n    But was: {resultAssembly.Name}");
            if (expectedAssembly.Runtime != null && expectedAssembly.Runtime != resultAssembly.Runtime)
                Errors.Add($"   Expected: Target runtime {expectedAssembly.Runtime}\r\n    But was: {resultAssembly.Runtime}");
        }
	}

	public TestReport(Exception ex)
	{
		Errors = new List<string>();
		Errors.Add($"   {ex.Message}");
	}

	public void DisplayErrors()
	{
		foreach (var error in Errors)
			Console.WriteLine(error);

		Console.WriteLine(Errors.Count == 0
			? "   SUCCESS: Test Result matches expected result!"
			: "\n   ERROR: Test Result not as expected!");
	}

	// File level errors, like missing or mal-formatted files, need to be highlighted
	// because otherwise it's hard to detect the cause of the problem without debugging.
	// This method finds and reports that type of error.
	private void ReportMissingFiles(ActualResult result)
	{
		// Start with all the top-level test suites. Note that files that
		// cannot be found show up as Unknown as do unsupported file types.
		var suites = result.Xml.SelectNodes(
			"//test-suite[@type='Unknown'] | //test-suite[@type='Project'] | //test-suite[@type='Assembly']");

		// If there is no top-level suite, it generally means the file format could not be interpreted
		if (suites.Count == 0)
			Errors.Add("   No top-level suites! Possible empty command-line or misformed project.");

		foreach (XmlNode suite in suites)
		{
			// Narrow down to the specific failures we want
			string suiteResult = GetAttribute(suite, "result");
			string label = GetAttribute(suite, "label");
			string site = suite.Attributes["site"]?.Value ?? "Test";
			if (suiteResult == "Failed" && site == "Test" && label == "Invalid")
			{
				string message = suite.SelectSingleNode("reason/message")?.InnerText;
				Errors.Add($"   {message}");
			}
		}
	}

	private void CheckCounter(string label, int expected, int actual)
	{
		// If expected value of counter is negative, it means no check is needed
		if (expected >= 0 && expected != actual)
			Errors.Add($"   Expected: {label} = {expected}\r\n    But was: {actual}");
	}

	private string GetAttribute(XmlNode node, string name)
	{
		return node.Attributes[name]?.Value;
	}
}
