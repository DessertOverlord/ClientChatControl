using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientChatControl.Configs
{
    public class ClientChatControlConfig
    {
        /// <summary>
        /// Dictionary of UUIDs and the expiration time of their mute in Unix Epoch Time. (-1 means no expiration).
        /// https://www.epochconverter.com/
        /// </summary>
        public Dictionary<string, long> PlayerUIDsMuted = new Dictionary<string, long>();
        /// <summary>
        /// Enables or disables the showing of a message in chat indicating that a chat message was blocked.
        /// </summary>
        public bool EnableBlockedChatMessageIndicator = true;
        /// <summary>
        /// List of words that will be filtered out of chat. Detects only whole words, not partial matches. For example, if "cat" is in the list, "cat" will be filtered but "caterpillar" will not be. Each word in the list will be replaced with asterisks in chat.
        /// </summary>
        public List<string> FilteredWordList = new List<string>();
    }
}