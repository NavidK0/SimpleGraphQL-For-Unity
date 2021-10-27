using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace SimpleGraphQL
{
    internal class LinkXmlInjector : IUnityLinkerProcessor
    {
        int IOrderedCallback.callbackOrder => 0;

        string IUnityLinkerProcessor.GenerateAdditionalLinkXmlFile(BuildReport report,
            UnityLinkerBuildPipelineData data)
        {
            // This is pretty ugly, but it was the only thing I could think of in order to reliably get the path to link.xml
            const string linkXmlGuid = "9bbe747a148f00540828ab8521feb770";
            var assetPath = AssetDatabase.GUIDToAssetPath(linkXmlGuid);
            // assets paths are relative to the unity project root, but they don't correspond to actual folders for
            // Packages that are embedded. I.e. it won't work if a package is installed as a git submodule
            // So resolve it to an absolute path:
            return Path.GetFullPath(assetPath);
        }

#if !UNITY_2021_2_OR_NEWER
        void IUnityLinkerProcessor.OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        void IUnityLinkerProcessor.OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }
#endif
    }
}