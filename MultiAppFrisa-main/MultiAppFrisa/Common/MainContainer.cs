#define AudioEnable
#define SQLTransactions
#define EmailSender
#define WebServerSender
#define MsjAppService
#define AcqAppService
#define ModbusAppService
#define AcqAppService_USBCard
#define Repository
#define VariableProcessing
//#define TCPSocket
#define ModbusConn
//#define Security
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using System.Data.SqlClient;
using System.Threading;
using PredictorV2.Common;
using MultiAppFrisa.Common;
using MultiAppFrisa.Models;
using System.Drawing;

namespace PredictorV2.Common
{
    public class MainContainer
    {        
        public static string AppServiceFamily = "nuvaHub.Predictor16_31q683s779pt2";
        public static int SecurityCounter = 0; // 8 hours is allowed
        public const int SecurityCounterLimit = 60*60*12; // 0.5hr is allowed
       
        public static void InitializeMainContainer()
        {
            //Lepton.scrs.Add(new Lepton.Scr { rect = new Rectangle(10, 10, 10, 10) });
            //Lepton.scrs.Add(new Lepton.Scr { rect = new Rectangle(50, 50, 10, 10) });
            //Lepton.scrs.Add(new Lepton.Scr { rect = new Rectangle(90, 90, 10, 10) });

            DAQ.NewHandler();
            Modbus.NewHandler();
            //TCPClient.RunProcess();
           // SQL.NewSQLHandler();
            //Debe crearse al final, cuando ya todos los dem[as objetos esten instanciados
            MainApp.NewVariableTasks();
            MultiAppFrisa.Common.SQL.RunProcess();
            OpenCV.Init(); 
            ProcesingImage.RunProcess(); 
           // InternetConnectionModel.InternetController = InternetConnectionModel.NewInternetConnectionController();
        }

        public static class Devices
        {
            public static string LOG = "none";
            public static bool CLOUDSQL = false;
            public static bool TCP = false;
            public static bool IsSQLCommSecurityPassed = false;
            public static bool Web = false;
            public static double SamplingTime = 0;
        }

        public static class ScreenProperties
        {
            public static Windows.UI.Xaml.Controls.RelativePanel rFrame;
            public static double Height = 0;
            public static double Width = 0;
            public static double WorkHeight = 0;
            public static double WorkWidth = 0;
        }


        /**********************************************************************************************************************************/
        /***********************************************  Variable Processing  ************************************************************/
        /**********************************************************************************************************************************/
        #region VariableProcessing
        public class MainApp
        {
            public static MainApp Handler = null;
            public SQLModelFrisaRadial FrisaRadialManagerApp = new SQLModelFrisaRadial(5000, PackTypeEnabled.Status.Enabled, "scsip1.frisa.com", "R3Novahub", "Rl7VU*Nw", "dbDAR3"); 
            public static void NewVariableTasks()
            {
                Handler = new MainApp();
            }

            private byte[] GetBytesFromFloatList(List<float> list)
            {
                byte[] bytes = new byte[list.Count * 4];

                for (int i = 0; i < list.Count; i++)
                {
                    BitConverter.GetBytes(BitConverter.SingleToInt32Bits(list[i])).CopyTo(bytes, i * 4);
                }

                return bytes;
            }

            private byte[] GetBytesFromFloatArray(float [] aArray)
            {
                byte[] bytes = new byte[aArray.Length * 4];

                for (int i = 0; i < aArray.Length; i++)
                {
                    BitConverter.GetBytes(BitConverter.SingleToInt32Bits(aArray[i])).CopyTo(bytes, i * 4);
                }
                return bytes;
            }
            public static class TimePacer
            {
                public const string Uri = "http://worldtimeapi.org/api/timezone/America/Monterrey";
                public static int Counter;
                public static int period = 60;
                public static int MinutesFromMiddleNight = 0;
                public static int PreviousMinutesFromMiddleNight = 0;
                public static int ProcessingBlockNumber = 0;
                public static int PreviousProcessingBlockNumber = 0;
                public static int MinutesInProcessingBlock = 0;
                public static DateTime InternetTime = DateTime.Now;
                public static DateTime PreviousInternetTime = DateTime.Now;
                public class LocalTime
                {
                    public DateTime utc_datetime = DateTime.Now;
                    public DateTime datetime = DateTime.Now;
                }
                static public int CurrentMinute = 0;
            }

