// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    // This file contains classes used to interpret the result XML that is
    // produced by test runs of the GUI.

    /// <summary>
    /// Counts of test results, shared by expected and actual results.
    /// </summary>
    public abstract class ResultSummary
    {
        public string OverallResult { get; set; }
        public int Total  { get; set; }
        public int Passed  { get; set; }
        public int Failed  { get; set; }
        public int Warnings  { get; set; }
        public int Inconclusive  { get; set; }
        public int Skipped { get; set; }
    }

    /// <summary>
    /// The result expected from a package test. Counts left at -1 are not checked.
    /// </summary>
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
        }

        public ExpectedAssemblyResult[] Assemblies { get; set; } = new ExpectedAssemblyResult[0];
    }

    /// <summary>
    /// An assembly expected in a package test's result, optionally with the agent expected to run it.
    /// </summary>
    public class ExpectedAssemblyResult
    {
        public ExpectedAssemblyResult(string assemblyName, string agentName = null)
        {
            AssemblyName = assemblyName;
            AgentName = agentName;
        }

        public string AssemblyName { get; }
        public string AgentName { get; }
    }

    /// <summary>
    /// The result of a package test, read from its result XML file.
    /// </summary>
    public class ActualResult : ResultSummary
    {
        public ActualResult(string resultFile)
        {
            var doc = new XmlDocument();
            doc.Load(resultFile);

            Xml = doc.DocumentElement;
            //if (Xml.Name != "test-run")
            //	throw new Exception("The test-run element was not found.");

            OverallResult = GetAttribute(Xml, "result");
            Total = IntAttribute(Xml, "total");
            Passed = IntAttribute(Xml, "passed");
            Failed = IntAttribute(Xml, "failed");
            Warnings = IntAttribute(Xml, "warnings");
            Inconclusive = IntAttribute(Xml, "inconclusive");
            Skipped = IntAttribute(Xml, "skipped");

            var assemblies = new List<ActualAssemblyResult>();

            foreach (XmlNode node in Xml.SelectNodes("//test-suite[@type='Assembly']"))
                assemblies.Add(new ActualAssemblyResult(node));

            Assemblies = assemblies.ToArray();
        }

        public XmlNode Xml { get; }

        public ActualAssemblyResult[] Assemblies { get; }

        private string GetAttribute(XmlNode node, string name)
        {
            return node.Attributes[name]?.Value;
        }

        private int IntAttribute(XmlNode node, string name)
        {
            string s = GetAttribute(node, name);
            // TODO: We should replace 0 with -1, representing a missing counter
            // attribute, after issue #707 is fixed.
            return s == null ? 0 : int.Parse(s);
        }
    }

    /// <summary>
    /// An assembly found in a package test's result XML.
    /// </summary>
    public class ActualAssemblyResult
    {
        public ActualAssemblyResult(XmlNode xml)
        {
            AssemblyName = xml.Attributes["name"]?.Value;

            //var env = xml.SelectSingleNode("environment");
            var settings = xml.SelectSingleNode("settings");

            // If TargetRuntimeFramework setting is not present, the Runner will probably crash
            // However, it is optional when using tc-lite directly.
            var runtimeSetting = settings?.SelectSingleNode("setting[@name='TargetRuntimeFramework']");
            TargetRuntime = runtimeSetting?.Attributes["value"]?.Value;

            var agentSetting = settings?.SelectSingleNode("setting[@name='SelectedAgentName']");
            AgentName = agentSetting?.Attributes["value"]?.Value;
        }

        public string AssemblyName { get; }
        public string AgentName { get; }

        public string TargetRuntime { get; }
    }
}
