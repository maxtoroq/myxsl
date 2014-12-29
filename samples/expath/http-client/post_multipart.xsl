<?xml version="1.0" encoding="utf-8"?>
<?page content-type="text/plain" ?>

<xsl:stylesheet version="2.1"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:http="http://expath.org/ns/http-client"
   xmlns:exsl="http://exslt.org/common"
   xmlns:request="http://maxtoroq.github.io/myxsl/web/request"
   exclude-result-prefixes="http exsl request">
   
   <xsl:output method="xml" indent="yes"/>

   <xsl:variable name="request" as="element()">
      <http:request method="POST" href="{concat(request:url('SchemeAndServer'), '/schematron/xslt2.xsl')}"
         override-media-type="application/xml">
         
         <http:multipart media-type="multipart/form-data">
            <http:header name="Content-Disposition" value='form-data; name="view-report"'/>
            <http:body media-type="text/plain">1</http:body>

            <http:header name="Content-Disposition" value='form-data; name="email"'/>
            <http:body media-type="text/plain">foo</http:body>
         </http:multipart>
      </http:request>
   </xsl:variable>

   <xsl:template match="/" name="main">
      <result-sequence>
         <xsl:copy-of select="http:send-request(exsl:node-set($request))"/>
      </result-sequence>
   </xsl:template>
   
</xsl:stylesheet>
