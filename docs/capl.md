![](./docs/images/readme-capl.png)

#### JSON serialization version for access control

Version: 4.0
Last Updated 12/03/2021

## Overview

Claims Authorization Policy Language, CAPL, is a serializable, logic-based, security token agnostic access control policy language useful in making the Boolean decisions to permit or deny access to a resource. CAPL uses claims associated with a security token presented to an application to bind variables to a policy's *evaluation expression* and return a Boolean access control decision. An *evaluation expression* in CAPL is either a simple expression or a complex expression.

CAPL is intended to be used by applications, where the application enforces access control decisions using CAPL policies. Because the policies are serializable, applications can acquire authorization policies from data stores or other services and control how policies are refreshed. This means that access control within applications can be managed externally without the need to redeploy applications.

Additional information regarding encoding and definitions is available [here](./capl/policy.md).

Evaluation expressions can be **simple**, e.g., (a==b), or **complex**, e.g., [(a==b)||(c<d)]. CAPL rules are used to create simple expressions and logical connectives (e.g., logical AND or logical OR are used to form complex expressions).

## Example

An authorization policy with a simple expression is defined in the example below using default values. The authorization policy states that the policy will evaluate to "true" if and only if the security token presented by the caller has at least one claim type of "roles" where at least one value is "reader".

```json
{
  "id": "ABC",
  "expression": {
    "type": "#Rule",
    "operation": {
      "type": "#EqualCaseSensitive",
      "value": "reader"
    },
    "match": {
      "claimType": "roles"
    }
  }
}
```
