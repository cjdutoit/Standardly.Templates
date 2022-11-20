// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using ADotNet.Clients;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks.SetupDotNetTaskV1s;

namespace Standardly.Templates.Infrastructure.Build.Services
{
    public class ScriptGenerationService
    {
        private readonly ADotNetClient adotNetClient;

        public ScriptGenerationService() =>
            this.adotNetClient = new ADotNetClient();

        public void GenerateBuildScript()
        {
            var githubPipeline = new GithubPipeline
            {
                Name = "Standardly.Templates Build",

                OnEvents = new Events
                {
                    Push = new PushEvent
                    {
                        Branches = new string[] { "main" }
                    },

                    PullRequest = new PullRequestEvent
                    {
                        Branches = new string[] { "main" }
                    }
                },

                Jobs = new Jobs
                {
                    Build = new BuildJob
                    {
                        RunsOn = BuildMachines.Windows2019,

                        Steps = new List<GithubTask>
                    {
                        new CheckoutTaskV2
                        {
                            Name = "Check Out"
                        },

                        new SetupDotNetTaskV1
                        {
                            Name = "Setup Dot Net Version",

                            TargetDotNetVersion = new TargetDotNetVersion
                            {
                                DotNetVersion = "7.0.100",
                                IncludePrerelease = true
                            }
                        },

                        new RestoreTask
                        {
                            Name = "Restore"
                        },

                        new DotNetBuildTask
                        {
                            Name = "Build"
                        },

                        new TestTask
                        {
                            Name = "Test"
                        }
                    }
                    }
                }
            };

            string yamlRelativeFilePath = "../../../../.github/workflows/build.yml";
            string yamlFullPath = System.IO.Path.GetFullPath(yamlRelativeFilePath);
            FileInfo yamlDefinition = new FileInfo(yamlFullPath);

            if (!yamlDefinition.Directory.Exists)
            {
                yamlDefinition.Directory.Create();
            }

            adotNetClient.SerializeAndWriteToFile(
                adoPipeline: githubPipeline,
                path: yamlRelativeFilePath);
        }
    }
}
