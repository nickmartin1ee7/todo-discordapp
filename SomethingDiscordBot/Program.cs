using System.ComponentModel;
using System.Drawing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Remora.Commands.Attributes;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Discord.Hosting.Extensions;
using Remora.Rest.Core;
using Remora.Results;

var host = Host.CreateDefaultBuilder();

host.ConfigureAppConfiguration(config =>
{
    config.AddJsonFile("appsettings.json", optional: false);
    config.AddEnvironmentVariables();

#if DEBUG
    config.AddUserSecrets("b88f09f6-d21f-4d83-b9a6-2651851c3c73");
#endif
});

host.AddDiscordService(tokenFactory: sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return config.GetConnectionString("DISCORD_TOKEN")
        ?? throw new ArgumentNullException("DISCORD_TOKEN doesn't exist!");
});

host.ConfigureServices(services =>
{
    services
        .AddDiscordCommands(enableSlash: true)
        .AddCommandTree()
            //.WithCommandGroup<TaskCommands>()
            .WithCommandGroup<MiscCommands>()
            ;
});


var app = host.Build();

var slashService = app.Services.GetRequiredService<SlashService>();
var updateCommandsResult = await slashService.UpdateSlashCommandsAsync();
if (!updateCommandsResult.IsSuccess)
{
    throw new ApplicationException("Failed to update slash commands!");
}

await app.RunAsync();

public class MiscCommands : CommandGroup
{
    private readonly IFeedbackService _feedbackService;

    public MiscCommands(
        IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [Command("ping")]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Check if the bot is working!")]
    public async Task<IResult> Ping()
    {
        // TODO
        var reply = await _feedbackService.SendContextualEmbedAsync(new Embed(nameof(Ping),
            Description: $"Pong!",
            Colour: new Optional<Color>(Color.Green)),
            ct: CancellationToken);


        return Result.FromSuccess();
    }
}
