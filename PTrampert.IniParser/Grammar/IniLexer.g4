lexer grammar IniLexer;

COMMENT
    : '#' ~[\r\n]*
    ;
    
UNQUOTED_STRING
    : ~[=[\]\r\n\t ]+
    ;
    
DQUOTED_STRING
    : '"' ( ~["\\] | '\\' . )* '"'
    ;
    
SQUOTED_STRING
    : '\'' ( ~['\\] | '\\' . )* '\''
    ;
    
EQUALS
    : '='
    ;
    
LBRACKET
    : '['
    ;
    
RBRACKET
    : ']'
    ;
    
NEWLINE
    : '\r'? '\n'
    ;
    
WS
    : [ \t]+
    ;