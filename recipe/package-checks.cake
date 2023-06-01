//////////////////////////////////////////////////////////////////////
// SYNTAX FOR EXPRESSING CHECKS
//////////////////////////////////////////////////////////////////////

public static class Check
{
	public static void That(DirectoryPath testDirPath, IList<PackageCheck> checks)
	{
		if (checks == null)
			throw new ArgumentNullException(nameof(checks));

		bool allOK = true;

		foreach (var check in checks)
			allOK &= check.ApplyTo(testDirPath);

        if (!allOK) throw new Exception("Verification failed!");
    }
}

public static FileCheck HasFile(FilePath file) => HasFiles(new[] { file });
public static FileCheck HasFiles(params FilePath[] files) => new FileCheck(files);

public static DirectoryCheck HasDirectory(DirectoryPath dir) => new DirectoryCheck(dir);

public static DependencyCheck HasDependency(string packageId) => new DependencyCheck(packageId);

//////////////////////////////////////////////////////////////////////
// PACKAGECHECK CLASS
//////////////////////////////////////////////////////////////////////

public abstract class PackageCheck
{
	protected ICakeContext _context;

	public PackageCheck()
	{
		_context = BuildSettings.Context;
	}
	
	public abstract bool ApplyTo(DirectoryPath testDirPath);

	protected bool CheckDirectoryExists(DirectoryPath dirPath)
	{
		if (!_context.DirectoryExists(dirPath))
		{
			RecordError($"Directory {dirPath} was not found.");
			return false;
		}

		return true;
	}

	protected bool CheckFileExists(FilePath filePath)
	{
		if (!_context.FileExists(filePath))
		{
			RecordError($"File {filePath} was not found.");
			return false;
		}

		return true;
	}

	protected bool CheckFilesExist(IEnumerable<FilePath> filePaths)
	{
		bool isOK = true;

		foreach (var filePath in filePaths)
			isOK &= CheckFileExists(filePath);

		return isOK;
	}

    protected static void RecordError(string msg)
    {
        Console.WriteLine("  ERROR: " + msg);
    }
}

//////////////////////////////////////////////////////////////////////
// FILECHECK CLASS
//////////////////////////////////////////////////////////////////////

public class FileCheck : PackageCheck
{
	FilePath[] _files;

	public FileCheck(FilePath[] files)
	{
		_files = files;
	}

	public override bool ApplyTo(DirectoryPath testDirPath)
	{
		return CheckFilesExist(_files.Select(file => testDirPath.CombineWithFilePath(file)));
	}
}

//////////////////////////////////////////////////////////////////////
// DIRECTORYCHECK CLASS
//////////////////////////////////////////////////////////////////////

public class DirectoryCheck : PackageCheck
{
	private DirectoryPath _relDirPath;
	private List<FilePath> _files = new List<FilePath>();

	public DirectoryCheck(DirectoryPath relDirPath)
	{
		_relDirPath = relDirPath;
	}

	public DirectoryCheck WithFiles(params FilePath[] files)
	{
		_files.AddRange(files);
		return this;
	}

    public DirectoryCheck AndFiles(params FilePath[] files)
    {
        return WithFiles(files);
    }

	public DirectoryCheck WithFile(FilePath file)
	{
		_files.Add(file);
		return this;
	}

    public DirectoryCheck AndFile(FilePath file)
    {
        return AndFiles(file);
    }

	public override bool ApplyTo(DirectoryPath testDirPath)
	{
		DirectoryPath absDirPath = testDirPath.Combine(_relDirPath);

		if (!CheckDirectoryExists(absDirPath))
			return false;

		return CheckFilesExist(_files.Select(file => absDirPath.CombineWithFilePath(file)));
	}
}

//////////////////////////////////////////////////////////////////////
// DEPENDENCYCHECK CLASS
//////////////////////////////////////////////////////////////////////

public class DependencyCheck : PackageCheck
{
	private string _packageId;

	private List<DirectoryCheck> _directoryChecks = new List<DirectoryCheck>();
	private List<FilePath> _files = new List<FilePath>();

	public DependencyCheck(string packageId)
	{
		_packageId = packageId;
	}

	public DependencyCheck WithFiles(params FilePath[] files)
	{
		_files.AddRange(files);
		return this;
	}

	public DependencyCheck WithFile(FilePath file)
	{
		_files.Add(file);
		return this;
	}

	public DependencyCheck WithDirectory(DirectoryPath relDirPath)
	{
		_directoryChecks.Add(new DirectoryCheck(relDirPath));
		return this;
	}

	public override bool ApplyTo(DirectoryPath testDirPath)
	{
		DirectoryPath packagePath = testDirPath.Combine($"../{_packageId}");

		if (!_context.DirectoryExists(packagePath))
		{
			RecordError($"Dependent package {_packageId} was not found.");
			return false;
		}

		bool isOK = CheckFilesExist(_files.Select(file => packagePath.CombineWithFilePath(file)));

		//foreach (var relFilePath in _files)
		//	isOK &= CheckFileExists(packagePath.CombineWithFilePath(relFilePath));

		foreach (var directoryCheck in _directoryChecks)
			isOK &= directoryCheck.ApplyTo(packagePath);

		return isOK;
	}
}
