using System.Diagnostics;
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

            var venvDirectory = GetFirstDirectory(config.PythonVenvDirectory) ?? config.PythonVenvDirectory.First();

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

            _pythonServerProcess.Start();
            _pythonServerProcess.BeginOutputReadLine();
            _pythonServerProcess.BeginErrorReadLine();

            bool isReady = await WaitForServerAsync(TimeSpan.FromSeconds(30));

            if (isReady)
            {
                Console.WriteLine($"Python server started successfully on port {config.PythonPort}");
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
                _pythonServerProcess.Kill();
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

    private string? GetFirstDirectory(string[] directories)
    {
        foreach (var directory in directories)
        {
            if (Directory.Exists(Path.Join(Directory.GetCurrentDirectory(), directory)))
            {
                return directory;
            }
        }
        return null;
    }

    private string? GetFirstFilePath(string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(Path.Join(Directory.GetCurrentDirectory(), path)))
            {
                return path;
            }
        }
        return null;
    }

    private async Task<bool> EnsureVenvReadyAsync()
    {
        var config = _configManager.Config;

        var venvExists = CheckVenvExists(GetFirstDirectory(config.PythonVenvDirectory) ?? config.PythonVenvDirectory.First());
        if (!venvExists)
        {
            Console.WriteLine("No python virtual environment");
            if(!await CreateVenvAsync())
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

    private async Task<bool> CreateVenvAsync()
    {
        var config = _configManager.Config;
        try
        {
            string venvDirectory = GetFirstDirectory(config.PythonVenvDirectory) ?? config.PythonVenvDirectory.First();
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
        var venvDirectory = GetFirstDirectory(config.PythonVenvDirectory) ?? config.PythonVenvDirectory.First();

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
    
    private async Task<bool> InstallVenvDependenciesAsync()
    {
        return false;
    }

    private bool CheckVenvExists(string venvDirectory)
    {
        if (string.IsNullOrEmpty(venvDirectory) || !Directory.Exists(venvDirectory))
        {
            return false;
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return File.Exists(Path.Combine(venvDirectory, "Scripts", "python.exe")) &&
                File.Exists(Path.Combine(venvDirectory, "bin", "pip"));
        }
        else
        {
            return File.Exists(Path.Combine(venvDirectory, "bin", "python")) &&
                File.Exists(Path.Combine(venvDirectory, "bin", "pip"));
        }
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

    public void Dispose()
    {
        if (!_disposed)
        {
            StopPythonServer();
            _disposed = true;
        }
    }
}