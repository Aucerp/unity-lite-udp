using System.Net;
using System.Linq;

namespace Sample_05
{
    public static class NetworkUtils
    {
        public static bool IsValidIPv4(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            IPAddress address;
            if (!IPAddress.TryParse(ipAddress, out address))
                return false;

            // 檢查是否為 IPv4
            byte[] addressBytes = address.GetAddressBytes();
            if (addressBytes.Length != 4)
                return false;

            // 檢查每個部分是否在有效範圍內 (0-255)
            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (string part in parts)
            {
                int value;
                if (!int.TryParse(part, out value) || value < 0 || value > 255)
                    return false;
            }

            return true;
        }
    }
} 