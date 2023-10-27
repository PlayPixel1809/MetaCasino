mergeInto(LibraryManager.library, {

  Hello: function () {
    window.alert("Hello, world!");
  },

  HelloString: function (str) {
    window.alert(UTF8ToString(str));
  },

  PrintFloatArray: function (array, size) {
    for(var i = 0; i < size; i++)
    console.log(HEAPF32[(array >> 2) + i]);
  },

  AddNumbers: function (x, y) {
    return x + y;
  },

  StringReturnValueFunction: function () {
    var returnStr = "bla";
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  
   GetEmailFromUrl: function () {
   
    const queryString = window.location.search;
    console.log(queryString);

    const urlParams = new URLSearchParams(queryString);

    var email = urlParams.get('email');
    console.log(email);
    
	var returnStr = email;
	console.log(returnStr);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
	
  },

  GetPasswordFromUrl: function () {
   
    const queryString = window.location.search;
    console.log(queryString);

    const urlParams = new URLSearchParams(queryString);

    var password = urlParams.get('password');
    console.log(password);
    
	var returnStr = password;
	console.log(returnStr);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
	
  },

  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },

});