using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PredictorV2.Common
{
    public class PackTypeEnabled
    {
        public enum Status
        {
            Enabled,
            Disabled
        }
        public Status IsEnabled = Status.Disabled;
        public string DataSource = "";
        public string UserID = "";
        public string Password = "";
        public string InitialCatalog = "";
    }
    public class SQLModelForja
    {
        private int deep = 0;
        public PackTypeEnabled Config = new PackTypeEnabled();
        public SQLModelForja(int d, PackTypeEnabled.Status enabled, string source, string id, string password, string catalog)
        {
            deep = d;
            Config.IsEnabled = enabled;
            Config.DataSource = source;
            Config.UserID = id;
            Config.Password = password;
            Config.InitialCatalog = catalog;
        }
        public class FlatPack
        {
            public DateTime Stamp = DateTime.Now;
            public float A_B01 = 0;
            public float A_B02 = 0;
            public float A_B03 = 0;
            public float A_B04 = 0;
            public float A_B05 = 0;
            public float A_B06 = 0;
            public float A_B07 = 0;
            public float A_B08 = 0;
            public float A_B09 = 0;
            public float A_C01 = 0;
            public float A_C02 = 0;
            public float A_C03 = 0;
            public float A_C04 = 0;
            public float A_C05 = 0;
            public float A_C06 = 0;
            public float A_C07 = 0;
            public float A_C08 = 0;
            public float A_C09 = 0;
            public float A_C10 = 0;
            public float A_C11 = 0;
            public float A_C12 = 0;
            public float A_C13 = 0;
            public float A_C14 = 0;
            public float A_C15 = 0;
            public float P_B01 = 0;
            public float P_B02 = 0;
            public float P_B03 = 0;
            public float P_B04 = 0;
            public float P_B05 = 0;
            public float P_B06 = 0;
            public float P_B07 = 0;
            public float P_B08 = 0;
            public float P_B09 = 0;
            public float P_C01 = 0;
            public float P_C02 = 0;
            public float P_C03 = 0;
            public float P_C04 = 0;
            public float P_C05 = 0;
            public float P_C06 = 0;
            public float P_C07 = 0;
            public float P_C08 = 0;
            public float P_C09 = 0;
            public float P_C10 = 0;
            public float P_C11 = 0;
            public float P_C12 = 0;
            public float P_C13 = 0;
            public float P_C14 = 0;
            public float P_C15 = 0;
            public float M_B01 = 0;
            public float M_B02 = 0;
            public float M_B03 = 0;
            public float M_B04 = 0;
            public float M_B05 = 0;
            public float M_B06 = 0;
            public float M_B07 = 0;
            public float M_B08 = 0;
            public float M_B09 = 0;
            public float M_C01 = 0;
            public float M_C02 = 0;
            public float M_C03 = 0;
            public float M_C04 = 0;
            public float M_C05 = 0;
            public float M_C06 = 0;
            public float M_C07 = 0;
            public float M_C08 = 0;
            public float M_C09 = 0;
            public float M_C10 = 0;
            public float M_C11 = 0;
            public float M_C12 = 0;
            public float M_C13 = 0;
            public float M_C14 = 0;
            public float M_C15 = 0;
            public float V_B1 = 0;
            public float V_B9 = 0;
            public float F_B1 = 0;
            public float F_B9 = 0;

            public FlatPack(DateTime s, 
                float aA_B01, float aA_B02, float aA_B03, float aA_B04, float aA_B05, float aA_B06, float aA_B07, float aA_B08, float aA_B09, float aA_C01, float aA_C02, float aA_C03, float aA_C04, float aA_C05, float aA_C06, float aA_C07, float aA_C08, float aA_C09, float aA_C10, float aA_C11, float aA_C12, float aA_C13, float aA_C14, float aA_C15,
                float aP_B01, float aP_B02, float aP_B03, float aP_B04, float aP_B05, float aP_B06, float aP_B07, float aP_B08, float aP_B09, float aP_C01, float aP_C02, float aP_C03, float aP_C04, float aP_C05, float aP_C06, float aP_C07, float aP_C08, float aP_C09, float aP_C10, float aP_C11, float aP_C12, float aP_C13, float aP_C14, float aP_C15,
                float aM_B01, float aM_B02, float aM_B03, float aM_B04, float aM_B05, float aM_B06, float aM_B07, float aM_B08, float aM_B09, float aM_C01, float aM_C02, float aM_C03, float aM_C04, float aM_C05, float aM_C06, float aM_C07, float aM_C08, float aM_C09, float aM_C10, float aM_C11, float aM_C12, float aM_C13, float aM_C14, float aM_C15,
                float aV_B1, float aV_B9, float aF_B1, float aF_B9)
            {
                Stamp = s;
                A_B01 = aA_B01;
                A_B02 = aA_B02;
                A_B03 = aA_B03;
                A_B04 = aA_B04;
                A_B05 = aA_B05;
                A_B06 = aA_B06;
                A_B07 = aA_B07;
                A_B08 = aA_B08;
                A_B09 = aA_B09;
                A_C01 = aA_C01;
                A_C02 = aA_C02;
                A_C03 = aA_C03;
                A_C04 = aA_C04;
                A_C05 = aA_C05;
                A_C06 = aA_C06;
                A_C07 = aA_C07;
                A_C08 = aA_C08;
                A_C09 = aA_C09;
                A_C10 = aA_C10;
                A_C11 = aA_C11;
                A_C12 = aA_C12;
                A_C13 = aA_C13;
                A_C14 = aA_C14;
                P_B01 = aP_B01;
                P_B02 = aP_B02;
                P_B03 = aP_B03;
                P_B04 = aP_B04;
                P_B05 = aP_B05;
                P_B06 = aP_B06;
                P_B07 = aP_B07;
                P_B08 = aP_B08;
                P_B09 = aP_B09;
                P_C01 = aP_C01;
                P_C02 = aP_C02;
                P_C03 = aP_C03;
                P_C04 = aP_C04;
                P_C05 = aP_C05;
                P_C06 = aP_C06;
                P_C07 = aP_C07;
                P_C08 = aP_C08;
                P_C09 = aP_C09;
                P_C10 = aP_C10;
                P_C11 = aP_C11;
                P_C12 = aP_C12;
                P_C13 = aP_C13;
                P_C14 = aP_C14;
                M_B01 = aM_B01;
                M_B02 = aM_B02;
                M_B03 = aM_B03;
                M_B04 = aM_B04;
                M_B05 = aM_B05;
                M_B06 = aM_B06;
                M_B07 = aM_B07;
                M_B08 = aM_B08;
                M_B09 = aM_B09;
                M_C01 = aM_C01;
                M_C02 = aM_C02;
                M_C03 = aM_C03;
                M_C04 = aM_C04;
                M_C05 = aM_C05;
                M_C06 = aM_C06;
                M_C07 = aM_C07;
                M_C08 = aM_C08;
                M_C09 = aM_C09;
                M_C10 = aM_C10;
                M_C11 = aM_C11;
                M_C12 = aM_C12;
                M_C13 = aM_C13;
                M_C14 = aM_C14;
                V_B1 = aV_B1;
                V_B9 = aV_B9;
                F_B1 = aF_B1;
                F_B9 = aF_B9;
            }
        }
        private bool IsAccessing = false;
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
        public List<FlatPack> VibrationPackList = new List<FlatPack>();

        public int GetCount()
        {
            return VibrationPackList.Count;
        }

        public FlatPack Getfirst()
        {
            if (VibrationPackList.Count > 0)
            {
                var a = VibrationPackList[0];
                return new FlatPack(a.Stamp, 
                    a.A_B01, a.A_B02, a.A_B03, a.A_B04, a.A_B05, a.A_B06, a.A_B07, a.A_B08, a.A_B09, a.A_C01, a.A_C02, a.A_C03, a.A_C04, a.A_C05, a.A_C06, a.A_C07, a.A_C08, a.A_C09, a.A_C10, a.A_C11, a.A_C12, a.A_C13, a.A_C14, a.A_C15,
                    a.P_B01, a.P_B02, a.P_B03, a.P_B04, a.P_B05, a.P_B06, a.P_B07, a.P_B08, a.P_B09, a.P_C01, a.P_C02, a.P_C03, a.P_C04, a.P_C05, a.P_C06, a.P_C07, a.P_C08, a.P_C09, a.P_C10, a.P_C11, a.P_C12, a.P_C13, a.P_C14, a.P_C15,
                    a.M_B01, a.M_B02, a.M_B03, a.M_B04, a.M_B05, a.M_B06, a.M_B07, a.M_B08, a.M_B09, a.M_C01, a.M_C02, a.M_C03, a.M_C04, a.M_C05, a.M_C06, a.M_C07, a.M_C08, a.M_C09, a.M_C10, a.M_C11, a.M_C12, a.M_C13, a.M_C14, a.M_C15,
                    a.V_B1, a.V_B9, a.F_B1, a.F_B9);
            }
            else return null;
        }

        public async Task<bool> RemoveFirst()
        {
            try
            {
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                if (VibrationPackList.Count > 0)
                {
                    VibrationPackList.RemoveAt(0);
                }
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> RemoveAll()
        {
            try
            {
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                VibrationPackList.Clear();
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> Attach(FlatPack a)
        {
            try
            {
                if (VibrationPackList.Count > deep) return false;
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                VibrationPackList.Add(a);
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }
    }

    public class SQLModelFrisaRadial
    {
        private int deep = 0;

        public PackTypeEnabled Config = new PackTypeEnabled();
        public SQLModelFrisaRadial(int d, PackTypeEnabled.Status enabled, string source, string id, string password, string catalog)
        {
            deep = d;
            Config.IsEnabled = enabled;
            Config.DataSource = source;
            Config.UserID = id;
            Config.Password = password;
            Config.InitialCatalog = catalog;
        }

        public class FlatPack
        {
            public DateTime Stamp = DateTime.Now;
            public float CH0A = 0;
            public float CH1A = 0;
            public float CH2A = 0;
            public float CH3A = 0;
            public float CH4A = 0;
            public float CH5A = 0;
            public float CH6A = 0;
            public float CH7A = 0;
            public float CH0V = 0;
            public float CH1V = 0;
            public float CH2V = 0;
            public float CH3V = 0;
            public float CH4V = 0;
            public float CH5V = 0;
            public float CH6V = 0;
            public float CH7V = 0;
            public float CH0F = 0;
            public float CH1F = 0;
            public float CH2F = 0;
            public float CH3F = 0;
            public float CH4F = 0;
            public float CH5F = 0;
            public float CH6F = 0;
            public float CH7F = 0;
            public float CH0M = 0;
            public float CH1M = 0;
            public float CH2M = 0;
            public float CH3M = 0;
            public float CH4M = 0;
            public float CH5M = 0;
            public float CH6M = 0;
            public float CH7M = 0;
            public float VX0 = 0;
            public float VZ0 = 0;
            public float VXH0 = 0;
            public float VZH0 = 0;
            public float T0 = 0;
            public float AX0 = 0;
            public float FX0 = 0;
            public float AZ0 = 0;
            public float FZ0 = 0;
            public FlatPack(DateTime s, float aCH0A, float aCH1A, float aCH2A, float aCH3A, float aCH4A, float aCH5A, float aCH6A, float aCH7A, float aCH0V, float aCH1V, float aCH2V, float aCH3V, float aCH4V, float aCH5V, float aCH6V, float aCH7V, float aCH0F, float aCH1F, float aCH2F, float aCH3F, float aCH4F, float aCH5F, float aCH6F, float aCH7F, float aCH0M, float aCH1M, float aCH2M, float aCH3M, float aCH4M, float aCH5M, float aCH6M, float aCH7M, float vx0, float vz0, float vxh0, float vzh0, float t0, float aAX0, float aFX0, float aAZ0, float aFZ0)
            {
                Stamp = s;
                CH0A = aCH0A;
                CH1A = aCH1A;
                CH2A = aCH2A;
                CH3A = aCH3A;
                CH4A = aCH4A;
                CH5A = aCH5A;
                CH6A = aCH6A;
                CH7A = aCH7A;
                CH0V = aCH0V;
                CH1V = aCH1V;
                CH2V = aCH2V;
                CH3V = aCH3V;
                CH4V = aCH4V;
                CH5V = aCH5V;
                CH6V = aCH6V;
                CH7V = aCH7V;
                CH0F = aCH0F;
                CH1F = aCH1F;
                CH2F = aCH2F;
                CH3F = aCH3F;
                CH4F = aCH4F;
                CH5F = aCH5F;
                CH6F = aCH6F;
                CH7F = aCH7F;
                CH0M = aCH0M;
                CH1M = aCH1M;
                CH2M = aCH2M;
                CH3M = aCH3M;
                CH4M = aCH4M;
                CH5M = aCH5M;
                CH6M = aCH6M;
                CH7M = aCH7M;
                VX0 = vx0;
                VZ0 = vz0;
                VXH0 = vxh0;
                VZH0 = vzh0;
                T0 = t0;
                AX0 = aAX0;
                FX0 = aFX0;
                AZ0 = aAZ0;
                FZ0 = aFZ0;
            }
        }

        private bool IsAccessing = false;
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
        public List<FlatPack> VibrationPackList = new List<FlatPack>();

        public int GetCount()
        {
            return VibrationPackList.Count;
        }

        public async Task<FlatPack> Getfirst()
        {
            FlatPack fp = null;
            try
            {
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                if (VibrationPackList.Count > 0)
                {
                    var a = VibrationPackList[0];
                    fp = new FlatPack(a.Stamp, a.CH0A, a.CH1A, a.CH2A, a.CH3A, a.CH4A, a.CH5A, a.CH6A, a.CH7A, a.CH0V, a.CH1V, a.CH2V, a.CH3V, a.CH4V, a.CH5V, a.CH6V, a.CH7V, a.CH0F, a.CH1F, a.CH2F, a.CH3F, a.CH4F, a.CH5F, a.CH6F, a.CH7F, a.CH0M, a.CH1M, a.CH2M, a.CH3M, a.CH4M, a.CH5M, a.CH6M, a.CH7M, a.VX0, a.VZ0, a.VXH0, a.VZH0, a.T0, a.AX0, a.FX0, a.AZ0, a.FZ0);
                }
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {

            }
            return fp;
        }

        public async Task<bool> RemoveFirst()
        {
            try
            {
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                if (VibrationPackList.Count > 0)
                {
                    VibrationPackList.RemoveAt(0);
                }
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> RemoveAll()
        {
            try
            {
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                VibrationPackList.Clear();
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> Attach(FlatPack a)
        {
            try
            {
                if (VibrationPackList.Count > deep) return false;
                if (IsAccessing)
                {
                    await semaphore.WaitAsync();
                }
                IsAccessing = true;
                VibrationPackList.Add(a);
                IsAccessing = false;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
            catch
            {
            }
            return true;
        }
    }
}
