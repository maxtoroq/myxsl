<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" 
   xmlns="http://www.w3.org/1999/xhtml"                
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:import href="layout.xslt"/>

   <xsl:variable name="xslt-screenshot" select="'https://lh5.googleusercontent.com/-xUV1XyueQCA/UFNv04cma-I/AAAAAAAAAps/dEcUQYIeE3E/s800/static_error_detection1.png'"/>
   <xsl:variable name="xquery-screenshot" select="'https://lh6.googleusercontent.com/-82Iio-KPZPY/UFNv09VbsgI/AAAAAAAAApw/j8AazUeKkUE/s800/static_error_detection2.png'"/>

   <xsl:template name="content">

      <h1>Compilation</h1>
      <p>
         <strong>myxsl</strong> provides compile-time notificacion of <a href="http://www.w3.org/TR/xslt20/#dt-static-error">static errors</a> on Visual Studio,
         helping you detect bugs earlier in the development process. Just click
         'Build Website' (Ctrl+Shift+B) and watch the 'Error List' pane.
      </p>
      <p>
         <a href="{$xslt-screenshot}">
            <img alt="" src="{$xslt-screenshot}" style="width: 500px;" />
         </a>
      </p>
      <p>
         <a href="{$xquery-screenshot}">
            <img alt="" src="{$xquery-screenshot}" style="width: 500px;" />
         </a>
      </p>

   </xsl:template>
   
</xsl:stylesheet>