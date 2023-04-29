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
		bool isOK = true;

		foreach (FilePath relFilePath in _files)
		{
			if (!_context.FileExists(testDirPath.CombineWithFilePath(relFilePath)))
			{
				RecordError($"File {relFilePath} was not found.");
				isOK = false;
			}
		}

		return isOK;
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

		if (!_context.DirectoryExists(absDirPath))
		{
			RecordError($"Directory {_relDirPath} was not found.");
			return false;
		}

		bool isOK = true;

		if (_files != null)
		{
			foreach (var relFilePath in _files)
			{
				if (!BuildSettings.Context.FileExists(absDirPath.CombineWithFilePath(relFilePath)))
				{
					RecordError($"File {relFilePath} was not found in directory {_relDirPath}.");
					isOK = false;
				}
			}
		}

		return isOK;
	}
}
