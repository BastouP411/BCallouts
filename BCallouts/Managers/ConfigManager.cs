using BCallouts.Beans;
using System.IO;
using System.Xml.Serialization;

namespace BCallouts.Managers
{
    class ConfigManager
    {
        public static Configuration Config { get; private set; }

        public static void UpdateConfig() {
            TextReader textReader = new StreamReader(Directory.GetCurrentDirectory() + "\\Plugins\\LSPDFR\\BCallouts\\config.xml");
            Config = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(textReader);
            textReader.Close();
        }
    }
}
