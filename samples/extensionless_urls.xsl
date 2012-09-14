<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0"
   xmlns="http://www.w3.org/1999/xhtml"                
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:import href="layout.xslt"/>

   <xsl:template name="content">

      <h1>Extensionless URLs</h1>
      <p>To enable extensionless URLs:</p>
      <ol>
         <li>Create a class library (or use an existing one).</li>
         <li>
            Reference <code>System.Web.WebPages</code>.
         </li>
         <li>
            <span>Add the following code:</span>
            <pre>
using System;
using System.Web;
using System.Web.WebPages;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]

public static class PreApplicationStartCode {

   public static void Start() {
      WebPageHttpHandler.RegisterExtension("xsl");
      WebPageHttpHandler.RegisterExtension("xqy");
   }
}
            </pre>
         </li>
      </ol>

   </xsl:template>

</xsl:stylesheet>