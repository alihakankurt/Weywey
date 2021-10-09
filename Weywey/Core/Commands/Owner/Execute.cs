using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Weywey.Core.Extensions;

namespace Weywey.Core.Commands.Owner
{
    public partial class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Name("Execute")]
        [Command("execute")]
        [Alias("exec")]
        [Summary("Executes a C# code.")]
        [RequireOwner]
        public async Task ExecuteCommand([Remainder] string code)
        {
            var embed = new EmbedBuilder()
                    .WithColor(Color.DarkMagenta)
                    .WithDescription("Executing...")
                    .Build();
            var message = await ReplyAsync(embed: embed).ConfigureAwait(false);

            try
            {
                var variables = Variables.FromContext(Context);

                var options = ScriptOptions.Default;
                options = options.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                    "System.Threading.Tasks", "Discord", "Discord.Commands", "Discord.WebSocket");
                IEnumerable<Assembly> asm = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));
                options = options.WithReferences(asm);

                Script<object> script = CSharpScript.Create(code.ClearCodeBlock(), options, typeof(Variables));
                script.Compile();

                ScriptState<object> result = await script.RunAsync(variables).ConfigureAwait(false);

                if (result?.ReturnValue is EmbedBuilder)
                    await message.ModifyAsync(x => x.Embed = (result.ReturnValue as EmbedBuilder).Build());

                else if (result?.ReturnValue is Embed)
                    await message.ModifyAsync(x => x.Embed = result.ReturnValue as Embed);

                else if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await message.ModifyAsync(x => x.Embed = new EmbedBuilder()
                        .WithTitle("Execution Result")
                        .WithDescription(result.ReturnValue.ToString())
                        .WithColor(Color.DarkMagenta)
                        .WithCurrentTimestamp().Build()).ConfigureAwait(false);
                
                else
                    await message.ModifyAsync(x => x.Embed = new EmbedBuilder()
                        .WithTitle("Execution Result")
                        .WithDescription("No return value")
                        .WithColor(Color.DarkMagenta)
                        .WithCurrentTimestamp().Build()).ConfigureAwait(false);
            }
            
            catch (Exception exc)
            {
                await message.ModifyAsync(x => x.Embed = new EmbedBuilder()
                        .WithTitle("Execution Result")
                        .WithDescription($"**{exc.GetType()}**: {exc.Message.Split('\n')[0]}")
                        .WithColor(Color.DarkMagenta)
                        .WithCurrentTimestamp().Build()).ConfigureAwait(false);
            }

        }

        public class Variables
        {
            public SocketGuild Guild { get; set; }
            public SocketTextChannel Channel { get; set; }
            public SocketUserMessage Message { get; set; }
            public SocketUser User { get; set; }
            public DiscordSocketClient Client { get; set; }

            public static Variables FromContext(SocketCommandContext context)
            {
                return new Variables
                {
                    Guild = context.Guild,
                    Channel = context.Channel as SocketTextChannel,
                    Message = context.Message,
                    User = context.User,
                    Client = context.Client
                };
            }
        }
    }
}
