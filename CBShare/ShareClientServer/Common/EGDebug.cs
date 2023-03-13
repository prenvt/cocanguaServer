using System;
using System.IO;
using System.Text;

namespace CBShare.Configuration
{
    class EGLogFile
    {
        DateTime logTime;
        string sErrorTime;

        public EGLogFile()
        {
            logTime = DateTime.Now;
            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            sErrorTime = DateTime.Now.ToString("yyyyMMdd");
        }

        public void ErrorLog(string sPathName, string sErrMsg)
        {
            StreamWriter sw = new StreamWriter(sPathName + sErrorTime, true, Encoding.UTF8);
            sw.WriteLine(string.Format("{0:G} EE=> {1}", logTime, sErrMsg));
            sw.Flush();
            sw.Close();
        }

        public void WarningLog(string sPathName, string sWrnMsg)
        {
            StreamWriter sw = new StreamWriter(sPathName + sErrorTime, true, Encoding.UTF8);
            sw.WriteLine(string.Format("{0:G} WW=> {1}", logTime, sWrnMsg));
            sw.Flush();
            sw.Close();
        }

        public void Log(string sPathName, string sLogMsg)
        {
            StreamWriter sw = new StreamWriter(sPathName + sErrorTime, true, Encoding.UTF8);
            sw.WriteLine(string.Format("{0:G} LL=> {1}", logTime, sLogMsg));
            sw.Flush();
            sw.Close();
        }
    }

    public class EGDebug
    {
        public static readonly string errorlog = "Logs/EGLog";

        public static System.Action<string, string> ClientLog;

        public static void LogChannel(object message, object message2)
        {
            if (ClientLog != null)
            {
                string _AgentID = (string) (message);
                try
                {
                    int agentID = Int32.Parse(_AgentID);
                    if (agentID >= 15)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                ClientLog.Invoke((string) message, (string) message2);
            }
        }

        public static void LogError(object message)
        {
            //EGLogFile Err = new EGLogFile();
            //Err.ErrorLog(HostingEnvironment.MapPath(errorlog), message.ToString());
        }

        static public void LogError(string msg)
        {
            if (ClientLog != null)
                ClientLog.Invoke("ERROR", msg);
        }

        static public void Log(object msg)
        {
            //EGLogFile Err = new EGLogFile();
            //Err.Log(HostingEnvironment.MapPath(errorlog), msg.ToString());
        }

        static public void Log(string msg)
        {
            //EGLogFile Err = new EGLogFile();
            //Err.Log(HostingEnvironment.MapPath(errorlog), msg);
        }

        static public void LogWarning(object msg)
        {
            //EGLogFile Err = new EGLogFile();
            //Err.WarningLog(HostingEnvironment.MapPath(errorlog), msg.ToString());
        }

        static public void LogWarning(string msg)
        {
            //EGLogFile Err = new EGLogFile();
            //Err.WarningLog(HostingEnvironment.MapPath(errorlog), msg);
        }

        static public void Break()
        {
            // null function to avoid missing type error in ShareClientServer
        }
    }
}
