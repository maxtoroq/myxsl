xquery version "1.0";

let $ref as xs:string? := request:referrer-url('Path,KeepDelimiter'),
$query as xs:string := request:query(),
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
      
      let $source-url as xs:string := concat('https://github.com/myxsl/myxsl/blob/master/samples', $path2)
      return response:redirect($source-url)