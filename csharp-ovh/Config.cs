//Copyright(c) 2013-2016, OVH SAS.
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//  * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.

// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.

// * Neither the name of OVH SAS nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY OVH SAS AND CONTRIBUTORS ``AS IS'' AND ANY
//EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL OVH SAS AND CONTRIBUTORS BE LIABLE FOR ANY
//DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using IniParser.Model;
using IniParser.Parser;
using Ovh.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ovh.Api
{
    //The straightforward way to use OVH's API keys is to embed them directly in the
    //application code. While this is very convenient, it lacks of elegance and
    //flexibility.
    //Alternatively it is suggested to use configuration files or environment
    //variables so that the same code may run seamlessly in multiple environments.
    //Production and development for instance.
    //This wrapper will first look for direct instanciation parameters then
    //OVH_ENDPOINT, OVH_APPLICATION_KEY, OVH_APPLICATION_SECRET and
    //OVH_CONSUMER_KEY environment variables. If either of these parameter is not
    //provided, it will look for a configuration file of the form:
    //.ini file:
    //    [default]
    //    ; general configuration: default endpoint
    //    endpoint=ovh-eu
    //    [ovh-eu]
    //    ; configuration specific to 'ovh-eu' endpoint
    //    application_key=my_app_key
    //    application_secret=my_application_secret
    //    consumer_key=my_consumer_key
    //The client will successively attempt to locate this configuration file in
    //1. Current working directory: "./ovh.conf"
    //2. Current user's home directory "%USERPROFILE%"
    //This lookup mechanism makes it easy to overload credentials for a specific
    //project or user.

    /// <summary>
    /// Application wide configuration manager
    /// </summary>
    public class ConfigurationManager
    {
        //Locations where to look for configuration file by *increasing* priority
        private readonly string[] _configPaths = { "%USERPROFILE%/.ovh.conf", "" };
        /// <summary>
        /// INI data from the configuration file
        /// </summary>
        public IniData Config { get; set; }

        /// <summary>
        /// Create a config parser and load config from environment.
        /// </summary>
        public ConfigurationManager()
        {
            string libraryPath = AppDomain.CurrentDomain.BaseDirectory;
            string confPath = Path.Combine(libraryPath, ".ovh.conf");
            _configPaths[1] = confPath;

            IniDataParser parser = new IniDataParser();
            string chosenPath = _configPaths.LastOrDefault(p => File.Exists(p));
            if (chosenPath == null)
            {
                Config = new IniData();
            }
            else
            {
                Config = parser.Parse(File.ReadAllText(chosenPath));
            }
        }

        /// <summary>
        /// Initializes a new <c>ConfigurationManager</c>
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="applicationKey">API application key</param>
        /// <param name="applicationSecret">API application secret</param>
        /// <param name="consumerKey">Client consumer key</param>
        public ConfigurationManager(string endpoint, string applicationKey = null,
            string applicationSecret = null, string consumerKey = null)
        {
            Config = new IniData();

            Config.Sections.Add(new SectionData("default"));
            Config.Sections["default"].AddKey("endpoint", endpoint);

            Config.Sections.Add(new SectionData(endpoint));
            if (applicationKey != null)
            {
                Config.Sections[endpoint].AddKey("application_key", applicationKey);
            }

            if (applicationSecret != null)
            {
                Config.Sections[endpoint].AddKey("application_secret", applicationSecret);
            }

            if (consumerKey != null)
            {
                Config.Sections[endpoint].AddKey("consumer_key", consumerKey);
            }
        }

        /// <summary>
        /// Load parameter "name" from configuration, respecting priority order.
        /// Most of the time, "section" will correspond to the current api
        /// "endpoint". "default" section only contains "endpoint" and general
        /// configuration.
        /// </summary>
        /// <param name="section">Configuration section or region name. Ignored when
        /// looking in environment</param>
        /// <param name="name">Configuration parameter to lookup</param>
        /// <returns>The value of the looked up configuration</returns>
        /// <exception cref="KeyNotFoundException">Configuration key is missing</exception>
        public string Get(string section, string name)
        {
            string value = Environment.GetEnvironmentVariable("OVH_" + name.ToUpper());
            if(value != null)
            {
                return value;
            }

            KeyDataCollection sectionData = Config[section];
            if (sectionData == null)
            {
                throw new ConfigurationKeyMissingException(
                    string.Format($"Could not find configuration section {section}"));
            }

            value = Config[section][name];
            if (value == null)
            {
                throw new ConfigurationKeyMissingException(
                    string.Format($"Could not find configuration key {name} in section {section}"));
            }
            return value;
        }

        /// <summary>
        /// Tries to get a the parameter <paramref name="name"/> from the section <paramref name="section"/>
        /// </summary>
        /// <param name="section">The section of the INI file to look into</param>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The found value</param>
        /// <returns>True if the call succeeded or false otherwise</returns>
        public bool TryGet(string section, string name, out string value)
        {
            value = null;
            try
            {
                value = Get(section, name);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (ConfigurationKeyMissingException)
            {
                return false;
            }
        }
    }
}
