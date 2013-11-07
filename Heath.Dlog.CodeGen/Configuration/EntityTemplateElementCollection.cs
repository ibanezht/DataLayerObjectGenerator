using System.Configuration;

namespace Heath.Dlog.CodeGen.Configuration
{
    internal class EntityTemplateElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EntityTemplateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EntityTemplateElement)element).Name;
        }
    }
}