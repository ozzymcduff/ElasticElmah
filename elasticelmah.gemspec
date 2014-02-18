# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name        = 'elasticelmah'
  s.version     = '0.0.2'
  s.platform    = Gem::Platform::RUBY
  s.authors     = ['Oskar Gewalli']
  s.email       = ''
  s.summary     = 'ElasticSearch log4r appender'
  s.description = <<-EOF
An appender for log4r that appends to elastic search
EOF

  s.add_dependency 'elasticsearch'
  s.add_dependency 'log4r'

  s.add_development_dependency 'rake'
  s.add_development_dependency 'bundler'

  s.files         = Dir.glob('lib/**/*.rb') + Dir.glob('bin/**/*')
  s.test_files    = Dir.glob('tests/**/*.rb')
  s.executables   = []
  s.require_paths = ['lib']
end
