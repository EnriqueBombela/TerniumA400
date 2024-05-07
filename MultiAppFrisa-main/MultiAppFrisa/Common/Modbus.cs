using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.ApplicationModel.AppService;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Threading;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;

namespace PredictorV2.Common
{
    class Modbus
    {
        public static Modbus Modbus1 = null;
        public static void NewHandler()
        {
            Modbus1 = new Modbus();
        }

        private Modbus()
        {
            BannerDataArray = new BannerData[1];
            BannerDataArray[0] = new BannerData();
            LoadCommModbus();
            ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
        }

        static Byte[] ZAxisVariables0 = new Byte[] { 0x01, 0x03, 0x09, 0x60, 0x00, 0x0A, 0xC6, 0x4F };
        static Byte[] XAxisVariables0 = new Byte[] { 0x01, 0x03, 0x09, 0x92, 0x00, 0x0A, 0x67, 0xBC };
        static Byte[] Temperature0 = new Byte[] { 0x01, 0x03, 0x00, 0x28, 0x00, 0x02, 0x44, 0x03 };
        static Byte[] ZAxisVariables1 = new Byte[] { 0x02, 0x03, 0x09, 0x60, 0x00, 0x0A, 0xC6, 0x7C };
        static Byte[] XAxisVariables1 = new Byte[] { 0x02, 0x03, 0x09, 0x92, 0x00, 0x0A, 0x67, 0x8F };
        static Byte[] Temperature1 = new Byte[] { 0x02, 0x03, 0x00, 0x28, 0x00, 0x02, 0x44, 0x30 };
        static Byte[] ZAxisVariables2 = new Byte[] { 0x03, 0x03, 0x09, 0x60, 0x00, 0x0A, 0xC7, 0xAD };
        static Byte[] XAxisVariables2 = new Byte[] { 0x03, 0x03, 0x09, 0x92, 0x00, 0x0A, 0x66, 0x5E };
        static Byte[] Temperature2 = new Byte[] { 0x03, 0x03, 0x00, 0x28, 0x00, 0x02, 0x45, 0xE1 };
        static Byte[] ZAxisVariables3 = new Byte[] { 0x04, 0x03, 0x09, 0x60, 0x00, 0x0A, 0xC6, 0x1A };
        static Byte[] XAxisVariables3 = new Byte[] { 0x04, 0x03, 0x09, 0x92, 0x00, 0x0A, 0x67, 0xE9 };
        static Byte[] Temperature3 = new Byte[] { 0x04, 0x03, 0x00, 0x28, 0x00, 0x02, 0x44, 0x56 };

        protected ThreadPoolTimer ActionTimer = null;
        private SerialDevice serialPort = null; //Name is register in Manifiest capability
        DataWriter SerialPortdataWriter = null;
        DataReader SerialPortdataReader = null;
        private CancellationTokenSource ReadCancellationTokenSource;
        byte[] ModulesAddress = null;
        private static Byte[] ReceiveBuffer = new Byte[30];

        bool processing = false;

        public class BannerData
        {
            public float Temperature = 0;
            public class Axis
            {
                public float RMSVelocidad = 0;
                public float PeakVelocity = 0;
                public float PeakFrequency = 0;
                public float RMSAcceleration = 0;
                public float PeakAcceleration = 0;
                public float CrestAcceleration = 0;
                public float Kurtosis = 0;
                public float HighFrequencyRMSAcceleration = 0;
            }
            public Axis Z = new Axis();
            public Axis X = new Axis();
        }

        public BannerData[] BannerDataArray = null;

        public void LoadCommModbus()
        {
            CommsArray = new Comms[4];
            CommsArray[0] = new Comms();
            CommsArray[1] = new Comms();
            CommsArray[2] = new Comms();
            CommsArray[3] = new Comms();
            CommsArray[0].ZAxisVariables = ZAxisVariables0;
            CommsArray[0].XAxisVariables = XAxisVariables0;
            CommsArray[0].Temperature = Temperature0;

            CommsArray[1].ZAxisVariables = ZAxisVariables1;
            CommsArray[1].XAxisVariables = XAxisVariables1;
            CommsArray[1].Temperature = Temperature1;

            CommsArray[2].ZAxisVariables = ZAxisVariables2;
            CommsArray[2].XAxisVariables = XAxisVariables2;
            CommsArray[2].Temperature = Temperature2;

            CommsArray[3].ZAxisVariables = ZAxisVariables2;
            CommsArray[3].XAxisVariables = XAxisVariables2;
            CommsArray[3].Temperature = Temperature2;
        }
        public class Comms
        {
            public Comms()
            {
            }
            public Byte[] ZAxisVariables;
            public Byte[] XAxisVariables;
            public Byte[] Temperature;
        }
        public Comms[] CommsArray = null;

