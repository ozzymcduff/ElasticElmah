# -*- mode: ruby -*-
# vi: set ft=ruby :

# Vagrantfile API/syntax version. Don't touch unless you know what you're doing!
VAGRANTFILE_API_VERSION = "2"

Vagrant.configure(VAGRANTFILE_API_VERSION) do |config|
  config.vm.box = "mono-3.2.8"
  config.vm.box_url = "https://dl.dropboxusercontent.com/u/20581307/mono3.2.8.box"
  config.vm.provision :shell, :path => "provisioning/install-rvm.sh",  :args => "stable"
  config.vm.provision :shell, :path => "provisioning/install-ruby.sh", :args => "2.1.0 puppet"
  
  config.vm.provision :puppet, :module_path => "modules" do |puppet|
    puppet.manifests_path = "manifests"
    puppet.manifest_file  = "default.pp"
  end
  
  config.vm.provider "virtualbox" do |v|
    v.customize ["modifyvm", :id, "--cpus", "2"]
    v.customize ["modifyvm", :id, "--memory", "1024"]
  end
  
  config.vm.provision :shell, :path => "provisioning/setup.sh"

end
