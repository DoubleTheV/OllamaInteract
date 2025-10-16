using System.Text;
using System.Threading.Channels;
using OllamaInteract.Core.Services;
using Spectre.Console;

namespace OllamaInteract.CLI.Services;

public class CLIApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigManager _configManager;
    private readonly IOllamaApiClient _ollamaClient;
    private readonly ServerManager _serverManager;

    private readonly Channel<ConsoleKeyInfo> _inputChannel;
    private string Input = string.Empty;

    private bool _shouldRefresh = true;
    private enum Mode
    {
        NORMAL,
        INPUT,
        VISUAL
    }
    private Mode currentMode = Mode.NORMAL;
    

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

        _inputChannel = Channel.CreateUnbounded<ConsoleKeyInfo>();
    }

    public async Task RunAsync(string[] args)
    {
        _ = Task.Run(ReadInputAsync);

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
                    while(_inputChannel.Reader.TryRead(out var key))
                    {
                        HandleInput(key);
                    }

                    if (_shouldRefresh)
                    {
                        var layout = CreateLayout();
                        ctx.UpdateTarget(layout);
                        ctx.Refresh();
                    }

                    await Task.Delay(30);
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
                new Text($"--{currentMode}--\n{Input}")
            ).Border(BoxBorder.Rounded).Expand()
        );


        return layout;
    }

    private async Task ReadInputAsync()
    {
        while (true)
        {
            try
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    await _inputChannel.Writer.WriteAsync(key);
                }

                await Task.Delay(15);
            }
            catch { break; }
        }
    }
    
    private void HandleInput(ConsoleKeyInfo key)
    {
        if(key.Key == ConsoleKey.Escape)
        {
            currentMode = Mode.NORMAL;
            Input = string.Empty;
            return;
        }

        switch (currentMode)
        {
            case Mode.NORMAL:
                if (key.Key == ConsoleKey.I)
                {
                    currentMode = Mode.INPUT;
                }
                else if(key.Key == ConsoleKey.V)
                {
                    currentMode = Mode.VISUAL;
                }
                break;

            case Mode.INPUT:
                if (key.Key == ConsoleKey.Backspace && Input.Length > 0)
                {
                    Input = Input.Substring(0, Input.Length - 1);
                }
                else if(key.Key != ConsoleKey.Backspace)
                {
                    Input += key.KeyChar;
                    _shouldRefresh = true;
                }
                break;
        }
    }
}