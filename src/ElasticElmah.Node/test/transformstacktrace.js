var assert=require("assert"),
	transformstacktrace = require('../transformstacktrace.js');

var tokenize = transformstacktrace.tokenize;
var colorize = transformstacktrace.colorize;

describe('transformstacktrace', function(){
  describe('single line with type and method', function(){
	var str="   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs:line 56";
    it('should tokenize',function(){
 		var m = tokenize(str);
 		var tokenized = [ { t: 'w', s: '   ' },
  { t: '_at', s: 'at' },
  { t: 'w', s: ' ' },
  { t: 'type', s: 'ElasticElmahMVC.Models.ErrorLogPage' },
  { t: 'type_method_delim', s: '.' },
  { t: 'method', s: 'OnLoad' },
  { t: '_(',s:'('},
  { t: '_)',s:')'},
  { t: 'w', s: ' ' },
  { t: '_in', s: 'in' },
  { t: 'w', s: ' ' },
  { t: 'file', s: 'c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs' },
  { t: '_:', s: ':' },
  { t: '_line', s: 'line' },
  { t: 'w', s: ' ' },
  { t: 'line', s: '56' } ];
 		assert.deepEqual(m,tokenized);
    });
    it('should colorize',function(){
    	var ct = colorize(str);
    	//console.log(ct);
    });
  });
  describe('line with ctor', function(){
  	var str ="     at System.Web.HttpContextWrapper..ctor(HttpContext httpContext)";
  	
  	it('should tokenize', function(){
  		var m = tokenize(str);
		var tokenized = [ { t: 'w', s: '     ' },
  { t: '_at', s: 'at' },
  { t: 'w', s: ' ' },
  { t: 'type', s: 'System.Web.HttpContextWrapper' },
  { t: 'type_method_delim', s: '.' },
  { t: 'method', s: '.ctor' },
  { t: '_(', s: '(' },
  { t: 'type', s: 'HttpContext' },
  { t: 'w', s: ' ' },
  { t: 'var', s: 'httpContext' },
  { t: '_)', s: ')' } ];
  		assert.deepEqual(m, tokenized);
  	});
  	it('should colorize',function(){
    	var ct = colorize(str);
    	//console.log(ct);
    });
  });

  describe('line without \'var\' name in parameter', function(){
  	var str ="   at lambda_method(Closure , Task )";
  	it('should tokenize', function(){
	  	var m = tokenize(str);
	  	var tokenized = [ { t: 'w', s: '   ' },
  { t: '_at', s: 'at' },
  { t: 'w', s: ' ' },
  { t: 'method', s: 'lambda_method' },
  { t: '_(', s: '(' },
  { t: 'type', s: 'Closure' },
  { t: 'w', s: ' ' },
  { t: '_,', s: ',' },
  { t: 'w', s: ' ' },
  { t: 'type', s: 'Task' },
  { t: 'w', s: ' ' },
  { t: '_)', s: ')' } ];
	  	assert.deepEqual(m, tokenized);
	});
	it('should colorize',function(){
    	var ct = colorize(str);
    	//console.log(ct);
    });
  });
  describe('unrecognized line', function(){
  	it('Could look like', function(){
  		var str ="System.ArgumentNullException: Value cannot be null.";
  		var m = tokenize(str);
  		var tokenized = [{t:'unreq',s:str}];
  		assert.deepEqual(m,tokenized);
	});
	it('Other', function(){
  		var str ="Parameter name: httpContext";
  		var m = tokenize(str);
  		var tokenized = [{t:'unreq',s:str}];
  		assert.deepEqual(m,tokenized);
	});
	it('Obvious', function(){
  		var str ="--- End of stack trace from previous location where exception was thrown ---";
  		var m = tokenize(str);
  		var tokenized = [{t:'unreq',s:str}];
  		assert.deepEqual(m,tokenized);
  		var ct = colorize(str);
	});
  });
//
});
