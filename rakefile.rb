
require 'albacore'

task :default => ['build']

def nunit_cmd()
  return Dir.glob(File.join(File.dirname(__FILE__),"src","packages","NUnit.Runners.*","tools","nunit-console.exe")).first
end
dir = File.dirname(__FILE__)
desc "build using msbuild"
msbuild :build do |msb|
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Rebuild
  msb.verbosity = 'quiet'
  msb.solution =File.join(dir,"src", "ElasticElmah.Core.sln")
end
desc "test using nunit console"
nunit :test => :build do |nunit|
  nunit.command = nunit_cmd()
  nunit.assemblies File.join(dir,"src","ElasticElmah.Appender.Tests/bin/Debug/ElasticElmah.Appender.Tests.dll")
end

task :core_copy_to_nuspec => [:build] do
  output_directory_lib = File.join(dir,"nuget/ElasticElmah.Appender/lib/40/")
  mkdir_p output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Appender/bin/Debug/ElasticElmah.Appender.dll"), output_directory_lib
  
end

task :runners_copy_to_nuspec => [:build] do
  output_directory_lib = File.join(dir,"nuget/ElasticElmah.Tail/tools/")
  mkdir_p output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Tail/bin/Debug/ElasticElmah.Tail.exe"), output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Tail/bin/Debug/log4net.dll"), output_directory_lib
  
end

desc "create the nuget package"
task :nugetpack => [:core_nugetpack, :runners_nugetpack]

task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
  cd File.join(dir,"nuget/ElasticElmah.Appender") do
    sh "..\\..\\src\\.nuget\\NuGet.exe pack ElasticElmah.Appender.nuspec"
  end
end

task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
  cd File.join(dir,"nuget/ElasticElmah.Tail") do
    sh "..\\..\\src\\.nuget\\NuGet.exe pack ElasticElmah.Tail.nuspec"
  end
end

desc "Install missing NuGet packages."
exec :install_packages do |cmd|
  FileList["src/**/packages.config"].each do |filepath|
    cmd.command = "./src/.nuget/NuGet.exe"
    cmd.parameters = "i #{filepath} -o ./src/packages"
  end
end

desc "Install missing NuGet packages."
task :install_packages_mono do |cmd|
  FileList["src/**/packages.config"].each do |filepath|
    sh "mono ./src/.nuget/NuGet.exe i #{filepath} -o ./src/packages"
  end
end
