<?xml version="1.0" encoding="utf-8" ?>
<?output-cache duration="60" vary-by-param="none" ?>

<xsl:stylesheet version="1.0" 
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:import href="~/layout.xslt"/>

   <xsl:template name="content">

      <h1>EXPath HTTP Client Module</h1>
      <p>
         <strong>myxsl</strong> provides an implementation of the <a href="http://www.expath.org/modules/http-client/">EXPath HTTP Client module</a>.
      </p>

      <h2>Examples</h2>
      <ul id="examples">
         <li>
            <a href="/expath/http-client/post_basic.xsl">POST Basic</a>
         </li>
         <li>
            <a href="/expath/http-client/post_src.xsl">POST using @src</a>
         </li>
         <li>
            <a href="/expath/http-client/post_multipart.xsl">POST Multipart</a>
         </li>
      </ul>

      <h2>See Also</h2>
      <ul>
         <li>
            <a href="/modules.xsl/expath.org/ns/http-client">Module documentation</a>
         </li>
      </ul>

   </xsl:template>
     
</xsl:stylesheet>
