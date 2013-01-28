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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl.Runtime;

namespace myxsl.net.system {
   
   [CLSCompliant(false)]
   public static class ExtensionObjectConvert {

      // NOTE:
      // - Extension functions cannot accept or return null values
      // - Sequences can be represented by XPathNodeIterator or XPathNavigator[]
      // - Nodes can be represented as XPathNavigator or IXPathNavigable
      // - Numeric structs like Int32, Decimal, Double, etc. are allowed
      // - Other structs like Char, Guid, TimeSpan are not allowed
      // - DateTime is allowed (transformed from and to string)
      // - Enums are allowed (transformed from and to their base type)

      public static readonly XPathNodeIterator EmptyIterator = new EmptyXPathNodeIterator();

      public static bool IsEmpty(object value) {

         XPathNodeIterator iter;

         return value == null
            || ((iter = value as XPathNodeIterator) != null 
               && IsEmpty(iter));
      }

      public static bool IsEmpty(XPathNodeIterator value) {
         return value == null || value.Count == 0;
      }

      public static object ToInput(object value) {

         if (value == null) throw new ArgumentNullException("value");

         Type type = value.GetType();

         switch (Type.GetTypeCode(type)) {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.DateTime:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.String:
               return value;

            case TypeCode.Char:
               return ToInput((char)value);

            case TypeCode.DBNull:
            case TypeCode.Empty:
               throw new ArgumentException("value cannot be a null type.", "value");

            case TypeCode.Object:
            default:

               XPathItem item = value as XPathItem;

               if (item != null)
                  return ToInput(item);

               IXPathNavigable navigable = value as IXPathNavigable;

               if (navigable != null)
                  return ToInput(navigable);

               IEnumerable enumerable = value as IEnumerable;

               if (enumerable != null)
                  return ToInput(enumerable);

               if (typeof(Guid).IsInstanceOfType(value))
                  return ToInput((Guid)value);

               if (typeof(TimeSpan).IsInstanceOfType(value))
                  return ToInput((TimeSpan)value);

               return ToNode(value);
         }
      }

      public static Boolean ToInput(Boolean value) {
         return value;
      }

      public static Int16 ToInput(Int16 value) {
         return value;
      }

      public static Int32 ToInput(Int32 value) {
         return value;
      }

      public static Int64 ToInput(Int64 value) {
         return value;
      }

      public static Byte ToInput(Byte value) {
         return value;
      }

      public static SByte ToInput(SByte value) {
         return value;
      }

      public static UInt16 ToInput(UInt16 value) {
         return value;
      }

      public static UInt32 ToInput(UInt32 value) {
         return value;
      }

      public static UInt64 ToInput(UInt64 value) {
         return value;
      }

      public static Decimal ToInput(Decimal value) {
         return value;
      }

      public static Single ToInput(Single value) {
         return value;
      }

      public static Double ToInput(Double value) {
         return value;
      }

      public static DateTime ToInput(DateTime value) {
         return value;
      }

      public static string ToInput(String value) {

         if (value == null) throw new ArgumentNullException("value");

         return value;
      }

      public static string ToInput(Char value) {
         return value.ToString();
      }

      public static string ToInput(Guid value) {
         return value.ToString();
      }

      public static string ToInput(TimeSpan value) {
         return value.ToString();
      }

      public static object ToInput(XPathItem value) {

         if (value == null) throw new ArgumentNullException("value");

         if (value.IsNode)
            return value;
         
         return value.TypedValue;
      }

      public static XPathNavigator ToInput(XPathNavigator value) {

         if (value == null) throw new ArgumentNullException("value");

         return value;
      }

      public static XPathNavigator ToInput(IXPathNavigable value) {

         if (value == null) throw new ArgumentNullException("value");

         return value.CreateNavigator();
      }

      public static XPathNavigator[] ToInput(IEnumerable value) {

         if (value == null) throw new ArgumentNullException("value");

         return ToInput(
            ((IEnumerable)value).Cast<object>()
               .Where(o => !IsEmpty(o))
               .Select(o => ToXPathItem(ToInput(o)))
         );
      }

      public static XPathNavigator[] ToInput(IEnumerable<XPathItem> value) {

         if (value == null) throw new ArgumentNullException("value");

         return value
            .Where(i => !IsEmpty(i))
            .Select(i => ToNode(i))
            .ToArray();
      }

      public static XPathNavigator[] ToInput(IEnumerable<XPathNavigator> value) {

         if (value == null) throw new ArgumentNullException("value");

         return value.ToArray();
      }

