
require 'albacore'

task :default => ['build']

def nunit_cmd()
  return Dir.glob(File.join(File.dirname(__FILE__),"src","packages","NUnit.Runners.*","tools","nunit-console.exe")).first
end
desc "build using msbuild"
msbuild :build do |msb|
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Rebuild
  msb.verbosity = 'quiet'
  msb.solution =File.join('.',"src", "ElasticElmah.Core.sln")
end
desc "test using nunit console"
nunit :test => :build do |nunit|
  nunit.command = nunit_cmd()
  nunit.assemblies File.join('.',"src","ElasticElmah.Appender.Tests","bin","Debug","ElasticElmah.Appender.Tests.dll")
end

task :core_copy_to_nuspec => [:build] do
  output_directory_lib = File.join('.',"nuget/ElasticElmah.Appender/lib/40/")
  mkdir_p output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Appender/bin/Debug/ElasticElmah.Appender.dll"), output_directory_lib
  
end

task :runners_copy_to_nuspec => [:build] do
  output_directory_lib = File.join('.',"nuget/ElasticElmah.Tail/tools/")
  mkdir_p output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Tail/bin/Debug/ElasticElmah.Tail.exe"), output_directory_lib
  cp Dir.glob("./src/ElasticElmah.Tail/bin/Debug/log4net.dll"), output_directory_lib
  
end

desc "create the nuget package"
task :nugetpack => [:core_nugetpack, :runners_nugetpack]

task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
  cd File.join('.',"nuget/ElasticElmah.Appender") do
    sh "..\\..\\src\\.nuget\\NuGet.exe pack ElasticElmah.Appender.nuspec"
  end
end

task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
  cd File.join('.',"nuget/ElasticElmah.Tail") do
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

namespace :mono do
  def with_properties hash
      solution_dir = File.join(File.dirname(__FILE__),'src')
      nuget_tools_path = File.join(solution_dir, '.nuget')

      to_add = {:SolutionDir => solution_dir,
      :NuGetToolsPath => nuget_tools_path,
      :NuGetExePath => File.join(nuget_tools_path, 'NuGet.exe'),
      :PackagesDir => File.join(solution_dir, 'packages')}.merge(hash)
      return to_add
  end

  desc "build isop on mono"
  task :build => [:install_packages, :appender_tests, :tail]

  xbuild :appender do |msb|
    msb.properties with_properties(configuration: :Debug)
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join('.', "src", "ElasticElmah.Appender", "ElasticElmah.Appender.csproj")
  end

  xbuild :tail do |msb|
    msb.properties with_properties(configuration: :Debug)
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join('.', "src", "ElasticElmah.Tail", "ElasticElmah.Tail.csproj")
  end

  xbuild :appender_tests => [:appender] do |msb|
    msb.properties with_properties(configuration: :Debug)
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join('.', "src", "ElasticElmah.Appender.Tests", "ElasticElmah.Appender.Tests.csproj")
  end

  desc "Install missing NuGet packages."
  task :install_packages do |cmd|
    FileList["src/**/packages.config"].each do |filepath|
      sh "mono --runtime=v4.0.30319 ./src/.nuget/NuGet.exe i #{filepath} -o ./src/packages -source http://www.nuget.org/api/v2/"
    end
  end
  
  desc "test with nunit"
  task :test => :appender_tests do
    # does not work for some reason 
    command = "nunit-console4"
    assemblies = "ElasticElmah.Appender.Tests.dll"
    cd "src/ElasticElmah.Appender.Tests/bin/Debug" do
      sh "#{command} #{assemblies}"
    end
  end

end


namespace :ruby do
  require 'bundler/gem_helper'

  Bundler::GemHelper.install_tasks({:dir=>File.dirname(__FILE__),:name=>'elasticelmah'})

  require 'rake/testtask'
  desc "test ruby version"
  Rake::TestTask.new(:test) do |t|
    dir = File.join(File.dirname(__FILE__),'tests')
      t.libs << "test"
      t.test_files = FileList[File.join(dir,'*_test*.rb')]
      t.verbose = true
  end

end
