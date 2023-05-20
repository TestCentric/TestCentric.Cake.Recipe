public class PackageContent
{
	protected ICakeContext _context;

	public PackageContent(FilePath[] rootFiles = null, params DirectoryContent[] directories)
	{
		_context = BuildSettings.Context;

		RootFiles = rootFiles ?? new FilePath[0];
		Directories = directories;
	}

	public FilePath[] RootFiles { get; set; }
	public DirectoryContent[] Directories { get; set; }

	public IEnumerable<NuSpecContent> GetNuSpecContent()
	{
		foreach (FilePath file in RootFiles)
			yield return new NuSpecContent { Source=file.ToString() };

		foreach(DirectoryContent directory in Directories)
			foreach(NuSpecContent item in directory.GetNuSpecContent())
				yield return item;
	}

	public IEnumerable<ChocolateyNuSpecContent> GetChocolateyNuSpecContent()
	{
		foreach (FilePath file in RootFiles)
			yield return new ChocolateyNuSpecContent { Source=file.ToString() };

		foreach(DirectoryContent directory in Directories)
			foreach(ChocolateyNuSpecContent item in directory.GetChocolateyNuSpecContent())
				yield return item;
	}
}

public class DirectoryContent
{
	private DirectoryPath _relDirPath;
	private List<FilePath> _files = new List<FilePath>();

	public DirectoryContent(DirectoryPath relDirPath)
	{
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

	public IEnumerable<ChocolateyNuSpecContent> GetChocolateyNuSpecContent()
	{
		foreach (FilePath file in _files)
			yield return new ChocolateyNuSpecContent { Source=file.ToString(), Target=_relDirPath.ToString() };
	}
}
