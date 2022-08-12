[CAPL](../capl.md)

## CAPL Operation

### Overview

An *operation* in CAPL creates Boolean result (true | false) by comparing a bound variable from claims to the *value* property of the *operation*, e.g., $\mathnormal{(lhs}$ $\mathnormal{==}$ $\mathnormal{rhs)}$.  The $\mathnormal{lhs}$ being the bound variable from claims found in a *[match](./match.md) expression* and the $\mathnormal{rhs}$ being the value* property of the *operation*. The comparison operator, e.g., $\mathnormal{==}$, is defined the *type* property of the operation.

### JSON Encoding

- *type*: The type of operation as defined in the [operations list](./operationslist.md).
- *value*: The variable to bind to the RHS argument of the *operation*.

An operation's parent element is a *[rule](./rule.md)*.

```json
{
  "type": "<type of operator>",
  "value": "<RHS argument to bind> "
}
```
