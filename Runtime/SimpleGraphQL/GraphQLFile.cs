using System.Collections.Generic;
using UnityEngine;

namespace SimpleGraphQL
{
    public class GraphQLFile : ScriptableObject
    {
        public List<Query> Queries = new List<Query>();
    }
}