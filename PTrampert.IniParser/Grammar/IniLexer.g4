lexer grammar IniLexer;

COMMENT
    : '#' ~[\r\n]*
    ;
    
DQUOTED_STRING
    : '"' ( ~["\\] | '\\' . )* '"'
    ;
    
SQUOTED_STRING
    : '\'' ( ~['\\] | '\\' . )* '\''
    ;
    
UNQUOTED_STRING
    : ~[=[\]\r\n\t ]+
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