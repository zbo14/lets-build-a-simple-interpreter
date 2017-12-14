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

: GETINT ( -- x token )
    0
    BEGIN
        ISDIGIT WHILE
        10 * VAL +
        ADVANCE
    REPEAT
    INT ;

\ TODO: cleanup
: GETTOKEN ( -- [x] token )
    RECURSIVE
    9 CHR= IF SKIPWHITESPACE GETTOKEN ELSE
    13 CHR= IF EOF ELSE
    32 CHR= IF SKIPWHITESPACE GETTOKEN ELSE
    '+' CHR= IF ADVANCE ADD ELSE
    '-' CHR= IF ADVANCE SUB ELSE
    '*' CHR= IF ADVANCE MUL ELSE
    '/' CHR= IF ADVANCE DIV ELSE
    '(' CHR= IF ADVANCE LPAREN ELSE
    ')' CHR= IF ADVANCE RPAREN ELSE
    ISDIGIT IF GETINT ELSE ERROR
    ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ENDIF ;

: EAT ( token1 token2  -- [x] token )
    = IF GETTOKEN ELSE ERROR ENDIF ;

: ISMULOP ( token -- token bool )
    DUP MUL = IF -1 ELSE DUP DIV = ENDIF ;

: DOMULOP ( term factor token -- term )
    MUL = IF * ELSE / ENDIF ;

DEFER _DOEXPR

: DOFACTOR ( token -- x token )
    DUP INT =
    IF INT EAT
    ELSE
        DUP LPAREN =
        IF
            LPAREN EAT
            _DOEXPR
            RPAREN EAT
        ELSE ERROR
        ENDIF
    ENDIF ;

: DOTERM                ( x token -- term token )
    DOFACTOR            ( x token -- x token )
    BEGIN
        ISMULOP WHILE   ( x token -- x token )
        >R              ( x token -- x )
        GETTOKEN        ( x -- x token' )
        DOFACTOR        ( x token' -- x factor token' )
        ROT ROT         ( x factor token' -- token' x factor )
        R> DOMULOP      ( token' x factor token -- token' x )
        SWAP            ( token' x -- x token' )
    REPEAT ;

: ISADDOP ( token -- token bool )
    DUP ADD = IF -1 ELSE DUP SUB = ENDIF ;

: DOADDOP ( expr term token -- expr token )
    ADD = IF + ELSE - ENDIF ;

: DOEXPR                  ( x token -- expr token )
    DOTERM                ( x token -- x token )
    BEGIN
        ISADDOP WHILE     ( x token -- x token )
        >R                ( x token -- x )
        GETTOKEN          ( x -- expr token' )
        DOTERM            ( x token' -- x term token' )
        ROT ROT           ( x term token' -- token' x term )
        R> DOADDOP        ( token' x term token -- token' x )
        SWAP              ( token' x -- x token' )
    REPEAT ;

' DOEXPR IS _DOEXPR

: MAIN ( -- expr )
    CR
    ADVANCE GETTOKEN
    DOEXPR
    EOF =
    IF CR .
    ELSE ERROR
    ENDIF ;
