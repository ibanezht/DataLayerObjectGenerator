using System.Configuration;

namespace Heath.Dlog.CodeGen.Configuration
{
    internal class EntityTemplateElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("language")]
        public string Language
        {
            get { return (string)this["language"]; }
            set { this["language"] = value; }
        }

        [ConfigurationProperty("template")]
        public string Template
        {
            get { return (string)this["template"]; }
            set { this["template"] = value; }
        }
    }
}