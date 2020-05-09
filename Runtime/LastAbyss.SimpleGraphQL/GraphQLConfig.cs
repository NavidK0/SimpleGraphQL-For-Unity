using System.Collections.Generic;
using UnityEngine;

namespace LastAbyss.SimpleGraphQL
{
    [CreateAssetMenu(menuName = "SimpleGraphQL/GraphQL Config")]
    public class GraphQLConfig : ScriptableObject
    {
        /// <summary>
        /// This is the endpoint that we will be talking to.
        /// </summary>
        [Header("GraphQL Endpoint")]
        public string Endpoint;
        
        /// <summary>
        /// This is all the GraphQL query files that are available to SimpleGraphQL.
        /// 
        /// </summary>
        [Header(".graphql Files")]
        public List<TextAsset> Files;
    }
}