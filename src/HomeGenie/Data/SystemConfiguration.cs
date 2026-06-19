/*

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as
   published by the Free Software Foundation, either version 3 of the
   License, or (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.

   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using MIG.Config;

using HomeGenie.Service;
using HomeGenie.Automation;

namespace HomeGenie.Data
{
    [Serializable]
    public class SystemConfiguration
    {
        private readonly object configWriteLock = new object();
        private string passphrase = string.Empty;

        // TODO: change this to use standard event delegates model
        public event Action<bool> OnUpdate;

        public HomeGenieConfiguration HomeGenie { get; set; }

        public MigServiceConfiguration MigService { get; set; }

        public SystemConfiguration()
        {
            HomeGenie = new HomeGenieConfiguration
            {
                SystemName = "HAL",
                Location = string.Empty,
                EnableLogFile = "false"
            };

            MigService = new MigServiceConfiguration();
        }

        public bool Update()
        {
            var success = false;

            try
            {
                var syscopy = this.DeepClone();
                var encryptionPassphrase = passphrase;
                var settings = syscopy.HomeGenie.Settings;

                foreach (ModuleParameter p in settings)
                {
                    if (!(p.GetData() is string))
                        continue;

                    var stringValue = p.Value;

                    try
                    {
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            p.Value = StringCipher.Encrypt(
                                stringValue,
                                encryptionPassphrase
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        MIG.MigService.Log.Error(ex);
                    }
                }

                var fileName = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "systemconfig.xml"
                );

                var writerSettings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                var serializer = new XmlSerializer(syscopy.GetType());

                lock (configWriteLock)
                {
                    using (var writer = System.Xml.XmlWriter.Create(fileName, writerSettings))
                    {
                        serializer.Serialize(writer, syscopy);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                MIG.MigService.Log.Error(ex);
            }

            OnUpdate?.Invoke(success);

            return success;
        }

        public void SetPassPhrase(string pass)
        {
            passphrase = pass;
        }

        public string GetPassPhrase()
        {
            return passphrase;
        }
    }

    [Serializable]
    public class HomeGenieConfiguration
    {
        public string GUID { get; set; }

        public string SystemName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Location { get; set; }

        public List<ModuleParameter> Settings = new List<ModuleParameter>();

        public HomeGenieConfiguration()
        {
            // default values
            Username = "admin";
            Password = string.Empty;
        }

        public string EnableLogFile { get; set; }

        public int ProgramIdNext { get; set; } =
            ProgramManager.USERSPACE_PROGRAMS_START;
    }
}
