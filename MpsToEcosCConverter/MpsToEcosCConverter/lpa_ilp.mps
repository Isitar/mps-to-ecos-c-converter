NAME          LPA_ILP
ROWS
 N  COST
 L  R01
 L  R02
COLUMNS
    MARK0000  'MARKER'                 'INTORG'
    X01       R01                 1.   R02                 1.
    X01       COST               -.5   
    MARK0000  'MARKER'                 'INTEND'
    X02       R01                 2.   R02                .25
    X02       COST              -.25   
RHS
    B         R01                12.   R02                 6.
BOUNDS
 UP bnd       X01                 4.
ENDATA