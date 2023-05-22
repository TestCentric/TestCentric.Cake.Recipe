public class PackageContent
{
	private ICakeContext _context;

	public PackageContent(FilePath[] rootFiles = null, params DirectoryContent[] directories)
	{
		_context = BuildSettings.Context;

		RootFiles = rootFiles ?? new FilePath[0];
		Directories = directories;
	}

	public FilePath[] RootFiles { get; set; }
	public DirectoryContent[] Directories { get; set; }

	public List<NuSpecContent> GetNuSpecContent()
	{
		var result = new List<NuSpecContent>();

		foreach (FilePath file in RootFiles)
			result.Add(new NuSpecContent { Source=file.ToString() });

		foreach(DirectoryContent directory in Directories)
			foreach(NuSpecContent item in directory.GetNuSpecContent())
				result.Add(item);

		return result;
	}

	public List<ChocolateyNuSpecContent> GetChocolateyNuSpecContent(string basePath)
	{
		var result = new List<ChocolateyNuSpecContent>();

		foreach (FilePath file in RootFiles)
			result.Add(new ChocolateyNuSpecContent { Source=basePath + file.ToString() });

		foreach(DirectoryContent directory in Directories)
			foreach(ChocolateyNuSpecContent item in directory.GetChocolateyNuSpecContent(basePath))
				result.Add(item);

		return result;
	}

	public bool VerifyInstallation(DirectoryPath installDirectory)
	{
		bool isOK = true;

		foreach(FilePath filePath in RootFiles)
		{
			var fileName = filePath.GetFilename();

			if (!_context.FileExists(installDirectory.CombineWithFilePath(fileName)))
			{
				RecordError($"File {fileName} was not found.");
				isOK = false;
			}
		}

		foreach (DirectoryContent directory in Directories)
			isOK &= directory.VerifyInstallation(installDirectory);
			
		return isOK;
	}

    public static void RecordError(string msg)
    {
        Console.WriteLine("  ERROR: " + msg);
    }
}

public class DirectoryContent
{
	private ICakeContext _context;
	private DirectoryPath _relDirPath;
	private List<FilePath> _files = new List<FilePath>();

	public DirectoryContent(DirectoryPath relDirPath)
	{
		_context = BuildSettings.Context;
		_relDirPath = relDirPath;
	}

	public DirectoryContent WithFiles(params FilePath[] files)
	{
		_files.AddRange(files);
		return this;
	}

	public DirectoryContent AndFiles(params FilePath[] files)
	{
		return WithFiles(files);
	}

	public DirectoryContent WithFile(FilePath file)
	{
		_files.Add(file);
		return this;
	}

	public DirectoryContent AndFile(FilePath file)
	{
		return AndFiles(file);
	}

	public IEnumerable<NuSpecContent> GetNuSpecContent()
	{
		foreach (FilePath file in _files)
			yield return new NuSpecContent { Source=file.ToString(), Target=_relDirPath.ToString() };
	}

	public IEnumerable<ChocolateyNuSpecContent> GetChocolateyNuSpecContent(string basePath)
	{
		foreach (FilePath file in _files)
			yield return new ChocolateyNuSpecContent { Source = basePath + file.ToString(), Target = _relDirPath.ToString() };
	}

	public bool VerifyInstallation(DirectoryPath installDirectory)
	{
		DirectoryPath absDirPath = installDirectory.Combine(_relDirPath);

		if (!_context.DirectoryExists(absDirPath))
		{
			PackageContent.RecordError($"Directory {_relDirPath} was not found.");
			return false;
		}

		bool isOK = true;

		foreach (var relFilePath in _files)
		{
			var fileName = relFilePath.GetFilename();

			if (!_context.FileExists(absDirPath.CombineWithFilePath(fileName)))
			{
				PackageContent.RecordError($"File {fileName} was not found in directory {_relDirPath}.");
				isOK = false;
			}
		}

		return isOK;
	}
}
