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
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using Saxon.Api;

namespace myxsl.net.saxon {

   [CLSCompliant(false)]
   public static class SaxonExtensions {

      #region XPathItem

      public static IXPathNavigable ToXPathNavigable(this XdmNode value) {
         return new SaxonNodeWrapper(value);
      }

      public static XPathNavigator ToXPathNavigator(this XdmNode value) {
         return ToXPathNavigable(value).CreateNavigator();
      }

      public static XPathItem ToXPathItem(this XdmItem value) {

         if (value == null) throw new ArgumentNullException("value");

         return (value.IsAtomic()) ?
            ToXPathItem((XdmAtomicValue)value)
            : ToXPathItem((XdmNode)value);
      }

      public static XPathItem ToXPathItem(this XdmAtomicValue value) {
         return new SaxonAtomicValueWrapper(value);
      }

      public static XPathItem ToXPathItem(this XdmNode value) {
         return ToXPathNavigator(value);
      }

      public static IEnumerable<XPathItem> ToXPathItems(this XdmValue value) {

         if (value == null)
            return Enumerable.Empty<XPathItem>();

         return ToXPathItems(value.Cast<XdmItem>());
      }

      public static IEnumerable<XPathItem> ToXPathItems(this IEnumerable<XdmItem> value) {

         if (value == null)
            return Enumerable.Empty<XPathItem>();

         return value.Select(i => (i.IsAtomic()) ?
            ToXPathItem((XdmAtomicValue)i) :
            ToXPathItem((XdmNode)i)
         );
      }

      #endregion

      #region Saxon helpers

      public static IEnumerable<XdmItem> AsItems(this IXdmEnumerator enumerator) {

         if (enumerator == null)
            yield break;

         while (enumerator.MoveNext()) 
            yield return (XdmItem)enumerator.Current;
      }

      public static IEnumerable<XdmNode> AsNodes(this IXdmEnumerator enumerator) {

         if (enumerator == null)
            yield break;

         while (enumerator.MoveNext())
            yield return (XdmNode)enumerator.Current;
      }

      public static IEnumerable<XdmAtomicValue> AsAtomicValues(this IXdmEnumerator enumerator) {

         if (enumerator == null)
            yield break;

         while (enumerator.MoveNext())
            yield return (XdmAtomicValue)enumerator.Current;
      }

      public static IXdmEnumerator GetXdmEnumerator(this XdmValue value) {

         if (value == null)
            return EmptyEnumerator.INSTANCE;

         return (IXdmEnumerator)value.GetEnumerator();
      }

      public static XdmValue ToXdmValue(this IEnumerable<XdmItem> value) {
         return new XdmValue(value);
      }

      public static T? SingleOrNull<T>(IEnumerable<T> source) where T : struct {

         // This method is used by generated integrated extension functions

         int count = source.Count();

         if (count == 0)
            return new T?();

         return source.Single();
      }

      #endregion

      #region XdmValue

      public static XdmValue ToXdmValue(this object value, SaxonItemFactory itemFactory) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         // Must check for string before checking for IEnumerable
         var str = value as string;

         if (str != null)
            return ToXdmAtomicValue(str);

         var xdmVal = value as XdmValue;

         if (xdmVal != null)
            return xdmVal;

         var item = value as XPathItem;

         if (item != null)
            return ToXdmItem(item, itemFactory);

         var nav = value as IXPathNavigable;

         if (nav != null)
            return ToXdmNode(nav, itemFactory);

         Type type = value.GetType();

