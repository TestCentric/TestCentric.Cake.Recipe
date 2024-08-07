//////////////////////////////////////////////////////////////////////
// CHECK FOR MISSING AND NON-STANDARD FILE HEADERS
//////////////////////////////////////////////////////////////////////

public static class Headers
{
    private static ICakeContext _context;

    static Headers()
    {
        _context = BuildSettings.Context;
    }

    public static void Check()
    {
        var NoHeader = new List<FilePath>();
        var NonStandard = new List<FilePath>();
        var Exempted = new List<FilePath>();
        int examined = 0;

        var sourceFiles = _context.GetFiles(BuildSettings.SourceDirectory + "**/*.cs");
        var exemptFiles = BuildSettings.ExemptFiles;
        foreach (var file in sourceFiles)
        {
            var path = file.ToString();

            // Ignore autogenerated files in an obj directory
            if (path.Contains("/obj/"))
                continue;

            // Ignore designer files
            if (path.EndsWith(".Designer.cs"))
                continue;

            // Ignore AssemblyInfo files
            if (SIO.Path.GetFileName(path) == "AssemblyInfo.cs")
                continue;

            examined++;
            var header = GetHeader(file);
            if (exemptFiles.Contains(file.GetFilename().ToString()))
                Exempted.Add(file);
            else if (header.Count == 0)
                NoHeader.Add(file);
            else if (!header.SequenceEqual(BuildSettings.StandardHeader))
                NonStandard.Add(file);
        }

        _context.Information("\nSTANDARD HEADER\n");
        foreach (string line in BuildSettings.StandardHeader)
            _context.Information(line);
        _context.Information("");

        if (NoHeader.Count > 0)
        {
            _context.Information("\nFILES WITH NO HEADER\n");
            foreach (var file in NoHeader)
                _context.Information(RelPathTo(file));
        }

        if (NonStandard.Count > 0)
        {
            _context.Information("\nFILES WITH A NON-STANDARD HEADER\n");
            foreach (var file in NonStandard)
            {
                _context.Information(RelPathTo(file));
                _context.Information("");
                foreach (string line in GetHeader(file))
                    _context.Information(line);
                _context.Information("");
            }
        }

        if (Exempted.Count > 0)
        {
            _context.Information("\nEXEMPTED FILES (NO CHECK MADE)\n");
            foreach (var file in Exempted)
                _context.Information(RelPathTo(file));
        }

        _context.Information($"\nFiles Examined: {examined}");
        _context.Information($"Missing Headers: {NoHeader.Count}");
        _context.Information($"Non-Standard Headers: {NonStandard.Count}");
        _context.Information($"Exempted Files: {Exempted.Count}");

        if (NoHeader.Count > 0 || NonStandard.Count > 0)
            throw new Exception("Missing or invalid file headers found");

        if (examined == 0)
            _context.Warning("\nWARNING: There were no '*.cs' files in the source directory. Use of the 'CheckHeaders' task may not make sense for this project.");

        List<string> GetHeader(FilePath file)
        {
            var header = new List<string>();
            var lines = SIO.File.ReadLines(file.ToString());

            foreach (string line in lines)
            {
                if (!line.StartsWith("//"))
                    break;

                header.Add(line);
            }

            return header;
        }

        string RelPathTo(FilePath file)
        {
            int CD_LENGTH = Environment.CurrentDirectory.Length + 1;

            return file.ToString().Substring(CD_LENGTH);
        }
	}
}