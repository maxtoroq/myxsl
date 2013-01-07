// Copyright 2009 Max Toro Q.
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
using System.Xml;
using System.Xml.XPath;
using Saxon.Api;

namespace myxsl.net.saxon {

   sealed class SaxonNodeNavigator : XPathNavigator {

      XdmNode currentNode;
      IEnumerator currentSequence;

      XmlNameTable _NameTable;

      public override bool CanEdit {
         get { return false; }
      }

      public override string BaseURI {
         get { return currentNode.BaseUri.AbsoluteUri; }
      }

      public override string Name {
         get {
            if (currentNode.NodeName == null)
               return "";
            
            if (!String.IsNullOrEmpty(currentNode.NodeName.Prefix))
               return String.Concat(currentNode.NodeName.Prefix, ":", currentNode.NodeName.LocalName);

            return currentNode.NodeName.LocalName ?? ""; 
         }
      }

      public override string NamespaceURI {
         get {
            if (currentNode.NodeName == null)
               return "";

            return currentNode.NodeName.Uri ?? "";
         }
      }

      public override string Prefix {
         get {
            if (currentNode.NodeName == null)
               return null;

            return currentNode.NodeName.Prefix; 
         }
      }

      public override string Value {
         get { return currentNode.StringValue; }
      }

      public override XPathNodeType NodeType {
         get {
            switch (currentNode.NodeKind) {
               case XmlNodeType.Attribute:
                  return XPathNodeType.Attribute;

               // XdmNode.NodeKind returns None for namespace nodes
               case XmlNodeType.None:
                  if (currentSequence != null) 
                     return XPathNodeType.Namespace;
                  else 
                     return XPathNodeType.All;

               case XmlNodeType.Text:
               case XmlNodeType.CDATA:
                  return XPathNodeType.Text;
               
               case XmlNodeType.Comment:
                  return XPathNodeType.Comment;
               
               case XmlNodeType.Document:
                  return XPathNodeType.Root;

               case XmlNodeType.Element:
               case XmlNodeType.EndElement:
                  return XPathNodeType.Element;

               case XmlNodeType.ProcessingInstruction:
                  return XPathNodeType.ProcessingInstruction;

               case XmlNodeType.SignificantWhitespace:
                  return XPathNodeType.SignificantWhitespace;

               case XmlNodeType.Whitespace:
                  return XPathNodeType.Whitespace;

               case XmlNodeType.DocumentType:
               case XmlNodeType.XmlDeclaration:
               case XmlNodeType.DocumentFragment:
               case XmlNodeType.EndEntity:
               case XmlNodeType.Entity:
               case XmlNodeType.EntityReference:
               case XmlNodeType.Notation:
               default:
                  return XPathNodeType.All;
            }
         }
      }

      public override string LocalName {
         get {
            if (currentNode.NodeName == null)
               return "";

            return currentNode.NodeName.LocalName ?? ""; 
         }
      }

      public override string OuterXml {
         get { return currentNode.OuterXml; }
         set {
            if (!this.CanEdit)
               throw new InvalidOperationException("Cannot set OuterXml because this navigator does not support editing.");
            
            throw new NotImplementedException("set_OuterXml not implemented.");
         }
      }

      public override object UnderlyingObject {
         get { return currentNode; }
      }

      public override XmlNameTable NameTable {
         get {
            if (_NameTable == null)
               _NameTable = new NameTable();
            return _NameTable;
         }
      }

      public override bool IsEmptyElement {
         get {
            if (NodeType != XPathNodeType.Element)
               return false;

            return currentNode.EnumerateAxis(XdmAxis.Child).MoveNext();
         }
      }

      public SaxonNodeNavigator(XdmNode node) {
         
         if (node == null) throw new ArgumentNullException("node");

         this.currentNode = node;
      }

      public override XPathNavigator Clone() {
         return new SaxonNodeNavigator(this.currentNode);
      }

      public override string GetAttribute(string localName, string namespaceURI) {
         return this.currentNode.GetAttributeValue(new QName(namespaceURI, localName)) ?? "";
      }

      public override bool MoveToFirstChild() {

         switch (this.currentNode.NodeKind) {
            case XmlNodeType.Document:
            case XmlNodeType.DocumentFragment:
            case XmlNodeType.Element:

               IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.Child);

               if (en.MoveNext()) {
                  this.currentNode = (XdmNode)en.Current;
                  this.currentSequence = en;
                  return true;
               }

               break;
         }

