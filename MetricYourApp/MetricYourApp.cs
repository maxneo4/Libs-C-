using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Deployment.API.Services //change namespace when is included as code
{
    public class MetricYourAppLog
    {
        private static string machineId = System.Environment.MachineName;
        private const string _baseUrl = "https://metricyourapp.azurewebsites.net";

        private string date = DateTime.Now.ToString("ddMMMyyyy/HH-mm", CultureInfo.InvariantCulture);
        private HashSet<string> bigTextsAdded = new HashSet<string>();
        private string logPathBase;
        private string logPathEnd;
        private string logBigPath;

        public MetricYourAppLog(string feature, string logName)
        {
            logPathBase = $"{feature}/{machineId}/{date}/";
            logPathEnd = $"{logName}_log.txt";
            logBigPath = $"{feature}/{machineId}/{date}/{logName}_big.txt";
        }

        public async void AppendAllTextToCloudLog(string logPath, string text)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var r = await client.PostAsync($"{_baseUrl}/addtolog?logpath={logPath}",
                        new StringContent(text, Encoding.UTF8, "text/plain"));
                }
                catch { }
            }
        }

        private string GetLogPath(string prefix)
        {
            return $"{logPathBase}{prefix}_{logPathEnd}";
        }

        public void LogsUse(object messageObj, string detail1 = "", string detail2 = "")
        {
            LogUse(messageObj, "", 10, detail1, detail2);
        }

        public void LogUse(object messageObj, string prefix = "", int limitFrameIndex = 8, string detail1 = "", string detail2 = "")
        {
            string message = messageObj != null ? messageObj.ToString() : "message is null";
            StringBuilder stackTrace = new StringBuilder();
            StackTrace t = new StackTrace();

            string callerR = t.GetFrame(1).GetMethod().DeclaringType?.Name + "." + t.GetFrame(1).GetMethod().Name;
            string className = GetType().Name;

            int minFrameIndex = 2;
            while (className.Equals(t.GetFrame(minFrameIndex).GetMethod().DeclaringType?.Name))
                minFrameIndex++;
            limitFrameIndex = minFrameIndex + limitFrameIndex;
            for (int i = minFrameIndex; i <= limitFrameIndex && i < t.FrameCount; i++)
            {
                var method = t.GetFrame(i).GetMethod();
                stackTrace.Append(" => ").Append(method.DeclaringType?.Name).Append(".").Append(method.Name);
            }

            if (message.Length > 250)
            {
                string textHash = sha256_hash(message);
                if (!bigTextsAdded.Contains(textHash))
                {
                    AppendAllTextToCloudLog(logBigPath, $">>{textHash}\r\n{message}\r\n\r\n");
                    bigTextsAdded.Add(textHash);
                }
                message = $"textHash={textHash}";
            }

            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            AppendAllTextToCloudLog(GetLogPath(prefix), $"{time}>>caller: {callerR} |details: | {detail1} | {detail2} \r\n{stackTrace}\r\n{message}\r\n\r\n");
        }

        public string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
