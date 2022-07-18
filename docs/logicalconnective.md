[CAPL](../ReadMe.md)
## CAPL Logical Connectives

### Overview
A logical connective is either a logical AND or logical OR and is used to create complex expressions.  The either logical connective encodes and array of terms which either element in the array is joined by the type of logical connective, i.e., AND or OR, to form the complex expression.

### JSON Encoding
- "id": Optional string that uniquely identifies the logical connective.
- *eval*:
- *type*: One of (#LogicalAnd | #LogicalOr)
- *terms*: The terms property is an array of any permutation of the following CAPL elements:
	- Rule
	- Logical And
	- Logical OR

A logical connective's parent element is a *[policy](policy.md)*.
```
{
  "id": (Optional string)
  "eval": ""
  "type": (#LogicalAnd | #LogicalOr)
  "terms": [ ]
}
```