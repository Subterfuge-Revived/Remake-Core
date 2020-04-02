using CommandLine;
using Newtonsoft.Json;

namespace SubterfugeCoreCLI
{
    [Verb("validateEvent", HelpText="Used to validate an event")]
    public class ValidateEventCommand
    {
        [Option('e', "eventJson", Required = true, HelpText = "The JSON text of the event to validate.")]
        public string eventText { get; set; }
        
        [Option('g', "gameId", Required = true, HelpText = "The Game ID of the game to validate the event for.")]
        public string gameId { get; set; }
        
        [Option('t', "token", Required = true, HelpText = "The user's session token.")]
        public string token { get; set; }
    }
}