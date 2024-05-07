using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
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
using Windows.Networking.Connectivity;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using static PredictorV2.Common.MainContainer;

namespace PredictorV2.Common
{
    public class DAQ
    {
        /************************ Estaticas /************************/
        public static string Generic = "\\\\?\\USB#VID_04D8"; //16
        private static List<string> CardNamesList = new List<string>();
        public static string ManufacturerName2 = "\\\\?\\USB#VID_04D8&PID_000A#6&9FC4A4B&0&1#{86e0d1e0-8089-11d0-9ce4-08003e301f73}";
        public static string ManufacturerName1 = "\\\\?\\USB#VID_04D8&PID_000A#6&9FC4A4B&0&2#{86e0d1e0-8089-11d0-9ce4-08003e301f73}";
        public static string ManufacturerName3 = "\\\\?\\USB#VID_04D8&PID_000A#6&9FC4A4B&0&3#{86e0d1e0-8089-11d0-9ce4-08003e301f73}";
        public static string ManufacturerName4 = "\\\\?\\USB#VID_04D8&PID_000A#6&9FC4A4B&0&4#{86e0d1e0-8089-11d0-9ce4-08003e301f73}";
        private static bool IsIdentifiedCard1 = true;
        private static bool IsIdentifiedCard2 = true;
        private static bool IsIdentifiedCard3 = true;
        private static bool IsIdentifiedCard4 = true;

        /***********************      Forja     *********************************************/
        ADCRange[] ADCCHInpRangeFR0 = { ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10 };
        ADCCHSam[] ADCCHSamplingFR0 = { ADCCHSam.CH0, ADCCHSam.CH1, ADCCHSam.CH2, ADCCHSam.CH3, ADCCHSam.CH4, ADCCHSam.CH5, ADCCHSam.CH6, ADCCHSam.CH7 };

        ADCRange[] ADCCHInpRangeFR1 = { ADCRange.B10, ADCRange.B10, ADCRange.B10, ADCRange.B10 };
        ADCCHSam[] ADCCHSamplingFR1 = { ADCCHSam.CH7, ADCCHSam.CH0, ADCCHSam.CH7, ADCCHSam.CH1 };

        public static DAQ DAQ1 = null;
        public static DAQ DAQ2 = null;
        public static DAQ DAQ3 = null;
        public static DAQ DAQ4 = null;

        public static void NewHandler()
        {
            //GetManufactureNames();
            DAQ1 = new DAQ(1, ManufacturerName1, samplingWindows.FullContinues200ms, Coupling.AC, FFTDCHarmonic.Stay, 10,5000);
            //DAQ1 = new DAQ(2, Generic, samplingWindows.FullContinues1000ms);
        }

        public string Name = "";
        public bool OK = false;
        public int CardNumber = 0;

        public enum Coupling
        {
            AC,
            DC
        }

        public enum FFTDCHarmonic
        {
            Remove,
            Stay
        }

        public DAQ(int NumberCard, string ManufacturerName, samplingWindows aSamplingWindows, Coupling aCoupling, FFTDCHarmonic aFFTDCRemove, int aFFTLeftBand, int aFFTRightBand)
        {
            CardNumber = NumberCard;
            IsCardEnabled = true;
            Name = ManufacturerName;
            SamplingWindows = aSamplingWindows;
            if (aCoupling == Coupling.AC)
            {
                ACCoupling = true;
            }
            else 
            {
                ACCoupling = false;
            }
            if (aFFTDCRemove == FFTDCHarmonic.Remove)
            {
                FFTDCRemove = true;
            }
            else
            {
                FFTDCRemove = false;
            }
            FFTLeftBand = aFFTLeftBand;
            FFTRightBand = aFFTRightBand;
            CreateBuffers();
            ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
        }

        public static class Gain
        {
            static public Double Bipolar_5V = 0.002441;
            static public Double Bipolar_10V = 0.004882;
            static public Double Single_5V = 0.001220;
            static public Double Single_10V = 0.002441;
        }
        public static double GainResistor = 1.098039215686; // Inv(204kohms/224khoms)

        public const int RawInputBufferTransportLenght200ms = 16632;
        public const int RawInputBufferLenght200ms = 11088; /*11088 *1.5*/
        public const UInt16 NumberOfPackages200ms = 33;    //165/5

        public const int RawInputBufferTransportLenght1000ms = 83160;
        public const int RawInputBufferLenght1000ms = 55440; /*55440 *1.5*/
        public const UInt16 NumberOfPackages1000ms = 165;    //98532/252

        public const int ReceivePackageBufferLenght = 504;
        public const int Blocks12RawReceivePackage = 42;
        /***************************************/
        public const double SendTimeout = 400;
        /***************************************/
        public const double ReceiveTimeout = 1400; //No puede ser menor a 1seg
        public const Byte CommandON_High = (Byte)(0xF0);
        public const Byte CommandON_Low = (Byte)(0x33);
        public const Byte CommandOFF_High = (Byte)(0xAA);
        public const Byte CommandOFF_Low = (Byte)(0xCC);
        /************************ Fin Estaticas ************************/

