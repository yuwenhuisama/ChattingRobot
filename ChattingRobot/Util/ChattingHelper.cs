#define RELEASE

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
        static string sm_filePath = AppDomain.CurrentDomain.BaseDirectory + "Temp.wav";

        public static string RecordText { get; set; }

        public static void StartRecording()
        {
            var result = log_in();
            if (result != 0)
            {
                log_out();
                throw new Exception("Error when logging in to xunfei service!");
            }

#if !RELEASE

            sm_waveSource = new WaveIn();
            sm_waveSource.WaveFormat = new NAudio.Wave.WaveFormat(16000, 16, 1);// 16bit,16KHz,Mono的录音格式
            sm_waveSource.DataAvailable += WaveSource_DataAvailable;
            sm_waveSource.RecordingStopped += WaveSource_RecordingStopped;

            if (File.Exists(sm_filePath))
            {
                File.Delete(sm_filePath);
            }

            sm_waveFile = new WaveFileWriter(sm_filePath, sm_waveSource.WaveFormat);

            ////开始录音
            sm_waveSource.StartRecording();
#endif
        }

        public static void StopRecording()
        {
#if !RELEASE
            if (sm_waveSource != null)
            {
                sm_waveSource.StopRecording();
            }
#else
            AudioToString();
#endif
        }

        private static void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            sm_waveFile.Close();

            AudioToString();
        }

        private static void AudioToString()
        {
            IntPtr pStr = IntPtr.Zero;
            var result = run_iat(sm_filePath, ref pStr);
            var recordStr = Ptr2Str(pStr);
            release(pStr);

            if (result == 0)
            {
                RecordText = recordStr;
            }
            else
            {
                log_out();
                throw new Exception(recordStr.Trim());
            }

            log_out();
        }

        private static void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (sm_waveFile != null)
            {
                sm_waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                sm_waveFile.Flush();
            }
        }
        private static string Ptr2Str(IntPtr intPtr2)
        {
            string ss = Marshal.PtrToStringAnsi(intPtr2);
            return ss;
        }

        [DllImport("xfiat.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int log_in();

        [DllImport("xfiat.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int log_out();

        [DllImport("xfiat.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int run_iat(string audioFilePath, ref IntPtr ptr);

        [DllImport("xfiat.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int release(IntPtr ptr);

    }
}
