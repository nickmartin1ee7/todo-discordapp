using System.ComponentModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Remora.Commands.Attributes;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Hosting.Extensions;
using Remora.Results;

var host = Host.CreateDefaultBuilder();

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
    [Command("ping")]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Check latency with Discord")]
    public async Task<IResult> Ping()
    {
        // TODO
        return Result.FromSuccess();
    }
}
