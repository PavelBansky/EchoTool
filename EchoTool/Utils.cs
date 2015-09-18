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

using System.Linq;

namespace EchoTool
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
            return !string.IsNullOrWhiteSpace(stringToTest) && stringToTest.All(char.IsDigit);
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
            return !byteArray1.Where((t, i) => t != byteArray2[i]).Any();
        }
    }
}
