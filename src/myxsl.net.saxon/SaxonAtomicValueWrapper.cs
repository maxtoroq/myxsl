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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using Saxon.Api;

namespace myxsl.net.saxon {
   
   sealed class SaxonAtomicValueWrapper : XPathItem  {

      readonly XdmAtomicValue atomicValue;

      XmlSchemaType _XmlType;

      public XdmAtomicValue UnderlyingObject { get { return atomicValue; } }

      public override bool IsNode {
         get { return false; }
      }

      public override object TypedValue {
         get { return this.atomicValue.Value; }
      }

      public override string Value {
         get { return this.atomicValue.ToString(); }
      }

      public override bool ValueAsBoolean {
         get { return (bool)this.TypedValue; }
      }

      public override DateTime ValueAsDateTime {
         get { return (DateTime)this.TypedValue; }
      }

      public override double ValueAsDouble {
         get { return (double)this.TypedValue; }
      }

      public override int ValueAsInt {
         get { return (int)this.TypedValue; }
      }

      public override long ValueAsLong {
         get { return (long)this.TypedValue; }
      }

      public override Type ValueType {
         get { return this.TypedValue.GetType(); }
      }

      public override XmlSchemaType XmlType {
         get { 
            return _XmlType
               ?? (_XmlType = XmlSchemaType.GetBuiltInSimpleType(this.atomicValue.GetPrimitiveTypeName().ToXmlQualifiedName()));
         }
      }

      public SaxonAtomicValueWrapper(XdmAtomicValue atomicValue) {

         if (atomicValue == null) throw new ArgumentNullException("atomicValue");

         this.atomicValue = atomicValue;
      }

      public override object ValueAs(Type returnType, IXmlNamespaceResolver nsResolver) {
         throw new NotImplementedException();
      }
   }
}
