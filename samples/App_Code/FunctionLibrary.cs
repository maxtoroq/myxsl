using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using myxsl.net.common;

public sealed class FunctionLibrary : IXmlSerializable {

   public static readonly FunctionLibrary Instance = new FunctionLibrary();

   private FunctionLibrary() { }

   public System.Xml.Schema.XmlSchema GetSchema() {
      throw new NotImplementedException();
   }

   public void ReadXml(XmlReader reader) {
      throw new NotImplementedException();
   }

   public void WriteXml(XmlWriter writer) {

      writer.WriteStartElement("library");

      int moduleIndex = 0;

      foreach (XPathModuleInfo module in XPathModules.Modules) {

         string modulePrefix = (module.Predeclare) ? 
            module.PredeclarePrefix 
            : module.NamespaceBindings.ContainsValue(module.Namespace) ?
            module.NamespaceBindings.First(p => p.Value == module.Namespace).Key
            : "m" + (++moduleIndex);

         writer.WriteStartElement("module");
         writer.WriteAttributeString("namespace", module.Namespace);

         if (module.Predeclare) 
            writer.WriteAttributeString("predeclaredPrefix", modulePrefix); 
         
         foreach (var item in module.NamespaceBindings) 
            writer.WriteAttributeString("xmlns", item.Key, null, item.Value);
         
         if (!module.NamespaceBindings.ContainsKey(modulePrefix))
            writer.WriteAttributeString("xmlns", modulePrefix, null, module.Namespace);

         foreach (XPathFunctionInfo function in module.Functions) {

            writer.WriteStartElement("function");
            writer.WriteAttributeString("name", modulePrefix + ":" + function.Name);
            writer.WriteAttributeString("as", function.ReturnType.ToString());
            writer.WriteAttributeString("hasSideEffects", XmlConvert.ToString(function.HasSideEffects));

            var descr = (DescriptionAttribute)Attribute.GetCustomAttribute(function.Method, typeof(DescriptionAttribute));

            if (descr != null)
               writer.WriteAttributeString("description", descr.Description);

            foreach (XPathVariableInfo param in function.Parameters) {

               writer.WriteStartElement("param");
               writer.WriteAttributeString("name", param.Name);
               writer.WriteAttributeString("as", param.Type.ToString());

               writer.WriteEndElement(); // param
            }

            writer.WriteEndElement(); // function
         }

         writer.WriteEndElement(); // module
      }

      writer.WriteEndElement(); // library
   }
}