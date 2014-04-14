<?xml version="1.0" encoding="utf-8" ?>
<?page processor="saxon" ?>
<?output-cache duration="10" vary-by-param="none" ?>

<xsl:stylesheet version="2.0"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:import href="layout.xslt"/>

   <xsl:template name="content">

      <h1>Output Caching</h1>
      <p>
         Output Caching is an ASP.NET feature, <strong>myxsl</strong> does not change or extend it
         in any way. You can configure output caching using the <code>output-cache</code>
         processing instruction in your XSLT stylesheets, which is analogous to the
         <code>@OutputCache</code> directive in .aspx
      </p>

      <p>This page uses the following output-cache processing instruction:</p>
      <pre id="current-output-cache">
         <xsl:value-of select="'&lt;?output-cache', document('')/processing-instruction(output-cache)/string(), '?&gt;'"/>
      </pre>

      <p>
         If you refresh this page you should see the following time
         change every 10 seconds:
      </p>
      <p>
         <xsl:value-of select="current-dateTime()"/>
      </p>

      <h2>Available Parameters</h2>
      <ul>
         <li>
            <strong>duration (Required)</strong>
         </li>
         <li>
            <strong>vary-by-param (Required)</strong>
         </li>
         <li>location</li>
         <li>no-store</li>
         <li>cache-profile</li>
         <li>vary-by-header</li>
         <li>vary-by-custom</li>
         <li>vary-by-content-encodings</li>
      </ul>

      <p>
         Visit <a href="http://msdn.microsoft.com/en-us/library/hdxfb6cy.aspx">this page</a>
         for more information on output cache parameters.
      </p>

   </xsl:template>

</xsl:stylesheet>
