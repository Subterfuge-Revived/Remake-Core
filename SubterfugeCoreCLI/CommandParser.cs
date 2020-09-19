using CommandLine;

namespace SubterfugeCoreCLI
{
    [Verb("validateEvent", HelpText="Used to validate an event")]
    public class ValidateEventCommand
    {
        [Option('e', "eventJson", Required = true, HelpText = "The JSON text of the event to validate.")]
        public string EventText { get; set; }
        
        [Option('g', "gameId", Required = true, HelpText = "The Game ID of the game to validate the event for.")]
        public string GameId { get; set; }
        
        [Option('t', "token", Required = true, HelpText = "The user's session token.")]
        public string Token { get; set; }
    }
}