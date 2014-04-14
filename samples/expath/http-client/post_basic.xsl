<?xml version="1.0" encoding="utf-8"?>
<?page content-type="text/plain" ?>

<xsl:stylesheet version="2.1"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:http="http://expath.org/ns/http-client"
   xmlns:exsl="http://exslt.org/common"
   xmlns:request="http://myxsl.github.io/ns/web/request"
   exclude-result-prefixes="xs http exsl request">
   
   <xsl:output method="xml" indent="yes"/>

   <xsl:param name="override-media-type" select="'application/xml'" as="xs:string" request:bind="query"/>

   <xsl:variable name="request" as="element()">
      <http:request method="POST" href="{concat(request:url('SchemeAndServer'), '/schematron/xslt2.xsl')}" 
         override-media-type="{$override-media-type}">
         <http:body media-type="application/x-www-form-urlencoded" method="text">
            <xsl:text>view-report=1&amp;email=foo</xsl:text>
         </http:body>
      </http:request>
   </xsl:variable>

   <xsl:template match="/" name="main">
      <result-sequence>
         <xsl:copy-of select="http:send-request(exsl:node-set($request))"/>
      </result-sequence>
   </xsl:template>
   
</xsl:stylesheet>
