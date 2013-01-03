<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon"?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>

   <xsl:template name="content">
      
      <h1>Saxon Extensions</h1>
      <p>The following are extension functions specifically targeted to the Saxon-HE processor.</p>
      <ul>
         <li>
            <a href="exslt.xsl">EXSLT</a>
         </li>
         <li>
            <a href="saxon.xsl">Saxon Extensions</a>
         </li>
         <li>
            <a href="w3c.xpath.math.xsl">XPath 3.0 Math Functions</a>
         </li>
      </ul>
   </xsl:template>

</xsl:stylesheet>
