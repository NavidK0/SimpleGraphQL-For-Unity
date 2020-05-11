# SimpleGraphQL For Unity
SimpleGraphQL is just that -- a simple GraphQL client that is mostly code based and works with Unity.

## About
This package attempts to provide a simple API that is able to interact with a GraphQL server.
Nothing more, nothing less. No complicated setup.

Also, the world could use some more Unity-friendly GraphQL libraries.

That being said, this is intended to be a primarily code based package, so keep that in mind if you decide to use this.

## What It Does and Doesn't

### Does
- Supports Queries, mutations, and subscriptions
- Checking for error codes
- Reads .graphql files within your project
- Supports multiple queries per file (with operation selectors)
- Supports custom headers
- Async/Await & Coroutine w/ Callback

### Doesn't
- Introspection (you are responsible for writing valid .graphql files)
  - There is very basic error checking, but beyond that you need to ensure that you are writing something compatible with your server (GraphiQL works great)

# Requirements
| Requirements      |
| ----------------- |
| .NET 4.6          |
| I guess that's it |

# Supported Platforms

| Platforms | Supported |
| --------- | --------- |
| Mono      | ✔         |
| IL2CPP    | ✔         |
| WebGL     | ❓         |

This should work with all platforms (Mono/IL2CPP) except for WebGL, since Unity WebGL has issues with threading. If you are using WebGL, this package may be hit-or-miss for you at the present time.

If you are having trouble with a platform, please open an issue.

## Unity Version
We're using this on Unity 2019.3.13f1. While it may work on older Unity versions, there is no strong guarantee because there have been many breaking API changes over the past couple of years, but also that some of the features being used here have not been backported. Your mileage may vary.

# Getting Started

> SimpleGraphQL makes use of .graphql files that you must write yourself. It is up to you to make sure they are valid. Many IDEs support this function natively or through plugins.

## Queries
TBA

## Mutations
TBA

## Subscriptions
TBA

# Authentication and Headers

> Depending on your authentication method, it is up to you to ensure that your authentication data and headers are set correctly.

# Things to Note
TBA

<!-- ## Auth with Hasura
TBA -->
TBA

<hr>
More to be added soon