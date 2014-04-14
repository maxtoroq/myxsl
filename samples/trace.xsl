<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="fn request"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:request="http://myxsl.github.io/ns/web/request">

   <xsl:import href="layout.xslt"/>

   <xsl:template match="/" name="main">

      <xsl:message>test from xsl:message</xsl:message>

      <xsl:if test="function-available('fn:trace')">
         <xsl:if test="fn:trace('test from fn:trace', '')"/>
      </xsl:if>

      <xsl:call-template name="layout"/>
   </xsl:template>

   <xsl:template name="content">
      <h1>Tracing</h1>
      <p>
         You can use <code>&lt;xsl:message&gt;</code> to send information to the ASP.NET trace.
      </p>
      <p>
         You can also use the built-in <code>fn:trace()</code> function on XSLT 2.0 and XQuery processors.
      </p>
      <xsl:if test="request:is-local()">
         <p>
            <a href="trace.axd" rel="nofollow">See the ASP.NET trace</a>
         </p>
      </xsl:if>
   </xsl:template>

</xsl:stylesheet>
