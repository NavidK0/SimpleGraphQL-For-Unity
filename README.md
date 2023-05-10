# ⚠️ NOTICE: This project is no longer being maintained.
Sincerest apologies, but I no longer have the time to maintain this fork. It may have bugs.  
Feel free to fork, however no pull request or bug report will be answered.

 If you would like to take over this project, feel free to reach out!

# SimpleGraphQL For Unity

SimpleGraphQL is just that -- a simple GraphQL client that is mostly code based and works with Unity.

## About

This package attempts to provide a simple API that is able to interact with a GraphQL server. Nothing more, nothing
less. No complicated setup.

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
    - There is very basic error checking, but beyond that you need to ensure that you are writing something compatible
      with your server (GraphiQL works great)

# Requirements

| Requirements       |
| ------------------ |
| .NET 4.6 or higher |
| I guess that's it  |

# Supported Platforms

| Platforms | Queries & Mutations | Subscriptions |
| --------- | ------------------- | ------------- |
| Mono      | ✔                  | ✔             |
| IL2CPP    | ✔                  | ✔             |
| WebGL     | ✔                  | ❌            |

This should work with all platforms (Mono/IL2CPP) except for subscriptions on WebGL.
It makes use of UnityWebRequest where possible, but C# WebSockets are the main issue, so subscriptions will not properly
work. If you do not need
subscriptions, WebGL will work just fine. Work may be added to support WebGL in the future, but for now, there is no
support.

If you are having trouble with a platform, please open an issue.

## Unity Version

We've tested this on Unity 2019.4 and higher (up to 2021.1). While it may work on older Unity versions, there is no
strong guarantee because there have been many breaking API changes over the past couple of years, but also that some of
the features being used here may have not been backported. Your mileage may vary.

# Installation

You can add this library to your project using the Package Manager.

Go to the package manager and click on "Add package from git URL".  
From there, add this URL:  
`https://github.com/ngoninteractive/SimpleGraphQL-For-Unity.git`

![show_tutorial_image](https://i.imgur.com/bZtYyfw.gif)

That's it.

# Quick Start

Simplest usage:

```c#
var client = new GraphQLClient("https://countries.trevorblades.com/");
var request = new Request
{
    Query = "query ContinentNameByCode($code: ID!) { continent(code: $code) { name } }",
    Variables = new
    {
        code = "EU"
    }
};
var responseType = new { continent = new { name = "" } };
var response = await client.Send(() => responseType, request);
Debug.Log(response.Data.continent.name);
```

SimpleGraphQL also lets you store queries in .graphql files that you must write yourself. It is up to you to make sure
they are valid. Many IDEs support this function natively or through plugins.

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

    string results = await graphQL.Send(
        query.ToRequest(new Dictionary<string, object>
        {
            {"variable", "value"}
        }),
        null,
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

    bool success = await graphQL.Subscribe(
        query.ToRequest(new Dictionary<string, object>
        {
            {"variable", "value"}
        }),
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

    await graphQL.Unsubscribe(query.ToRequest());
    graphQL.UnregisterListener(OnSubscriptionUpdated);
    Debug.Log("Unsubscribed!");
}

public void OnSubscriptionUpdated(string payload)
{
    Debug.Log("Subscription updated: " + payload);
}
```

### Coroutines

SimpleGraphQL includes a custom yield instruction for when you want to use coroutines.

```cs
/// <summary>
/// Create a new WaitForSend Yield Instruction.
/// </summary>
/// <param name="sendFunc">The graphQL send function.</param>
/// <param name="onComplete">The callback that will be invoked after the task is complete.</param>
public WaitForSend(Func<Task<string>> sendFunc, Action<string> onComplete)
```

```cs
private void Start()
{
    StartCoroutine(_CallQueryCoroutine());
}

public IEnumerator _CallQueryCoroutine() 
{
    yield return new WaitForSend(
        graphQL.Send(
            request
        ), 
        OnComplete
    );
}

public void OnComplete(string result) 
{
    Debug.Log("GraphQL Result: " + result);
}
```

# Authentication and Headers

> Depending on your authentication method, it is up to you to ensure that your authentication data and headers are set
> correctly.

### Custom headers and auth tokens are natively supported in SimpleGraphQL. They can be passed in as parameters when calling `Subscribe` or `Send`.

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

query ListLevelScores($level: String!) {
    leaderboards(where: {level: {_eq: $level}}) {
        user_id
        level
        score
        metadata
    }
}
```

### Subscriptions.graphql

```graphql
subscription OnScoresUpdated($level: String!) {
    leaderboards(where: {level: {_eq: $level}}) {
        user_id
        level
        score
        metadata
    }
}

subscription OnAnyScoresUpdated {
    leaderboards {
        user_id
        level
        score
        metadata
    }
}
```

> NOTE: We recommend putting graphQL subscriptions in a separate file. Mixing queries, mutations, and subscriptions
> together in one file may lead to odd/undocumented behavior on various servers.

# Things to Note

- During testing, we found that Unity's version of .NET occasionally has issues with HttpClient and WebSocket. If you
  find that you are having the same issues, please let us know. WebSocket is unavoidable for subscriptions, and Unity
  has no alternative like they do with UnityWebRequest.
- WebSockets sometimes take extraordinarily long amounts of time to start up on the first call. This is has probably
  been fixed in a recent .NET version (but we don't have those fixes yet.)

<!-- ## Auth with Hasura
TBA -->
