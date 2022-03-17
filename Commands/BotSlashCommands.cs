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

namespace Fixate.Commands;

public class BotSlashCommands : ApplicationCommandModule
{
    [SlashCommand("join", "Joins a voice channel.")]
    public async Task Join(InteractionContext ctx, [Option("channel", "Channel to join")] DiscordChannel chn = null)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("VNext is not enabled or configured."));
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc != null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Already connected in this guild."));
            return;
        }

        var vstat = ctx.Member?.VoiceState;
        if (vstat?.Channel == null && chn == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        if (chn == null)
            chn = vstat.Channel;

        vnc = await vnext.ConnectAsync(chn);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Connected to `{chn.Name}`"));
    }

    [SlashCommand("leave", "Leaves a voice channel.")]
    public async Task Leave(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("VNext is not enabled or configured."));
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Not connected in this guild."));
            return;
        }

        vnc.Disconnect();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Disconnected"));
    }

    [SlashCommand("start", "Starts the bot.")]
    public async Task Start(InteractionContext ctx, [Option("boss", "Name of the boss.")] string boss, [Option("names", "Names of the players seperated by space."), RemainingText] String names = null)
    {
        string[] objects = Array.Empty<string>();

        if (names is not null)
        {
            objects = names.Split(" ");
        }
        string res = "";

        res += $"You picked boss: {boss}";

        int players = Program.bossDatas[boss].Mechanics[0].PlayersInvolved;

        if (players is not 0)
        {
            if (objects.Length == 1 || !(objects.Length >= players))
            {
                res += $"\nPlease enter {players} names seperated by space.";
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(res));
                return;
            }

            List<string> playernames = new();
            res += "\nPlayers are:";
            foreach (string data in objects)
            {
                if (!data.Equals(boss))
                {
                    playernames.Add(data);
                    res += "\n" + data;
                }
            }

            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("VNext is not enabled or configured."));
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Not connected in this guild."));
                return;
            }

            Program.RunMechanic(Program.bossDatas[boss], playernames.ToArray(), vnc);
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(res));
    }

    [SlashCommand("stop", "Stops the bot.")]
    public async Task Stop(InteractionContext ctx)
    {
        Program.stop = true;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Stopped!!!"));
    }

    [SlashCommand("say", "Says the text in voice.")]
    public async Task Say(InteractionContext ctx, [Option("text", "TTS Text"), RemainingText] string text)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("VNext is not enabled or configured."));
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Not connected in this guild."));
            return;
        }

        while (vnc.IsPlaying)
            await vnc.WaitForPlaybackFinishAsync();

        Exception exc = null;
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Saying `{text}`"));

        try
        {
            await vnc.SendSpeakingAsync(true);
            var ffout = new MemoryStream(await Program.GetTTSStream(text) ?? Array.Empty<byte>());

            var txStream = vnc.GetTransmitSink();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();
            await vnc.WaitForPlaybackFinishAsync();
        }
        catch (Exception ex) { exc = ex; }
        finally
        {
            await vnc.SendSpeakingAsync(false);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Finished saying `{text}`"));
        }

        if (exc != null)
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`"));
    }
}