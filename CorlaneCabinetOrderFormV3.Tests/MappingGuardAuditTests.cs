using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Structural test: every OnXChanged handler in cabinet ViewModels must check
/// _isMapping to prevent handlers from overwriting model values during mapping.
/// Catches the bug regardless of reflection property ordering.
/// </summary>
public partial class MappingGuardAuditTests
{
    /// <summary>
    /// Uses the compile-time path of THIS source file to locate the workspace root.
    /// Immune to test-runner shadow-copying or temp directories.
    /// </summary>
    private static string FindWorkspaceRoot([CallerFilePath] string thisFile = "")
    {
        // thisFile = .../CorlaneCabinetOrderFormV3/CorlaneCabinetOrderFormV3.Tests/MappingGuardAuditTests.cs
        //   up one  = .../CorlaneCabinetOrderFormV3/CorlaneCabinetOrderFormV3.Tests/
        //   up two  = .../CorlaneCabinetOrderFormV3/   (workspace root)
        var root = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;

        // Sanity check: the main project folder must exist
        var projectDir = Path.Combine(root, "CorlaneCabinetOrderFormV3");
        if (!Directory.Exists(projectDir))
            throw new InvalidOperationException(
                $"Expected project folder not found at: {projectDir} (from compile-time path: {thisFile})");

        return root;
    }

    /// <summary>
    /// Regex that captures each OnXChanged handler body.
    /// Matches: "partial void OnSomethingChanged(…) { … }" including one-liner and multi-line forms.
    /// Uses balancing groups to match braces correctly.
    /// </summary>
    [GeneratedRegex(
        @"partial\s+void\s+(On\w+Changed)\s*\([^)]*\)\s*\{((?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*)\}",
        RegexOptions.Singleline)]
    private static partial Regex HandlerBodyRegex();

    /// <summary>
    /// Handler names that legitimately don't need _isMapping because they only
    /// set UI-presentation flags that are unconditionally recalculated by
    /// ApplyStyleVisibility after mapping completes. Add entries here sparingly.
    /// </summary>
    private static readonly HashSet<string> ExemptHandlers = new(StringComparer.Ordinal)
    {
        // These only toggle CustomXxxEnabled visibility — harmless and recalculated by ApplyStyleVisibility.
        "OnSpeciesChanged",
        "OnEBSpeciesChanged",
        "OnDoorSpeciesChanged",
        // Edit-buffer sync, not a data property:
        "OnDrwFrontHeight1EditChanged",
    };

    public static TheoryData<string, string> GetViewModelFiles()
    {
        var root = FindWorkspaceRoot();
        var projectDir = Path.Combine(root, "CorlaneCabinetOrderFormV3");

        var data = new TheoryData<string, string>();

        // Scan all partial files for each VM that uses _isMapping
        string[] vmFolders =
        [
            Path.Combine(projectDir, "ViewModels", "BaseCabinet"),
            Path.Combine(projectDir, "ViewModels", "UpperCabinet"),
            Path.Combine(projectDir, "ViewModels", "FillerEnd"),
            Path.Combine(projectDir, "ViewModels", "Panel"),
        ];

        foreach (var folder in vmFolders)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var file in Directory.GetFiles(folder, "*.cs"))
            {
                var relativePath = Path.GetRelativePath(root, file);
                data.Add(relativePath, file);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(GetViewModelFiles))]
    public void AllOnChangedHandlers_MustCheck_IsMapping(string relativePath, string fullPath)
    {
        var source = File.ReadAllText(fullPath);

        // Skip files that don't contain any OnXChanged handlers
        var matches = HandlerBodyRegex().Matches(source);
        if (matches.Count == 0)
            return;

        var failures = new List<string>();

        foreach (Match match in matches)
        {
            var handlerName = match.Groups[1].Value;
            var body = match.Groups[2].Value;

            if (ExemptHandlers.Contains(handlerName))
                continue;

            // The handler body must contain _isMapping (either as a guard or in any check)
            if (!body.Contains("_isMapping"))
            {
                failures.Add(handlerName);
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail(
                $"{relativePath}: The following OnXChanged handlers are missing an " +
                $"'if (_isMapping) return;' guard:\n  • {string.Join("\n  • ", failures)}\n\n" +
                $"Every handler that modifies data properties must bail out during mapping " +
                $"to prevent overwriting the model's authoritative values.\n" +
                $"If a handler truly doesn't need the guard, add it to ExemptHandlers in this test.");
        }
    }
}