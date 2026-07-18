using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Automatically logs compilation and domain reload timing to
    /// CompileTimeLog.txt (project root, outside Assets) every time scripts
    /// recompile. Intended to provide objective, reproducible before/after
    /// measurements for changes like introducing Assembly Definitions.
    /// Editor-only: must live in a folder named "Editor".
    /// </summary>
    [InitializeOnLoad]
    public static class CompileTimeLogger
    {
        private const string CompileStartTicksKey = "CompileTimeLogger.CompileStartTicks";
        private const string ReloadStartTicksKey = "CompileTimeLogger.ReloadStartTicks";
        private const string CompiledAssembliesKey = "CompileTimeLogger.CompiledAssemblies";
        private const string CompileSecondsKey = "CompileTimeLogger.CompileSeconds";

        private static readonly string LogFilePath = Path.Combine(Application.dataPath, "..", "CompileTimeLog.txt");

        static CompileTimeLogger()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnCompilationStarted(object context)
        {
            SessionState.SetString(CompiledAssembliesKey, string.Empty);
            SessionState.SetString(CompileStartTicksKey, DateTime.UtcNow.Ticks.ToString());
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            var existing = SessionState.GetString(CompiledAssembliesKey, string.Empty);
            var updated = string.IsNullOrEmpty(existing) ? assemblyName : $"{existing}, {assemblyName}";
            SessionState.SetString(CompiledAssembliesKey, updated);
        }

        private static void OnCompilationFinished(object context)
        {
            var startTicks = long.Parse(SessionState.GetString(CompileStartTicksKey, DateTime.UtcNow.Ticks.ToString()));
            var elapsedSeconds = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - startTicks).TotalSeconds;
            SessionState.SetFloat(CompileSecondsKey, (float)elapsedSeconds);
        }

        private static void OnBeforeAssemblyReload()
        {
            SessionState.SetString(ReloadStartTicksKey, DateTime.UtcNow.Ticks.ToString());
        }

        private static void OnAfterAssemblyReload()
        {
            var reloadStartTicksString = SessionState.GetString(ReloadStartTicksKey, string.Empty);

            if (string.IsNullOrEmpty(reloadStartTicksString))
            {
                return;
            }

            var assemblies = SessionState.GetString(CompiledAssembliesKey, string.Empty);
            var assemblyCount = string.IsNullOrEmpty(assemblies) ? 0 : assemblies.Split(',').Length;

            if (assemblyCount == 0)
            {
                // Domain reload with no actual compilation (e.g. entering or
                // exiting Play Mode). Not relevant to compile-time measurement.
                SessionState.EraseString(ReloadStartTicksKey);
                return;
            }

            var reloadStartTicks = long.Parse(reloadStartTicksString);
            var reloadSeconds = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - reloadStartTicks).TotalSeconds;
            var compileSeconds = SessionState.GetFloat(CompileSecondsKey, 0f);
            var total = compileSeconds + reloadSeconds;
            var assemblyList = assemblies;

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Compilation: {compileSeconds:F4}s | AssembliesChanged: {assemblyCount} ({assemblyList}) | DomainReload: {reloadSeconds:F3}s | Total: {total:F3}s";

            File.AppendAllText(LogFilePath, line + Environment.NewLine);
            Debug.Log($"[CompileTimeLogger] {line}");

            SessionState.EraseString(ReloadStartTicksKey);
            SessionState.EraseString(CompiledAssembliesKey);
            SessionState.EraseFloat(CompileSecondsKey);
        }
    }
}