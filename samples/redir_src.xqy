xquery version "1.0";

declare namespace util = "http://myxsl.net/ns/util";

let $ref as xs:string? := request:referrer-url('Path,KeepDelimiter'),
$query as xs:string := request:query(),
$rev as xs:string := util:app-settings('revision'),
$path as xs:string :=
   if (string-length($query) gt 0) then
      $query
   else
      ($ref, '')[1]

return
   if ($path = '') then
      response:set-status(400, 'Bad Request')
   else
      let $path2 as xs:string := 
         if (ends-with($path, '/')) then
            concat($path, 'index.xsl')
         else
            $path
      
      let $source-url as xs:string := concat('http://myxsl-net.svn.sourceforge.net/viewvc/myxsl-net/trunk/samples', $path2, '?revision=', $rev, '&amp;view=markup')
      return response:redirect($source-url)