# Project guidelines

Conventions for anyone changing this repository, people and AI tools alike.
CodeRabbit reads this file as review guidance.

## Layout

- `recipe` -- code for the shared Cake build package, used by other TestCentric projects.
  This project uses it directly in order to build and test itself.
- `build.cake` our own Cake build script, which loads  the recipe.

## Building and testing

- `dotnet tool restore`, then `dotnet cake --target=Build` or `dotnet cake --target=Test`.
  `build.ps1` / `build.cmd` are wrappers around the same thing.

## Code

- Follow `.editorconfig`: four spaces, Allman braces, `System.*` usings first.

## Pull requests

- Work on a branch named `issue-NNNN` after the GitHub issue, and reference the issue in the PR.
- Labels and milestones are set by team members, not by agents. An issue gets its label when
  it is created. When its PR is approved, the team decides whether to add it to the next
  release's milestone.
- Every closed issue in a milestone needs one of the labels under `issue-labels-include` or
  `issue-labels-exclude` in `GitReleaseManager.yaml`, or the release notes fail to build.
  Included labels appear in the release notes. Refactor is the only excluded label: refactoring
  stays out of the release notes because only our developers need to know about it.
- One human review is expected before merging.