      public static XPathNavigator[] ToInput(XPathNavigator[] value) {

         if (value == null) throw new ArgumentNullException("value");

         return value;
      }

      public static object ToInputOrEmpty(object value) {

         if (IsEmpty(value))
            return EmptyIterator;

         return ToInput(value);
      }

      public static object ToInputOrEmpty(String value) {
         return value ?? (object)EmptyIterator;
      }

      public static object ToInputOrEmpty<T>(Nullable<T> value) where T : struct {
         return (value.HasValue) ? ToInput(value.Value) : (object)EmptyIterator;
      }

      public static object ToInputOrEmpty(XPathItem value) {

         if (value == null)
            return EmptyIterator;

         return ToInput(value);
      }

      public static object ToInputOrEmpty(IXPathNavigable value) {
         return (value == null) ? EmptyIterator : (object)ToInput(value);
      }

      public static object ToInputOrEmpty(IEnumerable value) {

         IEnumerable<object> objects = (value != null) ? value.Cast<object>() : null;

         int count = (value == null) ? 0 : objects.Count();

         if (count == 0)
            return EmptyIterator;
         
         if (count == 1)
            return ToInputOrEmpty(objects.First());
         
         return ToInputOrEmpty(objects.Where(o => !IsEmpty(o)).Select(o => ToXPathItem(o)));
      }

      public static object ToInputOrEmpty(IEnumerable<XPathItem> value) {

         int count = (value == null) ? 0 : value.Count();

         if (count == 0)
            return EmptyIterator;
         
         if (count == 1)
            return ToInputOrEmpty(value.First());
         
         return ToInput(value);
      }

      public static XPathNavigator ToNode(object value) {

         if (value == null) throw new ArgumentNullException("value");

         XPathItem item = value as XPathItem;

         if (item != null)
            return ToNode(item);

         var itemFactory = new SystemItemFactory();

         return itemFactory.CreateDocument(value).CreateNavigator();
      }

      public static XPathNavigator ToNode(XPathItem value) {

         if (value == null) throw new ArgumentNullException("value");

         if (value.IsNode)
            return (XPathNavigator)value;

         return XsltConvert.ToNode(value);
      }

      public static XPathNavigator ToNode(XPathNavigator value) {

         if (value == null) throw new ArgumentNullException("value");

         return value;
      }

      public static XPathNavigator ToNode(IXPathNavigable value) {

         if (value == null) throw new ArgumentNullException("value");

         return value.CreateNavigator();
      }

      public static object ToNodeOrEmpty(object value) {

         if (IsEmpty(value))
            return EmptyIterator;

         return ToNode(value);
      }

      public static object ToNodeOrEmpty(XPathItem value) {

         if (value == null)
            return EmptyIterator;

         return ToNode(value);
      }

      public static object ToNodeOrEmpty(XPathNavigator value) {

         if (value == null)
            return EmptyIterator;

         return ToNode(value);
      }

      public static object ToNodeOrEmpty(IXPathNavigable value) {

         if (value == null)
            return EmptyIterator;

         return ToNode(value);
      }

      public static object FirstElementOrSelf(object value) {

         if (IsEmpty(value))
            return value;

         return FirstElementOrSelf((XPathNavigator)value);
      }

      public static XPathNavigator FirstElementOrSelf(XPathNavigator value) {

         if (value == null) throw new ArgumentNullException("value");

         if (value.NodeType == XPathNodeType.Element)
            return value;

         value = value.Clone();
         value.MoveToChild(XPathNodeType.Element);

         return value;
      }

      public static XPathItem ToXPathItem(object value) {

         if (value == null) throw new ArgumentNullException("value");

         Type type = value.GetType();

         switch (Type.GetTypeCode(type)) {
            case TypeCode.Boolean:
               return ToXPathItem((Boolean)value);
               
            case TypeCode.Byte:
               return ToXPathItem((Byte)value);
               
            case TypeCode.Char:
               return ToXPathItem((Char)value);
               
            case TypeCode.DateTime:
               return ToXPathItem((DateTime)value);
               
            case TypeCode.Decimal:
               return ToXPathItem((Decimal)value);
               
            case TypeCode.Double:
               return ToXPathItem((Double)value);
               
            case TypeCode.Int16:
               return ToXPathItem((Int16)value);
               
            case TypeCode.Int32:
               return ToXPathItem((Int32)value);
               
            case TypeCode.Int64:
               return ToXPathItem((Int64)value);
            
            case TypeCode.SByte:
               return ToXPathItem((SByte)value);
               
            case TypeCode.Single:
               return ToXPathItem((Single)value);
               
            case TypeCode.String:
               return ToXPathItem((String)value);

            case TypeCode.UInt16:
               return ToXPathItem((UInt16)value);
               
            case TypeCode.UInt32:
               return ToXPathItem((UInt32)value);
               
            case TypeCode.UInt64:
               return ToXPathItem((UInt64)value);

            case TypeCode.DBNull:
            case TypeCode.Empty:
               throw new ArgumentException("value cannot be a null type.", "value");

            case TypeCode.Object:
            default:

               XPathItem item = value as XPathItem;

               if (item != null)
                  return item;

               IXPathNavigable navigable = value as IXPathNavigable;

               if (navigable != null)
                  return navigable.CreateNavigator();

               XPathNodeIterator nodeIter = value as XPathNodeIterator;

               if (nodeIter != null)
                  return ToXPathItem(nodeIter);

               return ToNode(value);
         }
      }

