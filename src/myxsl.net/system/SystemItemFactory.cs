// Copyright 2010 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net.system {
   
   public sealed class SystemItemFactory : XPathItemFactory {

      static readonly Type atomicValueType;
      static readonly FieldInfo xmlTypeField, clrTypeField,
         objValField, unionValField, boolValField, dtValField, 
         dblValField, i32ValField, i64ValField;

      static SystemItemFactory() {
         
         atomicValueType = typeof(XmlAtomicValue);

         if (!CLR.IsMono) {
            xmlTypeField = atomicValueType.GetField("xmlType", BindingFlags.Instance | BindingFlags.NonPublic);
            clrTypeField = atomicValueType.GetField("clrType", BindingFlags.Instance | BindingFlags.NonPublic);
            objValField = atomicValueType.GetField("objVal", BindingFlags.Instance | BindingFlags.NonPublic);
            unionValField = atomicValueType.GetField("unionVal", BindingFlags.Instance | BindingFlags.NonPublic);
            boolValField = unionValField.FieldType.GetField("boolVal", BindingFlags.Instance | BindingFlags.Public);
            dtValField = unionValField.FieldType.GetField("dtVal", BindingFlags.Instance | BindingFlags.Public);
            dblValField = unionValField.FieldType.GetField("dblVal", BindingFlags.Instance | BindingFlags.Public);
            i32ValField = unionValField.FieldType.GetField("i32Val", BindingFlags.Instance | BindingFlags.Public);
            i64ValField = unionValField.FieldType.GetField("i64Val", BindingFlags.Instance | BindingFlags.Public);
         }
      }

      public static XPathItem CreateBoolean(Boolean value) {

         TypeCode clrType = TypeCode.Boolean;

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.Boolean);
         clrTypeField.SetValue(instance, clrType);

         object unionVal = unionValField.GetValue(instance);

         boolValField.SetValue(unionVal, value);
         unionValField.SetValue(instance, unionVal);

         return instance;
      }

      public static XPathItem CreateDateTime(DateTime value) {

         TypeCode clrType = TypeCode.DateTime;

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.DateTime);
         clrTypeField.SetValue(instance, clrType);

         object unionVal = unionValField.GetValue(instance);

         dtValField.SetValue(unionVal, value);
         unionValField.SetValue(instance, unionVal);

         return instance;
      }

      public static XPathItem CreateDouble(Double value) {

         TypeCode clrType = TypeCode.Double;

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.Double);
         clrTypeField.SetValue(instance, clrType);

         object unionVal = unionValField.GetValue(instance);

         dblValField.SetValue(unionVal, value);
         unionValField.SetValue(instance, unionVal);

         return instance;
      }

      public static XPathItem CreateInt(Int32 value) {

         TypeCode clrType = TypeCode.Int32;

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.Int);
         clrTypeField.SetValue(instance, clrType);

         object unionVal = unionValField.GetValue(instance);

         i32ValField.SetValue(unionVal, value);
         unionValField.SetValue(instance, unionVal);

         return instance;
      }

      public static XPathItem CreateLong(Int64 value) {

         TypeCode clrType = TypeCode.Int64;

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.Long);
         clrTypeField.SetValue(instance, clrType);

         object unionVal = unionValField.GetValue(instance);

         i64ValField.SetValue(unionVal, value);
         unionValField.SetValue(instance, unionVal);

         return instance;
      }

      public static XPathItem CreateString(String value) {

         if (value == null) throw new ArgumentNullException("value");

         XmlAtomicValue instance = CreateInstance(XmlTypeCode.String);
         objValField.SetValue(instance, value);

         return instance;
      }

      static XmlAtomicValue CreateInstance(XmlTypeCode typeCode) {

         XmlSchemaType xmlType = XmlSchemaType.GetBuiltInSimpleType(typeCode);

         return CreateInstance(xmlType);
      }

      static XmlAtomicValue CreateInstance(XmlQualifiedName qualifiedName) {

         XmlSchemaType xmlType = XmlSchemaType.GetBuiltInSimpleType(qualifiedName);

         return CreateInstance(xmlType);
      }

      static XmlAtomicValue CreateInstance(XmlSchemaType xmlType) {

         XmlAtomicValue instance = (XmlAtomicValue)FormatterServices.GetUninitializedObject(atomicValueType);
         xmlTypeField.SetValue(instance, xmlType);

         return instance;
      }
   
      public override XPathItem CreateAtomicValue(object value, XmlQualifiedName qualifiedName) {
         
         if (value == null) throw new ArgumentNullException("value");

         XmlAtomicValue instance = CreateInstance(qualifiedName);
         objValField.SetValue(instance, value);

         return instance;
      }

      public override IXPathNavigable CreateNodeReadOnly(Stream input, XmlParsingOptions options) {
         
         options = options ?? new XmlParsingOptions();
         
         XmlReaderSettings settings = (XmlReaderSettings)options;
         
         XmlReader reader;

         if (options.BaseUri != null)
            reader = XmlReader.Create(input, settings, options.BaseUri.AbsoluteUri);
         else
            reader = XmlReader.Create(input, settings);

         return CreateNodeReadOnly(reader);
      }

      public override IXPathNavigable CreateNodeReadOnly(TextReader input, XmlParsingOptions options) {

         options = options ?? new XmlParsingOptions();

         XmlReaderSettings settings = (XmlReaderSettings)options;

         XmlReader reader;

         if (options.BaseUri != null)
            reader = XmlReader.Create(input, settings, options.BaseUri.AbsoluteUri);
         else
            reader = XmlReader.Create(input, settings);

         return CreateNodeReadOnly(reader);
      }

      public override IXPathNavigable CreateNodeReadOnly(XmlReader reader) {
         return new XPathDocument(reader);
      }
   }
}
