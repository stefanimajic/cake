﻿using System.Collections.Generic;
using Cake.Common.Tests.Properties;
using Cake.Common.Tools.NuGet.Pack;
using Cake.Common.Tools.NuGet.Push;
using Cake.Common.Tools.NuGet.Restore;
using Cake.Common.Tools.NuGet.Sources;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tests.Fakes;
using NSubstitute;

namespace Cake.Common.Tests.Fixtures
{
    public class NuGetFixture
    {
        public FakeFileSystem FileSystem { get; set; }
        public ICakeEnvironment Environment { get; set; }
        public IGlobber Globber { get; set; }
        public IProcessRunner ProcessRunner { get; set; }
        public IProcess Process { get; set; }
        public ICakeLog Log { get; set; }
        public IToolResolver NuGetToolResolver { get; set; }

        public NuGetFixture(FilePath toolPath = null, bool defaultToolExist = true, string xml = null, KeyValuePair<string, string>? sourceExists = null)
        {
            Environment = Substitute.For<ICakeEnvironment>();
            Environment.WorkingDirectory.Returns("/Working");

            Globber = Substitute.For<IGlobber>();
            Globber.Match("./tools/**/NuGet.exe").Returns(new[] { (FilePath)"/Working/tools/NuGet.exe" });

            Process = Substitute.For<IProcess>();
            Process.GetExitCode().Returns(0);
            ProcessRunner = Substitute.For<IProcessRunner>();
            ProcessRunner.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()).Returns(Process);

            

            NuGetToolResolver = Substitute.For<IToolResolver>();
            NuGetToolResolver.Name.Returns("NuGet");
            NuGetToolResolver.ResolveToolPath().Returns(
                defaultToolExist ? "/Working/tools/NuGet.exe" : "/NonWorking/tools/NuGet.exe"
                );

            Log = Substitute.For<ICakeLog>();

            FileSystem = new FakeFileSystem(true);
            FileSystem.GetCreatedFile("/Working/existing.nuspec", xml ?? Resources.Nuspec_NoMetadataValues);

            if (defaultToolExist)
            {
                FileSystem.GetCreatedFile("/Working/tools/NuGet.exe");
            }

            if (toolPath != null)
            {
                FileSystem.GetCreatedFile(toolPath);
            }
            if (sourceExists.HasValue)
            {
                Process.GetStandardOutput().Returns(
                    new[]
                    {
                        "  1.  https://www.nuget.org/api/v2/ [Enabled]",
                        "      https://www.nuget.org/api/v2/",
                        string.Format(
                            "  2.  {0} [Enabled]",
                            sourceExists.Value.Key
                            ),
                        string.Format(
                            "      {0}",
                            sourceExists.Value.Value
                            )
                    }
                    );
            }
            else
            {
                Process.GetStandardOutput().Returns(
                    new string[0]
                    );
            }
        }

        public NuGetPacker CreatePacker()
        {
            return new NuGetPacker(FileSystem,Environment, ProcessRunner,Log, NuGetToolResolver);
        }

        public NuGetPusher CreatePusher()
        {
            return new NuGetPusher(FileSystem, Environment, Globber, ProcessRunner, NuGetToolResolver);
        }

        public NuGetRestorer CreateRestorer()
        {
            return new NuGetRestorer(FileSystem, Environment, ProcessRunner, NuGetToolResolver);
        }

        public NuGetSources CreateSources()
        {
            return new NuGetSources(FileSystem, Environment, ProcessRunner, NuGetToolResolver);
        }
    }
}