         return false;
      }

      public override bool MoveToNext() {

         if (this.currentNode.NodeKind == XmlNodeType.Attribute)
            return false;

         if (this.currentSequence == null)
            this.currentSequence = this.currentNode.EnumerateAxis(XdmAxis.FollowingSibling);

         if (this.currentSequence.MoveNext()) {
            this.currentNode = (XdmNode)this.currentSequence.Current;
            return true;
         }

         return false;
      }

      public override bool MoveToPrevious() {

         if (this.currentNode.NodeKind == XmlNodeType.Attribute)
            return false;

         IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.PrecedingSibling);

         if (en.MoveNext()) {
            this.currentNode = (XdmNode)en.Current;
            this.currentSequence = null;
            return true;
         }

         return false;
      }

      public override bool MoveToParent() {

         IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.Parent);

         if (en.MoveNext()) {
            this.currentNode = (XdmNode)en.Current;
            this.currentSequence = null;
            return true;
         }

         return false;
      }

      public override bool MoveToFirstAttribute() {

         if (this.currentNode.NodeKind != XmlNodeType.Element)
            return false;

         IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.Attribute);

         if (en.MoveNext()) {
            this.currentNode = (XdmNode)en.Current;
            this.currentSequence = en;
            return true;
         }

         return false;
      }

      public override bool MoveToNextAttribute() {

         if (this.currentNode.NodeKind != XmlNodeType.Attribute)
            return false;

         if (this.currentSequence.MoveNext()) {
            this.currentNode = (XdmNode)this.currentSequence.Current;
            return true;
         }

         return false;
      }

      public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) {

         if (this.currentNode.NodeKind != XmlNodeType.Element)
            return false;

         IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.Namespace);

         while (en.MoveNext()) {
            XdmNode node = (XdmNode)en.Current;

            if (!NamespaceScopeOk(node, namespaceScope))
               continue;

            this.currentNode = node;
            this.currentSequence = en;
            return true;
         }

         return false;
      }

      public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) {

         // XdmNode.NodeKind returns None for namespace nodes
         if (this.currentNode.NodeKind != XmlNodeType.None)
            return false;

         while (this.currentSequence.MoveNext()) {
            XdmNode node = (XdmNode)this.currentSequence.Current;

            if (!NamespaceScopeOk(node, namespaceScope))
               continue;

            this.currentNode = node;
            return true;
         }

         // NOTE: if false the position of the XdmNavigator is changed,
         // which goes against the spec.

         return false;
      }

      static bool NamespaceScopeOk(XdmNode nsNode, XPathNamespaceScope namespaceScope) {
         
         switch (namespaceScope) {
            case XPathNamespaceScope.All:
               return true;

            case XPathNamespaceScope.ExcludeXml:
               return (nsNode.NodeName.LocalName != "xml");

            case XPathNamespaceScope.Local:
               
               if (nsNode.NodeName != null
                  && nsNode.NodeName.LocalName == "xml") {
                  
                  return false;

               } else {
                  
                  XdmNode parentNode = nsNode.Parent;

                  if (parentNode != null) {
                     IEnumerator parentScope = parentNode.EnumerateAxis(XdmAxis.Namespace);

                     while (parentScope.MoveNext()) {
                        XdmNode pNsNode = (XdmNode)parentScope.Current;

                        if (nsNode.StringValue == pNsNode.StringValue)
                           return false;
                     }
                  }

                  return true;
               }

            default:
               throw new ArgumentOutOfRangeException("namespaceScope");
         }
      }

      public override bool MoveTo(XPathNavigator other) {

         SaxonNodeNavigator navigator = other as SaxonNodeNavigator;

         if ((navigator != null) && navigator.currentNode.Equals(this.currentNode)) {
            this.currentNode = navigator.currentNode;
            this.currentSequence = null;
            return true;
         }
         return false;
      }

      public override bool MoveToId(string id) {
         throw new NotImplementedException("MoveToId not implemented.");
      }

      public override bool IsSamePosition(XPathNavigator other) {

         SaxonNodeNavigator navigator = other as SaxonNodeNavigator;

         if (navigator == null)
            return false;

         return navigator.currentNode.Equals(this.currentNode);
      }

      //public override void WriteSubtree(XmlWriter writer) {
      //   this.currentNode.WriteTo(writer);
      //}
   }
}