        enum ADCRange : int { S05 = 0, S10 = 1, B05 = 2, B10 = 3 }
        enum ADCCHSam : int { CH0 = 0, CH1 = 1, CH2 = 2, CH3 = 3, CH4 = 4, CH5 = 5, CH6 = 6, CH7 = 7 }
        //Deben ser del mismo size los dos. Número de segmentos repartidos en 1 seg con aprox 55mil muestras

        protected ThreadPoolTimer ActionTimer = null;
        private bool ACCoupling = false;
        private bool FFTDCRemove = false;
        private int FFTLeftBand = 0;
        private int FFTRightBand = 0;
        public enum samplingWindows
        {
            FullContinues1000ms,
            FullContinues200ms
        }
        private samplingWindows SamplingWindows;

        public static async void GetManufactureNames()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            //Not all USB Port is recognized. Port 1 is tested in UP Board
            if (dis.Count > 0)
            {
                foreach (DeviceInformation item in dis)
                {
                    var acqName = item.Id.Substring(0, 16);
                    if (Generic == acqName)
                    {
                        CardNamesList.Add(item.Id);
                    }
                }
                if (CardNamesList.Count == 1)
                {
                    ManufacturerName1 = CardNamesList[0];
                    IsIdentifiedCard1 = true;
                }
                else if (CardNamesList.Count == 2)
                {
                    for (int i = 0; i < CardNamesList[0].Length; i++)
                    {
                        string c0 = CardNamesList[0].Substring(i, 1);
                        string c1 = CardNamesList[1].Substring(i, 1);
                        if (!c0.Equals(c1))
                        {
                            if (c0.CompareTo(c1) == 1) //c0 es más grande
                            {
                                ManufacturerName2 = CardNamesList[1];
                                ManufacturerName1 = CardNamesList[0];
                            }
                            else
                            {
                                ManufacturerName2 = CardNamesList[0];
                                ManufacturerName1 = CardNamesList[1];
                            }
                            IsIdentifiedCard2 = true;
                            IsIdentifiedCard1 = true;
                        }
                    }
                }
                else if (CardNamesList.Count == 4)
                {
                    for (int i = 0; i < CardNamesList[0].Length; i++)
                    {
                        string c0 = CardNamesList[0].Substring(i, 1);
                        string c1 = CardNamesList[1].Substring(i, 1);
                        if (!c0.Equals(c1))
                        {
                            if (c0.CompareTo(c1) == 1) //c0 es más grande
                            {
                                ManufacturerName2 = CardNamesList[1];
                                ManufacturerName1 = CardNamesList[0];
                            }
                            else
                            {
                                ManufacturerName2 = CardNamesList[0];
                                ManufacturerName1 = CardNamesList[1];
                            }
                            IsIdentifiedCard2 = true;
                            IsIdentifiedCard1 = true;
                        }
                    }
                }
            }
        }

        public class SamplingPack
        {
            public bool ACCopling = false;
            public bool FFTDCRemove = false;
            public int FFTLeftBand = 2000;
            public int FFTRightBand = 6000;
            public int[] range = null;
            public int[] Sampling = null;
            public Double[][] ChannelBuffers = null;
            public double[] ActivePowerCHToCH = new double[4];
            public double[] TimeInputRMS = new double[8];
            public double[] TimeInputAVG = new double[8];
            public double[] TimeInputMax = new double[8];
            public double[] FFTDominantFreq = new double[8];
            public double[] FFTDominantMag = new double[8];
        }
        public SamplingPack [] SamplingPackArray = null;

        public double FreqBuffSizeFactor = 0;
        public byte[] InputConfig = new byte[8];
        public double[] IndividualGain = new double[8];
        public int ActivatedChannels = 0;

        public DataWriter SerialPortdataWriter = null;
        public DataReader SerialPortdataReader = null;
        public Byte[] SendBuffer = new Byte[50];
        public int Channels = 0;
        public int RawInputBufferCounter = 0;
        public Byte[] ReceivePackageBuffer = new Byte[ReceivePackageBufferLenght];
        public UInt16[] RawInputBuffer = new UInt16[RawInputBufferLenght1000ms];
        public Double[][] ChannelBuffers = null;
        public bool IsUSBConnected = false;

        public double[] ActivePowerCHToCH = new double[4];
        public double[] TimeInputRMS = new double[8];
        public double[] TimeInputAVG = new double[8];
        public double[] TimeInputMax = new double[8];
        public double[] FFTDominantFreq = new double[8];
        public double[] FFTDominantMag = new double[8];
        public Double[][] FFTInputArrays = null;
        public Double[][] PowInputArrays = null;
        public double[] FFTInputRMS = new double[8];
        public Double[][] TimeIntegralArrays = null;
        public double[] TimeIntegralRMS = new double[8];
        public Double[][] FFTIntegralArrays = null;
        public double[] FFTIntegralRMS = new double[8];
        public int SamplesToFFTProccess = 0;
        private Integral IntegralAccel = new Integral();

