/* 
 *  Name:           Echo Tool
 *  
 *  Description:    Application provides functionality of the echo server and client.
 *                  Designed according to RFC 862 http://www.ietf.org/rfc/rfc0862.txt?number=862
 *                  
 *  Author:         Pavel Bansky
 *  Contact:        pavel@bansky.net
 *  Website:        http://bansky.net/echotool
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoToolCMD
{
    /// <summary>
    /// Usefull static methods
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Returns true whether string represents number
        /// </summary>
        /// <param name="stringToTest">String to test</param>
        /// <returns>True if string represents number</returns>
        public static bool IsNumber(string stringToTest)
        {
            bool digit = !string.IsNullOrEmpty(stringToTest);
            foreach (char c in stringToTest)
            {
                if (!Char.IsDigit(c))
                {
                    digit = false;
                    break;
                }
            }
            return digit;
        }

        /// <summary>
        /// Compares two arrays of bytes
        /// </summary>
        /// <param name="byteArray1">Byte array one</param>
        /// <param name="byteArray2">Byte array two</param>
        /// <returns>True if arrays are equal</returns>
        public static bool CompareByteArrays(byte[] byteArray1, byte[] byteArray2)
        {
            // If both are null, they're equal
            if (byteArray1 == null && byteArray2 == null)
            {
                return true;
            }
            // If either but not both are null, they're not equal
            if (byteArray1 == null || byteArray2 == null)
            {
                return false;
            }
            if (byteArray1.Length != byteArray2.Length)
            {
                return false;
            }
            for (int i = 0; i < byteArray1.Length; i++)
            {
                if (byteArray1[i] != byteArray2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
