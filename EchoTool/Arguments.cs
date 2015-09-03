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
using System.Text.RegularExpressions;

namespace EchoToolCMD
{
    /// <summary>
    /// Implements functionality to parse switches in command line arguments
    /// </summary>
    public class Arguments
    {
        #region Fields
        Dictionary<string, string> argumentsDictionary = new Dictionary<string, string>();
        string firstArgument = string.Empty;
        #endregion

        /// <summary>
        /// Creates new Arguments object
        /// </summary>
        /// <param name="argumentsArray"></param>
        public Arguments(string[] argumentsArray)
        {
            if (argumentsArray != null && argumentsArray.Length > 0)
            {
                firstArgument = argumentsArray[0];

                // Join the arguments to one string
                string argumentLine = string.Join(" ", argumentsArray);

                // Split it again by matches
                Regex argsRegex = new Regex(@"\/\w*[^/]*");
                MatchCollection argsMatches = argsRegex.Matches(argumentLine);
                foreach (Match argument in argsMatches)
                {
                    string argsString = argument.Value.Trim();

                    string argSwitch = Regex.Match(argument.Value, @"/\w*").Value.Trim();
                    string argData = Regex.Match(argument.Value, @"\s([\w|\s]*)$").Value.Trim();
                    
                    // Save it to the args dict.
                    argumentsDictionary.Add(argSwitch, (argData == null) ? string.Empty : argData);
                }
            }
        }

        /// <summary>
        /// Checks whether specified switch exists
        /// </summary>
        /// <param name="argSwitch">Switch</param>
        /// <returns>True if switch exists</returns>
        public bool Exists(string argSwitch)
        {             
            return argumentsDictionary.ContainsKey(argSwitch);
        }

        /// <summary>
        /// Gets the value for the switch
        /// </summary>
        /// <param name="argSwitch">Switch</param>
        /// <returns>Switchs value</returns>
        public string Get(string argSwitch)
        {            
            return argumentsDictionary[argSwitch];
        }

        /// <summary>
        /// Gets the value for the switch.
        /// If switch is emptry or note exists then returns default value.
        /// </summary>
        /// <param name="argSwitch">Switch</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Switchs value</returns>
        public string Get(string argSwitch, string defaultValue)
        {
            if (Exists(argSwitch))
            {
                string argValue = Get(argSwitch);
                return (string.IsNullOrEmpty(argValue)) ? defaultValue : argValue;
            }
            else
                return defaultValue;
        }

        /// <summary>
        /// Gets the value for the switch.
        /// If switch no exists then return dafault value
        /// </summary>
        /// <param name="argSwitch">Switch</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Switch value</returns>
        public string GetNotExists(string argSwitch, string defaultValue)
        {
            if (Exists(argSwitch))                            
                return Get(argSwitch);            
            else
                return defaultValue;
        }

        /// <summary>
        /// Contains first rgument on the line
        /// </summary>
        public string FirstArgument
        {
            get { return firstArgument; }
        }
    }
}
