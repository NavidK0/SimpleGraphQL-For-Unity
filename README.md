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
| Requirements       |
| ------------------ |
| .NET 4.6 or higher |
| I guess that's it  |

# Supported Platforms

| Platforms | Supported |
| --------- | --------- |
| Mono      | ✔         |
| IL2CPP    | ✔         |
| WebGL     | ❓         |

This should work with all platforms (Mono/IL2CPP) except for WebGL, since Unity WebGL has issues with threading. If you are using WebGL, this package may be hit-or-miss for you at the present time. It makes use of UnityWebRequest where possible, but the WebSockets are the main issue, so subscriptions may not properly work. If you do not need subscriptions, WebGL may work just fine.

If you are having trouble with a platform, please open an issue.

## Unity Version
We're using this on Unity 2019.3.13f1. While it may work on older Unity versions, there is no strong guarantee because there have been many breaking API changes over the past couple of years, but also that some of the features being used here have not been backported. Your mileage may vary.

# Quick Start

> SimpleGraphQL makes use of .graphql files that you must write yourself. It is up to you to make sure they are valid. Many IDEs support this function natively or through plugins.

## Configuration

### Import: Put your .graphql files somewhere in your Assets folder.

### Create a Config
1. Right Click -> Create -> SimpleGraphQL -> GraphQL Config
2. Fill in values
![img](https://i.imgur.com/rs8EIEM.png)
> This inspector looks this way because of Odin Inspector. Go check it out, it is a massive time saver.

### Reference GraphQL Config
```cs
public GraphQLConfig Config;
```

### Queries & Mutations
```cs
public async void QueryOrMutation()
{
    var graphQL = new GraphQLClient(Config);

    // You can search by file name, operation name, or operation type
    // or... mix and match between all three
    Query query = graphQL.FindQuery("FileName", "OperationName", OperationType.Query);

    string results = await graphQL.SendAsync(
        query,
        new Dictionary<string, object>
        {
            {"variable", "value"}
        },
        null,
       "authToken",
       "Bearer"
    );

    Debug.Log(results);
}
```

> NOTE: The code above is just an example, not all parameters are needed. Be sure to look at the optional parameters.

### Subscriptions
```cs
public async void Subscribe()
{
    var graphQL = new GraphQLClient(Config);
    Query query = graphQL.FindQuery("SubscribeFile");

    graphQL.RegisterListener(OnSubscriptionUpdated);

    bool success = await graphQL.SubscribeAsync(
        query,
        new Dictionary<string, object>
        {
            {"variable", "value"}
        },
        null,
       "authToken",
       "Bearer"
    );
    
    Debug.Log(success ? "Subscribed!" : "Subscribe failed!");
}

public async void Unsubscribe()
{
    var graphQL = new GraphQLClient(Config);
    Query query = graphQL.FindQuery("SubscribeScoresForLevel");

    await graphQL.Unsubscribe(query);
    graphQL.UnregisterListener(OnSubscriptionUpdated);
    Debug.Log("Unsubscribed!");
}

public void OnSubscriptionUpdated(string payload)
{
    Debug.Log("Subscription updated: " + payload);
}
```

# Authentication and Headers

> Depending on your authentication method, it is up to you to ensure that your authentication data and headers are set correctly.

### Custom headers and auth tokens are natively supported in SimpleGraphQL. They can be passed in as parameters when calling `SubscribeAsync` or `SendAsync`.

# Example Valid .graphql Files
### GetScoreById.graphql
```graphql
# fully defined query
query GetScoreById($user_id: String!, $level: String!) {
    leaderboards_by_pk(level: $level, user_id: $user_id) {
        user_id
        level
        score
        metadata
    }
}
```
### GetScoresForLevel.graphql
```graphql
# anonymous query
query ($level: String!) {
    leaderboards(where: {level: {_eq: $level}}) {
        user_id
        level
        score
        metadata
    }
}
```
### MoreScoreStuff.graphql
```graphql
# you can have multiple queries in one file, and long as they are uniquely named

mutation UpsertScore($user_id: String!, $level: String!, $score: bigint! $metadata: jsonb!) {
    insert_leaderboards_one(object: {user_id: $user_id, level: $level, score: $score, metadata: $metadata}, on_conflict: {constraint: leaderboards_pkey, update_columns: score, where: {score: {_lt: $score}}}) {
        user_id
        score
    }
}

subscription GetScoresForLevel($level: String!) {
    leaderboards(where: {level: {_eq: $level}}) {
        user_id
        level
        score
        metadata
    }
}
```

# Things to Note
- During testing, we found that Unity's version of .NET occasionally has issues with HttpClient and WebSocket. If you find that you are having the same issues, please let us know. WebSocket is unavoidable for subscriptions, and Unity has no alternative like they do with UnityWebRequest.
- WebSockets sometimes take extraordinarily long amounts of time to start up on the first call. This is has probably been fixed in a recent .NET version (but we don't have those fixes yet.)

<!-- ## Auth with Hasura
TBA -->

# Wiki will be added soon

More to be added soon