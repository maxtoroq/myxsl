<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" 
   xmlns="http://www.w3.org/1999/xhtml"                
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:import href="layout.xslt"/>

   <xsl:template name="content">

      <h1>Compilation</h1>
      <p>
         <strong>myxsl</strong> provides compile-time notification of <a href="http://www.w3.org/TR/xslt20/#dt-static-error">static errors</a> in Visual Studio,
         helping you detect bugs earlier in the development process. Just click
         'Build Website' (Ctrl+Shift+B) and watch the 'Error List' pane.
      </p>
      <p>
         <a href="Content/static-errors-vs-xslt.png">
            <img alt="" src="Content/static-errors-vs-xslt.png" style="width: 500px;" />
         </a>
      </p>
      <p>
         <a href="Content/static-errors-vs-xquery.png">
            <img alt="" src="Content/static-errors-vs-xquery.png" style="width: 500px;" />
         </a>
      </p>

   </xsl:template>
   
</xsl:stylesheet>