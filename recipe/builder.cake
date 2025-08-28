//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

public Builder Build => new Builder(() => RunTargets(CommandLineOptions.Targets.Values));

CakeReport RunTargets(ICollection<string> targets)
    => RunTarget(GetOrAddTargetsTask(targets).Name);

Task<CakeReport> RunTargetsAsync(ICollection<string> targets)
    => RunTargetAsync(GetOrAddTargetsTask(targets).Name);

private ICakeTaskInfo GetOrAddTargetsTask(ICollection<string> targets)
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