// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
/// <summary>
/// The build to run. A build script calls Build.Run() as its last statement, which
/// runs the targets given on the command line.
/// </summary>
public static Builder Build => new Builder(() => RunTargets(CommandLineOptions.Targets.Values));

static CakeReport RunTargets(ICollection<string> targets)
    => RunTarget(GetOrAddTargetsTask(targets).Name);

static System.Threading.Tasks.Task<CakeReport> RunTargetsAsync(ICollection<string> targets)
    => RunTargetAsync(GetOrAddTargetsTask(targets).Name);

private static ICakeTaskInfo GetOrAddTargetsTask(ICollection<string> targets)
{
    var targetsTaskName = string.Join('+', targets);
    var targetsTask = Tasks.FirstOrDefault(task => task.Name.Equals(targetsTaskName, StringComparison.OrdinalIgnoreCase));

    if (targetsTask == null)
    {
        var badTargets = new List<string>();
        foreach (var target in targets)
            if (Tasks.FirstOrDefault(t => t.Name.Equals(target, StringComparison.OrdinalIgnoreCase)) == null)
                badTargets.Add(target);
        
        if (badTargets.Count > 0)
            throw new Exception($"Unknown target(s): {string.Join(", ", badTargets)}");

        var task = Task(targetsTaskName);

        foreach(var target in targets)
            task.IsDependentOn(target);

        targetsTask = task.Task;
    }

    return targetsTask;
}

/// <summary>
/// Runs an action when Run is called. Used by Build.
/// </summary>
public class Builder
{
    private Action _action;

    public Builder(Action action)
    {
        _action = action;
    }

    public void Run()
    {
        _action();
    }
}
}
