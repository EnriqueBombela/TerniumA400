using Newtonsoft.Json;
using PredictorV2.Models;
using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace PredictorV2.Common
{
    class TCPClient
    {
        private static HostName hostName = new HostName("192.168.152.73");
        private static string serviceName = "22112";
        private static ThreadPoolTimer ProcessTimer;
        private static StreamSocket socket;
        private static DataWriter writer;
        private static State stateMachine;
        private static TCPData data = new TCPData();
        private static DateTime sendTime;

        private enum State
        {
            Init,
            Send,
            Close,
            Idle
        }

        public static void RunProcess()
        {
            stateMachine = State.Init;
            ProcessTimer = ThreadPoolTimer.CreateTimer(Process, TimeSpan.FromMilliseconds(100));
        }

        private static async void Process(ThreadPoolTimer timer)
        {
            try
            {
                int count = 2;

                while (true)
                {
                    if ((DateTime.Now - sendTime).TotalMilliseconds > 500)
                    {
                        sendTime = DateTime.Now;

                        switch (stateMachine)
                        {
                            case State.Init:
                                {
                                    if (socket != null)
                                    {
                                        socket.Dispose();
                                    }

                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }

                                    socket = new StreamSocket();
                                    await socket.ConnectAsync(hostName, serviceName);
                                    writer = new DataWriter(socket.OutputStream);

                                    stateMachine = State.Send;
                                }
                                break;

                            case State.Send:
                                {
                                    //DateTime initTime = DateTime.Now;
                                    if (MainPage.worksheet == null)
                                    {
                                        data.RMS_BS1 = DAQ.DAQ1.TimeInputRMS[0];
                                        data.RMS_BS2 = DAQ.DAQ1.TimeInputRMS[1];
                                        data.RMS_BH = DAQ.DAQ1.TimeInputRMS[2];
                                        data.RMS_BC1 = DAQ.DAQ1.TimeInputRMS[3];
                                        data.RMS_BC2 = DAQ.DAQ1.TimeInputRMS[4];
                                        data.RMS_CH6 = DAQ.DAQ1.TimeInputRMS[5];
                                        data.RMS_CH7 = DAQ.DAQ1.TimeInputRMS[6];
                                        data.RMS_CH8 = DAQ.DAQ1.TimeInputRMS[7];
                                    }
                                    else
                                    {
                                        data.RMS_BS1 = float.Parse(MainPage.worksheet.Range["B" + count].Value);
                                        data.RMS_BS2 = float.Parse(MainPage.worksheet.Range["C" + count].Value);
                                        data.RMS_BC1 = float.Parse(MainPage.worksheet.Range["D" + count].Value);
                                        data.RMS_BC2 = float.Parse(MainPage.worksheet.Range["E" + count].Value);
                                        data.RMS_BH = float.Parse(MainPage.worksheet.Range["F" + count].Value);
                                        count++;
                                        if (count > 1470) count = 2;
                                    }

                                    string stringToSend = JsonConvert.SerializeObject(data);
                                    //var size = stringToSend.Length;
                                    writer.WriteUInt32(writer.MeasureString(stringToSend));
                                    writer.WriteString(stringToSend);

                                    await writer.StoreAsync();

                                    //stateMachine = State.Idle;

                                    //long ms = (DateTime.Now.Ticks - initTime.Ticks) / 1000;
                                }
                                break;

                            case State.Idle:
                                break;
                        }
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                stateMachine = State.Init;
                ProcessTimer = ThreadPoolTimer.CreateTimer(Process, TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
