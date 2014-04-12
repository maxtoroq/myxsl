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
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Security;

namespace myxsl.net.common {

   [Serializable]
   public abstract class ProcessorException : Exception {

      public Uri ModuleUri { get; protected set; }
      public int LineNumber { get; protected set; }
      public XmlQualifiedName ErrorCode { get; protected set; }

      public string GetErrorCodeAsClarkName() {

         if (this.ErrorCode == null 
            || this.ErrorCode.IsEmpty) {

            return "";
         }

         if (String.IsNullOrEmpty(this.ErrorCode.Namespace)) {
            return this.ErrorCode.Name;
         }

         return String.Concat("{", this.ErrorCode.Namespace, "}", this.ErrorCode.Name);
      }

      protected ProcessorException() 
         : base() { }

      protected ProcessorException(string message) 
         : base(message) { }

      protected ProcessorException(string message, Exception innerException)
         : base(message, innerException) { }

      protected ProcessorException(SerializationInfo info, StreamingContext context)
         : base(info, context) { }

      [SecurityCritical]
      public override void GetObjectData(SerializationInfo info, StreamingContext context) {
         base.GetObjectData(info, context);
      }
   }
}
