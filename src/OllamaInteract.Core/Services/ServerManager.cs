using System.Diagnostics;

namespace OllamaInteract.Core.Services;

public class ServerManager
{
    private Process _pythonServerProcess;

    public async Task StartPythonServerAsync()
    {
        await Task.CompletedTask;
    }

    public void StopPythonServer()
    {
        // _pythonServerProcess.Kill();
        // _pythonServerProcess.Dispose();
    }

    public async Task<bool> EnsureServerRunningAsync()
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);

        try
        {
            var response = await client.GetAsync("http://localhost:8000/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}