            private MainApp()
            {
                ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(10));
            }

            protected ThreadPoolTimer ActionTimer = null;
            int HeartbeatCounter = 5;
            int Heartbeat = 5;
            bool IsVariableLIistLoadedTheFirstTime = false;
            DateTime StartDateToLoadList = DateTime.Now.AddYears(-1);
            DateTime FinishDateToLoadList = DateTime.Now.AddYears(-1);
            bool IsIPStaticConfigured = false;

            bool IsTimeInitialized = false;
            DateTime InternetRefTime = DateTime.Now;
            DateTime ComputerRefTime = DateTime.Now;
            long TimeGap = 0;

            DateTime PivotTime = DateTime.Now;
            int pastSecond = 0;

            public List<string> StringSignalList = new List<string>();
            public int FillIndex = 0;
            public int WatchdogDAQ = 0;
            public int WatchdogSQL = 0;
            public long WatchdogInicioDAQ = DateTime.Now.Ticks;
            public long WatchdogFInDAQ = DateTime.Now.Ticks;
            public long WatchdogInicioSQL = DateTime.Now.Ticks;
            public long WatchdogFInSQL = DateTime.Now.Ticks;



            public async void Proccess(ThreadPoolTimer timer)
            {
                try
                {
                    while (true)
                    {
                        Devices.IsSQLCommSecurityPassed = true;
                        if (FrisaRadialManagerApp.Config.IsEnabled == PackTypeEnabled.Status.Enabled)
                        {
                            if (!DAQ.DAQ1.OK)
                            {
                                var actualSecond = DateTime.Now.Second;
                                if (((actualSecond % 3) == 0) && (pastSecond != actualSecond))
                                {
                                    pastSecond = actualSecond;
                                    float Nok = -0.002f;
                                    var SinglePack = new SQLModelFrisaRadial.FlatPack(DateTime.Now, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok);
                                    var r = await FrisaRadialManagerApp.Attach(SinglePack);
                                }
                            }
                            else
                            {
                                //TODO UPDATE Compensated time
                                //var compensateTime = new DateTime(DateTime.Now.Ticks - CommModel.Controller.Pacer.TimeGap);
                                var compensateTime = DateTime.Now;
                                var actualSecond = compensateTime.Second;
                                if (((actualSecond % 3) == 0) && (pastSecond != actualSecond))
                                {
                                    pastSecond = actualSecond;
                                    if ((DAQ.DAQ1.OK) && (Modbus.Modbus1.OK))
                                    {
                                        var b = DAQ.DAQ1.SamplingPackArray[0].TimeInputRMS;
                                        var d = DAQ.DAQ1.SamplingPackArray[0].FFTDominantFreq;
                                        var e = DAQ.DAQ1.SamplingPackArray[0].FFTDominantMag;
                                        var c = DAQ.DAQ1.SamplingPackArray[0].TimeInputMax;

                                        var k = Modbus.Modbus1.BannerDataArray[0];
                                        var SinglePack = new SQLModelFrisaRadial.FlatPack(compensateTime, (float)b[0], (float)b[1], (float)b[2], (float)b[3], (float)b[4], (float)b[5], (float)b[6], (float)b[7], (float)c[0], (float)c[1], (float)c[2], (float)c[3], (float)c[4], (float)c[5], (float)c[6], (float)c[7], (float)d[0], (float)d[1], (float)d[2], (float)d[3], (float)d[4], (float)d[5], (float)d[6], (float)d[7], (float)e[0], (float)e[1], (float)e[2], (float)e[3], (float)e[4], (float)e[5], (float)e[6], (float)e[7], k.X.RMSVelocidad, k.Z.RMSVelocidad, k.X.HighFrequencyRMSAcceleration, k.Z.HighFrequencyRMSAcceleration, k.Temperature, k.X.RMSAcceleration, k.X.PeakFrequency, k.Z.RMSAcceleration, k.Z.PeakFrequency);
                                        var r = await FrisaRadialManagerApp.Attach(SinglePack);
                                    }
                                    else if ((DAQ.DAQ1.OK) && (!Modbus.Modbus1.OK))
                                    {
                                        var b = DAQ.DAQ1.SamplingPackArray[0].TimeInputRMS;
                                        var d = DAQ.DAQ1.SamplingPackArray[0].FFTDominantFreq;
                                        var e = DAQ.DAQ1.SamplingPackArray[0].FFTDominantMag;
                                        var c = DAQ.DAQ1.SamplingPackArray[0].TimeInputMax;
                                        float Nok = -0.001f;
                                        var SinglePack = new SQLModelFrisaRadial.FlatPack(compensateTime, (float)b[0], (float)b[1], (float)b[2], (float)b[3], (float)b[4], (float)b[5], (float)b[6], (float)b[7], (float)c[0], (float)c[1], (float)c[2], (float)c[3], (float)c[4], (float)c[5], (float)c[6], (float)c[7], (float)d[0], (float)d[1], (float)d[2], (float)d[3], (float)d[4], (float)d[5], (float)d[6], (float)d[7], (float)e[0], (float)e[1], (float)e[2], (float)e[3], (float)e[4], (float)e[5], (float)e[6], (float)e[7], Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok);
                                        var r = await FrisaRadialManagerApp.Attach(SinglePack);
                                    }
                                    else if ((!DAQ.DAQ1.OK) && (Modbus.Modbus1.OK))
                                    {
                                        float Nok = -0.001f;
                                        var k = Modbus.Modbus1.BannerDataArray[0];
                                        var SinglePack = new SQLModelFrisaRadial.FlatPack(compensateTime, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, k.X.RMSVelocidad, k.Z.RMSVelocidad, k.X.HighFrequencyRMSAcceleration, k.Z.HighFrequencyRMSAcceleration, k.Temperature, k.X.RMSAcceleration, k.X.PeakFrequency, k.Z.RMSAcceleration, k.Z.PeakFrequency);
                                        var r = await FrisaRadialManagerApp.Attach(SinglePack);
                                    }
                                    else
                                    {
                                        float Nok = -0.001f;
                                        var SinglePack = new SQLModelFrisaRadial.FlatPack(compensateTime, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok, Nok);
                                        var r = await FrisaRadialManagerApp.Attach(SinglePack);
                                    }
                                }
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
                    ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(10));
                }
            }

        }
        #endregion
        /**********************************************************************************************************************************/
        /**************************************++++++************   SQLTransactions   **************+**************************************/
        /**********************************************************************************************************************************/
        #region SQLTransactions
#if SQLTransactions
        public class SQL : ConnectionModel
        {
            public static SQL Handler = null;
            public static class CloudCommands
            {
                public static bool IsReadLatestVariableStampRequested = false;
                public static bool IsReadLatestVariableStampRequestedSuccessful = false;
                public static DateTime LatestVariableStamp = DateTime.Now;
                public static bool IsReadSuccessfulComandStampRequested = false;
                public static DateTime LatestCommandStamp = DateTime.Now;
                public static int Sampling = 0;
            }

            public double SQLProcessTime = 0;
            public DateTime SQLUltimateStamp = DateTime.Now;
            public bool IsPicsVarSent = false;
            public bool IsVarSent = false;

            private SQL()
            {
                //TryConnectTo();
            }
            public static void NewSQLHandler()
            {
                Handler = new SQL();
            }

            private enum SQLStateMachine
            {
                Initialize,
                GetLatestRegisterStamp,
                SQLTransaction,
            }
            private SQLStateMachine state = SQLStateMachine.Initialize;

            bool IsProccessOK = false;
            public DateTime Latest = DateTime.Now;
            public override async void Proccess(ThreadPoolTimer timer)
            {
                try
                {
                    var cfg = MainApp.Handler.FrisaRadialManagerApp.Config;
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = cfg.DataSource;
                    builder.UserID = cfg.UserID;
                    builder.Password = cfg.Password;
                    builder.InitialCatalog = cfg.InitialCatalog;
                    builder.ConnectTimeout = 50;
                    using (var connection = new SqlConnection(builder.ConnectionString))
                    {
                        var delay = (builder.ConnectTimeout * 1000) + 10000;
                        var cancellationTokenSource = new CancellationTokenSource();
                        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(delay));
                        await connection.OpenAsync(cancellationTokenSource.Token);
                        DateTime connectionTime = DateTime.Now;
                        IsProccessOK = true;
                        while (IsProccessOK && (DateTime.Now - connectionTime).TotalDays < 1)
                        {
                            DateTime Inicio = DateTime.Now;
                            if (MainApp.Handler.FrisaRadialManagerApp.GetCount() > 0)
                            {
                                #region
                                var v = await MainApp.Handler.FrisaRadialManagerApp.Getfirst();
                                StringBuilder sbV = new StringBuilder();
                                sbV.Append("INSERT INTO FrisaRadiales (Stamp, CH0A, CH1A, CH2A, CH3A, CH4A, CH5A, CH6A, CH7A, CH0V, CH1V, CH2V, CH3V, CH4V, CH5V, CH6V, CH7V, CH0F, CH1F, CH2F, CH3F, CH4F, CH5F, CH6F, CH7F, CH0M, CH1M, CH2M, CH3M, CH4M, CH5M, CH6M, CH7M, VX0, VZ0, VXH0, VZH0, T0, AX0, FX0, AZ0, FZ0)");
                                sbV.Append("VALUES");
                                var Stamp = v.Stamp.GetDateTimeFormats('s')[0];
                                var CH0A = v.CH0A.ToString();
                                var CH1A = v.CH1A.ToString();
                                var CH2A = v.CH2A.ToString();
                                var CH3A = v.CH3A.ToString();
                                var CH4A = v.CH4A.ToString();
                                var CH5A = v.CH5A.ToString();
                                var CH6A = v.CH6A.ToString();
                                var CH7A = v.CH7A.ToString();
                                var CH0V = v.CH0V.ToString();
                                var CH1V = v.CH1V.ToString();
                                var CH2V = v.CH2V.ToString();
                                var CH3V = v.CH3V.ToString();
                                var CH4V = v.CH4V.ToString();
                                var CH5V = v.CH5V.ToString();
                                var CH6V = v.CH6V.ToString();
                                var CH7V = v.CH7V.ToString();
                                var CH0F = v.CH0F.ToString();
                                var CH1F = v.CH1F.ToString();
                                var CH2F = v.CH2F.ToString();
                                var CH3F = v.CH3F.ToString();
                                var CH4F = v.CH4F.ToString();
                                var CH5F = v.CH5F.ToString();
                                var CH6F = v.CH6F.ToString();
                                var CH7F = v.CH7F.ToString();
                                var CH0M = v.CH0M.ToString();
                                var CH1M = v.CH1M.ToString();
                                var CH2M = v.CH2M.ToString();
                                var CH3M = v.CH3M.ToString();
                                var CH4M = v.CH4M.ToString();
                                var CH5M = v.CH5M.ToString();
                                var CH6M = v.CH6M.ToString();
                                var CH7M = v.CH7M.ToString();
                                var VX0 = v.VX0.ToString();
                                var VZ0 = v.VZ0.ToString();
                                var VXH0 = v.VXH0.ToString();
                                var VZH0 = v.VZH0.ToString();
                                var T0 = v.T0.ToString();
                                var AX0 = v.AX0.ToString();
                                var FX0 = v.FX0.ToString();
                                var AZ0 = v.AZ0.ToString();
                                var FZ0 = v.FZ0.ToString();
                                sbV.Append("('" + Stamp + "'," + CH0A + "," + CH1A + "," + CH2A + "," + CH3A + "," + CH4A + "," + CH5A + "," + CH6A + "," + CH7A + "," + CH0V + "," + CH1V + "," + CH2V + "," + CH3V + "," + CH4V + "," + CH5V + "," + CH6V + "," + CH7V + "," + CH0F + "," + CH1F + "," + CH2F + "," + CH3F + "," + CH4F + "," + CH5F + "," + CH6F + "," + CH7F + "," + CH0M + "," + CH1M + "," + CH2M + "," + CH3M + "," + CH4M + "," + CH5M + "," + CH6M + "," + CH7M + "," + VX0 + "," + VZ0 + "," + VXH0 + "," + VZH0 + "," + T0 + "," + AX0 + "," + FX0 + "," + AZ0 + "," + FZ0 + ");");
                                #endregion
                                using (var command = new SqlCommand(sbV.ToString(), connection))
                                {
                                    var cancellationTokenSource4 = new CancellationTokenSource();
                                    cancellationTokenSource4.CancelAfter(TimeSpan.FromMilliseconds(delay));
                                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationTokenSource4.Token);
                                    if (rowsAffected > 0)
                                    {
                                        Devices.CLOUDSQL = true;
                                        bool r = await MainApp.Handler.FrisaRadialManagerApp.RemoveFirst();
                                        IsVarSent = true;
                                        Latest = DateTime.Now;
                                    }
                                    else
                                    {
                                        Devices.CLOUDSQL = false;
                                    }
                                }
                            }
                            /******************************************** Send Variables ****************************************************/
                            DateTime final = DateTime.Now;
                            SQLProcessTime = (double)(final.Ticks - Inicio.Ticks) * 0.0000001d;
                            SQLUltimateStamp = DateTime.Now;
                            await Task.Delay(200);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Devices.CLOUDSQL = false;
                    Devices.LOG = Ex.Message;
                }
                finally
                {
                   // ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
                }
            }
        }
#endif
        #endregion
        /**********************************************************************************************************************************/
        /******************************************************   Connection Model   ******************************************************/
        /**********************************************************************************************************************************/
        #region ConnectionModel
        public class ConnectionModel
        {
            public enum ACK
            {
                NoStarted,
                InProgress,
                Success,
                Fail
            }

            public bool IsConnected = false;
            public bool IsInitialized = false;
            protected ThreadPoolTimer ActionTimer = null;
            protected ACK processStatus = ACK.NoStarted;

            public ACK ProcessStatus
            {
                get { return this.processStatus; }
            }

            public virtual void TryConnectTo()
            {
                if (processStatus == ACK.NoStarted)
                {
                    processStatus = ACK.InProgress;
                    //ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
                }
                else if (processStatus == ACK.Fail)
                {
                    processStatus = ACK.InProgress;
                    //ActionTimer = ThreadPoolTimer.CreateTimer(Proccess, TimeSpan.FromMilliseconds(1000));
                }
            }
            public virtual void Proccess(ThreadPoolTimer timer)
            {
            }
        }
        #endregion
        /**********************************************************************************************************************************/
        /********************************************************    Helpers    ***********************************************************/
        /**********************************************************************************************************************************/
        #region Helpers
        public class InternetConnectionModel : ConnectionModel
        {
            public static InternetConnectionModel InternetController = null;
            public static InternetConnectionModel NewInternetConnectionController()
            {
                return new InternetConnectionModel();
            }

            private InternetConnectionModel()
            {
                TryConnectTo();
                InternetStatus = Status.InternetIsOff;
            }

            private bool IsWlanProfile = false;
            private static DateTime WifiTick = DateTime.Now;
            private bool IsNetworkInitilized = false;
            public bool IsWiFiAdapter = false;
            public bool IsEthernetDetected = false;
            WiFiAdapter MyWifi;

            private class NetworkConfiguration
            {
                public string AdapterName { get; set; }
                public string IPAddress { get; set; }
                public string SubnetMask { get; set; }
                public string DefaultGateway { get; set; }
                public string PrimaryDNS { get; set; }
                public string SecondryDNS { get; set; }

                public static NetworkConfiguration networkConfiguration = new NetworkConfiguration()
                {
                    AdapterName = "{DBC19083-C7B5-4513-8692-10DAEC3D15DB}", //Se toma del windows device portal
                    IPAddress = "192.168.0.1",
                    SubnetMask = "255.255.255.0",
                    DefaultGateway = "192.168.0.2",
                    PrimaryDNS = "0.0.0.0",
                    SecondryDNS = "0.0.0.0"
                };
            }
            public string GetLocalIPAddress()
            {
                try
                {
                    var icp = NetworkInformation.GetInternetConnectionProfile();
                    if (icp?.NetworkAdapter == null) return null;
                    var hostname = NetworkInformation.GetHostNames().SingleOrDefault(
                                   hn => hn.IPInformation?.NetworkAdapter != null &&
                                   hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);
                    return hostname?.CanonicalName;
                }
                catch (Exception ex)
                {
                    return "an Error occured";
                }
            }
            public async Task<bool> SetStaticIP()
            {
                if (GetLocalIPAddress() != NetworkConfiguration.networkConfiguration.IPAddress)
                {
                    try
                    {
                        HttpClient client = new HttpClient
                        {
                            BaseAddress = new System.Uri("http://127.0.0.1:8080/", UriKind.RelativeOrAbsolute) //Es la dirección del windows device portal
                        };

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "administrator", "p@ssw0rd"))));
                        string json = JsonConvert.SerializeObject(NetworkConfiguration.networkConfiguration);
                        HttpContent content = new StringContent(json);
                        var result = await client.PutAsync("api/networking/ipv4config", content);
                        var name = GetLocalIPAddress();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                else return true;
            }

