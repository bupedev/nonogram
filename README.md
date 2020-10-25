# nonogram
C# Implementation of a Nonogram (Picross) Solver

## Dependencies
This program uses the following packages. These are pre-configured in the solution via NuGet:
* [System.CommandLine](https://github.com/dotnet/command-line-api)

## Requirements
Solution requires [.NET Core 3.1](https://dotnet.microsoft.com/download) or higher. Machines running this software must have at least two cores to benefit from any of the parallel solving methods (particularly `ThreadPool` method). Single threaded systems can still make sure of the `Sequential` solving method.

## Building
Open in Visual Studio and build using `ctrl+shift+B`. Or build on the command line using `dotnet build`

## Usage
Navigate to the build directory and run the program with commands. The following commands are available for use: `solve` and `benchmark`. 

Below are some examples of how to run the program (puzzles will be automatically downloaded so no local dataset is required).

**Solve puzzle #2 using the `Sequential` method:**
```
dotnet nonogram.dll solve --id 2 --method Sequential
```

**Solve puzzle #4 using the `ThreadPool` method with 6 threads:**
```
dotnet nonogram.dll solve --id 2 --method Sequential --threads 6
```

**Benchmark puzzles #2, #4 and #6, 10 times, using both solving methods (default):**
```
dotnet nonogram.dll benchmark --idSet 1,4,6,48,217,239 --trials 10
```

Run the program with the help command for full usage instructions (not fully implemented).
