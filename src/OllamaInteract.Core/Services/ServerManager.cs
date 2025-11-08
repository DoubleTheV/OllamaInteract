using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace OllamaInteract.Core.Services;

public class ServerManager
{
    private readonly IConfigManager _configManager;
    private readonly IOllamaApiClient _ollamaClient;
    private Process? _pythonServerProcess;
    private bool _disposed;

    public ServerManager(IConfigManager configManager, IOllamaApiClient ollamaClient)
    {
        _configManager = configManager;
        _ollamaClient = ollamaClient;
    }

    public async Task<bool> EnsureServerRunningAsync()
    {
        var config = _configManager.Config;

        bool isAlreadyRunning = await CheckServerHealthAsync();
        if (isAlreadyRunning)
        {
            Console.WriteLine("Server is already running");
            return true;
        }

        if (!config.PythonRunOnStartup)
        {
            Console.WriteLine("Auto-start disabled in config");
            return false;
        }

        await EnsureVenvReadyAsync();
        return await StartPythonServerAsync();
    }

    private async Task<bool> EnsureVenvReadyAsync()
    {
        var config = _configManager.Config;

        var venvExists = CheckVenvExists(GetFirstDirectory(config.PythonVenvDirectory));
        if (!venvExists)
        {
            Console.WriteLine("No python virtual environment");
            if (!await CreateVenvAsync())
            {
                return false;
            }
        }

        var venvDependencies = await CheckVenvDependenciesAsync();
        if (!venvDependencies)
        {
            Console.WriteLine("Python dependencies check failed");
            venvDependencies = await InstallVenvDependenciesAsync();
        }

        return venvDependencies;
    }