            private void NetworkInformationOnNetworkStatusChanged(object sender)
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    if (IsWiFiAdapter)
                    {
                        if (profile.IsWlanConnectionProfile)
                        {
                            IsWlanProfile = true;
                            switch (profile.GetSignalBars().Value)
                            {
                                case 0:
                                    IsWifiSignal = false;
                                    WiFiStrength = (char)(59652);
                                    break;
                                case 1:
                                    IsWifiSignal = true;
                                    WiFiStrength = (char)(59653);
                                    break;
                                case 2:
                                    IsWifiSignal = true;
                                    WiFiStrength = (char)(59654);
                                    break;
                                case 3:
                                    IsWifiSignal = true;
                                    WiFiStrength = (char)(59655);
                                    break;
                                default:
                                    IsWifiSignal = true;
                                    WiFiStrength = (char)(59656);
                                    break;
                            }
                        }
                        else
                        {
                            WiFiStrength = (char)(59652);
                            IsWifiSignal = false;
                            IsWlanProfile = false;
                        }
                    }
                }
                else
                {
                    WiFiStrength = (char)(59652);
                    IsWifiSignal = false;
                    IsEthernetDetected = false;
                }
            }

            public async Task<bool> ConnectToNetwork()
            {
                IsEthernetDetected = false;
                var profile = NetworkInformation.GetInternetConnectionProfile();
                NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
                var IsInitialized = false;
                if (profile != null)
                {
                    string eth = profile.ProfileName.Substring(0, 8);
                    if (eth == "Ethernet")
                    { 
                        IsEthernetDetected = true;
                        IsInitialized = true;
                    }
                }
                IsWiFiAdapter = false;
                var access = await WiFiAdapter.RequestAccessAsync();
                var devSel = WiFiAdapter.GetDeviceSelector();
                var results = await DeviceInformation.FindAllAsync(devSel);
                if (results.Count > 0)
                {
                    MyWifi = await WiFiAdapter.FromIdAsync(results[0].Id);
                    await MyWifi.ScanAsync();
                    var c = MyWifi.NetworkReport.AvailableNetworks;
                    WiFiAvailableNetwork myNet = null;
                    var pass = new PasswordCredential();
                    foreach (WiFiAvailableNetwork item in c)
                    {
                        if (item.Ssid == "nh")
                        {
                            myNet = item;
                            pass.Password = "Toycontento10";
                            break;
                        }
                        else if (item.Ssid == "IZZI-1A7F-5G")
                        {
                            myNet = item;
                            pass.Password = "F8F5329E1A7F";
                            break;
                        }
                        else if (item.Ssid == "IZZI-1A7F")
                        {
                            myNet = item;
                            pass.Password = "F8F5329E1A7F";
                            break;
                        }
                        else if (item.Ssid == "WRL-Visita")
                        {
                            myNet = item;
                            /*como pongo el user: QUIMMCO\FMTLGUERRERO???*/
                            pass.Password = "SCQ#Q2018";
                            break;
                        }
                    }
                    if (myNet != null)
                    {
                        var d = await MyWifi.ConnectAsync(myNet, WiFiReconnectionKind.Automatic, pass);
                        if (d.ConnectionStatus == WiFiConnectionStatus.Success)
                        {
                            IsWiFiAdapter = true;
                            IsInitialized = true;
                        }
                        else if (d.ConnectionStatus == WiFiConnectionStatus.UnspecifiedFailure)
                        {
                            throw new System.ArgumentException("UnspecifiedFailure", "original");
                        }
                    }
                }
                return IsInitialized;
            }

            public char WiFiStrength { get; private set; } = (char)(59652);

            public bool IsWifiSignal { get; private set; } = false;

            public bool IsInternetAccess { get; private set; } = false;

            bool IsFailedConnection = false;
            int InternetOffCounter = 0;
            int InternetOnCounter = 0;

            public enum Status
            {
                InternetTurnsOnToOff,
                InternetTurnsOffToOn,
                InternetIsOff,
                InternetIsOn                
            }

            public Status InternetStatus = new Status();

            public async override void Proccess(ThreadPoolTimer timer)
            {
                try
                {
                    IsInternetAccess = false;
                    IsWifiSignal = false;
                    IsWlanProfile = false;
                    IsFailedConnection = true;
                    while (IsFailedConnection)
                    {
                        if (!IsNetworkInitilized)
                        {
                            IsNetworkInitilized = await ConnectToNetwork();
                        }
                        else //WiFi connection success or Ethernet was detected
                        {
                            await Task.Delay(1000);
                            var profile = NetworkInformation.GetInternetConnectionProfile();
                            var net = profile.GetNetworkConnectivityLevel();
                            if (net != NetworkConnectivityLevel.None)
                            {
                                InternetOffCounter = 0; //Asegura 10 segundos continuos en este estado
                                if ((InternetOnCounter++) >= 30)
                                {
                                    InternetOnCounter = 0;
                                    if (!IsInternetAccess)
                                    {
                                        InternetStatus = Status.InternetTurnsOffToOn;
                                    }
                                    else
                                    {
                                        InternetStatus = Status.InternetIsOn;
                                    }
                                    IsInternetAccess = true;
                                }
                            }
                            else
                            {
                                InternetOnCounter = 0;
                                if ((InternetOffCounter++) >= 10)
                                {
                                    InternetOffCounter = 0; //Asegura 10 segundos continuos en este estado
                                    if (IsInternetAccess)
                                    {
                                        InternetStatus = Status.InternetTurnsOnToOff;
                                    }
                                    else
                                    {
                                        InternetStatus = Status.InternetIsOff;
                                    }
                                    IsInternetAccess = false;
                                    WiFiStrength = (char)(59652);
                                    IsEthernetDetected = false;
                                    IsWiFiAdapter = false;
                                    IsNetworkInitilized = false;
                                    processStatus = ACK.Fail;
                                    IsFailedConnection = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Task.Delay(3000);
                }
                finally
                {
                    WiFiStrength = (char)(59652);
                    IsEthernetDetected = false;
                    IsInternetAccess = false;
                    IsWiFiAdapter = false;
                    IsNetworkInitilized = false;
                    processStatus = ACK.Fail;
                    TryConnectTo();
                }
            }
        }
        #endregion
    }
}
