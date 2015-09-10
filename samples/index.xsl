<?xml version="1.0" encoding="utf-8" ?>
<?output-cache duration="60" vary-by-param="none" ?>

<xsl:stylesheet version="2.1" exclude-result-prefixes="fn web code"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:web="http://maxtoroq.github.io/myxsl/web"
   xmlns:code="http://maxtoroq.github.io/myxsl/code">

   <xsl:import href="layout.xslt"/>

   <xsl:param name="function-library" as="document-node(element(library))" web:bind="code:FunctionLibrary.Instance" />

   <xsl:variable name="title" select="''"/>
   <xsl:variable name="title-mode" select="'append'"/>
   
   <xsl:template name="content">

      <p>Visit the <a href="http://maxtoroq.github.io/myxsl/">project site</a> for the most recent information.</p>
      
      <h2>Features</h2>
      <p>
         This project is still on alpha stage. Some features are more stable than others.
      </p>
      <ul>
         <li>
            <h3>ASP.NET integration</h3>
            <ul>
               <li>
                  <a href="/compilation.xsl">Compilation / Static errors detection</a>
               </li>
               <li>
                  <a href="/params.xsl">Parameter binding (XSLT only)</a>
               </li>
               <li>
                  <a href="/outputcache.xsl">Output caching (XSLT only)</a>
               </li>
               <li>
                  <a href="/trace.xsl">Tracing</a>
               </li>
               <li>
                  <a href="/mvc">MVC</a>
               </li>
               <li>
                  <a href="/simplified.xhtml">Simplified stylesheet</a>
               </li>
            </ul>
         </li>
         <li>
            <h3>Other XML standards</h3>
            <ul>
               <li>
                  <a href="/schematron/">Schematron</a>
               </li>
               <li>
                  <a href="/xml-stylesheet.atom">
                     <code>xml-stylesheet</code>
                  </a>
               </li>
            </ul>
         </li>
         <li>
            <h3>Functions</h3>
            <ul>
               <li>
                  <a href="/xslcompiledtransform/extensions/">XslCompiledTransform Extensions</a>
               </li>
               <li>
                  <a href="/saxon/extensions/">Saxon Extensions</a>
               </li>
            </ul>
            <br/>
            <ul>
               <xsl:for-each select="$function-library/*/module">
                  <xsl:sort select="@namespace"/>

                  <li>
                     <a href="{concat('/modules.xsl/', substring-after(@namespace, 'http://'))}">
                        <xsl:value-of select="@namespace"/>
                     </a>
                  </li>
               </xsl:for-each>
            </ul>
         </li>
      </ul>
   
   </xsl:template>

</xsl:stylesheet>