    public async Task<bool> StartPythonServerAsync()
    {
        var config = _configManager.Config;

        try
        {
            var pythonScriptDirectory = GetFirstDirectory(config.PythonServerDirectory);

            if (pythonScriptDirectory == null)
            {
                Console.WriteLine("No python server directory found");
                return false;
            }

            var venvDirectory = GetFirstDirectory(config.PythonVenvDirectory);

            var startInfo = new ProcessStartInfo
            {
                FileName = GetVenvPythonPath(venvDirectory),
                Arguments = $"-m uvicorn app.main:app --host {config.PythonHost} --port {config.PythonPort} --reload",
                WorkingDirectory = Path.GetDirectoryName(pythonScriptDirectory),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _pythonServerProcess = new Process { StartInfo = startInfo };

            _pythonServerProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[Python]: {e.Data}");
                }
            };
            _pythonServerProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[Python Error]: {e.Data}");
                }
            };

            Console.WriteLine($"Starting python server on {config.PythonHost}:{config.PythonHost}");
            Console.WriteLine($"{GetVenvPythonPath(venvDirectory)} {startInfo.Arguments}. Working directory: {startInfo.WorkingDirectory}");

            _pythonServerProcess.Start();
            _pythonServerProcess.BeginOutputReadLine();
            _pythonServerProcess.BeginErrorReadLine();

            bool isReady = await WaitForServerAsync(TimeSpan.FromSeconds(30));

            if (isReady)
            {
                Console.WriteLine($"Python server started successfully on {config.PythonHost}:{config.PythonPort}");
                return true;
            }
            else
            {
                Console.WriteLine($"Python server failed to start within timeout");
                StopPythonServer();
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to start python server: {e.Message}");
            return false;
        }

    }

    public void StopPythonServer()
    {
        try
        {
            if (_pythonServerProcess != null && !_pythonServerProcess.HasExited)
            {
                _pythonServerProcess.Kill(entireProcessTree: true);
                _pythonServerProcess.WaitForExit(5000);
                _pythonServerProcess.Dispose();
                _pythonServerProcess = null;
                Console.WriteLine("Python server stopped");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error stopping python server: {e.Message}");
        }
    }

    private static bool CheckVenvExists(string venvDirectory)
    {
        if (string.IsNullOrEmpty(venvDirectory) || !Directory.Exists(venvDirectory))
        {
            return false;
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return File.Exists(Path.Combine(venvDirectory, "Scripts", "python.exe")) &&
                File.Exists(Path.Combine(venvDirectory, "Scripts", "pip.exe"));
        }
        else
        {
            return File.Exists(Path.Combine(venvDirectory, "bin", "python")) &&
                File.Exists(Path.Combine(venvDirectory, "bin", "pip"));
        }
    }

    private async Task<bool> CreateVenvAsync()
    {
        var config = _configManager.Config;
        try
        {
            string venvDirectory = GetFirstDirectory(config.PythonVenvDirectory);
            Directory.CreateDirectory(venvDirectory);

            var startInfo = new ProcessStartInfo()
            {
                FileName = config.PythonExectuable,
                Arguments = $"-m venv \"{venvDirectory}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var venvCreationProcess = Process.Start(startInfo);
            if (venvCreationProcess == null)
            {
                return false;
            }

            string output = await venvCreationProcess.StandardOutput.ReadToEndAsync();
            string error = await venvCreationProcess.StandardError.ReadToEndAsync();

            await venvCreationProcess.WaitForExitAsync();
            if (venvCreationProcess.ExitCode == 0)
            {
                Console.WriteLine("Python virtual environment was created successfully");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to create python virtual environment: {error}");
                return false;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"Issue creating python virtual environment: {e.Message}");
            return false;
        }
    }

    private async Task<bool> CheckVenvDependenciesAsync()
    {
        var config = _configManager.Config;
        var venvDirectory = GetFirstDirectory(config.PythonVenvDirectory);

        if (!CheckVenvExists(venvDirectory))
        {
            Console.WriteLine("Failed to find venv");
            return false;
        }
        try
        {
            string checkScript = @"
try:
    import fastapi
    import uvicorn
    import ollama
    import httpx
    from bs4 import BeautifulSoup
    print('success')
except ImportError as e:
    print(f'Missing: {e}')
    exit(1)
            ";

            var tempFile = Path.GetTempFileName() + ".py";
            await File.WriteAllTextAsync(tempFile, checkScript);

            var startInfo = new ProcessStartInfo()
            {
                FileName = GetVenvPythonPath(venvDirectory),
                Arguments = $"\"{tempFile}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var dependencyCheckProcess = Process.Start(startInfo);
            if (dependencyCheckProcess == null)
            {
                return false;
            }

            string output = await dependencyCheckProcess.StandardOutput.ReadToEndAsync();
            string error = await dependencyCheckProcess.StandardError.ReadToEndAsync();

            await dependencyCheckProcess.WaitForExitAsync();

            Console.WriteLine(output);
            if (dependencyCheckProcess.ExitCode == 0)
            {
                Console.WriteLine("All dependencies are installed");
                return true;
            }
            else
            {
                Console.WriteLine($"Dependencies are not installed: {output}");
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured when checking dependencies: {e.Message}");
            return false;
        }
    }

    private async Task<bool> InstallVenvDependenciesAsync()
    {
        var config = _configManager.Config;
        var venvDirectory = GetFirstDirectory(config.PythonVenvDirectory);

        if (!CheckVenvExists(venvDirectory))
        {
            Console.WriteLine("Failed to find venv");
            return false;
        }

        try
        {
            var requirementsFile = GetFirstFilePath(config.PythonRequirementsDirectory);
            if (string.IsNullOrEmpty(requirementsFile))
            {
                Console.WriteLine("Requirements file not found");
                return false;
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = GetVenvPipPath(venvDirectory),
                Arguments = $"install -r \"{requirementsFile}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var dependenciesInstallProcess = new Process() { StartInfo = startInfo };

            dependenciesInstallProcess.OutputDataReceived += (sender, e) =>
            {
                if(!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[pip]: {e.Data}");
                }
            };
            dependenciesInstallProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[pip Error]: {e.Data}");
                }
            };

            dependenciesInstallProcess.Start();
            dependenciesInstallProcess.BeginOutputReadLine();
            dependenciesInstallProcess.BeginErrorReadLine();

            await dependenciesInstallProcess.WaitForExitAsync();

            if (dependenciesInstallProcess.ExitCode == 0)
            {
                Console.WriteLine("Python dependency installation was completed successfully");
                return false;
            }
            else
            {
                Console.WriteLine("Something went wrong during installing python dependencies");
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error installing python dependencies: {e.Message}");
            return false;
        }
    }

    private string GetVenvPythonPath(string venvDirectory)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return Path.Combine(venvDirectory, "Scripts", "python.exe");
        }
        else
        {
            return Path.Combine(venvDirectory, "bin", "python");            
        }
    }
    private string GetVenvPipPath(string venvDirectory)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return Path.Combine(venvDirectory, "Scripts", "pip.exe");
        }
        else
        {
            return Path.Combine(venvDirectory, "bin", "pip");            
        }
    }
    
    private static string GetFirstDirectory(string[] directories)
    {
        foreach (var dir in directories)
        {
            var directory = ChangeDividerForOS(dir);
            if (Directory.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, directory)))
            {
                return Path.Join(AppDomain.CurrentDomain.BaseDirectory, directory);
            }
        }
        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, ChangeDividerForOS(directories[0]));
    }

    private static string? GetFirstFilePath(string[] paths)
    {
        foreach (var p in paths)
        {
            var path = ChangeDividerForOS(p);
            if (File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, path)))
            {
                return Path.Join(AppDomain.CurrentDomain.BaseDirectory, path);
            }
        }
        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, ChangeDividerForOS(paths[0]));
    }

    private async Task<bool> WaitForServerAsync(TimeSpan timeout)
    {
        var startTime = DateTime.Now;

        while (DateTime.Now - startTime < timeout)
        {
            if (await CheckServerHealthAsync())
            {
                return true;
            }
            await Task.Delay(1000);
        }

        return false;
    }

    public async Task<bool> CheckServerHealthAsync()
    {
        try
        {
            return await _ollamaClient.IsServerHealthyAsync();
        }
        catch
        {
            return false;
        }
    }

    private static string ChangeDividerForOS(string dir)
    {
        return dir.Replace('/', Path.DirectorySeparatorChar);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopPythonServer();
            _disposed = true;
        }
    }
}