using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;

[assembly: AssemblyTitle("myxsl.net.dll")]
[assembly: AssemblyDescription("myxsl.net.dll")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]

// SystemItemFactory needs this to create XmlAtomicValue instances
[assembly: SecurityRules(SecurityRuleSet.Level1)]

[assembly: PreApplicationStartMethod(typeof(myxsl.net.web.PreApplicationStartCode), "Start")]