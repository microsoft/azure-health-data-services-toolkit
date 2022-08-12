[CAPL](../capl.md)

## Operations List

A CAPL*operation* has 2 arguments, which depending on the type of operations could be required or omitted.  The RHS argument is bound by the value of a claim found by a *match expression*.  The LHS argument is bound by the value encoded in the CAPL *operation*.

### Table of Operations

1. [Contains](#Contains)
2. [EqualCaseInsensitive](#EqualCaseInsensitive)
3. [EqualCaseInsensitive](#EqualCaseInsensitive)
4. [EqualCaseSensitive](#EqualCaseSensitive)
5. [EqualDateTime](#EqualDateTime)
6. [EqualNumeric](#EqualNumeric)
7. [Exists](#Exists)
8. [GreaterThanDateTime](#GreaterThanDateTime)
9. [GreaterThan](#GreaterThan)
10. [GreaterThanOrEqualDateTime](#GreaterThanOrEqualDateTime)
11. [GreaterThanOrEqual](#GreaterThanOrEqual)
12. [LessThanDateTime](#LessThanDateTime)
13. [LessThan](#LessThan)
14. [LessThanOrEqualDateTime](#LessThanOrEqualDateTime)
15. [LessThanOrEqual](#LessThanOrEqual)
16. [NotEqualCaseInsensitive](#NotEqualCaseInsensitive)
17. [NotEqualCaseSensitive](#NotEqualCaseSensitive)
18. [BetweenDateTime](#BetweenDateTime)
19. [BetweenExclusive](#BetweenExclusive)
20. [BetweenInclusive](#BetweenInclusive)


<a name="Contains">**Contains**</a> 
- Type: #Contains
- Description: String compare that determines if the LHS variable string contains the RHS variable string.

<a name="EqualCaseInsensitive">**EqualCaseInsensitive**</a> 
- Type: #EqualCaseInsensitive
- Description: String compare of the LHS and RHS that determines if the strings are equal regardless of the casing.

<a name="EqualCaseSensitive">**EqualCaseSensitive**</a> 
- Type: #EqualCaseSensitive
- Description: String compare of the LHS and RHS that determines if the strings are precisely equal.

<a name="EqualDateTime">**EqualDateTime**</a> 
- Type: #EqualDateTime
- Description: Compares LHS and RHS as datetime values and determines if they are equal.

<a name="EqualNumeric">**EqualNumeric**</a>
- Type: #EqualNumeric
- Description: Compares LHS and RHS as decimal values and determines if they are equal.

<a name="Exists">**Exists**</a>
- Type: #Exists
- Description: Compares LHS with null or whitespace to determine if the claim value exists.

<a name="GreaterThanDateTime">**GreaterThanDateTime**</a>
- Type: #GreaterThanDateTime
- Description: Compares LHS with RHS as datetime values and determines if the LHS is greater than the RHS value.
- 
<a name="GreaterThan">**GreaterThan**</a>
- Type: #GreaterThan
- Description: Compares LHS with RHS as decimal values and determines if the LHS is greater than the RHS value.

<a name="GreaterThanOrEqualDateTime">**GreaterThanOrEqualDateTime**</a>
- Type: #GreaterThanOrEqualDateTime
- Description: Compares LHS with RHS as datetime values and determines if the LHS is greater or equal to the RHS value.

<a name="GreaterThanOrEqual">**GreaterThanOrEqual**</a>
- Type: #GreaterThanOrEqual
- Description: Compares LHS with RHS as decimal values and determines if the LHS is greater or equal to the RHS value.

<a name="LessThanDateTime">**LessThanDateTime**</a>
- Type: #LessThanDateTime
- Description: Compares LHS with RHS as datetime values and determines if the LHS is less than the RHS value.
- 
<a name="LessThan">**LessThan**</a>
- Type: #LessThan
- Description: Compares LHS with RHS as decimal values and determines if the LHS is less than the RHS value.

<a name="LessThanOrEqualDateTime">**LessThanOrEqualDateTime**</a>
- Type: #LessThanOrEqualDateTime
- Description: Compares LHS with RHS as datetime values and determines if the LHS is less or equal to the RHS value.

<a name="LessThanOrEqual">**LessThanOrEqual**</a>
- Type: #LessThanOrEqual
- Description: Compares LHS with RHS as decimal values and determines if the LHS is less or equal to the RHS value.

<a name="NotEqualCaseInsensitive">**NotEqualCaseInsensitive**</a>
- Type: #NotEqualCaseInsensitive
- Description: String compare of the LHS and RHS that determines if the strings are **not equal** regardless of the casing.

<a name="NotEqualCaseSensitive">**NotEqualCaseSensitive**</a>
- Type: #NotEqualCaseSensitive
- Description: String compare of the LHS and RHS that determines if the strings are **not equal**.

<a name="BetweenDateTime">**BetweenDateTime**</a>
- Type: #BetweenDateTime
- Description: The LHS is ignored and the RHS encodes 2 datetimes as a normalized string.  The compare checks to see if the current datetime (now) is between the datetimes of the normalized string of the RHS.

<a name="BetweenExclusive">**BetweenExclusive**</a>
- Type: #BetweenExclusive
- Description: The RHS encodes 2 numeric values as a normalized string, which are converted to decimals.  The LHS compared to determine its value is between the RHS values exclusively.
- 
<a name="BetweenInclusive">**BetweenInclusive**</a>
- Type: #BetweenInclusive
- Description: The RHS encodes 2 numeric values as a normalized string, which are converted to decimals.  The LHS compared to determine its value is between the RHS values inclusively.
