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
