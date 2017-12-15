0 CONSTANT EOF
1 CONSTANT INT
2 CONSTANT ADD
3 CONSTANT SUB
4 CONSTANT MUL
5 CONSTANT DIV
6 CONSTANT LPAREN
7 CONSTANT RPAREN

VARIABLE chr
: CHR@ chr c@ ;
: CHR= CHR@ = ;
: CHR! chr c! ;
: VAL CHR@ '0' - ;

: ERROR ( -- )
    ." Error parsing input" CR ABORT ;

: ISDIGIT ( char -- bool )
    CHR@ '0' '9' 1+ WITHIN ;

: ISWHITESPACE ( -- bool)
    9 CHR= 32 CHR= OR ;

: ADVANCE ( -- )
    KEY DUP CHR! EMIT ;

: SKIPWHITESPACE ( -- )
    BEGIN
        ISWHITESPACE WHILE
        ADVANCE
    REPEAT ;

: GETINT
    0
    BEGIN
        ISDIGIT WHILE
        10 * VAL +
        ADVANCE
    REPEAT
    INT ;

\ TODO: cleanup
: TOKEN
    RECURSIVE
    9 CHR= IF SKIPWHITESPACE TOKEN ELSE
    13 CHR= IF EOF ELSE
    32 CHR= IF SKIPWHITESPACE TOKEN ELSE
    '+' CHR= IF ADVANCE ADD ELSE
    '-' CHR= IF ADVANCE SUB ELSE
    '*' CHR= IF ADVANCE MUL ELSE
    '/' CHR= IF ADVANCE DIV ELSE
    '(' CHR= IF ADVANCE LPAREN ELSE
    ')' CHR= IF ADVANCE RPAREN ELSE
    ISDIGIT IF GETINT ELSE ERROR
    ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ;

: ISMULOP
    DUP MUL = IF -1 ELSE DUP DIV = ENDIF ;

DEFER _EXPR

\ factor : INTEGER | LPAREN expr RPAREN
: FACTOR
    DUP INT =
    IF TOKEN
    ELSE
        DUP LPAREN =
        IF
            DROP TOKEN
            _EXPR
            RPAREN =
            IF TOKEN
            ELSE ERROR
            ENDIF
        \ ELSE ...
        ENDIF
    ENDIF ;

: BINOP
    RECURSIVE
    DUP EOF = IF DROP THEN
    DUP INT = IF DROP ELSE
    DUP ADD = IF DROP BINOP >R BINOP R> + ELSE
    DUP SUB = IF DROP BINOP >R BINOP R> - ELSE
    DUP MUL = IF DROP BINOP >R BINOP R> * ELSE
    DIV = IF BINOP >R BINOP R> / ELSE ERROR
    ENDIF ENDIF ENDIF ENDIF ENDIF ;

\ term : factor ((MUL | DIV) factor)*
: TERM
    FACTOR
    BEGIN
        ISMULOP WHILE
        >R
        TOKEN
        FACTOR
        R> SWAP >R
        \ .s
        BINOP INT
        R>
    REPEAT
    FACTOR ;

: ISADDOP
    DUP ADD = IF -1 ELSE DUP SUB = ENDIF ;

\ expr : term ((PLUS | MINUS) term)*
: EXPR
    TERM
    BEGIN
        ISADDOP WHILE
        >R
        TOKEN
        TERM
        R> SWAP >R
        BINOP INT
        R>
    REPEAT
    TERM ;

' EXPR IS _EXPR

: MAIN
    CR
    ADVANCE TOKEN
    EXPR
    EOF =
    IF CR DROP .
    ELSE ERROR
    ENDIF ;
