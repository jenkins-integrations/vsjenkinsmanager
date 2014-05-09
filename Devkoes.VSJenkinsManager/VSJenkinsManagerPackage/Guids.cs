// Guids.cs
// MUST match guids.h
using System;

namespace Devkoes.VSJenkinsManagerPackage
{
    static class GuidList
    {
        public const string guidVSJenkinsManagerPackagePkgString = "252d354d-9787-42b0-b5cc-6b14e08f160f";
        public const string guidVSJenkinsManagerPackageCmdSetString = "e8edc4ae-193b-4440-8c4f-1f9b7d7c3435";
        public const string guidToolWindowPersistanceString = "b36671ed-ec18-4269-ac6d-a0dcc11fec14";

        public static readonly Guid guidVSJenkinsManagerPackageCmdSet = new Guid(guidVSJenkinsManagerPackageCmdSetString);
    };
}