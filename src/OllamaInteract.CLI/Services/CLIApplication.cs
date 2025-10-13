using System.Text;
using OllamaInteract.Core.Services;
using Spectre.Console;

namespace OllamaInteract.CLI.Services;

public class CLIApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigManager _configManager;
    private readonly IOllamaApiClient _ollamaClient;
    private readonly ServerManager _serverManager;

    public CLIApplication(
        IServiceProvider serviceProvider,
        IConfigManager configManager,
        IOllamaApiClient ollamaClient,
        ServerManager serverManager
    )
    {
        _serviceProvider = serviceProvider;
        _configManager = configManager;
        _ollamaClient = ollamaClient;
        _serverManager = serverManager;
    }

    public async Task RunAsync(string[] args)
    {
        await RenderLiveDisplay();
    }

    private async Task RenderLiveDisplay()
    {
        await AnsiConsole.Live(new Layout("Root"))
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .StartAsync(async ctx =>
            {
                while(true)
                {
                    var layout = CreateLayout();
                    ctx.UpdateTarget(layout);
                    ctx.Refresh();

                    await Task.Delay(33);
                }
            }
            );
    }
    
    private Layout CreateLayout()
    {
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Conversations").Ratio(1),
                new Layout("Main").Ratio(3)
                    .SplitRows(
                        new Layout("Header").Ratio(1),
                        new Layout("Content").Ratio(7),
                        new Layout("Input").Ratio(2)
                    )
            );

        return layout;
    }
}