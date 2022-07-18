[CAPL](../ReadMe.md)

## CAPL Match
A *match expression* binds a variable from a claim of the security token to the RHS argument of a CAP *operation*.


### JSON Encoding
- "type": Optional value with a default of "#Literal".  If the *type* is #Literal, then claims are returned that match the case sensitive value of the *claimType* property and optionally the *value* property if encoded as not null.  If the *type* property is #Pattern, the *value* property is encoded as a regular expression. The #Pattern will return claims where are of the following are true:
	- The *claimType* property is a case sensitive match with a claim type.
	- The value of a matched claim must match the regular expression encoded in the *value* property.
- *claimType*: The claim type to match.
- *value* - Optional for #Literal and required for #Pattern.  #Pattern types encoded *value* as regular expression.
- *required*: Optional (true | false).  The default value is true, if omitted.  If **true** and no matching claims are found, then the *[rule](rule.md)* that encodes the match expression will evaluate to **false** and the *[operation](operation.md)* will not be invoked.  Otherwise, the operation will be invoked.  If **false** and no matching claims are found, then the *[rule](rule.md)* that encodes the match expression will evaluate to **true** and the [operation](operation.md) will not be invoked.  Otherwise, any matching claims will be evaluated by the [operation](operation.md). 
```
{
  "type": (#Literal | #Pattern)
  "claimType": (string)
  "required" : (true | false) Default is true if omitted.
  "value": (string) Optional  
}
```
