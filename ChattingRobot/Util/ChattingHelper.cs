using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChattingRobot.Util
{
    static public class ChattingHelper
    {
        private static string APP_KEY = "f752c30077ff4cb9ab1f2eff92af390b";
        private static string API_URL = "http://www.tuling123.com/openapi/api";

        private static System.Text.UTF8Encoding sm_utf8 = new UTF8Encoding();

        public static async Task<string> SendMessageAsync(string message)
        {
            using (var client = new HttpClient())
            {
                JsonObject jobj = new JsonObject();

                jobj.Add("key", APP_KEY);
                jobj.Add("info", sm_utf8.GetString(sm_utf8.GetBytes(message)));
                jobj.Add("userid", "Python");

                var content = new StringContent(jobj.ToString());

                try
                {
                    var response = await client.PostAsync(API_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        return responseString;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (HttpRequestException e)
                {
                    return "";
                }

            }
        }

        static WaveIn sm_waveSource = null;
        static WaveFileWriter sm_waveFile = null;
        static int BUFFER_NUM = 128;
        static string sm_AppID = "5a1464ce";
        static string sm_filePath = AppDomain.CurrentDomain.BaseDirectory + "Temp.wav";

        public static string RecordText { get; set; }

        private static void StartRecord()
        {
            sm_waveSource = new WaveIn();
            sm_waveSource.WaveFormat = new NAudio.Wave.WaveFormat(16000, 16, 1);// 16bit,16KHz,Mono的录音格式
            sm_waveSource.DataAvailable += WaveSource_DataAvailable;
            sm_waveSource.RecordingStopped += WaveSource_RecordingStopped;

            if(File.Exists(sm_filePath))
            {
                File.Delete(sm_filePath);
            }

            sm_waveFile = new WaveFileWriter(sm_filePath, sm_waveSource.WaveFormat);

            //开始录音
            sm_waveSource.StartRecording();
        }

        private static void StopRecording()
        {
            if(sm_waveSource != null)
            {
                sm_waveSource.StopRecording();
            }
        }

        private static void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            sm_waveFile.Close();
            RecordText = AudioToString(sm_filePath);
        }

        private static void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (sm_waveFile != null)
            {
                sm_waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                sm_waveFile.Flush();
            }
        }

        public static string AudioToString(string inFile)
        {
            int ret = 0;
            string text = String.Empty;
            FileStream fileStream = new FileStream(inFile, FileMode.OpenOrCreate);
            byte[] array = new byte[BUFFER_NUM];
            IntPtr intPtr = Marshal.AllocHGlobal(BUFFER_NUM);
            int audioStatus = 2;
            int epStatus = -1;
            int recogStatus = -1;
            int rsltStatus = -1;
            while (fileStream.Position != fileStream.Length)
            {
                int waveLen = fileStream.Read(array, 0, BUFFER_NUM);
                Marshal.Copy(array, 0, intPtr, array.Length);
                ret = QISRAudioWrite(sm_AppID, intPtr, (uint)waveLen, audioStatus, ref epStatus, ref recogStatus);
                if (ret != 0)
                {
                    fileStream.Close();
                    throw new Exception("QISRAudioWrite err,errCode=" + ret);
                }
                if (recogStatus == 0)
                {
                    IntPtr intPtr2 = QISRGetResult(sm_AppID, ref rsltStatus, 0, ref ret);
                    if (intPtr2 != IntPtr.Zero)
                    {
                        text += Ptr2Str(intPtr2);
                    }
                }
                Thread.Sleep(500);
            }
            fileStream.Close();
            audioStatus = 4;
            ret = QISRAudioWrite(sm_AppID, intPtr, 1u, audioStatus, ref epStatus, ref recogStatus);
            if (ret != 0)
            {
                throw new Exception("QISRAudioWrite write last audio err,errCode=" + ret);
            }
            int timesCount = 0;
            while (true)
            {
                IntPtr intPtr2 = QISRGetResult(sm_AppID, ref rsltStatus, 0, ref ret);
                if (intPtr2 != IntPtr.Zero)
                {
                    text += Ptr2Str(intPtr2);
                }
                if (ret != 0)
                {
                    break;
                }
                Thread.Sleep(200);
                if (rsltStatus == 5 || timesCount++ >= 50)
                {
                    break;
                }
            }
            return text;
        }

        private static string Ptr2Str(IntPtr intPtr2)
        {
            string ss = Marshal.PtrToStringAnsi(intPtr2);
            Marshal.FreeHGlobal(intPtr2);
            return ss;
        }

        // 讯飞API
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogin(string usr, string pwd, string @params);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRSessionBegin(string grammarList, string _params, ref int errorCode);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRGrammarActivate(string sessionID, string grammar, string type, int weight);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRAudioWrite(string sessionID, IntPtr waveData, uint waveLen, int audioStatus, ref int epStatus, ref int recogStatus);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRGetResult(string sessionID, ref int rsltStatus, int waitTime, ref int errorCode);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRSessionEnd(string sessionID, string hints);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRGetParam(string sessionID, string paramName, string paramValue, ref uint valueLen);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogout();
    }
}
