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
using System.Xml.Xsl;
using System.Security;
using myxsl.net.common;

namespace myxsl.net.system {

   [Serializable]
   public sealed class SystemXsltException : ProcessorException {

      public SystemXsltException() 
         : base() { }

      public SystemXsltException(XsltException exception)
         : base(exception.Message, exception) {

         base.LineNumber = exception.LineNumber;

         if (!String.IsNullOrEmpty(exception.SourceUri))
            base.ModuleUri = new Uri(exception.SourceUri);
      }

      public SystemXsltException(string message)
         : base(message) { }

      public SystemXsltException(string message, Exception innerException)
         : base(message, innerException) { }

      private SystemXsltException(SerializationInfo info, StreamingContext context) 
         :base(info, context) { }
   }
}
