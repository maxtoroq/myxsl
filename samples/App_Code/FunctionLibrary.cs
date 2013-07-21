using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using myxsl.net.common;

[XmlRoot("library")]
public sealed class FunctionLibrary : IXmlSerializable {

   public static readonly FunctionLibrary Instance = new FunctionLibrary();

   private FunctionLibrary() { }

   public System.Xml.Schema.XmlSchema GetSchema() {
      return null;
   }

   public void ReadXml(XmlReader reader) {
      throw new NotImplementedException();
   }

   public void WriteXml(XmlWriter writer) {

      int moduleIndex = 0;

      foreach (XPathModuleInfo module in XPathModules.Modules) {

         string modulePrefix = (module.Predeclare) ? 
            module.PredeclarePrefix 
            : module.NamespaceBindings.ContainsValue(module.Namespace) ?
            module.NamespaceBindings.First(p => p.Value == module.Namespace).Key
            : "m" + (++moduleIndex);

         writer.WriteStartElement("module");
         writer.WriteAttributeString("namespace", module.Namespace);
         writer.WriteAttributeString("cref", CRef(module.Type));

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
            writer.WriteAttributeString("cref", CRef(function.Method));

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
   }

   string CRef(Type type) {
      return "T:" + type.FullName;
   }

   string CRef(MethodInfo method) {

      var crefBuilder = new StringBuilder();
      crefBuilder.AppendFormat("M:{0}.{1}", method.ReflectedType.FullName, method.Name);

      ParameterInfo[] parameters = method.GetParameters();
      string[] paramNames = parameters.Select(p => CRefId(p.ParameterType)).ToArray();

      if (paramNames.Length > 0) {
         crefBuilder.Append("(");
         crefBuilder.Append(String.Join(",", paramNames));
         crefBuilder.Append(")");
      }

      return crefBuilder.ToString();
   }

   string CRefId(Type type) {

      var sb = new StringBuilder();
      sb.Append(type.Namespace);
      sb.Append(".");
      sb.Append(type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name);

      if (type.IsGenericType) {
         sb.Append("{");

         foreach (var typeParam in type.GetGenericArguments()) 
            sb.Append(CRefId(typeParam));

         sb.Append("}");
      }

      return sb.ToString();
   }
}