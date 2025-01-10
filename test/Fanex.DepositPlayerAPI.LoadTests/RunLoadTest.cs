#pragma warning disable xUnit1004
namespace Fanex.DepositPlayerAPI.LoadTests;

using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

public class RunLoadTest(ITestOutputHelper testOutputHelper)
{
    // update Skip = null to run Test
    private const string Skip = "Long test, only run if needed, and run independently";
    private const string RunK6Command = "k6 run {0}";

    [Fact(Skip = Skip)]
    public void RunCheckK6Version()
    {
        const string runCommand = "k6 version";
        ExecuteCommand(runCommand);
    }

    [Fact(Skip = Skip)]
    public void RunCreateNewPlayerTest()
    {
        var runCommand = string.Format(RunK6Command, "CreateNewPlayerTest.js");
        ExecuteCommand(runCommand);
    }

    [Fact(Skip = Skip)]
    public void RunCreateShadowAccountTest()
    {
        var runCommand = string.Format(RunK6Command, "CreateShadowAccountTest.js");
        ExecuteCommand(runCommand);
    }

    [Fact(Skip = Skip)]
    public void RunReactivatePlayerTest()
    {
        var runCommand = string.Format(RunK6Command, "ReactivatePlayerTest.js");
        ExecuteCommand(runCommand);
    }

    [Fact(Skip = Skip)]
    public void RunMapSabaIdTest()
    {
        var runCommand = string.Format(RunK6Command, "MapSabaIdTest.js");
        ExecuteCommand(runCommand);
    }

    private void ExecuteCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "LoadTests"),
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                FileName = "powershell.exe",
                CreateNoWindow = false,
                Arguments = "/c " + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        //* Set your output and error (asynchronous) handlers
        process.OutputDataReceived += (_, e) => DataHandler(output, e.Data);
        process.ErrorDataReceived += (_, e) => DataHandler(error, e.Data);
        //* Start process and handlers
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        process.CancelOutputRead();
        process.CancelErrorRead();
        process.Close();

        PrintResult(error, output);
    }

    private static void DataHandler(StringBuilder builder, string? data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            builder.AppendLine(data);
        }
    }

    private void PrintResult(StringBuilder error, StringBuilder output)
    {
        testOutputHelper.WriteLine($"- Error:\n{error}");
        testOutputHelper.WriteLine($"- Output:\n{output}");
    }
}