using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Util.Neo.Log
{
    public class LogTxt
    {
        #region constants 
        public static char LEVEL_TRACE = 'T';
        public static char LEVEL_DEBUG = 'D';
        public static char LEVEL_WARN = 'W';
        public static char LEVEL_INFO = 'I';
        public static char LEVEL_ERROR = 'E';
        public static char LEVEL_FATAL ='F';
        public static char LEVEL_OFF = 'O';
        private static string FORMAT_TIME = "HH:mm:ss";    
        #endregion

        #region fields
        private static System.IO.StreamWriter _sw;
        private static string _folder="../Logs/"+DateTime.Now.ToString("MMM-yyyy");  
        private static Char _level;
        private static int _levelInt=-1;//-1 para indicar que no se ha iniciado
        private static string _nameLogger="Logger";
        #endregion

        #region properties


        public static string RemoveLastSlash(String source)
        {
            while (source.EndsWith("/") || source.EndsWith("\\"))
                source = source.Substring(0, source.Length - 1);
            return source;
        }

        /// <summary>
        /// Ruta del directorio de salida
        /// </summary>
        public static string OutDirectory
        {
            set 
            {                
                //un caracter al final '/' o '\' no es necesario y haria que arroje error
                _folder = RemoveLastSlash(value);               
                _folder += "/"+DateTime.Now.ToString("MMM-yyyy");                
            }
            get { return _folder; }
        }
       

        public static string NameLogger
        {
            set { _nameLogger = value; }
        }

        public static Char Level
        {
            set 
            { 
                _level = value;
                _levelInt = CharLevelToInt(_level);                              
            }
        }
        #endregion

        #region methods


        public static void Print(String category, String text, Char levelLog)
        {
            if (_levelInt < 0)
                SetLevelInfo();//nivel por defecto
            //si el nivel del log a imprimir es mayor al permitido no se hara nada
            if (CharLevelToInt(levelLog) < _levelInt)
                return;            

            if (StreamWriter == null)
                throw new Exception("StreamWriter is  null for: " + OutDirectory);
            StreamWriter.WriteLine(DateTime.Now.ToString(FORMAT_TIME) +" "+ levelLog + "<" + category + "> " + text);
            StreamWriter.Flush();
            StreamWriter.Close();
            _sw = null;
        }

        private static System.IO.StreamWriter StreamWriter
        {
            get
            {
                 if (OutDirectory == null)
                     return null;
                 DateTime currentDate = DateTime.Now; 
                 try
                 {                      
                    //si no existe el archivo
                    if (!File.Exists(OutDirectory))
                    {//crea todos los directorios necesarios                        
                        Directory.CreateDirectory(OutDirectory);
                    }                    
                    String outFile = OutDirectory + @"/"+_nameLogger+"{"+currentDate.ToString("dd-MMM-yyyy")+"}.txt";                    
                    return _sw == null ? (_sw = new System.IO.StreamWriter(outFile, true)) : _sw; 
                 }catch
                 {
                     return null;
                 }                                
            }
        }        

        #region set levels

        public static void SetLevelTrace()
        {
            Level = LEVEL_TRACE;
        }

        public static void SetLevelDebug()
        {
            Level = LEVEL_DEBUG;
        }

        public static void SetLevelWarn()
        {
            Level = LEVEL_WARN;
        }

        public static void SetLevelInfo()
        {
            Level = LEVEL_INFO;
        }

        public static void SetLevelError()
        {
            Level = LEVEL_ERROR;
        }

        public static void SetLevelFatal()
        {
            Level = LEVEL_FATAL;
        }

        public static void SetLevelOff()
        {
            Level = LEVEL_OFF;
        }

        #endregion

        #region prints levels

        public static void Trace<T>(string text)
        {  Print(typeof(T).Name, text, LEVEL_TRACE); }

        public static void Debug<T>(string text)
        { Print(typeof(T).Name, text, LEVEL_DEBUG); }

        public static void Warn<T>(string text)
        { Print(typeof(T).Name, text, LEVEL_WARN); }

        public static void Info<T>(string text)
        { Print(typeof(T).Name, text, LEVEL_INFO); }

        public static void Error<T>(string text)
        { Print(typeof(T).Name, text, LEVEL_ERROR); }

        public static void Fatal<T>(string text)
        { Print(typeof(T).Name, text, LEVEL_FATAL); }


        public static void Trace(String categoria ,string text)
        { Print(categoria, text, LEVEL_TRACE); }

        public static void Debug(String categoria ,string text)
        { Print(categoria , text, LEVEL_DEBUG); }

        public static void Warn(String categoria, string text)
        { Print(categoria, text, LEVEL_WARN); }

        public static void Info(String categoria, string text)
        { Print(categoria, text, LEVEL_INFO); }

        public static void Error(String categoria, string text)
        { Print(categoria, text, LEVEL_ERROR); }

        public static void Fatal(String categoria, string text)
        { Print(categoria, text, LEVEL_FATAL); }

        #endregion

        
        private static int CharLevelToInt(Char level)
        {
            if (level == LEVEL_TRACE)
                return 1;
            if (level == LEVEL_DEBUG)
                return 2;
            if (level == LEVEL_WARN)
                return 3;
            if (level == LEVEL_INFO)
                return 4;
            if (level == LEVEL_ERROR)
                return 5;
            if (level == LEVEL_FATAL)
                return 6;
            if (level == LEVEL_OFF)
                return 7;
            return -1;            
        }

        #endregion

    }
}
