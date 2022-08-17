[CAPL](../capl.md)

## CAPL Rule

### Overview 

A CAPL *rule* is a simple expression that encodes a CAPL *[operation](./operation.md)* and *[match expression](./match.md)*,  A single *rule* can be used within a *policy* has a standalone *evaluation expression* of a *policy* or can be used with *logical connectives* in forming a policy's *evaluation expression*.

A *rule* has the following encoding.

### JSON Encoding

- "id": Optional string that uniquely identifies the *rule*.
- *type*: Required and must always be #Rule.
- *eval*: The truthful evaluation of the *rule* (true | false).  When the rule's *eval* property is true, the *rule* will return true only is the operation's result is true; otherwise the *rule* will evaluate to false. When the rule's *eval* property is false, the *rule* will return true only when the operation's result is false; otherwise the *rule* will evaluate to false.
- *operation*: A CAPL [operation](./operation.md).
- *match*: A CAPL [match expression](./match.md).

```json
{
  "id": (Optional string)
  "eval": (true or false) Default is true.
  "type": "#Rule",  
  "operation": {
  ...
  }
  "match": {
  ...
  }
}
```

The *type* property is a required as "#Rule" and the *eval* property determines the truthful evaluation of the *rule*, i.e., true or false.  If the *eval* property is omitted, it has the default value of **true**.