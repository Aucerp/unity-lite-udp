using UnityEngine;
using NUnit.Framework;
using Sample_05;

public class NetworkUtilsTests
{
    [Test]
    public void IsValidIPv4_WithInvalidInputs_ReturnsFalse()
    {
        // Arrange
        string[] invalidIPs = new string[] 
        {
            null,
            "",
            "77",
            "88",
            "256.1.2.3",
            "1.2.3.256",
            "1.2.3.4.5",
            "abc.def.ghi.jkl",
            "192.168.1",
            "192.168.1.",
            "192.168.1.1.1"
        };

        // Act & Assert
        foreach (string ip in invalidIPs)
        {
            Assert.IsFalse(NetworkUtils.IsValidIPv4(ip), $"IP '{ip}' should be invalid");
        }
    }

    [Test]
    public void IsValidIPv4_WithValidInputs_ReturnsTrue()
    {
        // Arrange
        string[] validIPs = new string[] 
        {
            "127.0.0.1",
            "192.168.1.1",
            "10.0.0.0",
            "172.16.0.1",
            "0.0.0.0",
            "255.255.255.255"
        };

        // Act & Assert
        foreach (string ip in validIPs)
        {
            Assert.IsTrue(NetworkUtils.IsValidIPv4(ip), $"IP '{ip}' should be valid");
        }
    }
} 