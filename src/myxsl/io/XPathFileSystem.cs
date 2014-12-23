// Copyright 2013 Max Toro Q.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using myxsl.common;

namespace myxsl.io {
   
   [XPathModule(Prefix, Namespace)]
   public class XPathFileSystem {

      internal const string Namespace = "http://expath.org/ns/file";
      internal const string Prefix = "file";

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      // File Properties

      [XPathFunction("exists", "xs:string", As = "xs:boolean")]
      public bool Exists(string path) {
         return IsFile(path) || IsDirectory(path);
      }

      [XPathFunction("is-dir", "xs:string", As = "xs:boolean")]
      public bool IsDirectory(string path) {
         return Directory.Exists(ResolvePath(path));
      }

      [XPathFunction("is-file", "xs:string", As = "xs:boolean")]
      public bool IsFile(string path) {
         
         string normalizedPath;

         return IsFile(path, out normalizedPath);
      }

      bool IsFile(string path, out string normalizedPath) {

         normalizedPath = ResolvePath(path);

         return File.Exists(normalizedPath);
      }

      [XPathFunction("last-modified", "xs:string", As = "xs:dateTime")]
      public DateTime LastModified(string path) {

         string normalizedPath;

         if (IsFile(path, out normalizedPath)) {
            return File.GetLastWriteTime(normalizedPath);
         }

         return Directory.GetLastWriteTime(normalizedPath);
      }

      [XPathFunction("size", "xs:string", As = "xs:integer")]
      public long Size(string file) {
         return new FileInfo(ResolvePath(file)).Length;
      }

      // Input/Output

      [XPathFunction("append-text", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void AppendText(string file, string value) {
         File.AppendAllText(ResolvePath(file), value);
      }

      [XPathFunction("append-text", "xs:string", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void AppendText(string file, string value, string encoding) {
         File.AppendAllText(ResolvePath(file), value, Encoding.GetEncoding(encoding));
      }

      [XPathFunction("append-text-lines", "xs:string", "xs:string*", As = "empty-sequence()", HasSideEffects = true)]
      public void AppendTextLines(string file, IEnumerable<string> lines) {
         File.AppendAllLines(ResolvePath(file), lines);
      }

      [XPathFunction("append-text-lines", "xs:string", "xs:string*", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void AppendTextLines(string file, IEnumerable<string> lines, string encoding) {
         File.AppendAllLines(ResolvePath(file), lines, Encoding.GetEncoding(encoding));
      }

      [XPathFunction("copy", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void Copy(string source, string target) {
         File.Copy(ResolvePath(source), ResolvePath(target), overwrite: true);
      }

      [XPathFunction("create-dir", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void CreateDirectory(string dir) {
         Directory.CreateDirectory(ResolvePath(dir));
      }

      [XPathFunction("delete", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void Delete(string path) {

         string normalizedPath;

         if (IsFile(path, out normalizedPath)) {
            File.Delete(normalizedPath);
         } else {
            Directory.Delete(normalizedPath);
         }
      }

      [XPathFunction("delete", "xs:string", "xs:boolean", As = "empty-sequence()", HasSideEffects = true)]
      public void Delete(string path, bool recursive) {
         Directory.Delete(ResolvePath(path), recursive);
      }

      [XPathFunction("list", "xs:string", As = "xs:string*")]
      public IEnumerable<string> List(string dir) {
         return List(dir, false);
      }

      [XPathFunction("list", "xs:string", "xs:boolean", As = "xs:string*")]
      public IEnumerable<string> List(string dir, bool recursive) {
         return List(dir, recursive, "*");
      }

      [XPathFunction("list", "xs:string", "xs:boolean", "xs:string", As = "xs:string*")]
      public IEnumerable<string> List(string dir, bool recursive, string pattern) {

         Uri dirUri = PathToUri(dir);

         foreach (string file in Directory.EnumerateFiles(dirUri.LocalPath, pattern, (recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {

            var fileUri = new Uri(file, UriKind.Absolute);

            Uri relativeUri = dirUri.MakeRelativeUri(fileUri);

            yield return relativeUri.OriginalString;
         }
      }

      [XPathFunction("move", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void Move(string source, string target) {

         string normalizedPath;

         if (IsFile(source, out normalizedPath)) {
            File.Move(normalizedPath, target);
         } else {
            Directory.Move(normalizedPath, target);
         }
      }

      [XPathFunction("read-text", "xs:string", As = "xs:string")]
      public string ReadText(string file) {
         return File.ReadAllText(ResolvePath(file));
      }

      [XPathFunction("read-text", "xs:string", "xs:string", As = "xs:string")]
      public string ReadText(string file, string encoding) {
         return File.ReadAllText(ResolvePath(file), Encoding.GetEncoding(encoding));
      }

      [XPathFunction("read-text-lines", "xs:string", As = "xs:string*")]
      public string[] ReadTextLines(string file) {
         return File.ReadAllLines(ResolvePath(file));
      }

      [XPathFunction("read-text-lines", "xs:string", "xs:string", As = "xs:string*")]
      public string[] ReadTextLines(string file, string encoding) {
         return File.ReadAllLines(ResolvePath(file), Encoding.GetEncoding(encoding));
      }

      [XPathFunction("write-text", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void WriteText(string file, string value) {
         File.WriteAllText(ResolvePath(file), value);
      }

      [XPathFunction("write-text", "xs:string", "xs:string", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void WriteText(string file, string value, string encoding) {
         File.WriteAllText(ResolvePath(file), value, Encoding.GetEncoding(encoding));
      }

      [XPathFunction("write-text-lines", "xs:string", "xs:string*", As = "empty-sequence()", HasSideEffects = true)]
      public void WriteTextLines(string file, IEnumerable<string> values) {
         File.WriteAllLines(ResolvePath(file), values);
      }

      [XPathFunction("write-text-lines", "xs:string", "xs:string*", "xs:string", As = "empty-sequence()", HasSideEffects = true)]
      public void WriteTextLines(string file, IEnumerable<string> values, string encoding) {
         File.WriteAllLines(ResolvePath(file), values, Encoding.GetEncoding(encoding));
      }

      // Paths

      [XPathFunction("path-to-uri", "xs:string", As = "xs:anyURI")]
      public Uri PathToUri(string path) {
         return this.Resolver.ResolveUri(null, path);
      }

      [XPathFunction("resolve-path", "xs:string", As = "xs:string")]
      public string ResolvePath(string path) {
         return PathToUri(path).LocalPath;
      }

      // System Properties

      [XPathFunction("dir-separator", As = "xs:string")]
      public string DirectorySeparator() {
         return Path.DirectorySeparatorChar.ToString();
      }

      [XPathFunction("path-separator", As = "xs:string")]
      public string PathSeparator() {
         return Path.PathSeparator.ToString();
      }

      [XPathFunction("line-separator", As = "xs:string")]
      public string LineSeparator() {
         return Environment.NewLine;
      }
   }
}
