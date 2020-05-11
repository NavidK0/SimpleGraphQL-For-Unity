using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGraphQL
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
        public List<GraphQLFile> Files;

        /// <summary>
        /// Set the auth scheme to be used here if you need authentication.
        /// You can also use CustomHeaders to pass in authentication if needed, but this is inherently less secure.
        /// </summary>
        [Header("Authorization")]
        public string AuthScheme = "Bearer";

        /// <summary>
        /// You can use this to include Headers on every query. This is useful if you are debugging or need
        /// a special access code alongside authentication.
        /// </summary>
        [Header("Custom Headers")]
        public List<Header> CustomHeaders;
    }

    [Serializable]
    public class Header
    {
        public string Key;
        public string Value;
    }
}