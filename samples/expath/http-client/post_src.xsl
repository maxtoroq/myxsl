<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon" content-type="text/plain" ?>

<xsl:stylesheet version="2.0"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:http="http://expath.org/ns/http-client"
   xmlns:exsl="http://exslt.org/common"
   xmlns:request="http://maxtoroq.github.io/myxsl/web/request"
   exclude-result-prefixes="http exsl request">
   
   <xsl:output method="xml" indent="yes"/>

   <xsl:variable name="request" as="element()">
      <http:request method="POST" href="{concat(request:url('SchemeAndServer'), '/schematron/xslt2.xsl')}" 
         override-media-type="application/xml">
         <http:body src="src_input.txt" media-type="application/x-www-form-urlencoded"/>
      </http:request>
   </xsl:variable>

   <xsl:template match="/" name="main">
      <result-sequence>
         <xsl:copy-of select="http:send-request(exsl:node-set($request))"/>
      </result-sequence>
   </xsl:template>
   
</xsl:stylesheet>