        public double[] inputRaw1;
        public double[] inputRaw2;
        public double[] Imaginary1;
        public double[] Imaginary2;

        public bool IsCardEnabled = false;
        public double NoiseUmbral = 0;

        public bool IsRunning = false;
        public int consecutiveFails = 0;
        public int consecutiveFailsLimit = 5;
        public int instanceCounter = 0;
        public SerialDevice SerialPort = null;

        public int RawInputBufferLenght = 0;
        public int NumberOfPackages = 0;
        public int RawInputBufferTransportLenght = 0;

        public bool CreateBuffers()
        {
            bool anw = false;
            try
            {
                SamplingPackArray = new SamplingPack[1];
                SamplingPackArray[0] = new SamplingPack();
                for (int j = 0; j < SamplingPackArray.Length; j++)
                {
                    SamplingPackArray[j].ACCopling = false;
                    SamplingPackArray[j].FFTDCRemove = false;
                    SamplingPackArray[j].FFTLeftBand = 2000;
                    SamplingPackArray[j].FFTRightBand = 6000;

                    SamplingPackArray[j].range = new int[ADCCHInpRangeFR0.Length];
                    for (int i = 0; i < ADCCHInpRangeFR0.Length; i++)
                    {
                        SamplingPackArray[j].range[i] = (int)(ADCCHInpRangeFR0[i]);
                    }
                    SamplingPackArray[j].Sampling = new int[ADCCHSamplingFR0.Length];
                    for (int i = 0; i < ADCCHSamplingFR0.Length; i++)
                    {
                        SamplingPackArray[j].Sampling[i] = (int)(ADCCHSamplingFR0[i]);
                    }
                    ConfigChannels(SamplingPackArray[j].range, SamplingPackArray[j].Sampling);
                    SamplingPackArray[j].ChannelBuffers = new double[8][];
                    for (int i = 0; i < 8; i++)
                    {
                        SamplingPackArray[j].ChannelBuffers[i] = new Double[1];
                    }
                    ActivatedChannels = SamplingPackArray[j].Sampling.Length;
                    for (int i = 0; i < ActivatedChannels; i++)
                    {
                        SamplingPackArray[j].ChannelBuffers[i] = new Double[RawInputBuffer.Length / ActivatedChannels];
                    }
                }
                /******************************* Sección de registros auxiliares *******************************/

                ChannelBuffers = new double[8][];
                PowInputArrays = new double[4][];
                //Esto asegura que siempre devuelve los 8 arrays, los que no están disponibles será de 1 registro
                for (int i = 0; i < 8; i++)
                {
                    ChannelBuffers[i] = new Double[1];
                }
                for (int i = 0; i < ActivatedChannels; i++)
                {
                    ChannelBuffers[i] = new Double[RawInputBuffer.Length / ActivatedChannels];
                }
                for (int i = 0; i < 4; i++)
                {
                    PowInputArrays[i] = new Double[1];
                }
                for (int i = 0; i < (ActivatedChannels / 2); i++)
                {
                    PowInputArrays[i] = new Double[RawInputBuffer.Length / ActivatedChannels];
                }
                if (SamplingWindows == samplingWindows.FullContinues1000ms)
                {
                    RawInputBufferLenght = RawInputBufferLenght1000ms;
                    RawInputBufferTransportLenght = RawInputBufferTransportLenght1000ms;
                    NumberOfPackages = NumberOfPackages1000ms;
                    if (ChannelBuffers[0].Length > 16384)       /*27720*/
                    {
                        SamplesToFFTProccess = 16384;
                    }
                    else if (ChannelBuffers[0].Length > 8192)   /*13,860*/
                    {
                        SamplesToFFTProccess = 8192;
                    }
                    else if (ChannelBuffers[0].Length > 4096)   /*6,930*/
                    {
                        SamplesToFFTProccess = 4096;
                    }
                }
                else
                {
                    RawInputBufferTransportLenght = RawInputBufferTransportLenght200ms;
                    RawInputBufferLenght = RawInputBufferLenght200ms;
                    NumberOfPackages = NumberOfPackages200ms;

                    if (ChannelBuffers[0].Length > 16384)       /*27720/5 = 5,544*/
                    {
                        SamplesToFFTProccess = 4096;
                    }
                    else if (ChannelBuffers[0].Length > 8192)   /*13,860/5 =2,772*/
                    {
                        SamplesToFFTProccess = 2048;
                    }
                    else if (ChannelBuffers[0].Length > 4096)   /*6,930/5 =1,386*/
                    {
                        SamplesToFFTProccess = 1024;
                    }
                }
                FreqBuffSizeFactor = (double)(RawInputBuffer.Length) / ((double)(SamplesToFFTProccess) * (double)(ActivatedChannels));

                FFTInputArrays = new double[8][];
                for (int i = 0; i < 8; i++)
                {
                    FFTInputArrays[i] = new Double[1];
                }
                for (int i = 0; i < ActivatedChannels; i++)
                {
                    FFTInputArrays[i] = new Double[SamplesToFFTProccess];
                }
                TimeIntegralArrays = new double[8][];
                for (int i = 0; i < 8; i++)
                {
                    TimeIntegralArrays[i] = new Double[1];
                }
                for (int i = 0; i < ActivatedChannels; i++)
                {
                    TimeIntegralArrays[i] = new Double[SamplesToFFTProccess];
                }
                FFTIntegralArrays = new double[8][];
                for (int i = 0; i < 8; i++)
                {
                    FFTIntegralArrays[i] = new Double[1];
                }
                for (int i = 0; i < ActivatedChannels; i++)
                {
                    FFTIntegralArrays[i] = new Double[SamplesToFFTProccess];
                }
                inputRaw1 = new double[SamplesToFFTProccess];
                inputRaw2 = new double[SamplesToFFTProccess];
                Imaginary1 = new double[SamplesToFFTProccess];
                Imaginary2 = new double[SamplesToFFTProccess];
                anw = true;
            }
            catch (Exception ex)
            { 
            }
            return anw;
        }

