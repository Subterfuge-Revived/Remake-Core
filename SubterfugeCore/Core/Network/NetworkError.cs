using System.Collections.Generic;
using System.ComponentModel.Design;

namespace SubterfugeCore.Core.Network
{
    public class NetworkError
    {
        public string Message { get; set; }
        public List<List<string>> Errors { get; set; }
    }
}