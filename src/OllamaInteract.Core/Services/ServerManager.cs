using System.Diagnostics;

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
            var pythonScriptDirectory = GetPythonScriptDirectory(config.PythonServerDirectory);

            if (pythonScriptDirectory == null)
            {
                Console.WriteLine("No python server directory found");
                return false;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = config.PythonExectuable,
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

    private string? GetPythonScriptDirectory(string[] directories)
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