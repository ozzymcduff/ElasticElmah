(function(exports) {
	var root = this;
	function tokenize(str){
		var split = str.split('\n');
		var tokens = [];
		for (var i = 0; i < split.length; i++) {
			if (i>0){
				tokens.push({t:'newline',s:'\n'});
			}
			tokenizeLine(split[i], tokens);
		};
		return tokens;
	}
	
	exports.tokenize = tokenize;
	
	function tokenizeLine(str, tokens){
		if (!tryTokenizeAtInLine(str, tokens)){
			//--- End of stack trace from previous location where exception was thrown ---
			// or message, can be random something
			tokens.push({t:'unreq',s:str});
			//throw new Error('Line not req '+str);
		}
	}
	function pushWs(str, tokens){
		if (str.match(/^(\s*)$/)){
			if (str.length > 0){
				tokens.push({t:'w',s:str});
			}
		}else{
			throw new Error('expected whitespace, but found: '+str);
		}
	}
	function tryTokenizeAtInLine(str, tokens){
		var m = str.match(/^(\s*)(at)(\s+)([^\(]+)\(([^\(]*)\)(.*)$/);
		if (m){
			pushWs(m[1],tokens);
			tokens.push({t:'_at',s:m[2]});
			pushWs(m[3],tokens);
			tokenizeTypeAndMethod(m[4],tokens);
			tokens.push({t:'_(',s:'('});
			if (m[5]){
				tokenizeParams(m[5], tokens)
			}
			tokens.push({t:'_)',s:')'});
			tryTokenizeIn(m[6], tokens);
			return true;
		}else{
			return false;
		}
	}
	function tokenizeParams(str, tokens){
		var m = str.match(/^(\s*)(\w*)(\s*)([^, ]*)(\s*)(,?)/);
		if (m){
			//tokens.push({t:'params',s:m[5]});
			pushWs(m[1], tokens);
			tokens.push({t:'type',s:m[2]});
			pushWs(m[3], tokens);
			if (m[4].length>0){
				tokens.push({t:'var',s:m[4]});
			}
			pushWs(m[5], tokens);
			if (m[6]){
				tokens.push({t:'_,',s:m[6]});
				//console.log(str.substring(m[0].length));
				tokenizeParams(str.substring(m[0].length), tokens);
			}
		}else{
			throw new Error('expected param expression, but found '+str);
		}
	}
	function tryTokenizeIn(str, tokens){
		if (!str){
			return;
		}
		var m = str.match(/^(\s*)(in)(\s)(\w?\:?[^:]*)(\:)(line)(\s)(\d*)$/);
		if (m){
			tokens.push({t:'w',s:m[1]});
			tokens.push({t:'_in',s:m[2]});
			tokens.push({t:'w',s:m[3]});
			tokens.push({t:'file',s:m[4]});
			tokens.push({t:'_:',s:m[5]});
			tokens.push({t:'_line',s:m[6]});
			tokens.push({t:'w',s:m[7]});
			tokens.push({t:'line',s:m[8]});
		}else{
			pushWs(str,tokens);
		}
	}
	function tokenizeTypeAndMethod(str, tokens){
		var mm = str.match(/^(\w*)$/);
		if (mm){
			tokens.push({t:'method',s:mm[1]});
			return;
		}

		var m = str.match(/(\.)(\.?\w*)$/);
		if (!m){
			throw new Error("expected type and method but found: "+str);
		}
		var type = str.substring(0,str.length - m[0].length);
		tokens.push({t:'type',s:type});
		tokens.push({t:'type_method_delim', s:m[1]});
		tokens.push({t:'method',s:m[2]});
	}

	function colorize(str){
		var tokens = tokenize(str);
		var colors = [];
		var token;
		for (var i = 0; i < tokens.length; i++) {
			pushColorOf(tokens[i],colors);
		};
		return {
			tokens:tokens,
			colors:colors
		};
	}

	exports.colorize = colorize;

	function pushColorOf(token, colors){
		switch(token.t){
			case 'w':
				colors.push('None');
			break;
			case '_at':
			case '_in':
			case '_line':
				colors.push('Keyword');
			break;
			case 'type':
			case 'type_method_delim':
				colors.push('Type');
			break;
			case 'method':
				colors.push('Method');
			break;
			case 'var':
				colors.push('Variable');
				break;
			case '_(':
			case '_)':
			case '_,':
			case 'unreq':
				colors.push('None');
				break;
			case 'file':
			case '_:':
				colors.push('File');
				break;
			case 'line':
				colors.push('None');
				break;
			default:
				throw new Error("unknown token :"+token.t)
		}
	}

}).call(this, module.exports );
