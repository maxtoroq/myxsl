<?xml version="1.0" encoding="utf-8"?>
<?validator class-name="AppRules.contact" processor="saxon" ?>

<schema xmlns="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt2">
   
   <pattern>
      <title>Grammar check</title>
      <rule context="/">
         <assert test="count((*,*/(name|email|comments))) ge 4">
            Missing content.
         </assert>
      </rule>
   </pattern>

   <pattern id="name-length" is-a="string-length">
      <param name="el" value="name"/>
      <param name="max-chars" value="20"/>
      <param name="required" value="true()"/>
   </pattern>

   <pattern id="email-length" is-a="string-length">
      <param name="el" value="email"/>
      <param name="max-chars" value="50"/>
      <param name="required" value="true()"/>
   </pattern>

   <pattern id="email-syntax">
      <rule context="email[string-length() gt 0]">
         <assert test="matches(string(), '\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*')">
            The e-mail is invalid.
         </assert>
      </rule>
   </pattern>

   <pattern id="subject-length" is-a="string-length">
      <param name="el" value="subject"/>
      <param name="max-chars" value="20"/>
   </pattern>

   <pattern id="comments-length" is-a="string-length">
      <param name="el" value="comments"/>
      <param name="max-chars" value="1000"/>
      <param name="required" value="true()"/>
   </pattern>

   <pattern id="string-length" abstract="true">
      <title>Validates input length.</title>
      
      <rule context="$el">
         <let name="len" value="string-length()" />
         <let name="required" value="false()"/>

         <assert test="not($required) or $len > 0">
            You must enter your <name/>.
         </assert>
         <assert test="$len le $max-chars">
            Maximum characters allowed: <value-of select="$max-chars" />
            (current: <value-of select="$len" />).
         </assert>
      </rule>
   </pattern>
   
</schema>