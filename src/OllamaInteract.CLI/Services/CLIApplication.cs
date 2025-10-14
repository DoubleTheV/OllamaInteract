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
                        new Layout("Header").Ratio(1)
                            .SplitColumns(
                                new Layout("ConversationName"),
                                new Layout("ModelChosen")
                            ),
                        new Layout("Content").Ratio(8),
                        new Layout("Input").Ratio(1)
                    )
            );

        layout["Conversations"].Update(
            new Panel(
                new Text("Conversations")
            ).Border(BoxBorder.Rounded).Expand()
        );

        layout["ConversationName"].Update(
            new Panel(
                new Text("Conversation Name").Justify(Justify.Left)
            ).Border(BoxBorder.Rounded).Expand()
        );
        layout["ModelChosen"].Update(
            new Panel(
                new Text("Model: None").Justify(Justify.Right)
            ).Border(BoxBorder.Rounded).Expand()
        );

        layout["Content"].Update(
            new Panel(
                new Text("")
            ).Border(BoxBorder.Rounded).Expand()
        );

        layout["Input"].Update(
            new Panel(
                new Text("")
            ).Border(BoxBorder.Rounded).Expand()
        );


        return layout;
    }
}