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
        readonly string[] ConfigPaths = { "%USERPROFILE%/.ovh.conf", "" };
        public IniData Config { get; set; }

        /// <summary>
        /// Create a config parser and load config from environment.
        /// </summary>
        public ConfigurationManager()
        {
            string libraryPath = AppDomain.CurrentDomain.BaseDirectory;
            string confPath = Path.Combine(libraryPath, ".ovh.conf");
            ConfigPaths[1] = confPath;

            IniDataParser Parser = new IniDataParser();
            string chosenPath = ConfigPaths.Where(p => File.Exists(p)).LastOrDefault();
            if (chosenPath == null)
            {
                Config = new IniData();
            }
            else
            {
                Config = Parser.Parse(File.ReadAllText(chosenPath));
            }
        }

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
            string value = null;

            value = Environment.GetEnvironmentVariable("OVH_" + name.ToUpper());
            if(value != null)
            {
                return value;
            }

            var sectionData = Config[section];
            if (sectionData == null)
            {
                throw new ConfigurationKeyMissingException(
                    string.Format("Could not find configuration section {0}",
                        section));
            }

            value = Config[section][name];
            if (value == null)
            {
                throw new ConfigurationKeyMissingException(
                    string.Format("Could not find configuration key {0} in section {1}",
                        name, section));
            }
            return value;
        }

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
