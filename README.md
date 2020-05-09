# SimpleGraphQL For Unity
SimpleGraphQL is just that -- a simple GraphQL client that doesn't have any user friendly interfaces and works with Unity.

## Why and how?
Frankly, all the existing solutions either don't work with Unity or are too complicated/don't support assembly definitions.

Also, the world could use some more Unity-friendly GraphQL libraries.

That being said, this is intended to be a primarily code based package, so keep that in mind if you decide to use this.

## What It Does and Doesn't

### Does
- Supports Queries, mutations, and subscriptions
- Checking for error codes
- Reads .graphql files within your project
- Supports custom headers
- Async/Await & Coroutine w/ Callback

### Doesn't
- Introspection (you are responsible for writing valid .graphql files)
- Deserialization of JSON Responses (excluding errors)
  - You are responsible for deserializing successful responses
  - In turn, this allows you to use any serializer you wish

# Supported Platforms

| Platforms | Supported |
| --------- | --------- |
| Mono      | ✔         |
| IL2CPP    | ✔         |
| WebGL     | ❓         |

This should work with all platforms (Mono/IL2CPP) except for WebGL, since Unity WebGL has issues with threading. If you are using WebGL, this package may be hit-or-miss for you at the present time.

If you are having trouble with a platform, please open an issue.

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

## Auth with Hasura
TBA

<hr>
More to be added soon