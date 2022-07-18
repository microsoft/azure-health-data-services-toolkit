

![](./docs/readme-capl.png)

#### JSON serialization version for access control

Version: 4.0
Last Updated 12/03/2021

## Overview
Claims Authorization Policy Language, CAPL, is a serializable, logic-based, security token agnostic access control policy language useful in making the Boolean decisions to permit or deny access to a resource.  CAPL uses claims associated with a security token presented to an application to bind variables to a policy's *evaluation expression* and return a Boolean access control decision. An *evaluation expression* in CAPL is either a simple expression or a complex expression.
 
CAPL is intended to be used by applications, where the application enforces access control decisions using CAPL policies. Because the policies are serializable, applications can acquire authorization policies from data stores or other services and control how policies are refreshed.  This means that access control within applications can be managed externally without the need to redeploy applications.

Additional information regarding encoding and definitions [here](policy.md)

Evaluation expressions can be **simple**, e.g., (a==b), or **complex**, e.g., [(a==b)||(c<d)].  CAPL rules are used to create simple expressions and logical connectives, i.e., logical AND or logical OR, are used to form complex expressions.

**Example**
An authorization policy with a simple expression could be defined below using default values.  The authorization policy states that the policy with evaluate to "true" if and only if, the security token presented by the caller has at least one claim type of "roles" where at least one value is "reader".
```
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
## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
