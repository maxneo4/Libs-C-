using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;

namespace DynamicCommandForm
{
    static class Program
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        private const string CANCELED = "CANCELED";
        private const string FROM_CLIPBOARD = "--clipboard";
        private const string FROM_INPUT = "--input";
        private static Font controlFont = new Font(FontFamily.GenericMonospace, 10);
        private static bool _fromClipboard = false;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try { 
                Form form = BuildForm(args);
                form.FormClosed += Form_FormClosed;
                Application.Run(form);
            }
            catch (Exception ex)
            {
                SetOutError(ex);
            }
        }

        private static void SetOutError(Exception ex)
        {
            if(_fromClipboard)
                Clipboard.SetText(ex.ToString());
            Console.Error.WriteLine(ex.ToString());
            Console.Error.Flush();
        }

        private static void SetOut(string text)
        {
            if(_fromClipboard)
                Clipboard.SetText(text);
            Console.WriteLine(text);
            Console.Out.Flush();
        }

        private static void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
                SetOut(CANCELED);
        }

        private static Form BuildForm(string[] args)
        {
            Form form = new Form();

            string inputFile = args.Length>0? args[0] : "DynamicForm.json";
            _fromClipboard = args.Length > 0 && FROM_CLIPBOARD.Equals(args[0]);
            bool fromInput = args.Length > 0 && FROM_INPUT.Equals(args[0]);
            string jsonGuiDef = null;
            if(fromInput)
            {
                jsonGuiDef = Console.In.ReadToEnd();
            }
            else if (_fromClipboard)
            {
                jsonGuiDef = Clipboard.GetText();
                SetOut(CANCELED);
            }else
            {
                if (File.Exists(inputFile))
                    jsonGuiDef = File.ReadAllText(inputFile);
                else
                    throw new Exception($"input json file does not exist '{inputFile}' with workingDirectory '{Directory.GetCurrentDirectory()}'");
            }

            GuidDefinition guiDef = serializer.Deserialize<GuidDefinition>(jsonGuiDef);
                       
            form.Text = guiDef.Title;
            form.AutoSize = true;

            if(guiDef.Commands != null)
            {
                string[] commandOptions = guiDef.Commands.Keys.Select((k) => k).ToArray();
                string defaultCommandOption = guiDef.CommandValue ?? commandOptions[0];
                AutoCompletedComBobox commandsComboBox = BuildComboBox(new Input() {
                    Name = "Commands",
                    Options = commandOptions,
                    Value =  defaultCommandOption
                });                
                commandsComboBox.Width = guiDef.InputsWidth;                
                commandsComboBox.SelectedIndexChanged += CommandsComboBox_SelectionChangeValue;
                commandsComboBox.AllowNewValues = false;
                TableLayoutPanel panel = new TableLayoutPanel();
                panel.Location = new Point(5, 5);
                panel.ColumnCount = 1;
                panel.RowCount = 2;
                panel.AutoSize = true;
                panel.Controls.Add(commandsComboBox,1,1);
                panel.Controls.Add(BuildDynamicForm(guiDef, guiDef.Commands[defaultCommandOption]),1,2);
                form.Controls.Add(panel);

                commandsComboBox.ChangeItemAction = (string item) => {
                    if (item == null)
                        item = commandsComboBox.NextText;
                    panel.Controls.RemoveAt(1);
                    panel.Controls.Add(BuildDynamicForm(guiDef, guiDef.Commands[item]), 1, 2);
                };
            }
            else
            {
                Panel p = BuildDynamicForm(guiDef, guiDef.Inputs);
                form.Controls.Add(p);
            }            

            form.BringToFront();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            return form;
        }

        private static Panel BuildDynamicForm(GuidDefinition guiDef, Input[] inputDefs)
        {            
            TableLayoutPanel panel = BuildPanel(inputDefs);

            Dictionary<string, Control> controlsByName = new Dictionary<string, Control>();

            for (int i = 0; i < inputDefs.Length; i++)
            {
                Input inputDef = inputDefs[i];
                Label label = BuildLabel(inputDef.Name);
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

        private static Label BuildLabel(string text)
        {
            Label label = new Label();
            label.Font = controlFont;
            label.BorderStyle = BorderStyle.Fixed3D;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Text = text;
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

            SetOut(resultJson);
            Application.Exit();
        }

        private static AutoCompletedComBobox BuildComboBox(Input inputDef)
        {
            AutoCompletedComBobox options = new AutoCompletedComBobox()
            {
                AutoCompleteMode = AutoCompleteMode.Suggest,
                Options = inputDef.Options
            };
            options.Font = controlFont;
            options.Items.AddRange(inputDef.Options);
            options.Text = inputDef.Value;
            options.TextUpdate += new EventHandler(options_TextUpdate);
            options.Click += Options_Click;
            options.KeyDown += Options_KeyDown;

            return options;
        }

        private static void Options_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
                if (options.Items.Count > 0)
                {
                    if(options.SelectedItem != null)
                        options.NextText = options.SelectedItem.ToString();
                    else
                        options.NextText = options.Items[0].ToString();
                }
                else
                    options.NextText = string.Empty;
                options.Text = options.NextText;
                options.ChangeItemAction?.Invoke(options.Text);
                e.Handled = true;
            }
        }

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
            if (options.Items.Count == 0)
            {
                if (options.AllowNewValues)
                    options.Items.Add(item);
                else
                    options.DroppedDown = false;
            }
        }

        private static void CommandsComboBox_SelectionChangeValue(object sender, EventArgs e)
        {
            AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
            options.ChangeItemAction((options.SelectedItem?.ToString()));
        }

        class GuidDefinition
        {
            public string Title { get; set; } = "Dynamic form";
            public string ButtonText { get; set; } = "Ok";
            public int InputsWidth { get; set; } = 200;
            public Input[] Inputs { get; set; }
            public string CommandValue { get; set; }
            public Dictionary<string,Input[]> Commands { get; set; }
        }

        class Input
        {
            public string Name { get; set; }
            public string Value { get; set; } = string.Empty;
            public string[] Options { get; set; }
        }

        class AutoCompletedComBobox : ComboBox
        {
            public Action<string> ChangeItemAction { get; set; }
            public bool AllowNewValues { get; set; } = true;
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

            public string NextText { get; internal set; }

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