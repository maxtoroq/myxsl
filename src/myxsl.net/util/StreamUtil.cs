// StreamUtil.ReadFully written by 'Jon Skeet' (http://stackoverflow.com/a/221941/39923) 
// licensed under CC BY-SA 3.0 (http://creativecommons.org/licenses/by-sa/3.0/).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace myxsl.net {
   
   static class StreamUtil {

      public static byte[] ReadFully(Stream input) {
         
         byte[] buffer = new byte[16 * 1024];
         
         using (MemoryStream ms = new MemoryStream()) {
            
            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
               ms.Write(buffer, 0, read);
            }
            
            return ms.ToArray();
         }
      }
   }
}
