using System.Management;

namespace GZipTest.Helpers
{
    public static class CoreManager
    {
        public static int GetCoreCount()
        {
            int coreCount = 0;
            foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }
    }
}
