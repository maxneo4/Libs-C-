using System.Collections.Generic;

namespace DynamicCommandForm
{
    class GuidDefinition
    {
        public string Title { get; set; } = "Dynamic form";
        public string ButtonText { get; set; } = "Ok";
        public int InputsWidth { get; set; } = 200;
        public Input[] Inputs { get; set; }
        public string CommandValue { get; set; }
        public Dictionary<string, Input[]> Commands { get; set; }
    }

    class Input
    {
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;
        public string[] Options { get; set; }
    }
}