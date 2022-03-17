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

using DescriptionAttribute = DSharpPlus.CommandsNext.Attributes.DescriptionAttribute;

namespace Fixate.Commands;

public class BotCommands : BaseCommandModule
{
    [Command("join"), Description("Joins a voice channel.")]
    public async Task Join(CommandContext ctx, DiscordChannel chn = null)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.RespondAsync("VNext is not enabled or configured.");
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc != null)
        {
            await ctx.RespondAsync("Already connected in this guild.");
            return;
        }

        var vstat = ctx.Member?.VoiceState;
        if (vstat?.Channel == null && chn == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        if (chn == null)
            chn = vstat.Channel;

        vnc = await vnext.ConnectAsync(chn);
        await ctx.RespondAsync($"Connected to `{chn.Name}`");
    }

    [Command("leave"), Description("Leaves a voice channel.")]
    public async Task Leave(CommandContext ctx)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.RespondAsync("VNext is not enabled or configured.");
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.RespondAsync("Not connected in this guild.");
            return;
        }

        vnc.Disconnect();
        await ctx.RespondAsync("Disconnected");
    }

    [Command("start"), Description("Starts the bot.")]
    public async Task Start(CommandContext ctx, params string[] objects)
    {
        if (objects is null || objects.Length == 0)
        {
            await ctx.RespondAsync("Please enter a boss name");
            return;
        }

        string boss = objects[0];
        string res = "";

        res += $"You picked boss: {boss}";

        int players = Program.bossDatas[boss].Mechanics[0].PlayersInvolved;

        if (players is not 0)
        {
            if (objects.Length == 1 || !(objects.Length >= (players + 1)))
            {
                res += $"\nPlease enter {players} names seperated by space.";
                await ctx.RespondAsync(res);
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
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            Program.RunMechanic(Program.bossDatas[boss], playernames.ToArray(), vnc);
        }
        await ctx.RespondAsync(res);
    }

    [Command("stop"), Description("Stops the bot.")]
    public async Task Stop(CommandContext ctx)
    {
        Program.stop = true;
        await ctx.RespondAsync("Stopped!!");
    }

    [Command("say"), Description("Says the text in voice.")]
    public async Task Say(CommandContext ctx, [RemainingText] string text)
    {
        var vnext = ctx.Client.GetVoiceNext();
        if (vnext == null)
        {
            await ctx.RespondAsync("VNext is not enabled or configured.");
            return;
        }

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.RespondAsync("Not connected in this guild.");
            return;
        }

        while (vnc.IsPlaying)
            await vnc.WaitForPlaybackFinishAsync();

        Exception exc = null;
        await ctx.Message.RespondAsync($"Saying `{text}`");

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
            await ctx.Message.RespondAsync($"Finished saying `{text}`");
        }

        if (exc != null)
            await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
    }
}