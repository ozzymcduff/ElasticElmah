
package { "python-software-properties":
    ensure => installed
}

exec { "apt-get update":
    command => "/usr/bin/apt-get update",
}
 
package { "mono-devel":
    ensure => installed
}

package { "nunit-console":
    ensure => installed,
    require => Package["mono-devel"],
}
