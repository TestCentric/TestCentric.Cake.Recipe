//////////////////////////////////////////////////////////////////////
// SYNTAX FOR EXPRESSING CHECKS
//////////////////////////////////////////////////////////////////////

public static class Check
{
	public static void That(string testDir, IList<PackageCheck> checks)
	{
		if (checks == null)
			throw new ArgumentNullException(nameof(checks));

		bool allOK = true;

		foreach (var check in checks)
			allOK &= check.ApplyTo(testDir);

        if (!allOK) throw new Exception("Verification failed!");
    }
}

public static FileCheck HasFile(string file) => HasFiles(new[] { file });
public static FileCheck HasFiles(params string[] files) => new FileCheck(files);

public static DirectoryCheck HasDirectory(string dir) => new DirectoryCheck(dir);

//////////////////////////////////////////////////////////////////////
// PACKAGECHECK CLASS
//////////////////////////////////////////////////////////////////////

public abstract class PackageCheck
{
	public abstract bool ApplyTo(string testDir);

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
	string[] _files;

	public FileCheck(string[] files)
	{
		_files = files;
	}

	public override bool ApplyTo(string testDir)
	{
		bool isOK = true;

		foreach (string file in _files)
		{
			if (!System.IO.File.Exists(System.IO.Path.Combine(testDir, file)))
			{
				RecordError($"File {file} was not found.");
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
	private string _path;
	private List<string> _files = new List<string>();

	public DirectoryCheck(string path)
	{
		_path = path;
	}

	public DirectoryCheck WithFiles(params string[] files)
	{
		_files.AddRange(files);
		return this;
	}

    public DirectoryCheck AndFiles(params string[] files)
    {
        return WithFiles(files);
    }

	public DirectoryCheck WithFile(string file)
	{
		_files.Add(file);
		return this;
	}

    public DirectoryCheck AndFile(string file)
    {
        return AndFiles(file);
    }

	public override bool ApplyTo(string testDir)
	{
		string combinedPath = System.IO.Path.Combine(testDir, _path);

		if (!System.IO.Directory.Exists(combinedPath))
		{
			RecordError($"Directory {_path} was not found.");
			return false;
		}

		bool isOK = true;

		if (_files != null)
		{
			foreach (var file in _files)
			{
				if (!System.IO.File.Exists(System.IO.Path.Combine(combinedPath, file)))
				{
					RecordError($"File {file} was not found in directory {_path}.");
					isOK = false;
				}
			}
		}

		return isOK;
	}
}
