// Copyright 2012 Max Toro Q.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace myxsl.common {
   
   public sealed class XPathItemType {

      readonly Type _ClrType;

      XPathItemKind? _Kind;
      string atomicTypeOrNodeNamePrefix;
      string schemaTypeNamePrefix;

      public Type ClrType { get { return _ClrType; } }

      public XPathItemKind Kind {
         get {
            if (_Kind == null) {

               Type type = ClrType;

               var kindTemp = XPathItemKind.AnyItem;

               if (type.IsPrimitive
                  || type.IsEnum
                  || type == typeof(string)) {
                  
                  kindTemp = XPathItemKind.Atomic;
               
               } else {

                  if (typeof(IXPathNavigable).IsAssignableFrom(type)) {
                     kindTemp = XPathItemKind.AnyNode;
                  }
               }

               _Kind = kindTemp;
            }
            return _Kind.Value;
         }
         private set {
            _Kind = value;
         }
      }

      public bool KindIsNode {
         get {
            switch (Kind) {
               default:
               case XPathItemKind.AnyItem:
               case XPathItemKind.Atomic:
                  return false;

               case XPathItemKind.AnyNode:
               case XPathItemKind.Attribute:
               case XPathItemKind.SchemaAttribute:
               case XPathItemKind.Comment:
               case XPathItemKind.Document:
               case XPathItemKind.Element:
               case XPathItemKind.SchemaElement:
               case XPathItemKind.ProcessingInstruction:
               case XPathItemKind.Text:
                  return true;
            }
         }
      }

      public XmlQualifiedName AtomicTypeOrNodeName { get; private set; }

      public XmlQualifiedName SchemaTypeName { get; private set; }

      internal XPathItemType(Type clrType, string lexicalItemType, IDictionary<string, string> namespacesInScope) {

         if (clrType == null) throw new ArgumentNullException("clrType");

         this._ClrType = clrType;

         if (lexicalItemType != null) {
            ParseItemType(lexicalItemType, namespacesInScope);
         }
      }

      void ParseItemType(string itemType, IDictionary<string, string> namespacesInScope) {

         if (!itemType.Contains("(")) {
            this.Kind = XPathItemKind.Atomic;
            this.AtomicTypeOrNodeName = ParseQName(itemType, namespacesInScope, out this.atomicTypeOrNodeNamePrefix);

            return;
         }

         string[] parts = itemType.Split(new[] { '(', ',', ')', '"' }, StringSplitOptions.RemoveEmptyEntries);

         XPathItemKind? itemKind = ParseItemKind(parts[0]);

         if (itemKind == null) {
            return;
         }

         this.Kind = itemKind.Value;

         switch (itemKind.Value) {
            case XPathItemKind.Attribute:
            case XPathItemKind.Element:

               if (parts.Length > 1) {

                  if (parts[1] != "*") {
                     this.AtomicTypeOrNodeName = ParseQName(parts[1], namespacesInScope, out this.atomicTypeOrNodeNamePrefix);
                  }

                  if (parts.Length > 2) {
                     this.SchemaTypeName = ParseQName(parts[2], namespacesInScope, out this.schemaTypeNamePrefix);
                  }
               }

               break;

            case XPathItemKind.SchemaAttribute:
            case XPathItemKind.SchemaElement:

               if (parts.Length > 1) {

                  this.AtomicTypeOrNodeName = ParseQName(parts[1], namespacesInScope, out this.atomicTypeOrNodeNamePrefix);

                  if (parts.Length > 2) {
                     this.SchemaTypeName = ParseQName(parts[2], namespacesInScope, out this.schemaTypeNamePrefix);
                  }
               }

               break;

            case XPathItemKind.Document:

               if (parts.Length > 2) {

                  // document-node(element(NodeName, SchemaName))
                  if (parts[2] != "*") {
                     this.AtomicTypeOrNodeName = ParseQName(parts[2], namespacesInScope, out this.atomicTypeOrNodeNamePrefix);
                  }

                  if (parts.Length > 3)
                     this.SchemaTypeName = ParseQName(parts[3], namespacesInScope, out this.schemaTypeNamePrefix);
               }

               break;

            case XPathItemKind.ProcessingInstruction:

               if (parts.Length > 1) {
                  this.AtomicTypeOrNodeName = new XmlQualifiedName(parts[1]);
               }

               break;

            default:
               break;
         }
      }

      static XmlQualifiedName ParseQName(string qname, IDictionary<string, string> namespacesInScope, out string prefix) {

         string[] parts = qname.Split(':');

         prefix = parts[0];
         string local = parts[1];

         if (namespacesInScope == null) {
            throw new ArgumentNullException("namespacesInScope", "namespacesInScope is needed to resolve the ItemType.");
         }

         string ns;

         if (!namespacesInScope.TryGetValue(prefix, out ns)) {
            throw new ArgumentException("namespacesInScope does not contain a mapping for prefix '{0}'.".FormatInvariant(prefix), "namespacesInScope");
         }

         return new XmlQualifiedName(local, ns);
      }

      static XPathItemKind? ParseItemKind(string itemType) {

         switch (itemType) {
            case "item":
               return XPathItemKind.AnyItem;

            case "node":
               return XPathItemKind.AnyNode;

            case "attribute":
               return XPathItemKind.Attribute;

            case "schema-attribute":
               return XPathItemKind.SchemaAttribute;

            case "comment":
               return XPathItemKind.Comment;

            case "document-node":
               return XPathItemKind.Document;

            case "element":
               return XPathItemKind.Element;

            case "schema-element":
               return XPathItemKind.SchemaElement;

            case "processing-instruction":
               return XPathItemKind.ProcessingInstruction;

            case "text":
               return XPathItemKind.Text;

            default:
               break;
         }

         return null;
      }

      public override string ToString() {

         switch (this.Kind) {
            case XPathItemKind.AnyItem:
               return "item()";
               
            case XPathItemKind.Atomic:

               if (this.atomicTypeOrNodeNamePrefix != null
                  && this.AtomicTypeOrNodeName != null) {

                  return this.atomicTypeOrNodeNamePrefix + ":" + this.AtomicTypeOrNodeName.Name;
               }

               return this.ClrType.FullName;
            
            case XPathItemKind.AnyNode:
               return "node()";
               
            case XPathItemKind.Attribute:
               return "attribute({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair());
               
            case XPathItemKind.SchemaAttribute:
               return "schema-attribute({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair());

            case XPathItemKind.Comment:
               return "comment()";
               
            case XPathItemKind.Document:
               return "document-node({0})".FormatInvariant(
                  (this.AtomicTypeOrNodeName != null || this.SchemaTypeName != null) ?
                     "element({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair())
                     : ""
               );
            
            case XPathItemKind.Element:
               return "element({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair());
            
            case XPathItemKind.SchemaElement:
               return "schema-element({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair());
            
            case XPathItemKind.ProcessingInstruction:
               return "processing-instruction({0})".FormatInvariant(GetNodeAndSchemaTypeNamePair());
               
            case XPathItemKind.Text:
               return "text()";
               
            default:
               throw new InvalidOperationException();
         }
      }

      string GetNodeAndSchemaTypeNamePair() {

         if (this.AtomicTypeOrNodeName == null
            && this.SchemaTypeName == null) {

            return "";
         }

         StringBuilder sb = new StringBuilder();

         if (this.AtomicTypeOrNodeName != null) {

            if (this.atomicTypeOrNodeNamePrefix != null) { 
               sb.Append(this.atomicTypeOrNodeNamePrefix)
                  .Append(":");
            }

            sb.Append(this.AtomicTypeOrNodeName.Name);
         
         } else {
            sb.Append("*");
         }

         if (this.SchemaTypeName != null) {

            sb.Append(", ")
               .Append(this.schemaTypeNamePrefix)
               .Append(":")
               .Append(this.SchemaTypeName.Name);
         }

         return sb.ToString();
      }
   }
}
