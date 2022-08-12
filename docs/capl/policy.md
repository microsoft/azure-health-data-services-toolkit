[CAPL](../capl.md)

## CAPL Policy

### Overview

A CAPL *policy* is JSON serializable, logic-based, and uses the information from a security token in the form of claims to make access control decisions.

A *policy* encapsulates an *evaluation expression* that communicates the access control decision (true | false) to the application.  It is the responsibility of the application to enforce the decision.

### JSON Encoding

- *id*: A string that uniquely identifies the CAPL *policy*.
- *expression*: The evaluation expression for the *policy*, that return the Boolean decision (true | false) of the access control decision. It is encoded as one of the following *[rule](./rule.md)*, *[logicalAnd](./logicalconnective.md)*, or *[logicalOr](./logicalconnective.md)*.

A *policy* has no parent element.

```json
{
  "id": (unique string)
  "expression" : {
   ... (rule | logicalAnd | logicalOr)
  }
}
```

**Example - Simple Expression**

```json
{
  "id": "my-unique-id",
  "expression": {
    "type": "#Rule",
    "eval": true,
    "operation": {
      "type": "#CaseSensitiveEqual",
      "value": "reader"
    },
    "match": {
      "claimType": "roles",
      "required": true
    }
  }
}
```

**Example - Complex Expression**

```json
{
  "id": "my-complex-policy",
  "expression": {
    "type": "#LogicalAnd",
    "eval": true,
    "terms": [
      {
        "type": "#Rule",
        "eval": true,
        "operation": {
          "type": "#CaseInsensitveEqual",
          "value": "reader"
        },
        "match": {
          "claimType": "roles",
          "required": true
        }
      },
      {
        "type": "#Rule",
        "eval": true,
        "operation": {
          "type": "#CaseInsensitveEqual",
          "value": "writer"
        },
        "match": {
          "claimType": "roles",
          "required": true
        }
      }
    ]
  }
}
```
