
require 'albacore'
require 'rbconfig'
require_relative './src/.nuget/nuget'

def with_mono_properties msb
  solution_dir = File.join(File.dirname(__FILE__),'src')
  nuget_tools_path = File.join(solution_dir, '.nuget')
  msb.prop :SolutionDir, solution_dir
  msb.prop :NuGetToolsPath, nuget_tools_path
  msb.prop :NuGetExePath, File.join(nuget_tools_path, 'NuGet.exe')
  msb.prop :PackagesDir, File.join(solution_dir, 'packages')
end

desc "build"
build :build => [:install_packages] do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Mixed Platforms"
  if NuGet::os != :windows
    with_mono_properties msb
  end
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln =File.join('.',"src", "ElasticElmah.Core.sln")
end

desc "test using nunit console"
test_runner :test => [:build] do |nunit|
  nunit.exe = NuGet::nunit_path
  files = [File.join(File.dirname(__FILE__),"src","ElasticElmah.Appender.Tests","bin","Debug","ElasticElmah.Appender.Tests.dll")]
  nunit.files = files 
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
    NuGet::exec "pack ElasticElmah.Appender.nuspec"
  end
end

task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
  cd File.join('.',"nuget/ElasticElmah.Tail") do
    NuGet::exec "pack ElasticElmah.Tail.nuspec"
  end
end

desc "Install missing NuGet packages."
task :install_packages do
  package_paths = FileList["src/**/packages.config"]+["src/.nuget/packages.config"]

  package_paths.each.each do |filepath|
    begin
      NuGet::exec("i #{filepath} -o ./src/packages -source http://www.nuget.org/api/v2/")
    rescue
      puts "Failed to install missing packages ..."      
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
