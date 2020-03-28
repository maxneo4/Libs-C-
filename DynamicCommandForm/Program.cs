using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;
using System.Drawing;

namespace DynamicCommandForm
{
    static class Program
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        private const string CANCELED = "CANCELED";
        private const string FROM_CLIPBOARD = "--clipboard";
        private static Font controlFont = new Font(FontFamily.GenericMonospace, 10);

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { 
                Form form = BuildForm(args);
                form.FormClosed += Form_FormClosed;                
                Application.Run(form);
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
            }                        
        }

        private static void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
                Clipboard.SetText(CANCELED);
        }

        private static Form BuildForm(string[] args)
        {
            Form form = new Form();

            string inputFile = args.Length>0? args[0] : "DynamicForm.json";
            bool fromClipboard = args.Length > 0 && FROM_CLIPBOARD.Equals(args[0]);
            string jsonGuiDef = null;
            if (fromClipboard)
            {
                jsonGuiDef = Clipboard.GetText();
                Clipboard.SetText(CANCELED);
            }else
            {
               jsonGuiDef = File.ReadAllText(inputFile);
            }

            GuidDefinition guiDef = serializer.Deserialize<GuidDefinition>(jsonGuiDef);
                       
            form.Text = guiDef.Title;
            form.AutoSize = true;
                        
            Panel p = BuildDynamicForm(guiDef);
            form.Controls.Add(p);

            form.BringToFront();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            return form;
        }

        private static Panel BuildDynamicForm(GuidDefinition guiDef)
        {
            Input[] inputDefs = guiDef.Inputs;
            TableLayoutPanel panel = BuildPanel(inputDefs);

            Dictionary<string, Control> controlsByName = new Dictionary<string, Control>();

            for (int i = 0; i < inputDefs.Length; i++)
            {
                Input inputDef = inputDefs[i];
                Label label = BuildLabel(inputDef);
                panel.Controls.Add(label, 0, i);
                Control input = BuildInputControl(guiDef, controlsByName, i, inputDef);
                panel.Controls.Add(input, 1, i);
            }
            Button button = new OutButton()
            {
                DynamicControls = controlsByName
            };
            button.Text = guiDef.ButtonText;
            button.Click += Button_Click;

            panel.Controls.Add(button, 1, inputDefs.Length);

            return panel;
        }

        private static TableLayoutPanel BuildPanel(Input[] inputDefs)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Location = new Point(25, 25);
            panel.ColumnCount = 2;
            panel.RowCount = inputDefs.Length + 1;
            panel.AutoSize = true;
            return panel;
        }

        private static Label BuildLabel(Input inputDef)
        {
            Label label = new Label();
            label.Font = controlFont;
            label.BorderStyle = BorderStyle.Fixed3D;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Text = inputDef.Name;
            return label;
        }

        private static Control BuildInputControl(GuidDefinition guiDef, Dictionary<string, Control> controlsByName, int i, Input inputDef)
        {
            Control input;
            if (inputDef.Options == null)
                input = BuildTextBox();
            else
                input = BuildComboBox(inputDef);

            input.Text = inputDef.Value;
            input.Width = guiDef.InputsWidth;
            controlsByName[inputDef.Name] = input;
            return input;
        }

        private static Control BuildTextBox()
        {
            TextBox t = new TextBox();
            t.Font = controlFont;
            t.Click += T_Click;
            return t;
        }

        private static void T_Click(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            t.SelectAll();
        }

        private static void Button_Click(object sender, EventArgs e)
        {
            OutButton button = (OutButton)sender;
            Dictionary<string, string> resultOut = new Dictionary<string, string>();
            foreach (KeyValuePair<string, Control> pair in button.DynamicControls)            
                resultOut[pair.Key] = pair.Value.Text;
            string resultJson = serializer.Serialize(resultOut);
            Clipboard.SetText(resultJson);
            Application.Exit();
        }

        private static ComboBox BuildComboBox(Input inputDef)
        {
            ComboBox options = new AutoCompletedComBobox()
            {
                AutoCompleteMode = AutoCompleteMode.Suggest,
                Options = inputDef.Options
            };
            options.Font = controlFont;
            options.Items.AddRange(inputDef.Options);
            options.TextUpdate += new EventHandler(options_TextUpdate);
            options.Click += Options_Click;
            //options.LostFocus += Options_LostFocus;

            return options;
        }

        //private static void Options_LostFocus(object sender, EventArgs e)
        //{
        //    AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
        //    if (!string.IsNullOrEmpty(options.LastText) && !options.Items.Contains(options.LastText))
        //        options.Items.Add(options.LastText);
        //}

        private static void Options_Click(object sender, EventArgs e)
        {
            AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
            options.Items.Clear();
            options.Items.AddRange(options.Options);
            options.DroppedDown = true;
        }

        private static void options_TextUpdate(object sender, EventArgs e)
        {
            AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
            string item = options.Text;
            options.LastText = options.Text;
            if (!options.SelectedText.Equals(item) || string.IsNullOrEmpty(item))
            {
                string[] filteredItems = options.CalculateFilteredOptions(item);
                options.Items.Clear();
                options.Items.AddRange(filteredItems);
                options.SelectionStart = item.Length;
                if (filteredItems.Length > 1 || !options.SelectedText.Equals(item))
                    options.DroppedDown = true;
            }
        }

        class GuidDefinition
        {
            public string Title { get; set; }
            public string ButtonText { get; set; }
            public int InputsWidth { get; set; }
            public Input[] Inputs { get; set; }
        }

        class Input
        {
            public string Name { get; set; }
            public string Value { get; set; } = string.Empty;
            public string[] Options { get; set; }
        }

        class AutoCompletedComBobox : ComboBox
        {
            public string LastText { get; set; }
            private string[] _toLowerOptions;
            private string[] _options;
            public string[] Options {
                get { return _options; }
                set { _options = value;
                    _toLowerOptions = new string[_options.Length];
                    for (int i = 0; i < _options.Length; i++)
                        _toLowerOptions[i] = _options[i]?.ToLowerInvariant();                    
                } }

            public string[] CalculateFilteredOptions(string text)
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
            public Dictionary<string, Control> DynamicControls { get; set; }
        }
    }
}