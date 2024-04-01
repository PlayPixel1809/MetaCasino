mergeInto(LibraryManager.library, {

  GetToken: function () 
  {
    console.log("SetToken");
    window.addEventListener("message", function(event) 
    {
        console.log("new7");
        console.log(event);
        
        // Check the origin of the message to ensure it's from a trusted source

        if (event.origin != "https://theomniverse.city") 
        { 
            SendMessage('Scripts', 'ShowTokenError', event.origin);
            return; 
        }
  
        // The user's information is contained in event.data
        let data = event.data;
    
        if(data)
        {
            console.log(data);
        
            username = data.username;    
            email = data.email;
            jwt = data.token;
            baseURL = data.baseURL;
            console.log(username, email, jwt);

            SendMessage('Scripts', 'SetToken', jwt);
        }

        
    });

    window.parent.postMessage({ command: 'getUserInfo' }, '*');
  },


});