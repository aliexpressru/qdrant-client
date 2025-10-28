# .NET SDK for Qdrant vector database HTTP API

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2Faliexpressru%2Fqdrant-client%2Fbadge%3Fref%3Dmain&style=flat)](https://actions-badge.atrox.dev/aliexpressru/qdrant-client/goto?ref=main)
![Test Status](https://github.com/aliexpressru/qdrant-client/actions/workflows/test.yml/badge.svg)
[![NuGet Release][Qdrant-image]][Qdrant-nuget-url]

.NET SDK for [Qdrant vector database](https://qdrant.tech/).

## Getting started

### Creating a client

A client that will connect to Qdrant HTTP API on `http://localhost:6334` can be instantiated as follows

```csharp
var client = new QdrantHttpClient("localhost");
```

Additional constructor overloads provide more control over how the client is configured. The following example configures a client to use the different Qdrant port and an api key:

```csharp
var client = new QdrantHttpClient(
    httpAddress: "localhost", 
    port: 1567, 
    apiKey : "my-secret-api-key"
);
```

### Dependency Injection

To register Qdrant HTTP client in the dependency injection container use the following code:

```csharp
services.AddQdrantHttpClient(options =>
{
    options.HttpAddress = "localhost";
    options.Port = 6334;
    options.ApiKey = "my-secret-api-key";
});
```

This will register `QdrantHttpClient` as a singleton service. 

If you wish to register `IQdrantHttpClient` instead (supported from version 1.15.13 of this library), pass the `registerAsInterface` argument to the `AddQdrantHttpClient` method:

```csharp
services.AddQdrantHttpClient(options =>
    {
        options.HttpAddress = "localhost";
        options.Port = 6334;
        options.ApiKey = "my-secret-api-key";
    },
    registerAsInterface: true
);
```

### Working with collections

Once a client has been created, create a new collection

```csharp
var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
    "my_collection",
    new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize: 100, isServeVectorsFromDisk: true)
    {
        OnDiskPayload = true
    },
    cancellationToken
);
```

#### Insert vectors into a collection

```csharp
var upsertPoints = Enumerable.Range(0, 100).Select(
    i => new UpsertPointsRequest<TestPayload>.UpsertPoint(
        PointId.Integer((ulong) i),
        Enumerable.Range(0, 128)
            .Select(_ => float.CreateTruncating(Random.Shared.NextDouble()))
            .ToArray(),
        new TestPayload()
        {
            Integer = 123,
            Text = "test"
        }
    )
).ToList();

var upsertPointsResult = await _qdrantHttpClient.UpsertPoints(
    "my_collection",
    new UpsertPointsRequest<TestPayload>()
    {
        Points = upsertPoints
    },
    cancellationToken
);
```

#### Search for similar vectors

```csharp
var queryVector = Enumerable.Range(0, 128)
    .Select(_ => float.CreateTruncating(Random.Shared.NextDouble()))
    .ToArray()

// return the 5 closest points
var searchResult = await _qdrantHttpClient.SearchPoints(
    "my_collection",
    new SearchPointsRequest(
        queryVector,
        limit: 5)
    {
        WithVector = false,
        WithPayload = PayloadPropertiesSelector.All
    },
    cancellationToken
);
```

##### Search for similar vectors with filtering condition

```csharp
 var searchResult = await _qdrantHttpClient.SearchPoints(
    "my_collection",
    new SearchPointsRequest(
        queryVector,
        limit: 5)
    {
        WithVector = false,
        WithPayload = PayloadPropertiesSelector.All,
        Filter = 
            Q.Must(
                Q.BeInRange("int_field", greaterThanOrEqual: 0)
            )
            +
            !(
                Q.MatchValue("int_field_2", 1567)
                &
                Q.MatchValue("text_field", "test")
            )
    },
    cancellationToken
);
```

##### Search for similar vectors with filtering condition on typed payload

Here we are using typed builders for building filters for typed payload.

```csharp
 var searchResult = await _qdrantHttpClient.SearchPoints(
    "my_collection",
    new SearchPointsRequest(
        queryVector,
        limit: 5)
    {
        WithVector = false,
        WithPayload = PayloadPropertiesSelector.All,
        Filter = 
            Q.Must(
                Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
            )
            |
            !Q.Must(
                Q<TestPayload>.MatchValue(p => p.Integer, 1)
            )
    },
    cancellationToken
);
```

### Building collections

Conditions are built using `Q` (from Qdrant or Query) and `Q<TPayload>` condition builders.
Top level filter should contain only `Must`, `MustNot` or `Should` condition groups.
Result if any call on `Q` or `Q<TPayload>` is implicitly convertible to `QdrantFilter`, that is accepted everywhere the filter is expected, for ease of use.
`QdrantFilter` can be directly directly using `QdrantFilter.Create()` factory method.

#### Non-generic condition builder `Q`

Non-generic condition builder methods other than `Must`, `MustNot`, `Should` and `Nested` accept string payload field name as the first parameter.

```csharp
Q.Must(
    Q.MatchValue("test_key", 123),
    Q.MatchValue("test_key_2", "test_value")
)
```

Filters can be nested.

```csharp
Q.Should(
    Q.MustNot(Q.MatchValue("test_key", 123)),
    Q.MatchValue("test_key_2", "test_value")
)
```

#### Generic condition builder `Q<T>`

If the type of the payload is known beforehand the generic version of condition builder `Q<TPayload>` can be used to avoid typos in payload field names.
`Q<TPayload>` has the same methods but with payload field selector expression as the first parameter. 

If the payload is as defined as follows

```csharp
public class TestPayload : Payload
{
    public string Text { get; set; }

    [JsonPropertyName("int")]
    public int? IntProperty { get; set; }

    public NestedClass Nested { get; set; }

    public class NestedClass
    {
        public string Name { get; set; }

        public double Double { set; get; }

        public int Integer { get; set; }
    }
}
```

Filter can access its structure to derive the payload filed names. 
Property renames in payload json are supported through standard `JsonPropertyNameAttribute`.
Property nesting is also suppoerted in this case json dot-notation path will be constructed.

```csharp
Q.Should(
    Q<TestPayload>.MatchValue(p=>p.IntProperty, 123),
    Q<TestPayload>.MatchValue(p=>p.Nested.Name, "test_value")
)
```

#### Filter combination operators

In addition to combining filters explicitly, the more terse combination is possible through the use of operators `+` , `|`, `&` and `!`.

- `+` - combines top level filter conditions to simplify filter building.
  
    ```csharp
    Q.Must(
        Q.BeInRange("int_field", greaterThanOrEqual: 0)
    )
    +
    Q.MustNot(
        Q.MatchValue("int_field_2", 1567)
    )
    ```

    Which is equivalent to the following filter json

    ```json
    {
        "must": [
            {
                "key": "int_field",
                "range": {
                    "gte": 0
                }
            }
        ],
        "must_not": [
            {
                "key": "int_field_2",
                "match": {
                    "value": 1567
                }
            }
        ]
    }
    ```

- `|` combines two conditions using `Should` condiiton group. Nested `Should` groups are automatically unwrapped.

    ```csharp
    Q.Must(
        Q.BeInRange("int_field", greaterThanOrEqual: 0)
    )
    |
    Q.Should(
        Q.MatchValue("int_field_2", 1567)
    )
    ```

    Which is equivalent to the following filter json

    ```json
    {
        "should": [
            {
                "must": [
                    {
                        "key": "int_field",
                        "range": {
                            "gte": 0
                        }
                    }
                ]
            },
            {
                "key": "int_field_2",
                "match": {
                    "value": 1567
                }
            }
        ]
    }
    ```

- `&` combines two conditions using `Must` condiiton group. Nested `Must` groups are automatically unwrapped.

    ```csharp
    Q.MustNot(
        Q.BeInRange("int_field", greaterThanOrEqual: 0)
    )
    &
    Q.Must(
        Q.MatchValue("int_field_2", 1567)
    )
    ```

    Which is equivalent to the following filter json

    ```json
    {
        "must": [
            {
                "must_not": [
                    {
                        "key": "int_field",
                        "range": {
                            "gte": 0
                        }
                    }
                ]
            },
            {
                "key": "int_field_2",
                "match": {
                    "value": 1567
                }
            }
        ]
    }
    ```

- `!` wraps condition in `MustNot` condition group. Negates nested `Must` and `MustNot` condition groups.

    ```csharp
    Q.Should(
        Q<TestPayload>.MatchValue(p=>p.IntProperty, 123),
        !Q<TestPayload>.MatchValue(p=>p.Nested.Name, "test_value")
    )
    +
    !Q.Must(
        Q<TestPayload>.MatchValue(p=>p.IntProperty, 345)
    )
    ```

    Which is equivalent to the following filter json

    ```json
    {
        "should": [
            {
                "key": "int_property",
                "match": {
                    "value": 123
                }
            },
            {
                "must_not": [
                    {
                        "key": "nested.name",
                        "match": {
                            "value": "test_value"
                        }
                    }
                ]
            }
        ],
        "must_not": [
            {
                "key": "int_property",
                "match": {
                    "value": 345
                }
            }
        ]
    }
    ```

[Qdrant-nuget-url]:https://www.nuget.org/packages/Aerx.QdrantClient.Http/
[Qdrant-image]:
https://img.shields.io/nuget/v/Aerx.QdrantClient.Http.svg
