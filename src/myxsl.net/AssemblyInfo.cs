using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;

[assembly: AssemblyTitle("myxsl.dll")]
[assembly: AssemblyDescription("myxsl.dll")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]

// SystemItemFactory needs this to create XmlAtomicValue instances
[assembly: SecurityRules(SecurityRuleSet.Level1)]

[assembly: PreApplicationStartMethod(typeof(myxsl.web.PreApplicationStartCode), "Start")]