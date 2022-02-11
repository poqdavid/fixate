/*
 *      This file is part of Fixate distribution (https://github.com/vortex1409/fixate).
 *      Copyright (c) 2022 contributors
 *
 *      Fixate is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      Fixate is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with Fixate.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace Fixate;

internal class Program
{
    private static Logger? logger;

    public static Logger Logger
    { get { return logger ?? throw new Exception("Logger Is Null"); } set => logger = value; }

    public static string CurrentDir { get => Environment.CurrentDirectory; }
    public static string BossDataPath { get; set; } = Path.Combine(CurrentDir, @"bosses");
    public static string SettingPath { get; set; } = Path.Combine(CurrentDir, "config.json");

    public static Settings Config { get; set; } = new();
    public static Localization Localization { get; set; }

    public static Dictionary<string, BossData> bossDatas = new();

    public readonly EventId BotEventId = new(42, "Fixate");

    public DiscordClient Client { get; set; }
    public CommandsNextExtension Commands { get; set; }
    public VoiceNextExtension Voice { get; set; }

    private static void Main(string[] args)
    {
        Thread.CurrentThread.Name = "MainThread";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Config.LoadSettings();
        }
        catch (Exception ex) { Logger.Error(ex, $"Error loading {SettingPath}"); }
        finally { logger.Information("Settings loaded"); }

        Localization = GetLocalization() ?? new();

        var prog = new Program();
        prog.RunBotAsync().GetAwaiter().GetResult();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Program()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        if (!Directory.Exists(BossDataPath))
        {
            Directory.CreateDirectory(BossDataPath);
        }

        foreach (string file in Directory.GetFiles(BossDataPath))
        {
            if (file.Contains(".json"))
            {
                if (!file.Contains(".deps.json") || !file.Contains(".runtimeconfig.json"))
                {
                    string shortfilename = file.Replace(CurrentDir, "");

                    Logger.Information($"Loading {shortfilename}");
                    string json_string = File.ReadAllText(file);

                    if (Json.IsValid(json_string))
                    {
                        JsonSerializerSettings s = new()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ObjectCreationHandling = ObjectCreationHandling.Replace
                        };

                        try
                        {
                            BossData bd = JsonConvert.DeserializeObject<BossData>(json_string, s) ?? throw new Exception($"Error deserializing data"); ;
                            bossDatas.Add(bd.Name, bd);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Problem with deserializing {shortfilename}");
                        }
                    }
                    Logger.Information("Loaded");
                }
            }
        }
    }

    public async Task RunBotAsync()
    {
        ILoggerFactory logFactory = new LoggerFactory().AddSerilog(logger);

        var cfg = new DiscordConfiguration
        {
            Token = Config.Discord.Token,
            TokenType = TokenType.Bot,

            AutoReconnect = true,
            LoggerFactory = logFactory
        };

        this.Client = new DiscordClient(cfg);

        this.Client.Ready += this.Client_Ready;
        this.Client.GuildAvailable += this.Client_GuildAvailable;
        this.Client.ClientErrored += this.Client_ClientError;

        var ccfg = new CommandsNextConfiguration
        {
            StringPrefixes = new[] { Config.Discord.CommandPrefix },

            EnableDms = false,

            EnableMentionPrefix = true
        };

        var slash = this.Client.UseSlashCommands();
        this.Commands = this.Client.UseCommandsNext(ccfg);

        this.Commands.CommandExecuted += this.Commands_CommandExecuted;
        this.Commands.CommandErrored += this.Commands_CommandErrored;

        slash.SlashCommandExecuted += Slash_SlashCommandExecuted;
        slash.SlashCommandErrored += Slash_SlashCommandErrored;
        slash.SlashCommandInvoked += Slash_SlashCommandInvoked;

        this.Commands.RegisterCommands<BotCommands>();
        slash.RegisterCommands<BotSlashCommands>();

        this.Voice = this.Client.UseVoiceNext();

        await this.Client.ConnectAsync();

        await Task.Delay(-1);
    }

    private Task Slash_SlashCommandInvoked(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandInvokedEventArgs e)
    {
        e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully invoked '{e.Context.CommandName}'");

        return Task.CompletedTask;
    }

    private async Task Slash_SlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
    {
        e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Context?.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

        if (e.Exception is ChecksFailedException ex)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{embed}"));
        }
    }

    private Task Slash_SlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Context.CommandName}'");

        return Task.CompletedTask;
    }

    private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

        return Task.CompletedTask;
    }

    private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

        return Task.CompletedTask;
    }

    private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

        return Task.CompletedTask;
    }

    private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {

        e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

        return Task.CompletedTask;
    }

    private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

        if (e.Exception is ChecksFailedException ex)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed);
        }
    }

    public static async Task<byte[]?> GetTTSStream(string text)
    {
        string ssml = GenerateSsml(Config.Voice.Language, Config.Voice.Name, text, Config.Voice.Style);

        var config = SpeechConfig.FromSubscription(Config.Voice.APIToken, Config.Voice.APIRegion);
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw48Khz16BitMonoPcm);

        using (var synthesizer = new SpeechSynthesizer(config, null))
        {
            using var result = await synthesizer.SpeakSsmlAsync(ssml);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                Logger.Warning($"Speech synthesized for text [{text}], and the audio was written to output stream.");
                return AudioConverter.Resample(result.AudioData, 48000, 48000, 1, 2);
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Logger.Warning($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Logger.Warning($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Logger.Warning($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Logger.Warning($"CANCELED: Did you update the subscription info?");
                }

                return null;
            }
        }

        return null;
    }

    private static string GenerateSsml(string locale, string name, string text, string? style = null)
    {
        string ssmlbody;

        if (style is not null)
        {
            if (!style.Equals("none")) { ssmlbody = $"<mstts:express-as style=\"{style}\" >" + text + "</mstts:express-as>"; }
            else { ssmlbody = text; }
        }
        else
        {
            ssmlbody = text;
        }

        string ssmlDoc = $"<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"{locale}\">" +
                         $"<voice name=\"{name}\">" +
                            ssmlbody +
                         "</voice>" +
                         "</speak>";

        return ssmlDoc;
    }

    public static Localization? GetLocalization()
    {
        Localization output = new();
        string path = Path.Combine("localizations", $"{Config.Voice.Language.ToLower()}.json");

        Logger.Information($"Loading {path}");

        string json_string = File.ReadAllText(Path.Combine(CurrentDir, path));

        try
        {
            if (Json.IsValid(json_string))
            {
                JsonSerializerSettings s = new()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                output = JsonConvert.DeserializeObject<Localization>(json_string, s);
            }
            else
            {
                Logger.Error("Localization file isn't valid!!");
                return null;
            }
        }
        catch (Exception ex) { Logger.Error(ex, "Localization file isn't valid!!"); return null; }
        finally
        {
            Logger.Information($"Localization Loaded");
        }

        return output;
    }
}