using Vintagestory.API.Common;
using Vintagestory.API.Client;
using System.Diagnostics;
using ClientChatControl.Configs;
using System;

namespace ClientChatControl.ModSystems
{
    public class ClientChatControlModSystem : ModSystem
    {
        protected ICoreClientAPI capi;
        public ClientChatControlConfig config;
        private readonly string configName = "clientchatcontrol.json";
        private IClientPlayer cPlayer;
        private string cPlayerUID { get { return cPlayer.PlayerUID; } }

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            LoadConfig();

            ///pseudo-code... idk what the syntax is without hand-holding from Visual Studio
            capi.Event.ChatMessage += HandleChatMessages;
            cPlayer = capi.World.Player;
        }

        private void LoadConfig()
        {
            try
            {
                config = capi.LoadModConfig<ClientChatControlConfig>(configName);
            }
            catch (Exception)
            {
                capi.Logger.Error("");
            }

            if (config == null)
            {
                config = new ClientChatControlConfig();
                capi.StoreModConfig(config, configName);
            }
        }

        private void SaveConfig() 
        {
            capi.StoreModConfig(config, configName);
        }

        private void RegisterClientChatCommands() 
        {
            capi.ChatCommands.Create("c3")
                //.RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .WithDescription("Change settings for Client Chat Control")

                .BeginSubCommand("mute")
                    .BeginSubCommand("playername")
                    .WithDescription("Mutes a player given a player's name. Will be stored using their UID, so even if they change their name, it will be up-to-date. Player must be online! Duration is measured in minutes. Leave blank for forever.")
                    .WithArgs(capi.ChatCommands.Parsers.OnlinePlayer("Player Name"), capi.ChatCommands.Parsers.OptionalInt("Duration"))
                    .HandleWith((args) => {
                        string placeholder = "";
                        if ((int)args[1] <= 0)
                        {
                            config.PlayerUIDsMuted.Add(placeholder, -1);
                            SaveConfig();
                            return TextCommandResult.Success("Muted player " + args[0] + " indefinitely.");
                        } 
                        else 
                        {
                            //find a C# function that can get unix time
                            long currentUnixEpochTime = 0;
                            config.PlayerUIDsMuted.Add(placeholder, currentUnixEpochTime + ((int)args[1] * 60));
                            return TextCommandResult.Success("Muted player " + args[0] + " for " + args[1] + " minutes.");
                        }


                    })
                    .EndSubCommand()

                    .BeginSubCommand("playeruid")
                    .WithDescription("Mutes a player given a player's UID. Not recommended, as usually you won't have this. Use playername instead. Even if they change their name, it will be up-to-date. Duration is measured in minutes. Leave blank for forever.")
                    .WithArgs(capi.ChatCommands.Parsers.PlayerUids("Player UID"), capi.ChatCommands.Parsers.OptionalInt("Duration"))
                    .HandleWith((args) => { return TextCommandResult.Success(); }
                        
                    )
                    .EndSubCommand()

                    .BeginSubCommand("list")
                    .WithDescription("Lists all player UIDs that you currently have muted. Leave blank or enter \"false\" to list only online players, which will list their usernames. Otherwise, enter \"true\" to list all muted UIDs.")
                    .WithArgs(capi.ChatCommands.Parsers.OptionalBool("List all"))
                    .HandleWith((args) => { return TextCommandResult.Success(); }

                    )
                    .EndSubCommand()

                .EndSubCommand()

                .BeginSubCommand("filter")
                    .BeginSubCommand("add")
                    .WithDescription("Adds a word to the word filter list. Any word added to this list will be replaced with asterisks in chat. Detects only whole words.")
                    .WithArgs(capi.ChatCommands.Parsers.Word("Word or Phrase"))
                    .HandleWith((args) => {
                        config.FilteredWordList.Add((string)args[0]);
                        SaveConfig();
                        return TextCommandResult.Success("Added \"" + args[0] + "\" to filter list.");
                    })
                    .EndSubCommand()

                    .BeginSubCommand("remove")
                    .WithDescription("Removes a word from the word filter list. Must be exactly what was entered with .c3 filter add. (This may be easier to do by editing the config file.)")
                    .WithArgs(capi.ChatCommands.Parsers.Word("Word or Phrase"))
                    .HandleWith((args) => {
                        //add some sort of bool to detect if we found the word in the list or not, with a corresponding failure message
                        config.FilteredWordList.Remove((string)args[0]);
                        SaveConfig();
                        return TextCommandResult.Success("Removed \"" + args[0] + "\" from filter list.");
                    })
                        .BeginSubCommand("index")
                        .WithDescription("Removes a word from the word filter list using its index in the list. Use \".c3 filter list\" to list all words and their indicies.")
                        .WithArgs(capi.ChatCommands.Parsers.Int("Index"))
                        .HandleWith((args) => {
                            //as above, failure detection and/or out of bounds access detection so we don't crash
                            string removedWord = config.FilteredWordList[(int)args[0]];
                            config.FilteredWordList.RemoveAt((int)args[0]);
                            SaveConfig();
                            return TextCommandResult.Success("Removed word \"" + removedWord + "\"" + " at index " + args[0] + " from filter list.");
                        })
                        .EndSubCommand()
                    .EndSubCommand()

                    .BeginSubCommand("list")
                    .WithDescription("Lists all currently filtered words.")
                    .HandleWith((args) => {
                        if (config.FilteredWordList.Count == 0) 
                        {
                            return TextCommandResult.Success("Filtered word list is currently empty.");
                        } 
                        else
                        {
                            int i = 0;
                            string output = "";
                            foreach (string filteredWord in config.FilteredWordList) 
                            {
                                output += "[" + i +"]: " + filteredWord;
                                i++;
                                //We want to add a newline to every line except the last one.
                                //Doing this after i++ above means that counting is accurate, since count = maximum index + 1.
                                if (i != config.FilteredWordList.Count) 
                                {
                                    output += "\n";
                                }
                            }
                            return TextCommandResult.Success("Filtered word list: \n");
                        }
                    })
                    .EndSubCommand()

                    .BeginSubCommand("clear")
                    .WithDescription("Clears the entire word filter list.")
                    .HandleWith((args) => { return TextCommandResult.Success(); }

                    )
                        .BeginSubCommand("confirm")
                        .WithDescription("Are you sure?")
                        .HandleWith((args) => {

                            config.FilteredWordList.Clear();
                            SaveConfig();

                            return TextCommandResult.Success("Cleared all words from the word filter list.");
                        })
                        .EndSubCommand()
                    .EndSubCommand()

                .EndSubCommand();
        }

        ///psudo-code below... just outlining. Idk syntax
        /// 

        private void HandleChatMessages(int groupId, string message, EnumChatType chattype, string data)
        {
            //Q: how do I actually get the UID of the person sending the chat message?
            //Q: Do chat messages captured by this event actually provide any sort of information about who sent it?

            /// We can probably use the name of the player as input for the chat command, use that to look up the UID using
            /// IClientWorldAccessor's AllOnlinePlayers List, which includes IPlayer objects for all players currently connected.
            /// We can add that UID to our list of blocked players.
            /// Then, when we receive a chat command, we use that same list to look up the UID, find the player's name,
            /// and then filter out chat messages with that name. I think that should work.
            /// 
            /// We can add an option for UID blocking too if people care that much.
            /// 

            //We only want to block chat messages that are from other players, not server notifications or our own.
            if (chattype == EnumChatType.OthersMessage) 
            {
                string senderPlayerName = "";
                //this command can be used to send a client-side notification to the player running the mod.
                capi.ShowChatMessage("Blocked chat message from player " + senderPlayerName);
            }
        }   
    }
}