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
using System.Runtime.Serialization;
using Saxon.Api;
using System.Security;
using myxsl.common;

namespace myxsl.saxon {

   [Serializable]
   public sealed class SaxonException : ProcessorException {

      public SaxonException() 
         : base() { }

      [CLSCompliant(false)]
      public SaxonException(StaticError error)
         : base(error.Message, error) {

         this.LineNumber = error.LineNumber;

         if (!String.IsNullOrEmpty(error.ModuleUri)) {
            this.ModuleUri = new Uri(error.ModuleUri);
         }

         if (error.ErrorCode != null) {
            this.ErrorCode = error.ErrorCode.ToXmlQualifiedName();
         }
      }

      [CLSCompliant(false)]
      public SaxonException(DynamicError error)
         : base(error.Message, error) {

         this.LineNumber = error.LineNumber;

         if (!String.IsNullOrEmpty(error.ModuleUri)) {
            this.ModuleUri = new Uri(error.ModuleUri);
         }

         if (error.ErrorCode != null) {
            this.ErrorCode = error.ErrorCode.ToXmlQualifiedName();
         }
      }

      public SaxonException(string message)
         : base(message) { }

      public SaxonException(string message, Exception innerException)
         : base(message, innerException) { }

      private SaxonException(SerializationInfo info, StreamingContext context)
         : base(info, context) { }
   }
}