        bool IsCommsLoaded = false;
        bool IsRunning = false;
        public bool OK = false;
        private async void Proccess(ThreadPoolTimer timer)
        {
            if (!IsRunning)
            {
                try
                {
                    IsRunning = true;
                    string aqs = SerialDevice.GetDeviceSelector();
                    //string aqs = SerialDevice.GetDeviceSelectorFromUsbVidPid(0x10C4, 0xEA60);                
                    var dis = await DeviceInformation.FindAllAsync(aqs);
                    if (dis.Count > 0)
                    {
                        string EasySyncId = "\\\\?\\FTDIBUS#VID_0403+PID_6001";     //"\\\\?\\FTDIBUS#VID_0403+PID_6015+DM26NI1NA#0000#{86e0d1e0-8089-11d0-9ce4-08003e301f73}";   //EasySync ES-U-3001-M
                        string ESU3001MID = "\\\\?\\FTDIBUS#VID_0403+PID_6015";
                        string Generic1 = "\\\\?\\USB#VID_10C4&PID_EA60#000";
                        foreach (DeviceInformation item in dis)
                        {
                            var s = item.Id.Substring(0, 29);
                            if (EasySyncId == s)
                            {
                                serialPort = await SerialDevice.FromIdAsync(item.Id);
                                break;
                            }
                            if (ESU3001MID == s)
                            {
                                serialPort = await SerialDevice.FromIdAsync(item.Id);
                                break;
                            }
                            if (Generic1 == s)
                            {
                                serialPort = await SerialDevice.FromIdAsync(item.Id);
                                break;
                            }
                        }
                        if (serialPort != null)
                        {
                            // Time waiting for before callback
                            serialPort.WriteTimeout = TimeSpan.FromMilliseconds(200);
                            serialPort.ReadTimeout = TimeSpan.FromMilliseconds(200);
                            serialPort.BaudRate = 19200;
                            serialPort.Parity = SerialParity.None;
                            serialPort.StopBits = SerialStopBitCount.One;
                            serialPort.DataBits = 8;
                            serialPort.Handshake = SerialHandshake.None;
                            SerialPortdataReader = new DataReader(serialPort.InputStream);
                            SerialPortdataReader.InputStreamOptions = InputStreamOptions.Partial;
                            SerialPortdataWriter = new DataWriter(serialPort.OutputStream);
                            ReadCancellationTokenSource = new CancellationTokenSource();
                            ReadCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            processing = true;
                            while (processing)
                            {
                                for (int i = 0; i < BannerDataArray.Length; i++)
                                {
                                    if (await WriteAsync(CommsArray[i].Temperature))
                                    {
                                        if (await ReadAsync(ReceiveBuffer))
                                        {
                                            BannerDataArray[i].Temperature = ((float)(ReceiveBuffer[0]) * 256 + (float)(ReceiveBuffer[1])) / 20;
                                            OK = true;
                                        }
                                        else OK = false;
                                    }
                                    else OK = false;
                                    await Task.Delay(100);
                                    if (await WriteAsync(CommsArray[i].ZAxisVariables))
                                    {
                                        if (await ReadAsync(ReceiveBuffer))
                                        {
                                            BannerDataArray[i].Z.RMSVelocidad = ((float)(ReceiveBuffer[4]) * 256 + (float)(ReceiveBuffer[5])) / 1000;
                                            BannerDataArray[i].Z.PeakVelocity = ((float)(ReceiveBuffer[6]) * 256 + (float)(ReceiveBuffer[7])) / 1000;
                                            BannerDataArray[i].Z.PeakFrequency = ((float)(ReceiveBuffer[8]) * 256 + (float)(ReceiveBuffer[9])) / 10;
                                            BannerDataArray[i].Z.RMSAcceleration = ((float)(ReceiveBuffer[10]) * 256 + (float)(ReceiveBuffer[11])) / 1000;
                                            BannerDataArray[i].Z.PeakAcceleration = ((float)(ReceiveBuffer[12]) * 256 + (float)(ReceiveBuffer[13])) / 1000;
                                            BannerDataArray[i].Z.CrestAcceleration = ((float)(ReceiveBuffer[14]) * 256 + (float)(ReceiveBuffer[15])) / 1000;
                                            BannerDataArray[i].Z.Kurtosis = ((float)(ReceiveBuffer[16]) * 256 + (float)(ReceiveBuffer[17])) / 1000;
                                            BannerDataArray[i].Z.HighFrequencyRMSAcceleration = ((float)(ReceiveBuffer[18]) * 256 + (float)(ReceiveBuffer[19])) / 1000;
                                            OK = true;
                                        }
                                        else OK = false;
                                    }
                                    else OK = false;
                                    await Task.Delay(100);
                                    if (await WriteAsync(CommsArray[i].XAxisVariables))
                                    {
                                        if (await ReadAsync(ReceiveBuffer))
                                        {
                                            BannerDataArray[i].X.RMSVelocidad = ((float)(ReceiveBuffer[4]) * 256 + (float)(ReceiveBuffer[5])) / 1000;
                                            BannerDataArray[i].X.PeakVelocity = ((float)(ReceiveBuffer[6]) * 256 + (float)(ReceiveBuffer[7])) / 1000;
                                            BannerDataArray[i].X.PeakFrequency = ((float)(ReceiveBuffer[8]) * 256 + (float)(ReceiveBuffer[9])) / 10;
                                            BannerDataArray[i].X.RMSAcceleration = ((float)(ReceiveBuffer[10]) * 256 + (float)(ReceiveBuffer[11])) / 1000;
                                            BannerDataArray[i].X.PeakAcceleration = ((float)(ReceiveBuffer[12]) * 256 + (float)(ReceiveBuffer[13])) / 1000;
                                            BannerDataArray[i].X.CrestAcceleration = ((float)(ReceiveBuffer[14]) * 256 + (float)(ReceiveBuffer[15])) / 1000;
                                            BannerDataArray[i].X.Kurtosis = ((float)(ReceiveBuffer[16]) * 256 + (float)(ReceiveBuffer[17])) / 1000;
                                            BannerDataArray[i].X.HighFrequencyRMSAcceleration = ((float)(ReceiveBuffer[18]) * 256 + (float)(ReceiveBuffer[19])) / 1000;
                                            OK = true;
                                        }
                                        else OK = false;
                                    }
                                    else OK = false;
                                    await Task.Delay(100);
                                }
                                await Task.Delay(100);
                            }
                        }
                        else
                        {
                            processing = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    processing = false;
                }
                finally
                {
                    OK = false;
                    IsRunning = false;
                    ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
                }
            }
        }

        private async Task<bool> WriteAsync(Byte[] comm)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(2000));
            Task<UInt32> storeAsyncTask;
            SerialPortdataWriter.WriteBytes(comm);
            storeAsyncTask = SerialPortdataWriter.StoreAsync().AsTask(cancellationTokenSource.Token);
            UInt32 bytesWritten = await storeAsyncTask;
            if (bytesWritten > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> ReadAsync(Byte[] data)
        {
            bool getback = false;
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));
                Task<UInt32> loadAsyncTask;
                loadAsyncTask = SerialPortdataReader.LoadAsync((uint)data.Length).AsTask(cancellationTokenSource.Token);
                UInt32 bytesRead = await loadAsyncTask;
                Byte[] input = new Byte[bytesRead];
                if (bytesRead > 0)
                {
                    SerialPortdataReader.ReadBytes(input);
                    if (checkAnswer(input, bytesRead))
                    {
                        for (int i = 0; i < (bytesRead - 5); i++)
                        {
                            data[i] = input[i + 3];
                        }
                        getback = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return getback;
        }

        private static bool checkAnswer(Byte[] respuesta, UInt32 size_answer)
        {
            Int32[] int_array = new Int32[100];    //para CRC, se toma el buffer más largo - 2bytes
            for (int i = 0; i < (size_answer - 2); i++)
            {
                int_array[i] = respuesta[i];
            }
            Byte int_crc_byte_a = 0;
            Byte int_crc_byte_b = 0;
            Int32 int_crc = 0xFFFF;
            Int32 int_lsb = 0;
            for (Int16 int_i = 0; int_i < (size_answer - 2); int_i++)
            {
                int_crc = int_crc ^ int_array[int_i];
                for (Int16 int_j = 0; int_j < 8; int_j++)
                {
                    int_lsb = int_crc & 0x0001; // Mask of LSB
                    int_crc = int_crc >> 1;
                    int_crc = int_crc & 0x7FFF;
                    if (int_lsb == 1) int_crc = int_crc ^ 0xA001;
                }
            }
            int_crc_byte_a = (Byte)(int_crc & 0x00FF);
            int_crc_byte_b = (Byte)((int_crc >> 8) & 0x00FF);
            //Todo el buffer está bien
            if ((int_crc_byte_a == respuesta[(size_answer - 2)]) && (int_crc_byte_b == respuesta[(size_answer - 1)]))
            {
                return true;    //(UInt16)(((int_crc_byte_a << 8) & 0xFF00) | (int_crc_byte_b & 0x00FF));
            }
            else return false;
        }
    }
}
