parser grammar IniGrammar;
options { tokenVocab=IniLexer; }

iniFile
    : iniLine (NEWLINE iniLine)* EOF
    ;
    
iniLine
    : (COMMENT | sectionHeader | keyValuePair)?
    ;
    
sectionHeader
    : LBRACKET sectionName RBRACKET
    ;
    
sectionName
    : string (WS string)*
    ;
    
string
    : UNQUOTED_STRING | DQUOTED_STRING | SQUOTED_STRING
    ;
    
keyValuePair
    : key WS* EQUALS WS* value
    ;
    
key
    : string
    ;
    
value
    : string (WS+ string)*
    ;