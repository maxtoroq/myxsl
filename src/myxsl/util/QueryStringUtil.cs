// Copyright 2014 Max Toro Q.
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

#region QueryStringUtil is based on HttpUtility from Mono
// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace myxsl {

   static class QueryStringUtil {

      public static NameValueCollection ParseQueryString(string query) {
         return ParseQueryString(query, Encoding.UTF8);
      }

      public static NameValueCollection ParseQueryString(string query, Encoding encoding) {

         if (query == null) throw new ArgumentNullException("query");

         var result = new NameValueCollection();

         ParseQueryString(query, encoding, result);

         return result;
      }

      static void ParseQueryString(string query, Encoding encoding, NameValueCollection result) {
         if (query.Length == 0)
            return;

         //string decoded = HtmlDecode(query);
         string decoded = query;
         int decodedLength = decoded.Length;
         int namePos = 0;
         bool first = true;
         while (namePos <= decodedLength) {
            int valuePos = -1, valueEnd = -1;
            for (int q = namePos; q < decodedLength; q++) {
               if (valuePos == -1 && decoded[q] == '=') {
                  valuePos = q + 1;
               } else if (decoded[q] == '&') {
                  valueEnd = q;
                  break;
               }
            }

            if (first) {
               first = false;
               if (decoded[namePos] == '?')
                  namePos++;
            }

            string name, value;
            if (valuePos == -1) {
               name = null;
               valuePos = namePos;
            } else {
               name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1), encoding);
            }
            if (valueEnd < 0) {
               namePos = -1;
               valueEnd = decoded.Length;
            } else {
               namePos = valueEnd + 1;
            }
            value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos), encoding);

            result.Add(name, value);
            if (namePos == -1)
               break;
         }
      }

      public static string UrlDecode(string str) {
         return UrlDecode(str, Encoding.UTF8);
      }

      public static string UrlDecode(string s, Encoding e) {
         if (null == s)
            return null;

         if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
            return s;

         if (e == null)
            e = Encoding.UTF8;

         long len = s.Length;
         var bytes = new List<byte>();
         int xchar;
         char ch;

         for (int i = 0; i < len; i++) {
            ch = s[i];
            if (ch == '%' && i + 2 < len && s[i + 1] != '%') {
               if (s[i + 1] == 'u' && i + 5 < len) {
                  // unicode hex sequence
                  xchar = GetChar(s, i + 2, 4);
                  if (xchar != -1) {
                     WriteCharBytes(bytes, (char)xchar, e);
                     i += 5;
                  } else
                     WriteCharBytes(bytes, '%', e);
               } else if ((xchar = GetChar(s, i + 1, 2)) != -1) {
                  WriteCharBytes(bytes, (char)xchar, e);
                  i += 2;
               } else {
                  WriteCharBytes(bytes, '%', e);
               }
               continue;
            }

            if (ch == '+')
               WriteCharBytes(bytes, ' ', e);
            else
               WriteCharBytes(bytes, ch, e);
         }

         byte[] buf = bytes.ToArray();
         bytes = null;
         return e.GetString(buf);

      }

      static int GetChar(string str, int offset, int length) {
         int val = 0;
         int end = length + offset;
         for (int i = offset; i < end; i++) {
            char c = str[i];
            if (c > 127)
               return -1;

            int current = GetInt((byte)c);
            if (current == -1)
               return -1;
            val = (val << 4) + current;
         }

         return val;
      }

      static int GetInt(byte b) {
         char c = (char)b;
         if (c >= '0' && c <= '9')
            return c - '0';

         if (c >= 'a' && c <= 'f')
            return c - 'a' + 10;

         if (c >= 'A' && c <= 'F')
            return c - 'A' + 10;

         return -1;
      }

      static char[] GetChars(MemoryStream b, Encoding e) {
         return e.GetChars(b.GetBuffer(), 0, (int)b.Length);
      }

      static void WriteCharBytes(IList buf, char ch, Encoding e) {
         if (ch > 255) {
            foreach (byte b in e.GetBytes(new char[] { ch }))
               buf.Add(b);
         } else
            buf.Add((byte)ch);
      }
   }
}
