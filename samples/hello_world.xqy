xquery version "1.0";

declare variable $name as xs:string? := request:query('name');

<html>
<body>
{
   if ($name) then
      ("Hello", $name)
   else
      <form>
         Enter your name: <input name="name" />
      </form>
}
</body>
</html>