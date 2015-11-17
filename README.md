# Resharper Code Contract Nullability FxCop Rules

[![Build status](https://ci.appveyor.com/api/projects/status/pm2fkhbaip5fqrov?svg=true)](https://ci.appveyor.com/project/bkoelman/resharpercodecontractnullabilityfxcop)

This Visual Studio analyzer supports you in consequently annotating your codebase with Resharpers nullability attributes. Doing so improves the [nullability analysis engine in Resharper](https://www.jetbrains.com/resharper/help/Code_Analysis__Code_Annotations.html), so `NullReferenceException`s at runtime will become something from the past.

## Get started

* You need [Visual Studio](https://www.visualstudio.com/) 2013 (or lower) and [Resharper](https://www.jetbrains.com/resharper/) v8 or higher to use this analyzer. See [here](https://github.com/bkoelman/ResharperCodeContractNullability) if you use Visual Studio 2015.

* From the NuGet package manager console:

  `Install-Package ResharperCodeContractNullabilityFxCop`

  `Install-Package JetBrains.Annotations`

* Rebuild your solution

Instead of adding the JetBrains package, you can [put the annotation definitions directly in your source code](https://www.jetbrains.com/resharper/help/Code_Analysis__Annotations_in_Source_Code.html).

![Analyzer in action](https://github.com/bkoelman/ResharperCodeContractNullabilityFxCop/blob/gh-pages/images/fxcop-analyzer-in-action.png)

## Building, Testing, and Debugging

To build, open `ResharperCodeContractNullabilityFxCop.sln` in Visual Studio 2013. Press `Ctrl + A` to run all unit tests. See [How-to-debug.md](src/How-to-debug.md) for debugging instructions.
