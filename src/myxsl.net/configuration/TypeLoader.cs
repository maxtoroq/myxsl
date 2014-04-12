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
using System.Configuration;
using System.Globalization;
using System.Web.Compilation;

namespace myxsl.net.configuration {
   
   static class TypeLoader {

      public static Type LoadType(string typeName, Type requiredBaseType, ConfigurationElement configElement, string propertyName) {

         Type type;

         try {
            type = BuildManager.GetType(typeName, true, false);

         } catch (Exception ex) {

            if (configElement != null) {
               throw new ConfigurationErrorsException(ex.Message, ex, configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
            
            throw new ConfigurationErrorsException(ex.Message, ex);
         }

         CheckAssignableType(requiredBaseType, type, configElement, propertyName);

         return type;
      }

      static void CheckAssignableType(Type baseType, Type type, ConfigurationElement configElement, string propertyName) {

         if (!baseType.IsAssignableFrom(type)) {
            throw new ConfigurationErrorsException(String.Format(CultureInfo.InvariantCulture, "Type {0} doesn't inherit from type {1}.", type.FullName, baseType.FullName), configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
         }
      }
   }
}
