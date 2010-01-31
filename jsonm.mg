module jsonm
{
  language jsonm
  {
    syntax Main = object:Object => object;
    
    syntax Object 
      = ObjectStart first:KeyValuePair rest:ObjectPart* ObjectEnd => Object { Pairs { first, valuesof(rest) } };
    
    syntax ObjectPart
      = Comma pair:KeyValuePair Comma? => pair;
      
    syntax KeyValuePair
      = key:String Colin value:Value => Pair { Key { key }, Value { value } };
      
    syntax Array
      = ArrayStart first:Value rest:ArrayPart* ArrayEnd => Array [ first, valuesof(rest) ];
    
    syntax ArrayPart
      = Comma value:Value Comma? => value;
    
    syntax Value 
      = string:String => string
      | number:Number => Number { number }
      | object:Object => object
      | array:Array => array
      | primitive:Primitive => Primitive { primitive };
    
    // TODO: Refine this grammar rule
    
    syntax String = '"' text:(text:StringText | empty) '"' => String { valuesof(text) } ;
    token StringText = !('\u0022')+;
    
    token Number = Minus? Digit+ ('.' Digit+)? Exponent?;
    token Exponent = ("e" | "E") Sign? Digit+;
    token Digit = "0" .. "9";
    token Sign = Plus | Minus;
    token Minus = "-";
    token Plus = "+";
    
    token Primitive = 'true' | 'false' | 'null';
    
    token Colin = ':';
    token Comma = ',';
    
    token ObjectStart = '{';
    token ObjectEnd = '}';
    token ArrayStart = '[';
    token ArrayEnd = ']';
    
    token Newline = '\r' | '\n' | '\r\n';
    token Whitespace = ' ' | '\t' | Newline;
    
    interleave Ignore = Whitespace+;
  }
}