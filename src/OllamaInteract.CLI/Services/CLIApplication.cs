using System.Text;
using System.Threading.Channels;
using OllamaInteract.Core.Models;
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

    private bool _shouldRefresh = true;
    private bool _isRunning = true;
    private bool _isConnected = false;

    private enum Mode
    {
        NORMAL,
        INPUT,
        VISUAL
    }
    private Mode currentMode = Mode.NORMAL;
    private string statusMessage = string.Empty;
    private List<AvailableModel> availableModels = new List<AvailableModel>();
    private AvailableModel currentModel = new AvailableModel() {Name = "None"};
    private string Input = string.Empty;
    

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

        BlockExit();
    }

    public async Task RunAsync(string[] args)
    {
        _ = Task.Run(InitializeAsync);

        _ = Task.Run(ReadInputAsync);

        await RenderLiveDisplay();

        _serverManager.Dispose();
    }

    private async Task InitializeAsync()
    {
        statusMessage = "Initializing";

        try
        {
            _isConnected = await _serverManager.EnsureServerRunningAsync();

            if (_isConnected)
            {
                statusMessage = "Successfully connected to Ollama";
            }
            else
            {
                statusMessage = "Failed to connect to Python bridge-server";
                return;
            }

            availableModels = await _ollamaClient.GetAvailableModelsAsync();

            if (availableModels.Count > 0)
            {
                currentModel = availableModels.First();
                statusMessage = "Loaded available models";
            }

            statusMessage = "Successfully initialized";
        }
        catch (Exception e)
        {
            statusMessage = $"Error during initialization: {e.Message}";
            _isConnected = false;
        }
    }

    private async Task RenderLiveDisplay()
    {
        await AnsiConsole.Live(new Layout("Root"))
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .StartAsync(async ctx =>
            {
                while(_isRunning)
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
                                new Layout("ConversationName").Ratio(1),
                                new Layout("ModelChosen").Ratio(1)
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
                new Text($"Model: {currentModel.Name}").Justify(Justify.Right)
            ).Border(BoxBorder.Rounded).Expand()
        );

        var columnContent = new Rows(
            new Columns(
                new Panel(
                    new Text("AI messages").Justify(Justify.Left)
                ).Border(BoxBorder.None).Expand(),
                new Panel(
                    new Text("User messages").Justify(Justify.Right)
                ).Border(BoxBorder.None).Expand()
            )
        );

        layout["Content"].Update(
            new Panel(columnContent).Border(BoxBorder.Rounded).Expand()
        );

        layout["Input"].Update(
            new Panel(
                new Text($"{(currentMode == Mode.NORMAL? statusMessage : Input)}")
            ).Header($"|{currentMode}|").Border(BoxBorder.Rounded).Expand()
        );


        return layout;
    }

    private async Task ReadInputAsync()
    {
        while (_isRunning)
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
        if (key.Key == ConsoleKey.Escape)
        {
            currentMode = Mode.NORMAL;
            Input = string.Empty;
            return;
        }
        if (key.Key == ConsoleKey.Enter)
        {
            if (Input.First() == ':')
            {
                HandleCommand();
            }
            else
            {
                HandlePrompt();
            }
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
                else if(key.KeyChar == ':')
                {
                    currentMode = Mode.INPUT;
                    Input = ":";
                }
                else if (key.Key == ConsoleKey.V)
                {
                    currentMode = Mode.VISUAL;
                }
                break;

            case Mode.INPUT:
                if (key.Key == ConsoleKey.Backspace && Input.Length > 0)
                {
                    Input = Input.Substring(0, Input.Length - 1);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    Input += key.KeyChar;
                    _shouldRefresh = true;
                }
                break;
        }
    }

    private void BlockExit()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Input = "[Use :q to close the app properly]";
        };
    }

    private void HandleCommand()
    {
        switch(Input.Substring(1, Input.Length - 1))
        {
            case "q":
                _isRunning = false;
                break;
        }
    }
    
    private void HandlePrompt()
    {
        
    }

}