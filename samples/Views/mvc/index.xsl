<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"  
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   
   <xsl:import href="~/layout.xslt"/>

   <xsl:param name="generated-at"/>

   <xsl:template name="content">
      
      <h1>ASP.NET MVC</h1>
      <p>
         <strong>myxsl</strong> integrates with <a href="http://www.asp.net/mvc">ASP.NET MVC</a>
         by implementing view engines for XSLT and XQuery. The ViewData dictionary items are passed
         as XSLT global parameters and XQuery external variables, and the model object is converted
         to XML and used as initial context node.
      </p>
      <p>
         This page is handled by a controller and rendered by an XSLT-powered view engine.
      </p>
      <h2>Download using NuGet</h2>
      <ul>
         <li>
            <a href="http://www.nuget.org/packages/XsltViewEngine">XsltViewEngine</a>
         </li>
         <li>
            <a href="http://www.nuget.org/packages/SaxonViewEngine">SaxonViewEngine</a>
         </li>
      </ul>

      <xsl:comment>
         <xsl:value-of select="$generated-at"/>
      </xsl:comment>
      
   </xsl:template>
   
</xsl:stylesheet>