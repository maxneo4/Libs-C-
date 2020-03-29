using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DynamicCommandForm
{
    class AutoCompletedComBobox : ComboBox
    {
        public Action<string> ChangeItemAction { get; set; }
        public bool AllowNewValues { get; set; } = true;
        public string LastText { get; set; }
        private string[] _toLowerOptions;
        private string[] _options;
        public string[] Options
        {
            get { return _options; }
            set
            {
                _options = value;
                _toLowerOptions = new string[_options.Length];
                for (int i = 0; i < _options.Length; i++)
                    _toLowerOptions[i] = _options[i]?.ToLowerInvariant();
            }
        }

        internal string[] CalculateFilteredOptions(string text)
        {
            List<string> filtered = new List<string>();
            string toLowerText = text.ToLower();
            for (int i = 0; i < _options.Length; i++)
            {
                if (_toLowerOptions[i].Contains(toLowerText))
                    filtered.Add(_options[i]);
            }
            return filtered.ToArray();
        }
    }

    class OutButton : Button
    {
        public string CommandValue { get; set; }
        public Dictionary<string, Control> DynamicControls { get; set; }
    }
}