      public static XPathItem ToXPathItem(Boolean value) {
         return SystemItemFactory.CreateBoolean(value);
      }

      public static XPathItem ToXPathItem(Int16 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Int32 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Int64 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Byte value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(SByte value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(UInt16 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(UInt32 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(UInt64 value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Decimal value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Single value) {
         return SystemItemFactory.CreateDouble((Double)value);
      }

      public static XPathItem ToXPathItem(Double value) {
         return SystemItemFactory.CreateDouble(value);
      }

      public static XPathItem ToXPathItem(DateTime value) {
         return SystemItemFactory.CreateDateTime(value);
      }

      public static XPathItem ToXPathItem(Char value) {
         return SystemItemFactory.CreateString(value.ToString());
      }

      public static XPathItem ToXPathItem(String value) {

         if (value == null) throw new ArgumentNullException("value");

         return SystemItemFactory.CreateString(value);
      }

      public static XPathItem ToXPathItem(XPathNodeIterator value) {
         return ToXPathNavigator(value);
      }

      public static IEnumerable<XPathItem> ToXPathItems(XPathNodeIterator value) {

         if (value == null)
            return Enumerable.Empty<XPathItem>();

         return value.Cast<XPathItem>();
      }

      public static XPathNavigator ToXPathNavigator(XPathNodeIterator value) {

         if (IsEmpty(value))
            return null;

         value.MoveNext();

         return value.Current;
      }

      public static IEnumerable<XPathNavigator> ToXPathNavigators(XPathNodeIterator value) {

         if (value == null)
            return Enumerable.Empty<XPathNavigator>();

         return value.Cast<XPathNavigator>();
      }

      public static string ToString(object value) {

         if (value == null)
            return null;

         string str = value as string;

         if (str != null)
            return str;

         XPathNodeIterator iter = value as XPathNodeIterator;

         if (iter != null) 
            return ToString(iter);

         return value.ToString();
      }

      public static string ToString(XPathNodeIterator value) {

         if (IsEmpty(value))
            return null;

         value.MoveNext();
         
         return value.Current.Value;
      }

      public static Nullable<T> ToNullableValueType<T>(object value) where T : struct {

         if (value == null)
            return null;

         if (value is T)
            return (T)value;

         XPathNodeIterator iter = value as XPathNodeIterator;

         if (iter != null) 
            return ToNullableValueType<T>(iter);

         throw new ArgumentException("value must be an instance of {0}.".FormatInvariant(typeof(T).FullName), "value");
      }

      public static Nullable<T> ToNullableValueType<T>(XPathNodeIterator value) where T : struct {

         if (IsEmpty(value))
            return null;

         value.MoveNext();

         return (T)value.Current.ValueAs(typeof(T));
      }

      public static Nullable<Double> ToNullableDouble(object value) {

         if (value == null)
            return null;

         if (value is double) 
            return (double)value;

         XPathNodeIterator iter = value as XPathNodeIterator;

         if (iter != null) {

            if (ExtensionObjectConvert.IsEmpty(iter))
               return null;

            return iter.Cast<XPathNavigator>().First().ValueAsDouble;
         } 

         return XmlConvert.ToDouble(value.ToString());
      }

      public static XmlQualifiedName ToXmlQualifiedName(string value) {

         if (value == null)
            return null;

         return new XmlQualifiedName(value);
      }

      public static XmlQualifiedName ToXmlQualifiedName(XPathNodeIterator value) {

         if (IsEmpty(value))
            return null;

         value.MoveNext();

         return ToXmlQualifiedName(value.Current.Value);
      }
   }
}
