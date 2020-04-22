using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace DynamicCommandForm
{
    class ControlBuilders
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        private static Font controlFont = new Font(FontFamily.GenericMonospace, 10);

        internal static TableLayoutPanel BuildDynamicFormCommands(GuidDefinition guiDef)
        {
            string[] commandOptions = guiDef.Commands.Keys.Select((k) => k).ToArray();
            string defaultCommandOption = guiDef.CommandValue ?? commandOptions[0];
            AutoCompletedComBobox commandsComboBox = BuildComboBox(new Input()
            {
                Name = "Commands",
                Options = commandOptions,
                Value = defaultCommandOption
            });
            commandsComboBox.Width = guiDef.InputsWidth;
            commandsComboBox.SelectedIndexChanged += CommandsComboBox_SelectionChangeValue;
            commandsComboBox.AllowNewValues = false;
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Location = new Point(5, 5);
            panel.ColumnCount = 1;
            panel.RowCount = 2;
            panel.AutoSize = true;
            panel.Controls.Add(commandsComboBox, 1, 1);
            panel.Controls.Add(BuildDynamicForm(guiDef, guiDef.Commands[defaultCommandOption], defaultCommandOption), 1, 2);
            commandsComboBox.ChangeItemAction = (string item) =>
            {
                panel.Controls.RemoveAt(1);
                panel.Controls.Add(BuildDynamicForm(guiDef, guiDef.Commands[item], item), 1, 2);
            };
            return panel;
        }

        internal static Panel BuildDynamicForm(GuidDefinition guiDef, Input[] inputDefs, string commandValue = null)
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
                CommandValue = commandValue,
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
            {
                input = BuildTextBox();
                input.Text = inputDef.Value;
            }
            else            
                input = BuildComboBox(inputDef);
            
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
            if (!string.IsNullOrEmpty(button.CommandValue))
                resultOut["commandValue"] = button.CommandValue;
            foreach (KeyValuePair<string, Control> pair in button.DynamicControls)
                resultOut[pair.Key] = pair.Value.Text;
            string resultJson = serializer.Serialize(resultOut);

            Program.SetOut(resultJson);
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
            options.SelectedItem = inputDef.Value;
            options.TextUpdate += new EventHandler(options_TextUpdate);
            options.Click += Options_Click;
            options.KeyDown += Options_KeyDown;
            options.VisibleChanged += Options_VisibleChanged;

            return options;
        }

        private static void Options_VisibleChanged(object sender, EventArgs e)
        {
            ((ComboBox)sender).SelectionStart = ((ComboBox)sender).Text.Length;
        }

        private static void Options_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AutoCompletedComBobox options = (AutoCompletedComBobox)sender;
                if (options.Items.Count > 0)
                {
                    if (options.SelectedItem != null)
                        options.Text = options.SelectedItem.ToString();
                    else
                        options.Text = options.Items[0].ToString();
                }
                else
                    options.Text = string.Empty;

                if (!string.IsNullOrEmpty(options.Text))
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
    }
}