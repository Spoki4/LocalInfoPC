using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;


namespace PCInfo
{
    public class Info
    {
        public Info()
        { }
        
        public String GetProcessorName()
        {
            ManagementObjectSearcher mosProc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in mosProc.Get())
            {
                return mo["Name"] as String;
            }

            return null;
        }

        public String getVideoCardName()
        {
            ManagementObjectSearcher videocard = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in videocard.Get())
            {
                return mo["Name"] as String;
            }
            return null;
        }

        public String getSystemName()
        {
            ManagementObjectSearcher videocard = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject mo in videocard.Get())
            {
                return mo["Name"] as String;
            }
            return null;
        }

        public long getRAMSize()
        {
            return (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        public string getMachineName()
        {
            return System.Environment.MachineName;
        }

        public ComputerInfo getAllInfo()
        {
            var info = new ComputerInfo();
            info.processorName = GetProcessorName();
            info.videoCardName = getVideoCardName();
            info.OSName = getSystemName();
            info.ramSize = getRAMSize();
            info.computerName = getMachineName();

            return info; 
        }
    }
}
