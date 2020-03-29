using System;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;

namespace DynamicCommandForm
{
    static class Program
    {        
        private const string CANCELED = "CANCELED";
        private const string FROM_CLIPBOARD = "--clipboard";
        private const string FROM_INPUT = "--input";        
        private static bool _fromClipboard = false;
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

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

        public static void SetOut(string text)
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
                TableLayoutPanel panel = ControlBuilders.BuildDynamicFormCommands(guiDef);
                form.Controls.Add(panel);
            }
            else
            {
                Panel p = ControlBuilders.BuildDynamicForm(guiDef, guiDef.Inputs);
                form.Controls.Add(p);
            }            

            form.BringToFront();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            return form;
        }
    }
}