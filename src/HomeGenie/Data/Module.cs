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
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using HomeGenie.Service;
using MIG.Interfaces.HomeAutomation.Commons;

namespace HomeGenie.Data
{
    /// <summary>
    /// Module instance.
    /// </summary>
    [XmlInclude(typeof(VirtualModule))]
    [Serializable]
    public class Module
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ModuleTypes DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public TsList<ModuleParameter> Properties { get; set; }

        // TODO: deprecate 'Stores' field!!! (DataHelper/LiteDb can be used now to store data for a module)
        [JsonIgnore]
        public TsList<Store> Stores { get; set; }

        public Module()
        {
            Name = string.Empty;
            Address = string.Empty;
            Description = string.Empty;
            DeviceType = ModuleTypes.Generic;

            Properties = new TsList<ModuleParameter>();
            Stores = new TsList<Store>();
        }

        public Module Clone()
        {
            return new Module
            {
                Domain = Domain,
                Address = Address,
                DeviceType = DeviceType,
                Name = Name,
                Description = Description,
                Properties = new TsList<ModuleParameter>(Properties),
                Stores = new TsList<Store>(Stores)
            };
        }
    }
}
