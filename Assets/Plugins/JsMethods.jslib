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
	if(!email)
	{
		alert("email not set in url");
		return;
	}
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
	if(!password)
	{
		alert("password not set in url");
		return;
	}
	
    console.log(password);
    
	var returnStr = password;
	console.log(returnStr);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
	
  },


  GetToken: function () 
  {
    console.log("SetToken");
    window.addEventListener("message", function(event) 
    {
        console.log("addEventListener");

        // Check the origin of the message to ensure it's from a trusted source
        if (event.origin !== "http://theomniverse.city") 
        { 
            SendMessage('Scripts', 'ShowTokenError', 'event.origin !== http://theomniverse.city');
            return; 
        }
  
        // The user's information is contained in event.data
        let data = event.data;
    
        
        if(data && (data.event == "userInfoEvent"))
        {
            console.log("userInfoEvent");
            
            let userInfo = data.data;
        
            username = userInfo.username;    
            email = userInfo.email;
            jwt = userInfo.token;
            baseURL = userInfo.baseURL;
            console.log(username, email, jwt);

            SendMessage('Scripts', 'SetToken', jwt);
        }
    });

    window.parent.postMessage({ command: 'getUserInfo' }, '*');
  },


  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },



});