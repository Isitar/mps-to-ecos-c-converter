* MPS file created by lp_solve v5.5!
* Model constraints = 0 (of which 0 equalities)
*       variables   = 0 (of which 0 integer-valued and 0 semi-continuous)
*
NAME          ILPHS18
ROWS
 N  COST
 L  R1
 L  R2
COLUMNS
    MARK0000  'MARKER'                 'INTORG'
    X01       R1                  1.   R2                  5.
    X01       COST              -10.
    X02       R1                  1.   R2                  9.
    X02       COST              -17.
    MARK0000  'MARKER'                 'INTEND'
RHS
    B         R1                 12.   R2                90.
ENDATA