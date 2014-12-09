using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;
using myxsl;

[assembly: AssemblyTitle("myxsl.web.dll")]
[assembly: AssemblyDescription("myxsl.web.dll")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

[assembly: PreApplicationStartMethod(typeof(myxsl.web.PreApplicationStartCode), "Start")]
[assembly: XPathModuleExport]