        private bool IsClosed = false;
        public void Close()
        {
            IsRunning = false;
            IsClosed = true;
            if (SerialPort != null)
            {
                SerialPort.Dispose();
                SerialPort = null;

            }
        }


        public async void Proccess(ThreadPoolTimer timer)
        {
            if (!IsRunning)
            {
                try
                {
                    IsRunning = true;
                    OK = false;
                    //if (SerialPort != null)
                    //{
                    //    SerialPort.Dispose();
                    //}
                    string aqs = SerialDevice.GetDeviceSelector();
                    var dis = await DeviceInformation.FindAllAsync(aqs);
                    //Not all USB Port is recognized. Port 1 is tested in UP Board
                    if (dis.Count > 0)
                    {
                        foreach (DeviceInformation item in dis)
                        {
                            //Este codigo no funcionaba porque estaba leyendo un iD estatico que hay que validar que se pueda leer dinámico
                            //    var acqName = item.Id;
                            //    if (Name == acqName)
                            //    {
                            //        SerialPort = await SerialDevice.FromIdAsync(item.Id);
                            //        break;
                            //    }
                            var acqName = item.Id.Substring(0, 16);
                            if (Generic == acqName)
                            {
                                SerialPort = await SerialDevice.FromIdAsync(item.Id);
                                break;
                            }
                        }
                        if (SerialPort != null)
                        {
                            SerialPort.WriteTimeout = TimeSpan.FromMilliseconds(200);
                            SerialPort.ReadTimeout = TimeSpan.FromMilliseconds(1200);
                            //TurboT support at least 4Mb, UP support at least 2Mb because of there is non USB2.0
                            SerialPort.BaudRate = 4000000;
                            SerialPort.Parity = SerialParity.None;
                            SerialPort.StopBits = SerialStopBitCount.One;
                            SerialPort.DataBits = 8;
                            SerialPort.Handshake = SerialHandshake.None;
                            SerialPortdataReader = new DataReader(SerialPort.InputStream);
                            SerialPortdataReader.InputStreamOptions = InputStreamOptions.Partial;
                            SerialPortdataWriter = new DataWriter(SerialPort.OutputStream);
                            IsUSBConnected = true;
                            while (IsUSBConnected)
                            {
                                DateTime Inicio = DateTime.Now;
                                for (int k = 0; k < SamplingPackArray.Length; k++)
                                {
                                    FFTLeftBand = (int)(SamplingPackArray[k].FFTLeftBand / FreqBuffSizeFactor);
                                    FFTRightBand = (int)(SamplingPackArray[k].FFTRightBand / FreqBuffSizeFactor);
                                    RawInputBufferCounter = 0;
                                    for (UInt16 i = 0; i < NumberOfPackages; i++)
                                    {
                                        SendBuffer[0] = 0x00;   //SPI Source
                                        SendBuffer[1] = 0x00;   //It is not used
                                        SendBuffer[2] = (Byte)(i >> 8);   //Number of package
                                        SendBuffer[3] = (Byte)(i);

                                        SendBuffer[12] = InputConfig[0];
                                        SendBuffer[13] = InputConfig[1];
                                        SendBuffer[14] = InputConfig[2];
                                        SendBuffer[15] = InputConfig[3];
                                        SendBuffer[16] = InputConfig[4];
                                        SendBuffer[17] = InputConfig[5];
                                        SendBuffer[18] = InputConfig[6];
                                        SendBuffer[19] = InputConfig[7];

                                        SendBuffer[20] = 0x00; /*Siempre va a ser cero*/
                                        SendBuffer[21] = (Byte)(RawInputBufferTransportLenght >> 16);   //Package total
                                        SendBuffer[22] = (Byte)(RawInputBufferTransportLenght >> 8);   //Package total
                                        ushort temp = (ushort)RawInputBufferTransportLenght;
                                        SendBuffer[23] = (Byte)(temp);
                                        SendBuffer[24] = (Byte)NumberOfPackages;
                                        SendBuffer[25] = 0xAC; //Significa que si se quiere sobre escribir la adquisicion

                                        if (await WriteAsync(SendBuffer))
                                        {
                                            if (await ReadAsync(ReceivePackageBuffer))
                                            {
                                                for (UInt32 j = 0; j < Blocks12RawReceivePackage; j++)
                                                {
                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[0 + j * 12]) & 0x0F) << 8 | (int)(ReceivePackageBuffer[1 + j * 12]));
                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[0 + j * 12]) & 0xF0) << 4 | (int)(ReceivePackageBuffer[2 + j * 12]));

                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[3 + j * 12]) & 0x0F) << 8 | (int)(ReceivePackageBuffer[4 + j * 12]));
                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[3 + j * 12]) & 0xF0) << 4 | (int)(ReceivePackageBuffer[5 + j * 12]));

                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[6 + j * 12]) & 0x0F) << 8 | (int)(ReceivePackageBuffer[7 + j * 12]));
                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[6 + j * 12]) & 0xF0) << 4 | (int)(ReceivePackageBuffer[8 + j * 12]));

                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[9 + j * 12]) & 0x0F) << 8 | (int)(ReceivePackageBuffer[10 + j * 12]));
                                                    RawInputBuffer[RawInputBufferCounter++] = (UInt16)(((int)(ReceivePackageBuffer[9 + j * 12]) & 0xF0) << 4 | (int)(ReceivePackageBuffer[11 + j * 12]));
                                                }
                                            }
                                            else
                                            {   //Reading error
                                                break;
                                            }
                                        }
                                        else
                                        {   //Writing error
                                            break;
                                        }
                                    }
                                    if (RawInputBufferCounter == RawInputBufferLenght)
                                    {
                                        consecutiveFails = 0;
                                        LoadAndCopyInputSamples(k);
                                        /************************ End Write DO ********************************/
                                        OK = true;
                                        GetRMSValues(k);
                                        GetMaxValues(k);
                                        ProcessInputVariables(k);
                                    }
                                    else
                                    {
                                        if (++consecutiveFails > consecutiveFailsLimit)
                                        {
                                            consecutiveFails = 0;
                                            IsUSBConnected = false;
                                        }
                                    }
                                }
                                await Task.Delay(50);
                                DateTime final = DateTime.Now;
                                Devices.SamplingTime = (double)(final.Ticks - Inicio.Ticks) * 0.0000001d;
                            }
                        }
                        else
                        {   //Port error
                            IsUSBConnected = false;
                            MainContainer.Devices.LOG = "SerialPort null";
                        }
                    }
                    else
                    {   // None port recognized
                        IsUSBConnected = false;
                    }
                }
                catch (Exception ex)
                {
                    MainContainer.Devices.LOG = ex.Message;
                }
                finally
                {
                    OK = false;
                    IsRunning = false;
                    if (!IsClosed)
                    {
                        ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(10)); //No disminuir, los dos tarjetas deben inicializarse antes de entrar al process
                    }
                    else
                    {
                        if (SerialPort != null)
                        {
                            SerialPort.Dispose();
                            SerialPort = null;
                        }
                    }
                }
            }
        }

        async Task<bool> WriteAsync(Byte[] comm)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(SendTimeout));
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
            catch (Exception ex)
            {
            }
            return false;
        }

        async Task<bool> ReadAsync(Byte[] data)
        {
            bool getback = false;
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(ReceiveTimeout));
                Task<UInt32> loadAsyncTask;
                loadAsyncTask = SerialPortdataReader.LoadAsync((uint)data.Length).AsTask(cancellationTokenSource.Token);
                UInt32 bytesRead = await loadAsyncTask;
                Byte[] input = new Byte[bytesRead]; //avoid to read out of range
                if (bytesRead > 0)
                {
                    SerialPortdataReader.ReadBytes(input);
                    for (int i = 0; i < bytesRead; i++)
                    {
                        data[i] = input[i];
                    }
                    getback = true;
                }
            }
            catch (Exception ex)
            {
            }
            return getback;
        }

        void ConfigChannels(int[] input, int[] CHSampling)
        {
            int GainConfig = 0;
            double SingleFFTnoise = 0.001;
            while (GainConfig < InputConfig.Length)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        case 0:
                            InputConfig[GainConfig] = 0x01;
                            IndividualGain[GainConfig] = Gain.Single_5V * GainResistor;
                            NoiseUmbral = SingleFFTnoise * 0.5;
                            break;
                        case 1:
                            InputConfig[GainConfig] = 0x09;
                            IndividualGain[GainConfig] = Gain.Single_10V * GainResistor;
                            NoiseUmbral = SingleFFTnoise * 1;
                            break;
                        case 2:
                            InputConfig[GainConfig] = 0x05;
                            IndividualGain[GainConfig] = Gain.Bipolar_5V * GainResistor;
                            NoiseUmbral = SingleFFTnoise * 1;
                            break;
                        case 3:
                            InputConfig[GainConfig] = 0x0D;
                            IndividualGain[GainConfig] = Gain.Bipolar_10V * GainResistor;
                            NoiseUmbral = SingleFFTnoise * 2;
                            break;
                    }
                    GainConfig++;
                }
            }
            int SamplingConfig = 0;
            while (SamplingConfig < InputConfig.Length)
            {
                for (int i = 0; i < CHSampling.Length; i++)
                {
                    switch (CHSampling[i])
                    {
                        case 0:
                            InputConfig[SamplingConfig] |= 0x80;
                            break;
                        case 1:
                            InputConfig[SamplingConfig] |= 0x90;
                            break;
                        case 2:
                            InputConfig[SamplingConfig] |= 0xA0;
                            break;
                        case 3:
                            InputConfig[SamplingConfig] |= 0xB0;
                            break;
                        case 4:
                            InputConfig[SamplingConfig] |= 0xC0;
                            break;
                        case 5:
                            InputConfig[SamplingConfig] |= 0xD0;
                            break;
                        case 6:
                            InputConfig[SamplingConfig] |= 0xE0;
                            break;
                        case 7:
                            InputConfig[SamplingConfig] |= 0xF0;
                            break;
                    }
                    SamplingConfig++;
                }
            }
        }
        void LoadAndCopyInputSamples(int samplingIndex)
        {
            int Buffsize = ChannelBuffers[0].Length;
            int ChSize = ChannelBuffers.Length;
            for (int i = 0; i < Buffsize; i++)
            {
                for (int j = 0; j < ActivatedChannels; j++)
                {
                    var index = j + i * ActivatedChannels;
                    ushort CurrData = RawInputBuffer[index];
                    if ((int)(CurrData & 0x0800) == 0)
                    {//Positive
                        ChannelBuffers[j][i] = IndividualGain[j] * (double)(CurrData);
                    }
                    else
                    {
                        ChannelBuffers[j][i] = IndividualGain[j] * ((double)(CurrData) - 4096);
                    }
                    SamplingPackArray[samplingIndex].ChannelBuffers[j][i] = ChannelBuffers[j][i];
                }
            }
        }
        void ProcessInputVariables(int samplingIndex)
        {
            try
            {
                /******************************* Integral ******************************************/
                for (int k = 0; k < ActivatedChannels; k++)
                {
                    double rms = 0;
                    for (int i = 0; i < SamplesToFFTProccess; i++)
                    {
                        inputRaw1[i] = ChannelBuffers[k][i]; //Load first part of the buffer
                        Imaginary1[i] = 0;
                        Imaginary2[i] = 0;
                    }
                    SignalProcessing.FFTComplex(SignalProcessing.FFTDirection.ForwardTransform, inputRaw1, Imaginary1, SamplesToFFTProccess);
                    if (FFTDCRemove)
                    {
                        inputRaw1[0] = 0;
                        inputRaw1[SamplesToFFTProccess - 1] = 0;
                        Imaginary1[0] = 0;
                        Imaginary1[SamplesToFFTProccess - 1] = 0;
                    }
                    rms = 0;
                    if (FFTRightBand > SamplesToFFTProccess) FFTRightBand = SamplesToFFTProccess;
                    for (int i = 0; i < FFTLeftBand; i++)
                    {
                        FFTInputArrays[k][i] = Math.Sqrt(inputRaw1[i] * inputRaw1[i] + Imaginary1[i] * Imaginary1[i]);
                    }
                    for (int i = FFTLeftBand; i < FFTRightBand; i++)
                    {
                        FFTInputArrays[k][i] = Math.Sqrt(inputRaw1[i] * inputRaw1[i] + Imaginary1[i] * Imaginary1[i]);
                        rms += FFTInputArrays[k][i] * FFTInputArrays[k][i];
                    }
                    for (int i = FFTRightBand; i < SamplesToFFTProccess; i++)
                    {
                        FFTInputArrays[k][i] = Math.Sqrt(inputRaw1[i] * inputRaw1[i] + Imaginary1[i] * Imaginary1[i]);
                    }
                    FFTInputRMS[k] = Math.Sqrt(rms / SamplesToFFTProccess);
                    var midd = SamplesToFFTProccess / 4;
                    double[] MaxArray = new double[midd];
                    for (int i = 0; i < midd; i++)
                    {
                        MaxArray[i] = FFTInputArrays[k][i];
                    }
                    var tempFFT0 = MaxArray.ToList();
                    var mx = tempFFT0.Max();
                    FFTDominantMag[k] = mx;
                    SamplingPackArray[samplingIndex].FFTDominantMag[k] = FFTDominantMag[k];
                    if (mx > NoiseUmbral)
                    {
                        double Max = tempFFT0.IndexOf(mx);
                        FFTDominantFreq[k] = Max * FreqBuffSizeFactor;
                    }
                    else
                    {
                        FFTDominantFreq[k] = 0;
                    }
                    SamplingPackArray[samplingIndex].FFTDominantFreq[k] = FFTDominantFreq[k];
                }
            }
            catch (Exception Ex)
            {

            }
        }
        void GetMaxValues(int samplingIndex)
        {
            double[] MaxValues = new double[8];
            int size = ChannelBuffers[0].Length;
            if (SamplingWindows == samplingWindows.FullContinues200ms)
            {
                size = ChannelBuffers[0].Length / 5;
            }
            //TODO. Ver porque un desbordamiento aqui hace que ya nunca entre
            for (int j = 0; j < ActivatedChannels; j++)
            {
                MaxValues[j] = -11;
                for (int i = 0; i < size; i++)
                {
                    if (ChannelBuffers[j][i] > MaxValues[j]) MaxValues[j] = ChannelBuffers[j][i];
                }
                TimeInputMax[j] = MaxValues[j];
                SamplingPackArray[samplingIndex].TimeInputMax[j] = TimeInputMax[j];
            }
        }
        void GetRMSValues(int samplingIndex)
        {
            double[] Pwr = new double[4];
            double[] RmsArray = new double[8];
            double[] AVGArray = new double[8];
            int size = ChannelBuffers[0].Length;
            if (SamplingWindows == samplingWindows.FullContinues200ms)
            {
                size = ChannelBuffers[0].Length / 5;
            }
            //TODO. Ver porque un desbordamiento aqui hace que ya nunca entre
            for (int k = 0; k < (ActivatedChannels / 2); k++)
            {
                Pwr[k] = 0;
                for (int i = 0; i < size; i++)
                {
                    PowInputArrays[k][i] = ChannelBuffers[(k * 2)][i] * ChannelBuffers[(k * 2) + 1][i];
                    Pwr[k] += PowInputArrays[k][i];
                }
                ActivePowerCHToCH[k] = Pwr[k] / size;
                SamplingPackArray[samplingIndex].ActivePowerCHToCH[k] = ActivePowerCHToCH[k];
            }
            for (int j = 0; j < ActivatedChannels; j++)
            {
                AVGArray[j] = 0;
                for (int i = 0; i < size; i++)
                {
                    AVGArray[j] += ChannelBuffers[j][i];
                }
                TimeInputAVG[j] = AVGArray[j] / size;
                SamplingPackArray[samplingIndex].TimeInputAVG[j] = TimeInputAVG[j];
            }
            if (ACCoupling)
            {
                for (int j = 0; j < ActivatedChannels; j++)
                {
                    for (int i = 0; i < size; i++)
                    {
                        ChannelBuffers[j][i] = ChannelBuffers[j][i] - TimeInputAVG[j];
                        SamplingPackArray[samplingIndex].ChannelBuffers[j][i] = ChannelBuffers[j][i];
                    }
                }
            }
            for (int j = 0; j < ActivatedChannels; j++)
            {
                RmsArray[j] = 0;
                for (int i = 0; i < size; i++)
                {
                    RmsArray[j] += ChannelBuffers[j][i] * ChannelBuffers[j][i];
                }
                TimeInputRMS[j] = Math.Sqrt(RmsArray[j] / size);
                SamplingPackArray[samplingIndex].TimeInputRMS[j] = TimeInputRMS[j];
            }
        }
        /**********************************************************************************************************************************/
        /*******************************************************   FFT and Filter   *******************************************************/
        /**********************************************************************************************************************************/
        #region FFTandFilter
        //http://zone.ni.com/reference/en-XX/help/371361R-01/gmath/integral_xt/
        private class Integral
        {
            double j = 0;
            double jPlusOne = 0;
            double jMinusOne = 0;
            double y = 0;
            double dt = 1;
            double[] IntegralOutput;
            public double[] EvaluateIntegralOfArray(double[] input)
            {
                j = 0;
                jPlusOne = 0;
                jMinusOne = 0;
                y = 0;
                dt = 1;
                IntegralOutput = new double[input.Length];
                IntegralOutput[0] = 0;
                for (int i = 1; i < input.Length; i++)
                {
                    jMinusOne = j;
                    j = jPlusOne;
                    jPlusOne = input[i];
                    y += (dt / 6) * (jMinusOne + 4 * j + jPlusOne);
                    IntegralOutput[i] = y;
                }

                //It represents the next point
                //jMinusOne = j;
                //j = jPlusOne;
                //jPlusOne = 0;
                //y += (dt / 6) * (jMinusOne + 4 * j + jPlusOne);
                //IntegralOutput[input.Length] = y;
                return IntegralOutput;
            }
        }
        // see http://astronomy.swin.edu.au/~pbourke/analysis/dft/
        // real and imaginary are the real and imaginary arrays of 2^m points.
        private class Filter
        {
            //Samplerate is a data size, but it should windowed in a ONE second. Otherwise frequency is distorted
            public Filter(Type filterType, int sampleRate, double frequency, double FrameTimeInSeconds)
            {
                double FrequencyWindowed = frequency / FrameTimeInSeconds;
                double resonance = Math.Sqrt(2);  //Filter approach
                switch (filterType)
                {
                    case Type.ButterWorthLowPass:
                        c = 1.0d / Math.Tan(Math.PI * FrequencyWindowed / sampleRate);
                        a1 = 1.0d / (1.0d + resonance * c + c * c);
                        a2 = 2d * a1;
                        a3 = a1;
                        b1 = 2.0d * (1.0d - c * c) * a1;
                        b2 = (1.0d - resonance * c + c * c) * a1;
                        break;
                    case Type.ButterWorthHighPass:
                        c = Math.Tan(Math.PI * FrequencyWindowed / sampleRate);
                        a1 = 1.0d / (1.0d + resonance * c + c * c);
                        a2 = -2d * a1;
                        a3 = a1;
                        b1 = 2.0d * (c * c - 1.0f) * a1;
                        b2 = (1.0d - resonance * c + c * c) * a1;
                        break;
                }
            }
            private double c;
            private double a1;
            private double a2;
            private double a3;
            private double b1;
            private double b2;

            public enum Type
            {
                ButterWorthLowPass,
                ButterWorthHighPass
            }
            private double[] inputHistory = new double[2];
            private double[] outputHistory = new double[3];
            public double EvaluateFilterButterworth(double newInput)
            {
                double newOutput = a1 * newInput + a2 * inputHistory[0] + a3 * inputHistory[1] - b1 * outputHistory[0] - b2 * outputHistory[1];
                inputHistory[1] = inputHistory[0];
                inputHistory[0] = newInput;
                outputHistory[2] = outputHistory[1];
                outputHistory[1] = outputHistory[0];
                outputHistory[0] = newOutput;
                return newOutput;
            }
        }
        private class SignalProcessing
        {
            // see http://astronomy.swin.edu.au/~pbourke/analysis/dft/
            // real and imaginary are the real and imaginary arrays of 2^m points.
            public enum FFTDirection
            {
                ForwardTransform,
                ReverseTransform
            }

            public static void FFTComplex(FFTDirection dir, double[] rawreal, double[] rawimag, int lenght)
            {
                int i, i1, j, k, i2, l, l1, l2;
                double c1, c2, tx, ty, t1, t2, u1, u2, z;
                int n = lenght; //Power of 2;
                int bitspowerhelper = n; //Power of 2;
                int m = 0;
                while (bitspowerhelper > 1)
                {
                    m++;
                    bitspowerhelper = bitspowerhelper / 2;
                }
                // Do the bit reversal
                i2 = n >> 1;
                j = 0;
                for (i = 0; i < n - 1; i++)
                {
                    if (i < j)
                    {
                        tx = rawreal[i];
                        ty = rawimag[i];
                        rawreal[i] = rawreal[j];
                        rawimag[i] = rawimag[j];
                        rawreal[j] = tx;
                        rawimag[j] = ty;
                    }
                    k = i2;
                    while (k <= j)
                    {
                        j -= k;
                        k >>= 1;
                    }
                    j += k;
                }
                // Compute the FFT
                c1 = -1.0;
                c2 = 0.0;
                l2 = 1;
                for (l = 0; l < m; l++)
                {
                    l1 = l2;
                    l2 <<= 1;
                    u1 = 1.0;
                    u2 = 0.0;
                    for (j = 0; j < l1; j++)
                    {
                        for (i = j; i < n; i += l2)
                        {
                            i1 = i + l1;
                            t1 = u1 * rawreal[i1] - u2 * rawimag[i1];
                            t2 = u1 * rawimag[i1] + u2 * rawreal[i1];
                            rawreal[i1] = rawreal[i] - t1;
                            rawimag[i1] = rawimag[i] - t2;
                            rawreal[i] += t1;
                            rawimag[i] += t2;
                        }
                        z = u1 * c1 - u2 * c2;
                        u2 = u1 * c2 + u2 * c1;
                        u1 = z;
                    }
                    c2 = Math.Sqrt((1.0 - c1) / 2.0);
                    if (dir == FFTDirection.ForwardTransform)
                        c2 = -c2;
                    c1 = Math.Sqrt((1.0 + c1) / 2.0);
                }
                // Scaling for forward transform
                if (dir == FFTDirection.ForwardTransform)
                {
                    for (i = 0; i < n; i++)
                    {
                        rawreal[i] /= n;
                        rawimag[i] /= n;
                    }
                }
            }
        }
        #endregion
    }
}