         if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type)) 
            return ToXdmValue((IEnumerable)value, itemFactory);

         return ToXdmItem(value, itemFactory);
      }

      public static XdmValue ToXdmValue(this string value) {
         return (value != null) ? (XdmValue)ToXdmItem(value) : XdmEmptySequence.INSTANCE;
      }

      public static XdmValue ToXdmValue(this string value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Boolean value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Boolean value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Int16 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Int16 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Int32 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Int32 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Int64 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Int64 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Byte value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Byte value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value, itemFactory);
      }

      public static XdmValue ToXdmValue(this SByte value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this SByte value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this UInt16 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this UInt16 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this UInt32 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this UInt32 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this UInt64 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this UInt64 value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Single value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Single value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Double value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Double value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Decimal value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmValue ToXdmValue(this Decimal value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this Uri value) {
         return (value != null) ? (XdmValue)ToXdmItem(value) : XdmEmptySequence.INSTANCE;
      }

      public static XdmValue ToXdmValue(this Uri value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this QName value) {
         return (value != null) ? (XdmValue)ToXdmItem(value) : XdmEmptySequence.INSTANCE;
      }

      public static XdmValue ToXdmValue(this QName value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this XmlQualifiedName value) {
         return (value != null) ? (XdmValue)ToXdmItem(value) : XdmEmptySequence.INSTANCE;
      }

      public static XdmValue ToXdmValue(this XmlQualifiedName value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this IEnumerable<string> value) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         return new XdmValue(value.Select(s => ToXdmValue(s)));
      }

      public static XdmValue ToXdmValue(this IEnumerable<string> value, SaxonItemFactory itemFactory) {
         return ToXdmValue(value);
      }

      public static XdmValue ToXdmValue(this IEnumerable<XPathItem> value, SaxonItemFactory itemFactory) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         return new XdmValue(value.Select(i => ToXdmValue(i, itemFactory)));
      }

      public static XdmValue ToXdmValue(this IEnumerable value, SaxonItemFactory itemFactory) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         XdmValue result = XdmEmptySequence.INSTANCE;

         foreach (object item in value) 
            result = result.Append(ToXdmValue(item, itemFactory));

         return result;
      }

      public static XdmValue ToXdmValue(this XPathItem value, SaxonItemFactory itemFactory) {
         return (value != null) ? (XdmValue)ToXdmItem(value, itemFactory) : XdmEmptySequence.INSTANCE;
      }

      public static XdmValue ToXdmValue(this XPathNavigator value, SaxonItemFactory itemFactory) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         return ToXdmItem(value, itemFactory);
      }

      public static XdmValue ToXdmValue(this IXPathNavigable value, SaxonItemFactory itemFactory) {

         if (value == null)
            return XdmEmptySequence.INSTANCE;

         return ToXdmItem(value, itemFactory);
      }

      #endregion

      #region XdmItem

      public static XdmItem ToXdmItem(this object value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         var xdmItem = value as XdmItem;

         if (xdmItem != null)
            return xdmItem;

         var item = value as XPathItem;

         if (item != null)
            return ToXdmItem(item, itemFactory);

         var nav = value as IXPathNavigable;

         if (nav != null)
            return ToXdmNode(nav, itemFactory);

         Type type = value.GetType();
         TypeCode typeCode = Type.GetTypeCode(type);

         switch (typeCode) {
            case TypeCode.Boolean:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Char:
            case TypeCode.String:
            case TypeCode.DateTime:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
               return ToXdmAtomicValue(value, type, typeCode, itemFactory.processor);

            case TypeCode.DBNull:
            case TypeCode.Empty:
               throw new ArgumentException("value cannot be null or empty.", "value");

            default:
            case TypeCode.Object:
               break;
         }

         if (typeof(Uri).IsAssignableFrom(type))
            return ToXdmAtomicValue((Uri)value);

         if (typeof(XmlQualifiedName).IsAssignableFrom(type))
            return ToXdmAtomicValue((XmlQualifiedName)value);

         if (typeof(QName).IsAssignableFrom(type))
            return ToXdmAtomicValue((QName)value);

         return ToXdmNode(value, itemFactory);
      }

      public static XdmItem ToXdmItem(this string value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Boolean value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Int16 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Int32 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Int64 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Byte value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this SByte value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this UInt16 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this UInt32 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this UInt64 value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Single value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Double value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Decimal value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this Uri value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this QName value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this XmlQualifiedName value) {
         return ToXdmAtomicValue(value);
      }

      public static XdmItem ToXdmItem(this XPathItem value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         return (value.IsNode) ?
            (XdmItem)ToXdmNode((XPathNavigator)value, itemFactory)
            : ToXdmAtomicValue(value, itemFactory);
      }
      
      public static XdmItem ToXdmItem(this XPathNavigator value, SaxonItemFactory itemFactory) {
         return ToXdmNode(value, itemFactory);
      }

      public static XdmItem ToXdmItem(this IXPathNavigable value, SaxonItemFactory itemFactory) {
         return ToXdmNode(value, itemFactory);
      }
      
      #endregion

      #region XdmAtomicValue

      public static XdmAtomicValue ToXdmAtomicValue(this object value, SaxonItemFactory itemFactory) {
         return ToXdmAtomicValue(value, itemFactory.processor);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this object value, Processor processor) {

         if (value == null) throw new ArgumentNullException("value");

         Type type = value.GetType();

         return ToXdmAtomicValue(value, type, Type.GetTypeCode(type), processor);
      }

      static XdmAtomicValue ToXdmAtomicValue(this object value, Type type, TypeCode typeCode, Processor processor) {

         switch (Type.GetTypeCode(type)) {
            case TypeCode.Boolean:
               return ToXdmAtomicValue((Boolean)value);

            case TypeCode.Int16:
               return ToXdmAtomicValue((Int16)value);

            case TypeCode.Int32:
               return ToXdmAtomicValue((Int32)value);

            case TypeCode.Int64:
               return ToXdmAtomicValue((Int64)value);

            case TypeCode.Byte:
               return ToXdmAtomicValue((Byte)value);

            case TypeCode.SByte:
               return ToXdmAtomicValue((SByte)value);

            case TypeCode.UInt16:
               return ToXdmAtomicValue((UInt16)value);

            case TypeCode.UInt32:
               return ToXdmAtomicValue((UInt32)value);

            case TypeCode.UInt64:
               return ToXdmAtomicValue((UInt64)value);

            case TypeCode.Char:
            case TypeCode.String:
               return ToXdmAtomicValue(value.ToString());

            case TypeCode.DateTime:
               return new XdmAtomicValue(XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind), new QName(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime).QualifiedName), processor);

            case TypeCode.Decimal:
               return ToXdmAtomicValue((Decimal)value);

            case TypeCode.Double:
               return ToXdmAtomicValue((Double)value);

            case TypeCode.DBNull:
            case TypeCode.Empty:
               throw new ArgumentException("value cannot be null or empty.", "value");

            case TypeCode.Single:
               return ToXdmAtomicValue((Single)value);

            default:
            case TypeCode.Object:
               break;
         }

         if (typeof(XdmAtomicValue).IsAssignableFrom(type))
            return (XdmAtomicValue)value;

         if (typeof(XPathItem).IsAssignableFrom(type))
            return ToXdmAtomicValue((XPathItem)value, processor);

         if (typeof(Uri).IsAssignableFrom(type))
            return ToXdmAtomicValue((Uri)value);

         if (typeof(XmlQualifiedName).IsAssignableFrom(type))
            return ToXdmAtomicValue((XmlQualifiedName)value);

         if (typeof(QName).IsAssignableFrom(type))
            return ToXdmAtomicValue((QName)value);

         return ToXdmAtomicValue(value.ToString());
      }

      public static XdmAtomicValue ToXdmAtomicValue(this string value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Boolean value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Int16 value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Int32 value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Int64 value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Byte value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this SByte value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this UInt16 value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this UInt32 value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this UInt64 value) {
         return ToXdmAtomicValue((Int64)value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Single value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Double value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Decimal value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this Uri value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this QName value) {
         return new XdmAtomicValue(value);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this XmlQualifiedName value) {
         return ToXdmAtomicValue(new QName(value));
      }

      public static XdmAtomicValue ToXdmAtomicValue(this XPathItem value, SaxonItemFactory itemFactory) {
         return ToXdmAtomicValue(value, itemFactory.processor);
      }

      public static XdmAtomicValue ToXdmAtomicValue(this XPathItem value, Processor processor) {

         if (value == null) throw new ArgumentNullException("value");

         SaxonAtomicValueWrapper wrapper = value as SaxonAtomicValueWrapper;

         if (wrapper != null) {
            return wrapper.UnderlyingObject;
         } else {
            XmlQualifiedName typeName = value.XmlType.QualifiedName;

            try {
               return (!typeName.IsEmpty) ?
                  new XdmAtomicValue(value.Value, new QName(typeName), processor) :
                  new XdmAtomicValue(value.Value)
                  ;
            } catch (Exception ex) {
               throw new SaxonException(ex.Message, ex);
            }
         }
      }

      #endregion

      #region XdmNode

      public static XdmNode ToXdmNode(this object value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         XPathItem item = value as XPathItem;

         if (item != null)
            return ToXdmNode(item, itemFactory);
         
         return ToXdmNode(itemFactory.CreateDocument(value), itemFactory);
      }

      public static XdmNode ToXdmNode(this XPathItem value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         if (value.IsNode)
            return ToXdmNode((XPathNavigator)value, itemFactory);

         return ToXdmNode(itemFactory.CreateDocument(value.TypedValue), itemFactory);
      }

      public static XdmNode ToXdmNode(this IXPathNavigable value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         return ToXdmNode(value.CreateNavigator(), itemFactory);
      }

      public static XdmNode ToXdmNode(this IXPathNavigable value, DocumentBuilder documentBuilder) {

         if (value == null) throw new ArgumentNullException("value");

         return ToXdmNode(value.CreateNavigator(), documentBuilder);
      }

      public static XdmNode ToXdmNode(this XPathNavigator value, SaxonItemFactory itemFactory) {

         if (value == null) throw new ArgumentNullException("value");

         XdmNode node;

         if (!TryGetXdmNode(value, out node)) 
            return MoveToElementOrReturnUnchanged(value, itemFactory.CreateXdmNode(value.ReadSubtree()));

         return node;
      }

      public static XdmNode ToXdmNode(this XPathNavigator value, DocumentBuilder documentBuilder) {

         if (value == null) throw new ArgumentNullException("value");

         XdmNode node;

         if (!TryGetXdmNode(value, out node)) 
            return MoveToElementOrReturnUnchanged(value, documentBuilder.Build(value.ReadSubtree()));

         return node;
      }

      internal static bool TryGetXdmNode(XPathNavigator navigator, out XdmNode xdmNode) {

         if (navigator == null) throw new ArgumentNullException("navigator");

         xdmNode = navigator.UnderlyingObject as XdmNode;

         return (xdmNode != null);
      }

      static XdmNode MoveToElementOrReturnUnchanged(XPathNavigator navigator, XdmNode node) {

         // node is newly constructed document

         if (navigator.NodeType == XPathNodeType.Element) {
            return ((IXdmEnumerator)node.EnumerateAxis(XdmAxis.Child)).AsNodes().SingleOrDefault(n => n.NodeKind == XmlNodeType.Element)
               ?? node;
         }
         
         return node;
      }

      public static XdmValue FirstElementOrSelf(this XdmValue value) {

         if (value == null) throw new ArgumentNullException("value");

         var node = value as XdmNode;

         if (node == null)
            return value;

         return FirstElementOrSelf(node);
      }

      public static XdmNode FirstElementOrSelf(this XdmNode value) {

         if (value == null) throw new ArgumentNullException("value");

         if (value.NodeKind == XmlNodeType.Element)
            return value;

         return ((IXdmEnumerator)value.EnumerateAxis(XdmAxis.Child)).AsNodes().SingleOrDefault(n => n.NodeKind == XmlNodeType.Element)
            ?? value;
      }

      #endregion
   }
}
